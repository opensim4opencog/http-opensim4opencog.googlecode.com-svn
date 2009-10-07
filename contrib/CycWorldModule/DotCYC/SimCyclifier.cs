﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Xml.Linq;
using cogbot;
using cogbot.Listeners;
using cogbot.TheOpenSims;
using cogbot.Utilities;
using OpenMetaverse;
using org.opencyc.api;
using org.opencyc.cycobject;
using CUID = org.opencyc.util.UUID;
//using Guid=org.opencyc.cycobject.Guid;
//using CycFort = org.opencyc.cycobject.CycObject;
using OpenMetaverse.Assets;
using PathSystem3D.Navigation;
using Exception=System.Exception;
using Object=System.Object;
using UUID=OpenMetaverse.UUID;

namespace CycWorldModule.DotCYC
{
    public class SimCyclifier : SimEventSubscriber
    {
        // ReSharper disable InconsistentNaming

        public static bool UseCyc = true;
        public static bool ClearKBBetweenSessions = false;
        static public CycAccess cycAccess;
        static public CycFort vocabMt;
        static public CycFort assertMt;
        static public bool ProcessEvents = true;
        static public Dictionary<object, CycFort> simFort
        {
            get
            {
                return cycTerms;
            }
        }// new Dictionary<string, CycFort>();
        static CycConnectionForm cycConnection;
        static readonly DateTime baseTime = new DateTime(1970, 1, 1, 0, 0, 0);
        private static readonly TaskQueueHandler cycAccessQueueHandler = new TaskQueueHandler("CycAssertions", 0);
        private static TaskQueueHandler taskQueueHandler = null;// new TaskQueueHandler("SimCyclifier", 0);

        readonly static public Dictionary<object, CycFort> cycTerms = new Dictionary<object, CycFort>();
        readonly static public Dictionary<Object, int> hashChanges = new Dictionary<Object, int>();
        List<UUID> SimObjectsAdded = new List<UUID>();
        internal static readonly Dictionary<Type, CycTypeInfo> typeFort = new Dictionary<Type, CycTypeInfo>();

        //static readonly List<String> SkipVerbs = new List<string>() { "on-log-message", "on-login", "on-event-queue-running", "on-sim-connecting" };
        public static SimCyclifier Master;
        static object SimCyclifierLock = new object();
        // ReSharper restore InconsistentNaming
        public void OnEvent(SimObjectEvent evt)
        {
            if (cycAccess == null)
            {
                //Console.WriteLine("No Cyc connection");
                return;
            }
            if (evt.EventType == SimEventType.UNKNOWN) return;
            if (evt.EventType == SimEventType.NETWORK) return;
            if (evt.EventClass == SimEventClass.PERSONAL) return;
            //if (SkipVerbs.Contains(evt.Verb.ToLower())) return;
            if (!UseQueue)
            {
                OnEvent0(evt);
                return;
            }
            taskQueueHandler.Enqueue(() => OnEvent0(evt));
        }

        static public void OnEvent0(SimObjectEvent evt)
        {
            if (!ProcessEvents) return;
            try
            {
                if (evt.EventType == SimEventType.DATA_UPDATE)
                {
                    foreach (var v in evt.GetArgs())
                    {
                        Master.DataUpdate(v);
                    }
                    return;
                }
                if (evt.EventType == SimEventType.EFFECT)
                {
                    if (evt.Verb == "LookAtType-Idle") return;
                    if (evt.Verb == "LookAtType-FreeLook") return;
                }
                CycFort fort = Master.FindOrCreateCycFort(evt);
                // Debug("to fort -> " + fort);
            }
            catch (Exception e)
            {

                Exception(e);
                Debug("Error to cyc -> " + evt + " " + e);
            }

        }

        internal void DataUpdate(object v)
        {
            object constant = ToFort(v);
            if (constant is CycFort && v is SimObject)
            {
                SimObject o = (SimObject) v;
                if (WorldObjects.GridMaster != null && WorldObjects.GridMaster.TheSimAvatar.Distance(o) < 30)
                {
                    SaveInfoMap(constant as CycFort, o);
                }
                else
                    taskQueueHandler.Enqueue(() => SaveInfoMap(constant as CycFort, o));
            }
        }

        public void Dispose()
        {
            ProcessEvents = false;            
            taskQueueHandler.Dispose();
            cycAccessQueueHandler.Dispose();
        }

        public SimCyclifier(CycWorldModule tf)
        {
            lock (SimCyclifierLock)
            {
                if (Master == null) Master = this;
                if (!UseCyc) return;
                if (cycConnection == null)
                {
                    cycConnection = tf.CycConnectionForm;
                }
                if (taskQueueHandler == null)
                {
                    taskQueueHandler = new TaskQueueHandler("SimCyclifier", 0);
                    taskQueueHandler.AddFirst(AssertKE);
                }

            }
        }

        private void AssertKE()
        {
            cycAccessQueueHandler.NoQueue = true;
            cycAccess = cycConnection.getCycAccess();
            if (cycAccess == null)
            {
                Console.WriteLine("No Cyc connection");
                return;
            }
            assertMt = cycAccess.createIndividual("SimDataMt", "#$DataMicrotheory for the simulator", "UniversalVocabularyMt",
                                        "DataMicrotheory");
            vocabMt = cycAccess.createIndividual("SimVocabMt", "#$VocabularyMicrotheory for the simulator",
                                       "UniversalVocabularyMt",
                                       "VocabularyMicrotheory");


            if (ClearKBBetweenSessions)
            {
                cycAccess.converseVoid("(fi-kill (find-or-create-constant \"SimEvent-LISPFn\"))");
                cycAccess.converseVoid("(fi-kill (find-or-create-constant \"SimEvent-EFFECTFn\"))");
                cycAccess.converseVoid("(fi-kill (find-or-create-constant \"SimEvent-ANIMFn\"))");
                cycAccess.converseVoid("(fi-kill (find-or-create-constant \"SimEvent-UNKNOWNFn\"))");
                cycAccess.converseVoid("(fi-kill (find-or-create-constant \"SimEvent-NETWORKFn\"))");
                cycAccess.converseVoid("(fi-kill (find-or-create-constant \"SimEvent-SOCIALFn\"))");
                cycAccess.converseVoid("(fi-kill (find-or-create-constant \"SimEvent-MOVEMENTFn\"))");
            }
            cycAccess.converseVoid("(fi-kill (find-or-create-constant \"TheDefaultSimInstance\"))");

            assertIsa(C("TheDefaultSimInstance"), C("Thing"));

            simFort["SimObject"] = createCollection("SimObject", "#$SpatiallyDisjointObjectType for the simulator",
                                                    "SimVocabMt", "SpatiallyDisjointObjectType", null);
            simFort["SimAsset"] = createCollection("SimAsset",
                                                   "A SimAsset is a #$PartiallyTangibleTypeByPhysicalFeature from the simulator such as #$BlueColor or #$BlowingAKiss animation",
                                                   "SimVocabMt", "Collection", "PartiallyTangibleTypeByPhysicalFeature");
            simFort["SimAvatar"] = createCollection("SimAvatar", "#$Agent-Generic for the simulator", "SimVocabMt",
                                                    "Collection", "Agent-Generic");

            assertGenls(simFort["SimAvatar"], simFort["SimObject"]);
            FunctionToIndividual("SimRegionFn", "SimRegion", "A region in the simulator");
            FunctionToIndividual("SimAvatarFn", "SimAvatar", "An avatar in the simulator");
            FunctionToIndividual("SimObjectFn", "SimObject", "A primitive in the simulator");
            assertIsa(simFort["SimObject"], C("SpatiallyDisjointObjectType"));

            // FunctionToCollection("SimAnimationFn", "SimAnimation", "An animation in the simulator");

            assertIsa(C("simEventData"), C("VariableArityPredicate"));
            assertIsa(C("SimObjectEvent"), C("Collection"));
            assertIsa(C("SimProperty"), C("Collection"));
            assertIsa(C("SimPredicate"), C("Collection"));
            assertGenls(C("SimPredicate"), C("Predicate"));
            assertGenls(C("SimProperty"), C("BinaryPredicate"));
            assertGenls(C("SimProperty"), C("SimPredicate"));
            assertIsa(C("SimObjectType"), C("Collection"));
            assertGenls(C("SimObjectEvent"), C("Event"));
            FunctionToCollection("SimAssetFn", "SimAsset", "Simulator assets that denote a feature set");
            assertIsa(C("SimAssetFn"), C("CollectionDenotingFunction"));


            // visit libomv
            if (cycAccess.find("SimEnumCollection") == null)
            {
                Debug("Loading SimEnumCollection Collections ");
                assertIsa(C("SimEnumCollection"), C("Collection"));
                assertGenls(C("SimEnumCollection"), C("Collection"));
                assertIsa(C("SimEnumCollection"), C("CollectionType"));
                assertGaf(C("comment"), C("SimEnumCollection"), "Enums collected from SecondLife");
                VisitAssembly(Assembly.GetAssembly(typeof(AssetType)));
                VisitAssembly(Assembly.GetAssembly(typeof(Vector4)));
                VisitAssembly(Assembly.GetAssembly(typeof(AgentManager)));
            }
            //else
            {
                Debug("Found SimEnumCollection Collections ");
            }
            if (cycAccess.find("SimEventType-SCRIPT") == null)
            {
                VisitAssembly(Assembly.GetAssembly(typeof(SimEventType)));
            }

            simFort["SimRegion"] = cycAccess.createCollection("SimRegion", "A region in the simulator", vocabMt, C("Collection"),
                           C("GeographicalPlace-3D"));
            assertGenls(simFort["SimRegion"], C("Polyhedron"));

            /*
             * (isa SimLSLTextFn CollectionDenotingFunction) in UniversalVocabularyMt
(isa SimLSLText Collection) in UniversalVocabularyMt
             */
            assertGenls(C("BodyMovementEvent"), C("SimAnimation"));
            conceptuallyRelated(C("EmittisgSound"), C("SimSound"));
            conceptuallyRelated(C("Sound"), C("SimSound"));
            conceptuallyRelated(C("AnimalBodyRegion"), C("SimBodypart"));
            conceptuallyRelated(C("SomethingToWear"), C("SimWearable"));
            conceptuallyRelated(C("Landmark"), C("SimLandmark"));
            conceptuallyRelated(C("VisualImage"), C("SimTexture"));



            bool newlyCreated;
            createIndividual("SimGlobalMapCoordinateSystem", "the secondlife global coordinate system", "CartesianCoordinateSystem", out newlyCreated);


            cycAccess.createCollection("SimRegionMapCoordinateSystem",
                                       "instances are secondlife regional coordinate systems", vocabMt, C("Collection"),
                                       C("ThreeDimensionalCoordinateSystem"));

            ResultIsa("SimRegionCoordinateSystemFn", "SimRegionMapCoordinateSystem",
                "#$SimRegionCoordinateSystemFn Takes a #$SimRegion and returns a example: (#$SimRegionCoordinateSystemFn (#$SimRegionFn \"LogicMoo\")) => #$ThreeDimensionalCoordinateSystem",
                simFort["SimRegion"]);

            createIndividual("PointInRegionFn", "Creates region 3D #$Point relative to (#$SimRegionFn :ARG1)", "SimVocabMt", "QuaternaryFunction");
            assertIsa(C("PointInRegionFn"), C("TotalFunction"));
            //assertGaf(C("isa"), simFort["PointInRegionFn"], C(""));
            assertGaf(C("arg1Isa"), simFort["PointInRegionFn"], C("IDString"));
            assertGaf(C("arg2Isa"), simFort["PointInRegionFn"], C("NumericInterval"));
            assertGaf(C("arg3Isa"), simFort["PointInRegionFn"], C("NumericInterval"));
            assertGaf(C("arg4Isa"), simFort["PointInRegionFn"], C("NumericInterval"));
            assertGaf(C("resultIsa"), simFort["PointInRegionFn"], C("Point"));
            cycAssert("(#$expansion #$PointInRegionFn "
                + " (#$PointIn3DCoordinateSystemFn (#$SimRegionCoordinateSystemFn"
                + " (#$SimRegionFn :ARG1)) :ARG2 :ARG3 :ARG4 ))");

            createIndividual("PointRelativeToFn", "Creates Local 3D #$Point relative to the #$SpatialThing-Localized in :ARG1", "SimVocabMt", "QuaternaryFunction");
            assertIsa(C("PointRelativeToFn"), C("QuaternaryFunction"));
            assertIsa(C("PointRelativeToFn"), C("TotalFunction"));
            assertGaf(C("arg1Isa"), simFort["PointRelativeToFn"], C("SpatialThing-Localized"));
            assertGaf(C("arg2Isa"), simFort["PointRelativeToFn"], C("NumericInterval"));
            assertGaf(C("arg3Isa"), simFort["PointRelativeToFn"], C("NumericInterval"));
            assertGaf(C("arg4Isa"), simFort["PointRelativeToFn"], C("NumericInterval"));
            assertGaf(C("resultIsa"), C("PointRelativeToFn"), C("Point"));


            cycAssert("(#$pointInSystem (#$PointInRegionFn ?STR ?X ?Y ?Z) (#$SimRegionCoordinateSystemFn (#$SimRegionFn ?STR)))");
            cycAssert("(#$pointInSystem (#$PointInRegionFn \"Daxlandia\" 128 120 27) (#$SimRegionCoordinateSystemFn (#$SimRegionFn \"Daxlandia\")))");
            ProbeNewAssets();
            cycAccessQueueHandler.NoQueue = true;
        }

        static int lastAssetCount = 0;
        static void ProbeNewAssets()
        {
            ICollection<SimAsset> v = SimAssetStore.GetAssets(AssetType.Unknown);
            lock (v)
                if (v.Count != lastAssetCount)
                {
                    lastAssetCount = v.Count;
                    {
                        foreach (SimAsset asset in v)
                        {
                            Master.ToFort(asset);
                        }
                    }
                }
        }

        private void VisitAssembly(Assembly assembly)
        {
            foreach (Type t in assembly.GetTypes())
            {

                try
                {
                    VisitType(t);
                }
                catch (Exception e)
                {
                    Exception(e);
                }
            }

        }

        public void VisitType(Type type)
        {
            if (type.IsSubclassOf(typeof(Asset)))
            {
                string fn = type.Name;
                if (fn.StartsWith("Asset")) fn = fn.Substring(5);
                string col = string.Format("Sim{0}", fn);
                string comment =
                    "A spec of #$SimAsset that is a #$PartiallyTangibleTypeByPhysicalFeature from the simulator's type " +
                    type.FullName + " such as #$BlueColor or #$BlowingAKiss animation";
                fn = string.Format("Sim{0}Fn", fn);
                FunctionToCollection(fn, col, comment);
                assertGaf(C("genlFuncs"), simFort[fn], simFort["SimAssetFn"]);
                assertGenls(simFort[col], simFort["SimAsset"]);
            }
            else //if (type.Namespace ==null || type.Namespace.StartsWith("OpenMetaverse"))
            {
                try
                {
                    if (type.IsEnum) VisitEnumType(type);
                    else
                    {

                    }
                }
                catch (Exception e)
                {
                    Exception(e);
                }
            }
        }

        private CycFort VisitEnumType(Type type)
        {
            lock (typeFort)
            {
                CycTypeInfo ctype;
                if (typeFort.TryGetValue(type, out ctype)) return ctype.cycFort;
                string name = string.Format("Sim{0}", type.Name);
                if (name.StartsWith("SimSim"))
                {
                    name = name.Substring(3);
                }
                CycFort cn = C(name);
                ctype = new CycTypeInfo(cn, type);
                return cn;
            }
        }

        public void ResultIsa(string fn, string col, string comment, CycFort arg1Isa)
        {
            simFort[fn] = createIndividual(fn, comment, "SimVocabMt", "ReifiableFunction");
            assertGaf(C("resultIsa"), simFort[fn], C(col));
            assertGaf(C("arg1Isa"), simFort[fn], arg1Isa);

        }

        public void conceptuallyRelated(CycFort a, CycFort b)
        {
            assertGaf(C("conceptuallyCoRelated"), a, b);
        }

        public void FunctionToCollection(string fn, string col, string comment)
        {
            simFort[fn] = createIndividual(fn, comment, "SimVocabMt", "UnaryFunction");
            assertIsa(simFort[fn], C("CollectionDenotingFunction"));
            assertIsa(simFort[fn], C("ReifiableFunction"));
            simFort[col] = createCollection(col, comment, "SimVocabMt", "Collection", null);
            assertIsa(simFort[col], C("PartiallyTangibleTypeByPhysicalFeature"));
            //not true assertIsa(simFort[fn], C("SubcollectionDenotingFunction"));
            //not true assertIsa(simFort[fn], C("TotalFunction"));
            assertGaf(C("resultIsa"), simFort[fn], C("PartiallyTangibleTypeByPhysicalFeature"));
            assertGaf(C("resultIsa"), simFort[fn], C("FirstOrderCollection"));
            assertGaf(C("resultGenl"), simFort[fn], simFort[col]);
        }

        private CycFort createCollection(string col, string comment, string simvocabmt, string isa, string genls)
        {
            CycFort fcol = C(col);
            assertIsa(fcol, C("Collection"));
            assertIsa(fcol, C(isa));
            if (genls != null) assertGenls(fcol, C(genls));
            assertGaf(C("comment"), fcol, comment);

            return fcol;
        }

        public void FunctionToIndividual(string fn, string col, string comment)
        {
            simFort[fn] = createIndividual(fn, comment, "SimVocabMt", "UnaryFunction");
            assertIsa(simFort[fn], C("IndividualDenotingFunction"));
            assertIsa(simFort[fn], C("ReifiableFunction"));
            CycFort fcol = C(col);
            assertIsa(fcol, C("Collection"));
            assertGaf(C("comment"), fcol, comment);
            // simFort[col] = createIndividual(col, comment, "SimVocabMt", "Collection");
            assertGaf(C("resultIsa"), simFort[fn], simFort[col]);
        }

        public CycFort createIndividual(string term, string comment, string mt, string type)
        {
            bool newlyCreated;
            return createIndividual(term, comment, mt, type, out newlyCreated);
        }
        public void assertGenls(CycFort a, CycFort b)
        {
            cycAccessQueueHandler.Enqueue(() =>
            {
                try
                {
                    cycAccess.assertGenls(a, b, vocabMt);
                }
                catch (Exception e)
                {

                    Exception(e);
                }
            });
        }

        public void assertIsa(CycFort a, CycFort b)
        {
            cycAccessQueueHandler.Enqueue(() =>
            {
                try
                {
                    cycAccess.assertIsa(a, b, vocabMt);
                }
                catch (Exception e)
                {

                    Exception(e);
                }
            });
        }

        public void assertGaf(CycFort a, CycFort b, CycFort c)
        {
            cycAccessQueueHandler.Enqueue(() =>
            {
                try
                {
                    cycAccess.assertGaf(vocabMt, a, b, c);
                }
                catch (Exception e)
                {

                    Exception(e);
                }
            });
        }

        public void assertGaf(CycFort a, CycFort b, string c)
        {
           // cycAccessQueueHandler.Enqueue(() =>
                              {
                                  try
                                  {
                                      cycAccess.assertGaf(vocabMt, a, b, c);
                                  }
                                  catch (Exception e)
                                  {

                                      Exception(e);
                                  }
                              }
            //);
        }


        private void CycListAssert(CycList list)
        {
            try
            {
                string ast = list.cyclifyWithEscapeChars();
                cycAssert(ast);
            }
            catch (Exception re)
            {
                //re.printStackTrace();
                Exception(re);
            }
        }

        public void cycAssert(string s)
        {
            string asert = string.Format("(fi-assert '{0} {1})", s, vocabMt.cyclifyWithEscapeChars());
            cycAccessQueueHandler.Enqueue(() =>
            {
                try
                {
                    if (!cycAccess.converseBoolean(asert))
                    {
                        Debug("Assertion failed: " + asert);
                    }
                }
                catch (Exception e)
                {

                    Exception(e);
                    return;
                }
            });
        }


        public CycFort createIndividual(string term, string comment, string mt, string type, out bool created)
        {
            lock (simFort)
            {
                if (simFort.ContainsKey(term))
                {
                    created = false;
                    return simFort[term];
                }
                created = true;
                if (type == "Collection")
                {
                    Trace();
                }
                CycFort indv = simFort[term] = C(term);
                bool NoQueue = cycAccessQueueHandler.NoQueue;
                cycAccessQueueHandler.NoQueue = true;
                assertIsa(indv, C("Individual"));
                assertIsa(indv, C(type));
                cycAccessQueueHandler.NoQueue = true;
                assertGaf(C("comment"), indv, comment);
                cycAccessQueueHandler.NoQueue = NoQueue;
                return indv;// simFort[term] = cycAccess.createIndividual(term, comment, mt, type);
            }

        }
        public CycFort createIndividual(string term, string comment, string type, out bool created)
        {
            return createIndividual(term, comment, "SimVocabMt", type, out created);

        }

        public CycFort FindOrCreateCycFort(SimObject obj)
        {
            CycFort constant;
            lock (cycTerms)
            {
                if (cycTerms.TryGetValue(obj, out constant))
                {
                    // SaveInfoMap(constant, obj);
                    return constant;
                }
                string name;
                string type;
                if (obj is SimAvatar)
                {
                    type = "SimAvatar";
                    name = obj.GetName();
                    if (name == null)
                    {
                        name = WorldObjects.GridMaster.GetUserName(obj.ID);
                    }
                    if (name == null)
                    {
                        name = obj.ID.ToString();
                    }
                }
                else
                {
                    type = "SimObject";
                    name = obj.ID.ToString();
                }

                //byte[] ba = id.GetBytes();
                ////ulong umsb = Utils.BytesToUInt64(ba);
                ////long msb = umsb;
                ////long lsb = 0L;
                //System.Guid g = new System.Guid();
                ////CUID cycid = CUID.nameUUIDFromBytes(ba);

                cycTerms[obj] = constant =
                                createIndividualFn(
                                    type, name,
                                    String.Format("{0} {1}", obj.GetName(), obj.ID), "SimVocabMt", type);
            }
            {
                if (obj is SimAvatar)
                {
                    // assertGaf(C("comment"), constant, obj.DebugInfo());
                }
                else
                {
                    assertGaf(C("comment"), constant, obj.DebugInfo());
                }
                return constant;
            }
        }

        private void SaveInfoMap(CycFort constant, SimObject obj)
        {
            var im = obj.GetInfoMap();
            lock (hashChanges)
            {
                if (hashChanges.ContainsKey(obj))
                {
                    if (hashChanges[obj] == im.Count)
                    {
                        return;
                    }
                }
                hashChanges[obj] = im.Count;
            }
            ICollection<NamedParam> infomap = im;
            lock (infomap)
            {
                infomap = new List<NamedParam>(infomap);
            }

            foreach (NamedParam o in infomap)
            {
                assertEventData(constant, o);
            }
        }

        private string ToPropName(string key)
        {
            if (key.StartsWith("sim"))
            {
                return key;
            }
            if (key.ToLower().StartsWith("sim"))
            {
                return "sim" + key;
            }
            return string.Format("sim{0}{1}", key.Substring(0, 1).ToUpper(), key.Substring(1));

        }

        public CycFort FindOrCreateCycFort(SimAsset simObj)
        {
            CycFort constant;
            lock (cycTerms)
            {
                if (cycTerms.TryGetValue(simObj, out constant)) return constant;
                constant = createIndividualColFn("Sim" + simObj.AssetType, simObj.Name, simObj.DebugInfo(), "SimVocabMt", "Sim" + simObj.AssetType);
                cycTerms[simObj] = constant;
            }
            assertGaf(C("comment"), constant, simObj.DebugInfo());
            return constant;
        }
        public CycFort FindOrCreateCycFort(Asset simObj)
        {
            return FindOrCreateCycFort(SimAssetStore.GetSimAsset(simObj));
        }

        public CycFort FindOrCreateCycFort(SimObjectType simObj)
        {
            lock (cycTerms)
            {
                CycFort constant;
                if (cycTerms.TryGetValue(simObj, out constant)) return constant;
                constant = cycAccess.createIndividual(simObj.AspectName, simObj.ToDebugString(), "SimVocabMt",
                                                      "SimObjectType");
                cycTerms[simObj] = constant;
                return constant;
            }
        }

        public static int TimeRep = 0;
        /// <summary>
        ///  (MilliSecondFn 34 (SecondFn 59 (MinuteFn 12 (HourFn 18 (DayFn 14 (MonthFn February (YearFn 1966))))))
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public CycFort FindOrCreateCycFort(DateTime dateTime)
        {
            switch (TimeRep)
            {
                case 0:
                    return new CycNart(makeCycList(C("DateFromStringFn"), dateTime.ToString("MMMM dd, yyyy HH:mm:ss.ffffZ")));
                case 1:
                    return new CycNart(
                        makeCycList(C("MilliSecondFn"), FindOrCreateCycFort(dateTime.Millisecond),
                                    makeCycList(C("SecondFn"), FindOrCreateCycFort(dateTime.Second),
                                                makeCycList(C("MinuteFn"), FindOrCreateCycFort(dateTime.Minute),
                                                            makeCycList(C("HourFn"), FindOrCreateCycFort(dateTime.Hour),
                                                                        makeCycList(C("DayFn"),
                                                                                    FindOrCreateCycFort(dateTime.Day),
                                                                                    makeCycList(C("MonthFn"),
                                                                                                C(dateTime.ToString("MMMM")),
                                                                                                makeCycList(
                                                                                                    C("YearFn"),
                                                                                                    FindOrCreateCycFort(
                                                                                                        dateTime.Year)))))))));

                default:
                    return
                        new CycNart(makeCycList(C("NuSketchSketchTimeFn"),
                                                FindOrCreateCycFort((dateTime - baseTime).Ticks / 10000)));
            }
        }

        //public CycFort FindOrCreateCycFort(Permissions simObj)
        //{
        //    return SomeValueFn(""+simObj);
        //}

        private CycFort SomeValueFn(string s)
        {
            return new CycNart(C("SubLStringParameterValueFn"), s);
        }

        public CycFort FindOrCreateCycFort(Primitive simObj)
        {
            return FindOrCreateCycFort(WorldObjects.GridMaster.GetSimObject(simObj));
        }

        public CycFort FindOrCreateCycFort(SimObjectEvent evt)
        {
            if (evt.EventType == SimEventType.DATA_UPDATE)
            {
                return null;
            }
            else
            {
                CycFort constant;
                lock (cycTerms)
                {
                    if (cycTerms.TryGetValue(evt, out constant)) return constant;
                    //   object[] forts = ToForts(simObj.Parameters);
                    cycTerms[evt] = constant = createIndividualFn("SimEvent-" + evt.EventType,
                                                                  evt.ToEventString(),
                                                                  evt.ToString(),
                                                                  "SimVocabMt",
                                                                  "SimObjectEvent");
                }
                bool wasNew;
                CycFort col = createIndividual(evt.GetVerb().Replace(" ", "-"),
                                               "Event subtype of #$SimObjectEvent", "SimVocabMt", "Collection",
                                               out wasNew);
                if (wasNew)
                {
                    assertGenls(col, C("SimObjectEvent"));
                }
                assertIsa(constant, col);
                string[] names = evt.ParameterNames();
                IList<NamedParam> args = evt.Parameters;
                string datePred;
                switch (evt.EventStatus)
                {
                    case SimEventStatus.Start:
                        datePred = (TimeRep == 2 ? "startingPoint" : "startingDate");
                        break;
                    case SimEventStatus.Stop:
                        datePred = (TimeRep == 2 ? "endingPoint" : "endingDate");
                        break;
                    case SimEventStatus.Once:
                    default:
                        datePred = (TimeRep == 2 ? "timePoint" : "dateOfEvent");
                        break;
                }
                assertEventData(constant, datePred, ToFort(evt.Time), new HashSet<MemberInfo>());

                int i = -1;
                foreach (NamedParam list in args)
                {
                    i++;
                    object o = list.Value;
                    if (o.GetType().IsEnum)
                    {
                        VisitEnumType(o.GetType());
                    }

                    if (list.Key != null)
                    {
                        string k = list.Key.ToString();
                        if (k == "isa")
                        {
                            if (o is String)
                            {
                                assertIsa(constant, C(o.ToString()));
                                continue;
                            }
                        }
                        if (k == "senderOfInfo" || k == "doneBy")
                        {
                            if (o is String)
                            {
                                o = new CycNart(C("SimAvatarFn"), o.ToString());
                            }
                        }
                        assertEventData(constant, list.Key.ToString(), o, newHashSet(args[i].info));
                    }
                    else
                    {
                        o = args[i].Value;
                        if (o.GetType().IsEnum)
                        {
                            ForEachEnumValue(delegate(CycFort f)
                                              {
                                                  assertIsa(constant, f);
                                              }, o);
                        }
                        else
                        {
                            assertEventData(constant, names[i], o, newHashSet(args[i].info));
                        }
                    }
                }
                return constant;
            }
        }

        private HashSet<MemberInfo> newHashSet(MemberInfo info)
        {
            HashSet<MemberInfo> set = new HashSet<MemberInfo>();
            if (info != null) set.Add(info);
            return set;
        }

        static System.Collections.IEnumerable Unfold(object value)
        {
            IList<object> results = new List<object>();
            var type = value.GetType();
            var utype = Enum.GetUnderlyingType(type);
            var values = Enum.GetValues(type);
            if (utype == typeof(byte) || utype == typeof(sbyte) || utype == typeof(Int16) || utype == typeof(UInt16) || utype == typeof(Int32))
            {
                var num = (Int32)Convert.ChangeType(value, typeof(Int32));
                foreach (var val in values)
                {
                    var v = (Int32)Convert.ChangeType(val, typeof(Int32));
                    if ((v & num) == v) results.Add(Enum.ToObject(value.GetType(), val));
                }
            }
            else if (utype == typeof(UInt32))
            {
                var num = (UInt32)value;
                foreach (var val in values)
                {
                    var v = (UInt32)Convert.ChangeType(val, typeof(UInt32));
                    if ((v & num) == v) results.Add(Enum.ToObject(value.GetType(), val));
                }
            }
            else if (utype == typeof(Int64))
            {
                var num = (Int64)value;
                foreach (var val in values)
                {
                    var v = (Int64)Convert.ChangeType(val, typeof(Int64));
                    if ((v & num) == v) results.Add(Enum.ToObject(value.GetType(), val));
                }
            }
            else if (utype == typeof(UInt64))
            {
                var num = (UInt64)value;
                foreach (var val in values)
                {
                    var v = (UInt64)Convert.ChangeType(val, typeof(UInt64));
                    if ((v & num) == v) results.Add(Enum.ToObject(value.GetType(), val));
                }
            }
            else
            {
                throw new NotSupportedException();
            }
            return results;
        }

        private delegate void WithEnum(CycFort p);
        private void ForEachEnumValue(WithEnum withValue, object p)
        {
            Type pType = p.GetType();
            Array pTypeValues = System.Enum.GetValues(pType);
            if (p is byte)
            {
                byte b = (byte)p;
                if (b == 0)
                {
                    withValue((CycFort)ToFort(p));
                }
                foreach (object v in pTypeValues)
                {
                    byte bv = (byte)v;
                    if (bv >= b)
                    {
                        withValue((CycFort)ToFort(v));
                        b -= bv;
                    }
                    if (b == 0) return;
                }
                return;
            }
            if (p is sbyte)
            {
                sbyte b = (sbyte)p;
                if (b == 0)
                {
                    withValue((CycFort)ToFort(p));
                }
                foreach (object v in pTypeValues)
                {
                    sbyte bv = (sbyte)v;
                    if (bv >= b)
                    {
                        withValue((CycFort)ToFort(v));
                        b -= bv;
                    }
                    if (b == 0) return;
                }
                return;
            }
            if (p is UInt16)
            {
                ushort b = (UInt16)p;
                if (b == 0)
                {
                    withValue((CycFort)ToFort(p));
                }
                foreach (object v in pTypeValues)
                {
                    ushort bv = (ushort)v;
                    if (bv >= b)
                    {
                        withValue((CycFort)ToFort(v));
                        b -= bv;
                    }
                    if (b == 0) return;
                }
                return;
            }
            if (p is Int16)
            {
                short b = (Int16)p;
                if (b == 0)
                {
                    withValue((CycFort)ToFort(p));
                }
                foreach (object v in pTypeValues)
                {
                    short bv = (short)v;
                    if (bv >= b)
                    {
                        withValue((CycFort)ToFort(v));
                        b -= bv;
                    }
                    if (b == 0) return;
                }
                return;
            }
            if (p is UInt32)
            {
                uint b = (UInt32)p;
                if (b == 0)
                {
                    withValue((CycFort)ToFort(p));
                }
                foreach (object v in pTypeValues)
                {
                    uint bv = (uint)v;
                    if (bv >= b)
                    {
                        withValue((CycFort)ToFort(v));
                        b -= bv;
                    }
                    if (b == 0) return;
                }
                return;
            }
            if (p is Int32)
            {
                int b = (Int32)p;
                if (b == 0)
                {
                    withValue((CycFort)ToFort(p));
                }
                foreach (object v in pTypeValues)
                {
                    int bv = (int)v;
                    if (bv >= b)
                    {
                        withValue((CycFort)ToFort(v));
                        b -= bv;
                    }
                    if (b == 0) return;
                }
                return;
            }
            if (p is UInt64)
            {
                ulong b = (UInt64)p;
                if (b == 0)
                {
                    withValue((CycFort)ToFort(p));
                }
                foreach (object v in pTypeValues)
                {
                    ulong bv = (ulong)v;
                    if (bv >= b)
                    {
                        withValue((CycFort)ToFort(v));
                        b -= bv;
                    }
                    if (b == 0) return;
                }
                return;
            }
            if (p is Int64)
            {
                long b = (Int64)p;
                if (b == 0)
                {
                    withValue((CycFort)ToFort(p));
                }
                foreach (object v in pTypeValues)
                {
                    long bv = (long)v;
                    if (bv >= b)
                    {
                        withValue((CycFort)ToFort(v));
                        b -= bv;
                    }
                    if (b == 0) return;
                }
                return;
            }
            string s = p.ToString();
            foreach (var unfold in Unfold(p))
            {
                withValue((CycFort)ToFort(unfold));
                return;
            }

            if (p is IConvertible)
            {
                withValue((CycFort)ToFort(p));
                return;
            }

            if (p is Enum)
            {
                withValue((CycFort)ToFort(p));
                return;
            }
            withValue((CycFort)ToFort(p));
        }


        private void assertEventData(CycFort constant, IEnumerable<NamedParam> args)
        {
            CycFort fort = null;
            foreach (NamedParam o in args)
            {
                 fort = assertEventData(constant, ToPropName(o.Key.ToString()), o.Value, newHashSet(o.info));                
            }
          //  return fort;
        }

        private CycFort assertEventData(CycFort constant, NamedParam o)
        {
            return assertEventData(constant, ToPropName(o.Key.ToString()), o.Value, newHashSet(o.info));
        }

        private CycFort assertEventData(CycFort constant, string name, object fort, HashSet<MemberInfo> info)
        {
            CheckConstantName(name);
            CycFort prop = createSimProperty(name);
            if (fort != null)
            {
                Type t = fort.GetType();
                if (t.IsEnum)
                {

                    ForEachEnumValue(delegate(CycFort f)
                                         {
                                             defaultAssert(prop, constant, f);
                                         }, fort);
                    docPredicate(prop, info);
                    return prop;
                }


                if (IsMultiValued(fort))
                {
                    IEnumerable mv = GetMultiValues(fort);
                    foreach (var e in mv)
                    {
                        assertEventData(constant, name, e, info);
                    }
                    docPredicate(prop, info);
                    return prop;
                }
                if (IsCycEntity(t))
                {
                    defaultAssert(prop, constant, ToFort(fort));
                    docPredicate(prop, info);
                    return prop;
                }
                var infomap = WorldObjects.GetMemberValues(name + "-", fort);
                docPredicate(prop, info);
                if (infomap.Count < 2)
                {
                    Trace();
                    ///if (t.IsValueType)
                    {
                        defaultAssert(prop, constant, ToFort(fort));
                        docPredicate(prop, info);
                        return prop;
                    }
                }
                foreach (NamedParam o in infomap)
                {
                    HashSet<MemberInfo> mis = new HashSet<MemberInfo>(info);
                    String propname = ToPropName(o.Key.ToString());
                    MemberInfo oinfo = o.info;
                    if (oinfo != null) mis.Add(oinfo);
                    CycFort sub = assertEventData(constant, propname, o.Value, mis);
                    assertGaf(C("genlPreds"), sub, prop);
                }
                return prop;
            }
            return prop;

        }

        private void defaultAssert(CycFort fort, CycFort constant, object o)
        {
            if (o is String || (o is CycNart && ((CycNart)o).getFunctor() != C("TheList")))
            {
                CycListAssert(CycList.makeCycList(fort, constant, o));
                return;
            }
            if (o is CycVariable)
            {
                return;
            }
            if (o is float || o is java.lang.Float || o is double || o is java.lang.Double)
            {
                if (o.ToString()!="0.0" && o.ToString()!="1.0")
                {
                    CycListAssert(CycList.makeCycList(fort, constant, o));
                    return;                    
                }
            }
            lock (DefaultNotNeededInAssert)
            {

                object temp;
                if (DefaultNotNeededInAssert.TryGetValue(fort, out temp))
                {
                    if (temp.Equals(o))
                    {
                        return;
                    }
                    else
                    {
                        CycListAssert(CycList.makeCycList(fort, constant, o));
                    }
                    return;
                }
                DefaultNotNeededInAssert.Add(fort,o);
                CycListAssert(CycList.makeCycList(fort, C("TheDefaultSimInstance"), o));
                return;
            }
        }

        Dictionary<object,object> DefaultNotNeededInAssert = new Dictionary<object, object>();

        private void docPredicate(CycFort prop, HashSet<MemberInfo> info)
        {
            if (info == null) return;
            HashSet<MemberInfo> mi;
            lock (PropDoc)
            {
                if (!PropDoc.TryGetValue(prop, out mi))
                {
                    PropDoc[prop] = mi = new HashSet<MemberInfo>(info);
                    foreach (MemberInfo set in info)
                    {
                        docIt(set, prop);
                    }
                }
                else
                {
                    foreach (MemberInfo set in info)
                    {
                        if (mi.Add(set)) docIt(set, prop);
                    }
                }
            }
        }

        private void docIt(MemberInfo info, CycFort prop)
        {
            var doc = GetDocString(info);
            if (!string.IsNullOrEmpty(doc))
            {
                Debug("Docing " + info + " " + prop + " " + doc);
                assertGaf(C("comment"), prop, doc);
            }
        }


        private IEnumerable GetMultiValues(object fort)
        {
            return fort as IEnumerable;
        }

        private bool IsMultiValued(object fort)
        {
            if (fort is IList) return true;
            // if (fort is Array) return true;
            return false;
        }

        private CycFort createSimProperty(string p)
        {
            return createIndividual(p, "sim #$BinaryPredicate", "SimVocabMt", "SimProperty");
        }


        public CycFort createIndividualFn(string typename, string name, string comment, string simvocabmt, string simobjecttype)
        {
            bool newlyCreated;
            CycFort fn = createIndividual(typename + "Fn", comment,
                                          simvocabmt, "UnaryFunction", out newlyCreated);
            if (newlyCreated)
            {
                assertGaf(C("isa"), fn, C("ReifiableFunction"));
                assertGaf(C("resultIsa"), fn, C(simobjecttype));
            }
            CycFort indv;
            bool b = typename.StartsWith("SimEvent");
            if (b) // right now we dont intern Events
            {
                indv = new CycNart(CycList.list(fn, name));
            }
            else
                lock (cycTerms)
                {
                    {
                        string nv = name + "-" + typename;
                        if (cycTerms.TryGetValue(nv, out indv)) return indv;
                        indv = new CycNart(CycList.list(fn, name));
                        cycTerms[nv] = indv;
                        assertGaf(C("comment"), indv, comment);
                    }
                }
            return indv;
        }

        public CycFort createIndividualColFn(string typename, string name, string comment, string simvocabmt, string simobjecttype)
        {
            bool newlyCreated;
            CycFort fn = createIndividual(typename + "Fn", comment,
                                          simvocabmt, "UnaryFunction", out newlyCreated);
            if (newlyCreated)
            {
                assertGaf(C("isa"), fn, C("ReifiableFunction"));
                assertGaf(C("resultGenl"), fn, C(simobjecttype));
            }
            CycFort indv;
            bool b = typename.StartsWith("SimEvent");
            if (b) // right now we dont intern Events
            {
                indv = new CycNart(CycList.list(fn, name));
            }
            else
                lock (cycTerms)
                {
                    {
                        string nv = name + "-" + typename;
                        if (cycTerms.TryGetValue(nv, out indv)) return indv;
                        indv = new CycNart(CycList.list(fn, name));
                        cycTerms[nv] = indv;
                    }
                    assertGaf(C("comment"), indv, comment);
                }
            return indv;
        }

        private bool IndividualExists(CycFort indv)
        {
            return cycAccess.converseInt("(length (term-assertions '" + indv.cyclifyWithEscapeChars() + "))") > 1;
        }


        public object[] ToForts(object[] parameters)
        {
            object[] forts = new object[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                forts[i] = ToFort(parameters[i]);
            }
            return forts;
        }

        public object ToFort(object parameter)
        {
            if (parameter == null)
            {
                return CYC_NULL;
            }
            Type t = parameter.GetType();
            var conv = GetConverter(t);
            if (conv != null)
            {
                try
                {
                    object obj = conv.Invoke(parameter);
                    return obj;
                }
                catch (Exception e)
                {
                    Exception(e);
                }
            }
            if (t.IsArray)
            {
                Array ar = (Array)parameter;
                return FindOrCreateCycArray(ar, t);
            }
            if (t.IsEnum)
            {
                VisitEnumType(t);
                return C(string.Format("{0}-{1}", t.Name, parameter.ToString()));
            }
            if (t.IsValueType)
            {
                try
                {
                    return C(string.Format("{0}-{1}", t.Name, parameter.ToString()));
                }
                catch (Exception e)
                {
                    Exception(e);
                }
            }
            Debug("Cant convert " + t);
            return CYC_NULL;
        }

        //public CycObject FindOrCreateCycFort(Primitive.SculptData b)
        //{
        //    return ToFort(b.SculptTexture);
        //}

        //public CycObject FindOrCreateCycFort(Avatar.Statistics b)
        //{
        //    return ToStructMembers(b);
        //}

        private CycObject ToStructMembers(ValueType statistics)
        {
            CycList lst = new CycList(C("TheList"));
            Type t = statistics.GetType();
            int found = 0;
            foreach (var fi in t.GetFields())
            {
                if (fi.IsStatic) continue;
                lst.add(CreateCycNart("TheList", fi.Name, fi.GetValue(statistics)));
                found++;
            }
            if (found == 0) throw new NullReferenceException("" + statistics);
            return new CycNart(lst);
        }

        private CycFort FindOrCreateCycArray(IEnumerable args, Type t)
        {
            return CreateCycNartEnumerable("TheList", args);
        }

        private CycFort CreateCycNartEnumerable(String header, IEnumerable args)
        {
            CycList lst = new CycList(C(header));
            IEnumerator enumer = args.GetEnumerator();
            while (enumer.MoveNext())
            {
                lst.add(ToFort(enumer.Current));
            }
            return new CycNart(lst);
        }

        private CycFort CreateCycNart(String header, params object[] args)
        {
            return CreateCycNartEnumerable(header, args);
        }

        public CycFort FindOrCreateCycFort(IDictionary b)
        {
            CycList lst = new CycList(C("TheList"));
            lock (b)
            {
                foreach (var key in b.Keys)
                {
                    lst.add(CreateCycNart("TheList", ToFort(key), ToFort(b[key])));
                }
            }
            return new CycNart(lst);
        }


        //public CycFort FindOrCreateCycFort(Vector2 b)
        //{
        //    return new CycNart(makeCycList(C("TheList"),
        //        new java.lang.Float(b.X),
        //        new java.lang.Float(b.Y)));
        //}

        //public CycFort FindOrCreateCycFort(Vector3 b)
        //{
        //    return new CycNart(makeCycList(C("TheList"),
        //        new java.lang.Float(b.X),
        //        new java.lang.Float(b.Y),
        //        new java.lang.Float(b.Z)));
        //}

        //public CycFort FindOrCreateCycFort(Vector4 b)
        //{
        //    return new CycNart(makeCycList(C("TheList"),
        //        new java.lang.Float(b.W),
        //        new java.lang.Float(b.X),
        //        new java.lang.Float(b.Y),
        //        new java.lang.Float(b.Z)));
        //}

        public CycFort FindOrCreateCycFort(Color4 b)
        {
            return new CycNart(makeCycList(C("TheList"),
                new java.lang.Float(b.R),
                new java.lang.Float(b.G),
                new java.lang.Float(b.B),
                new java.lang.Float(b.A)));
        }

        public object FindOrCreateCycFort(GridClient client)
        {
            return FindOrCreateCycFort(client.Self.AgentID);
        }

        public object FindOrCreateCycFort(cogbot.BotClient client)
        {
            return FindOrCreateCycFort(client.WorldSystem);
        }

        public object FindOrCreateCycFort(WorldObjects client)
        {
            return FindOrCreateCycFort(client.TheSimAvatar);
        }


        public CycObject FindOrCreateCycFort(NullType region)
        {
            return CYC_NULL;
        }

        // TODO confirm this will work
        public object FindOrCreateCycFort(ulong b)
        {
            return new java.math.BigInteger("" + b);
        }
        public object FindOrCreateCycFort(Int16 b)
        {
            return new java.lang.Integer(b);
        }

        public object FindOrCreateCycFort(Single b)
        {
            return new java.lang.Float(b);
        }
        public object FindOrCreateCycFort(double b)
        {
            return new java.lang.Double(b);
        }
        public object FindOrCreateCycFort(int b)
        {
            return new java.lang.Integer(b);
        }
        public object FindOrCreateCycFort(long b)
        {
            return new java.lang.Long(b);
        }
        public object FindOrCreateCycFort(byte b)
        {
            return new java.lang.Integer(b);
        }
        public CycFort FindOrCreateCycFort(bool b)
        {
            return b ? C("True") : C("False");
        }

        public bool IsCycEntity(Type ptype)
        {
            if (converters.ContainsKey(ptype)) return true;
            foreach (var t in converters.Keys)
            {
                if (t.IsAssignableFrom(ptype)) return true;
            }
            return (GetConverter(ptype) != null);
        }

        static public readonly Dictionary<Type, Converter<object, object>> converters = new Dictionary<Type, Converter<object, object>>();
        static object PassThruConversion(object inp)
        {
            return inp;
        }

        void LoadConverters()
        {
            lock (converters)
            {
                if (converters.Count > 0) return;
                converters[typeof(string)] = PassThruConversion;
                converters[typeof(CycObject)] = PassThruConversion;
                foreach (var fort in typeof(SimCyclifier).GetMethods())
                {
                    if (fort.Name != "FindOrCreateCycFort") continue;
                    ParameterInfo[] ps = fort.GetParameters();
                    if (ps.Length != 1) continue;
                    Type ptype = ps[0].ParameterType;
                    if (converters.ContainsKey(ptype)) continue;
                    converters[ptype] = MethodInfoToConverter(ptype, this, fort);
                    Debug("Created conversion " + ptype.FullName);
                }
                //converters[typeof(IConvertible)] = PassThruConversion;
            }
        }

        static Converter<object, object> MethodInfoToConverter(Type sane, SimCyclifier cyclifier, MethodInfo info)
        {
            Converter<object, object> ret = delegate(object inp)
                                                {
                                                    if (inp == null) return CYC_NULL;
                                                    if (!sane.IsInstanceOfType(inp))
                                                    {
                                                        throw new ArgumentException("" + inp + " is not " + sane);
                                                    }
                                                    try
                                                    {
                                                        return info.Invoke(cyclifier, new[] { inp });
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        string errr = "" + e;
                                                        Exception(e);
                                                        return errr;
                                                    }
                                                };
            return ret;
        }

        static Converter<object, object> MethodInfoToConverterFunSyntax(Type sane, SimCyclifier cyclifier, MethodInfo info)
        {
            return inp =>
                       {
                           if (inp == null) return CYC_NULL;
                           if (!sane.IsInstanceOfType(inp))
                               throw new ArgumentException("" + inp + " is not " + sane);
                           return info.Invoke(cyclifier, new[] { inp });
                       };
        }

        public static int nullCount = 0;
        static public bool UseQueue = true;
        static readonly Dictionary<CycFort, HashSet<MemberInfo>> PropDoc = new Dictionary<CycFort, HashSet<MemberInfo>>();

        public static CycObject CYC_NULL
        {
            get { return new CycVariable("NULL-" + (nullCount++)); }
        }

        public Converter<object, object> GetConverter(Type type)
        {
            lock (converters)
            {
                LoadConverters();
                //one day will be Converter<object, CycFort> converter;
                Converter<object, object> converter;
                if (converters.TryGetValue(type, out converter)) return converter;
                foreach (var v in converters)
                {
                    if (type.IsSubclassOf(v.Key))
                    {
                        return converters[type] = v.Value;
                    }
                }
                foreach (var v in converters)
                {
                    if (!v.Key.IsValueType && v.Key.IsAssignableFrom(type))
                    {
                        return converters[type] = v.Value;
                    }
                }
            }
            return null;
        }

        public CycObject FindOrCreateCycFort(Simulator sim)
        {
            return FindOrCreateCycFort(SimRegion.GetRegion(sim));
        }
        public CycFort FindOrCreateCycFort(Vector3 simObjectEvent, Object regionName)
        {
            CycList makeCycList1 = makeCycList(C("PointInRegionFn"),
                                               regionName,
                                               FindOrCreateCycFort(simObjectEvent.X),
                                               FindOrCreateCycFort(simObjectEvent.Y),
                                               FindOrCreateCycFort(simObjectEvent.Z));
            return new CycNart(makeCycList1);
        }

        public static CycList makeCycList(params object[] argsObject)
        {
            CycList list = new CycList(argsObject.Length);
            foreach (object e in argsObject)
            {
                list.add(e);
            }
            return list;
        }

        public CycFort C(string p)
        {
            lock (simFort)
            {
                if (simFort.ContainsKey(p)) return simFort[p];
                try
                {
                    p = CheckConstantName(p);
                    CycConstant c = cycAccess.findOrCreate(p);
                    return simFort[p] = c;
                }
                catch (Exception e)
                {
                    Exception(e);
                    throw e;
                    //return CYC_NULL;
                }
            }
        }

        private String CheckConstantName(string s)
        {
            if (s.EndsWith("-6") || s.EndsWith("-4") || s.Contains("offsetU"))
            {
                Trace();
            }
            if (!cycAccess.isValidConstantName(s))
            {
                Trace();
            }
            CycConstant colided = cycAccess.constantNameCaseCollision(s);
            if (colided != null)
            {
                Trace();
                return CheckConstantName(s + "-Pred");
            }
            return s;
        }

        public object FindOrCreateCycFort(UUID region)
        {
            if (region == UUID.Zero) return CYC_NULL;
            object o = WorldObjects.GridMaster.GetObject(region);
            if (!(o is UUID)) return ToFort(o);
            return "" + region;
            //return createIndividualFn("SimRegion", region.RegionName, vocabMt.ToString(), "SimRegion " + region, "GeographicalPlace-3D");
        }

        public CycObject FindOrCreateCycFort(SimRegion region)
        {
            if (region == SimRegion.UNKNOWN) return CYC_NULL;
            CycFort obj;
            lock (simFort)
            {
                if (simFort.TryGetValue(region, out obj)) return obj;

                    obj = createIndividualFn("SimRegion", region.RegionName, "SimRegion " + region, vocabMt.ToString(),
                                             "GeographicalPlace-3D");
            }
            if (region.GridInfo.MapImageID != UUID.Zero)
            {
                simFort[region] =
                    obj;
                assertEventData(obj, WorldObjects.GetMemberValues("GridInfo-", region.GridInfo));
            }
            return obj;
        }

        public CycObject FindOrCreateCycFort(SimPathStore region)
        {
            if (region.RegionName == "<0,0>") return CYC_NULL;
            return createIndividualFn("SimRegion", region.RegionName, "SimRegion " + region, vocabMt.ToString(), "GeographicalPlace-3D");
        }

        public CycObject FindOrCreateCycFort(SimHeading b)
        {
            if (SimHeading.UNKNOWN == b) return CYC_NULL;
            if (b.IsRegionAttached)
            {
                SimPathStore r = b.PathStore;
                CycObject findOrCreateCycFort = FindOrCreateCycFort(r);
                return FindOrCreateCycFort(b.SimPosition, r.RegionName);
            }
            else
            {
                object arg1 = ToFort(b.GetRoot());
                Vector3 offset = b.GetOffset();
                //return "PointRelativeToFn";
                return new CycNart(makeCycList(C("PointRelativeToFn"), arg1,
                                               FindOrCreateCycFort(offset.X),
                                               FindOrCreateCycFort(offset.Y),
                                               FindOrCreateCycFort(offset.Z)));
            }
        }

        public CycFort FindOrCreateCycFort(Vector3d simObjectEvent)
        {
            SimRegion r = SimRegion.GetRegion(simObjectEvent);
            CycObject findOrCreateCycFort = FindOrCreateCycFort(r);
            return FindOrCreateCycFort(SimRegion.GlobalToLocal(simObjectEvent), r.RegionName);
        }

        public object FindOrCreateCycFort(uint b)
        {
            return new java.lang.Long(b);
        }

        public object FindOrCreateCycFort(ushort b)
        {
            return new java.lang.Integer(b);
        }

        public object FindOrCreateCycFort(char b)
        {
            return new java.lang.Character(b);
        }

        public CycFort FindOrCreateCycFort(BotSocialAction botSocialAction)
        {
            throw new NotImplementedException();
        }

        public CycFort FindOrCreateCycFort(BotObjectAction botObjectAction)
        {
            throw new NotImplementedException();
        }

        //public CycFort FindOrCreateCycFort(Primitive.TextureEntry te)
        //{
        //    return createIndividualFn("SimTextEntry", te);
        //}
        //public CycFort FindOrCreateCycFort(Avatar.AvatarProperties te)
        //{
        //    return createIndividualFn("SimAvatarProperties", te);
        //}

        private CycFort createIndividualFn(string p, object te)
        {
            lock (cycTerms)
            {
                CycFort constant;
                if (cycTerms.TryGetValue(te, out constant)) return constant;
                //   object[] forts = ToForts(simObj.Parameters);
                return cycTerms[te] = createIndividualFn(p, te.ToString(), te.ToString(), "SimVocabMt", p);
            }
        }

        public CycFort FindOrCreateCycFort(SimTypeUsage simTypeUsage)
        {
            throw new NotImplementedException();
        }

        public CycFort FindOrCreateCycFort(SimObjectUsage simObjectUsage)
        {
            throw new NotImplementedException();
        }

        public CycFort FindOrCreateCycFort(MoveToLocation moveToLocation)
        {
            throw new NotImplementedException();
        }

        public void World_OnSimObject(SimObject obj)
        {
            //  FindOrCreateCycFort(obj);
        }

        static Dictionary<Assembly, XElement> AssmblyXDoics = new Dictionary<Assembly, XElement>();
        public static XElement GetXmlDocMembers(Assembly typeAssembly)
        {
            XElement ele;
            lock (AssmblyXDoics)
                if (!AssmblyXDoics.TryGetValue(typeAssembly, out ele))
                {
                    try
                    {
                        AssmblyXDoics[typeAssembly] = ele = GetXmlDocMembers0(typeAssembly);
                    }
                    catch (Exception e)
                    {
                        Debug("Cannot doc " + typeAssembly + " " + e);
                        AssmblyXDoics[typeAssembly] = ele = null;
                    }
                }
            return ele;
        }

        public static XElement GetXmlDocMembers0(Assembly typeAssembly)
        {
            var file = GetXmlDocFile(typeAssembly);
            XDocument f = XDocument.Load(file.FullName);
            if (f.Root == null) return null;
            return f.Root.Element("members");
        }

        public static XElement GetXmlDocMembers(Type type)
        {
            return GetXmlDocMembers(type.Assembly);
        }


        private static FileInfo GetXmlDocFile(Assembly assembly)
        {
            string assemblyDirPath = Path.GetDirectoryName(assembly.Location);
            string fileName = String.Format("{0}.xml", Path.GetFileNameWithoutExtension(assembly.Location).ToLower());
            foreach (string file in Directory.GetFiles(assemblyDirPath))
            {
                if (file.ToLower().Equals(fileName))
                {
                    return new FileInfo(file);
                }
            }
            Debug("Assebly Doc File not found {0}", fileName);
            return new FileInfo(fileName);
        }

        static string GetMemberId(MemberInfo member)
        {
            char memberKindPrefix = GetMemberPrefix(member);
            string memberName = GetMemberFullName(member);
            return memberKindPrefix + ":" + memberName;
        }

        static char GetMemberPrefix(MemberInfo member)
        {
            return member.GetType().Name
              .Replace("Runtime", "")[0];
        }

        static string GetMemberFullName(MemberInfo member)
        {
            string memberScope = "";
            if (member.DeclaringType != null)
                memberScope = GetMemberFullName(member.DeclaringType);
            else if (member is Type)
                memberScope = ((Type)member).Namespace;

            return memberScope + "." + member.Name;
        }


        private string GetDocString(MemberInfo memberInfo)
        {
            var docMembers = GetXmlDocMembers(memberInfo.DeclaringType.Assembly);
            return GetDocString(docMembers, memberInfo);
        }

        public static string GetDocString(XElement docMembers, MemberInfo info)
        {

            string memberId = GetMemberId(info);
            foreach (XElement e in docMembers.Elements("member"))
            {
                var anme = e.Attribute("name");
                if (anme != null)
                    if (anme.Value.ToLower().Substring(1) == memberId.ToLower().Substring(1))
                    {
                        var fn = e.FirstNode as XElement;
                        if (fn != null)
                        {
                            var vv = fn.Value;
                            if (!string.IsNullOrEmpty(vv))
                            {
                                return vv.Trim();
                            }

                        }

                    }
            }
            return null;
        }


        static void Debug(string s, params object[] args)
        {
            if (args == null || args.Length == 0)
            {
                args = new object[] { s };
                s = "{0}";
            }
            Console.WriteLine(string.Format("[SimCyclifier] {0}", s), args);
        }

        static void Exception(Exception e)
        {
            Debug("" + e);
            Trace();
        }

        private static void Trace()
        {

        }

    }
}
