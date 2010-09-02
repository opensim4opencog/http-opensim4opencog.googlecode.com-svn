using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Xml;
using System.IO;
using System.Xml.Serialization;
using Lucene.Net.Store;
using MushDLR223.ScriptEngines;
using MushDLR223.Utilities;
using MushDLR223.Virtualization;
using RTParser;
using RTParser.Database;
using RTParser.Normalize;
using RTParser.Utils;

namespace RTParser.Variables
{
    public delegate ISettingsDictionary ParentProvider();
    public interface ISettingsDictionary
    {
        /// <summary>
        /// Adds a bespoke setting to the Settings class (accessed via the grabSettings(string name)
        /// method.
        /// </summary>
        /// <param name="name">The name of the new setting</param>
        /// <param name="value">The value associated with this setting</param>
        bool addSetting(string name, Unifiable value);
        /// <summary>
        /// Removes the named setting from this class
        /// </summary>
        /// <param name="name">The name of the setting to remove</param>
        bool removeSetting(string name);
        /// <summary>
        /// Updates the named setting with a new value whilst retaining the position in the
        /// dictionary
        /// </summary>
        /// <param name="name">the name of the setting</param>
        /// <param name="value">the new value</param>
        bool updateSetting(string name, Unifiable value);
        /// <summary>
        /// Returns the value of a setting given the name of the setting
        /// </summary>
        /// <param name="name">the name of the setting whose value we're interested in</param>
        /// <returns>the value of the setting</returns>
        Unifiable grabSetting(string name);
        /// <summary>
        /// Checks to see if a setting of a particular name exists
        /// </summary>
        /// <param name="name">The setting name to check</param>
        /// <returns>Existential truth value</returns>
        bool containsLocalCalled(string name);
        bool containsSettingCalled(string name);

        string NameSpace { get; }
        bool IsTraced { get; set; }

        IEnumerable<string> SettingNames(int depth);
    }

    /// <summary>
    /// A bespoke Dictionary<,> for loading, adding, checking, removing and extracting
    /// settings.
    /// </summary>
    public class SettingsDictionary : ISettingsDictionary
    {
        #region Attributes

        /// <summary>
        /// Holds a dictionary of settings
        /// </summary>
        readonly public Dictionary<string, Unifiable> settingsHash = new Dictionary<string, Unifiable>();

        /// <summary>
        /// Contains an ordered collection of all the keys (unfortunately Dictionary<,>s are
        /// not ordered)
        /// </summary>
        readonly private List<string> orderedKeys = new List<string>();

        // prechecks and uses if settings exist
        private List<ParentProvider> _overides = new List<ParentProvider>();
        // fallbacks (therefore inherits)
        private List<ParentProvider> _fallbacks = new List<ParentProvider>();
        // fallbacks (therefore inherits)
        private List<ParentProvider> _listeners = new List<ParentProvider>();
        // fallbacks (therefore inherits)
        private PrefixProvider prefixProvideer;

        /// <summary>
        /// The bot this dictionary is associated with (only for writting log)
        /// </summary>
        protected RTParser.RTPBot bot;

        private string theNameSpace;
        public bool TrimKeys = true;
        private string fromFile;

        /// <summary>
        /// The number of items in the dictionary
        /// </summary>
        public int Count
        {
            get
            {
                return this.orderedKeys.Count;
            }
        }

        public string NameSpace
        {
            get { return theNameSpace; }
            set { theNameSpace = value; }
        }

        public bool IsTraced { get; set; }

        public override string ToString()
        {
            return theNameSpace + "(" + Count + ") ";
        }
        public string ToDebugString()
        {
            return theNameSpace + "(" + Count + ") " + DictionaryAsXML.DocumentElement.InnerXml.Replace("<item name=", "\n<item name =");
        }

        /// <summary>
        /// An XML representation of the contents of this dictionary
        /// </summary>
        public XmlDocument DictionaryAsXML
        {
            get
            {
                XmlDocument result = new XmlDocument();
                XmlDeclaration dec = result.CreateXmlDeclaration("1.0", "UTF-8", "");
                result.AppendChild(dec);
                XmlNode root = result.CreateNode(XmlNodeType.Element, "root", "");
                XmlAttribute newAttr = result.CreateAttribute("name");
                lock (orderedKeys)
                {

                    newAttr.Value = NameSpace;
                    if (fromFile != null)
                    {
                        newAttr = result.CreateAttribute("fromfile");
                        newAttr.Value = fromFile;
                    }
                    result.AppendChild(root);

                    foreach (var normalizedName in ProvidersFrom(this._overides))
                    {
                        XmlNode item = result.CreateNode(XmlNodeType.Element, "override", "");
                        XmlAttribute name = result.CreateAttribute("name");
                        name.Value = normalizedName.NameSpace;
                        item.Attributes.Append(name);
                        root.AppendChild(item);
                    }
                    foreach (var normalizedName in ProvidersFrom(this._fallbacks))
                    {
                        XmlNode item = result.CreateNode(XmlNodeType.Element, "fallback", "");
                        XmlAttribute name = result.CreateAttribute("name");
                        name.Value = normalizedName.NameSpace;
                        item.Attributes.Append(name);
                        root.AppendChild(item);
                    }
                    foreach (var normalizedName in ProvidersFrom(this._listeners))
                    {
                        XmlNode item = result.CreateNode(XmlNodeType.Element, "synchon", "");
                        XmlAttribute name = result.CreateAttribute("name");
                        name.Value = normalizedName.NameSpace;
                        item.Attributes.Append(name);
                        root.AppendChild(item);
                    }
                    foreach (var normalizedName in this.prefixProvideer._prefixes)
                    {
                        XmlNode item = result.CreateNode(XmlNodeType.Element, "prefixes", "");
                        XmlAttribute name = result.CreateAttribute("name");
                        name.Value = normalizedName.Key;
                        XmlAttribute value = result.CreateAttribute("value");
                        value.Value = normalizedName.Value().NameSpace;
                        item.Attributes.Append(name);
                        item.Attributes.Append(value);
                        root.AppendChild(item);
                    }
                    foreach (var normalizedName in ProvidersFrom(this.SetReturnProviders))
                    {
                        XmlNode item = result.CreateNode(XmlNodeType.Element, "settingtypes", "");
                        XmlAttribute name = result.CreateAttribute("name");
                        name.Value = normalizedName.NameSpace;
                        item.Attributes.Append(name);
                        root.AppendChild(item);
                    }
                    foreach (var normalizedName in ProvidersFrom(this.GetSetFormatters))
                    {
                        XmlNode item = result.CreateNode(XmlNodeType.Element, "formatter", "");
                        XmlAttribute name = result.CreateAttribute("name");
                        name.Value = normalizedName.NameSpace;
                        item.Attributes.Append(name);
                        root.AppendChild(item);
                    }
                    foreach (var normalizedName in this.makedvars)
                    {
                        XmlNode item = result.CreateNode(XmlNodeType.Element, "maskedvar", "");
                        XmlAttribute name = result.CreateAttribute("name");
                        name.Value = normalizedName;
                        item.Attributes.Append(name);
                        root.AppendChild(item);
                    }
                    foreach (string n in this.orderedKeys)
                    {
                        XmlNode item = result.CreateNode(XmlNodeType.Element, "item", "");
                        XmlAttribute name = result.CreateAttribute("name");
                        name.Value = n.ToLower();
                        XmlAttribute value = result.CreateAttribute("value");
                        value.Value = this.settingsHash[TransformKey(n)];
                        item.Attributes.Append(name);
                        item.Attributes.Append(value);
                        root.AppendChild(item);
                    }

                }
                return result;
            }
        }

        #endregion

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="bot">The bot for whom this is a settings dictionary</param>
        public SettingsDictionary(String name, RTParser.RTPBot bot, ParentProvider parent)
        {
            theNameSpace = name;
            IsSubsts = name.Contains("subst");
            TrimKeys = !name.Contains("subst");
            this.bot = bot;
            if (!IsSubsts)
            {
                if (bot.SetPredicateReturn != null) this.InsertSettingReturnTypes(bot.GetSetPredicateReturn);
                if (bot.SetRelationFormat != null) this.InsertGetSetFormatter(bot.GetSetRelationFormat);
            }
            IsTraced = true;
            if (parent != null) _fallbacks.Add(parent);
            prefixProvideer = new PrefixProvider();
            prefixProvideer.NameSpace = name + ".evalprefix";
            ParentProvider pp = () => prefixProvideer;
            _fallbacks.Add(pp);
            _listeners.Add(pp);
            bot.RegisterDictionary(name, this);
        }

        #region Methods
        /// <summary>
        /// Loads bespoke settings into the class from the file referenced in pathToSettings.
        /// 
        /// The XML should have an XML declaration like this:
        /// 
        /// <?xml version="1.0" encoding="utf-8" ?> 
        /// 
        /// followed by a <root> tag with child nodes of the form:
        /// 
        /// <item name="name" value="value"/>
        /// </summary>
        /// <param name="pathToSettings">The file containing the settings</param>
        public void loadSettings(string pathToSettings, Request request)
        {
            OutputDelegate writeToLog = request.writeToLog;
            if (pathToSettings == null) return;
            lock (orderedKeys)
            {
                if (pathToSettings.Length > 0)
                {
                    if (HostSystem.FileExists(pathToSettings))
                    {
                        XmlDocumentLineInfo xmlDoc = new XmlDocumentLineInfo(pathToSettings, true);
                        try
                        {
                            var stream = HostSystem.GetStream(pathToSettings);
                            xmlDoc.Load(stream);
                            HostSystem.Close(stream);
                            this.loadSettings(xmlDoc, request);
                        }
                        catch (Exception e)
                        {
                            writeToLog("ERROR loadSettings '{1}'\n {0} ", e, pathToSettings);
                        }
                    }
                    else
                    {
                        writeToLog("No settings found in: " + pathToSettings);
                        return;
                    }
                    writeToLog("Loaded Settings found in: " + pathToSettings);
                    if (fromFile == null) fromFile = pathToSettings;
                }
                else
                {
                    throw new FileNotFoundException(pathToSettings);
                }
            }
        }

        static public void loadSettings(ISettingsDictionary dict0, string pathToSettings,
            bool overwriteExisting, bool onlyIfUnknown, Request request)
        {
            if (pathToSettings == null) return;
            SettingsDictionary dict = ToSettingsDictionary(dict0);
            OutputDelegate writeToLog = dict.writeToLog;
            // or else
// ReSharper disable ConstantNullColescingCondition
            writeToLog = writeToLog ?? request.writeToLog;
// ReSharper restore ConstantNullColescingCondition
            lock (dict.orderedKeys)
            {
                if (pathToSettings.Length > 0)
                {
                    if (HostSystem.DirExists(pathToSettings))
                    {
                        foreach (string s in HostSystem.GetFiles(pathToSettings, "*.xml"))
                        {
                            loadSettings(dict, s, overwriteExisting, onlyIfUnknown, request);
                        }
                        return;
                    }
                    if (!HostSystem.FileExists(pathToSettings))
                    {
                        writeToLog("ERROR No settings found in: " + pathToSettings);
                        //throw new FileNotFoundException(pathToSettings);
                        return;
                    }

                    try
                    {
                        XmlDocumentLineInfo xmlDoc = new XmlDocumentLineInfo(pathToSettings, true);
                        var stream = HostSystem.GetStream(pathToSettings);
                        xmlDoc.Load(stream);
                        HostSystem.Close(stream);
                        loadSettingNode(dict, xmlDoc, overwriteExisting, onlyIfUnknown, request);
                        writeToLog("Loaded Settings found in: " + pathToSettings);
                        if (dict.fromFile == null) dict.fromFile = pathToSettings;
                    }
                    catch (Exception e)
                    {
                        writeToLog("ERROR loadSettings: " + pathToSettings + "\n" + e);
                    }
                }
                else
                {
                    throw new FileNotFoundException("settings for " + dict);
                }
            }
        }

        private void writeToLog(string message, params object[] args)
        {
            string tol = message.Trim().ToLower();
            if (tol.StartsWith("error")) message = "-DICTRACE: " + message;
            if (!tol.Contains("dictlog")) message = "DICTLOG: " + message;
            if (bot != null) bot.writeToLog(message, args); else RTPBot.writeDebugLine(message, args);
        }

        /// <summary>
        /// Loads bespoke settings to the class from the XML supplied in the args.
        /// 
        /// The XML should have an XML declaration like this:
        /// 
        /// <?xml version="1.0" encoding="utf-8" ?> 
        /// 
        /// followed by a <root> tag with child nodes of the form:
        /// 
        /// <item name="name" value="value"/>
        /// </summary>
        /// <param name="settingsAsXML">The settings as an XML document</param>
        public void loadSettings(XmlDocument settingsAsXML, Request request)
        {
            lock (orderedKeys)
            {
                if (settingsAsXML.DocumentElement == null)
                {
                    writeToLog("ERROR no doc element in " + settingsAsXML);
                }
                loadSettingNode(this, settingsAsXML.Attributes, true, false, request);
                loadSettingNode(this, settingsAsXML.DocumentElement, true, false, request);
            }
        }

        static public void loadSettingNode(ISettingsDictionary dict, IEnumerable Attributes, bool overwriteExisting, bool onlyIfUnknown, Request request)
        {
            if (Attributes == null) return;
            foreach (object o in Attributes)
            {
                if (o is XmlNode)
                {
                    XmlNode n = (XmlNode)o;
                    loadSettingNode(dict, n, overwriteExisting, onlyIfUnknown, request);
                }
            }
        }

        private static void loadNameValueSetting(ISettingsDictionary dict, string name, string value, string updateOrAddOrDefualt, XmlNode myNode, bool overwriteExisting, bool onlyIfUnknown, Request request)
        {
            updateOrAddOrDefualt = updateOrAddOrDefualt.ToLower().Trim();

            overwriteExisting =
                Boolean.Parse(RTPBot.GetAttribValue(myNode, "overwriteExisting", "" + overwriteExisting));

            onlyIfUnknown =
                Boolean.Parse(RTPBot.GetAttribValue(myNode, "onlyIfKnown", "" + onlyIfUnknown));

            string returnNameWhenSet =
                RTPBot.GetAttribValue(myNode, "return-name-when-set", null);
            if (returnNameWhenSet != null)
            {
                returnNameWhenSet = returnNameWhenSet.Trim();
                if (returnNameWhenSet.Length == 0) returnNameWhenSet = "false";
                else if (Unifiable.IsNullOrEmpty(returnNameWhenSet)) returnNameWhenSet = "value";
                else if (Unifiable.IsFalseOrNo(returnNameWhenSet)) returnNameWhenSet = "value";
                else if (Unifiable.IsTrueOrYes(returnNameWhenSet)) returnNameWhenSet = "name";
            }
            returnNameWhenSet =
                RTPBot.GetAttribValue(myNode, "set-return", returnNameWhenSet);
            if (returnNameWhenSet != null)
            {
                ToSettingsDictionary(dict).addSetReturn(name, returnNameWhenSet);
            }

            SettingsDictionary dictionary = ToSettingsDictionary(dict);
            string englishFormatter =
                RTPBot.GetAttribValue(myNode, "formatter,pred-format,genformat,format,printf,lucene,english", null);
            if (englishFormatter != null)
            {
                string formatter = englishFormatter;
                formatter = " " + formatter + " ";
                int formatterQA = formatter.IndexOf(" | ");
                if (formatterQA != -1)
                {
                        // query mode
                        formatter = formatter.Substring(formatterQA + 2).Trim();
                        dictionary.addFormatter(name + ".format-assert", formatter);
                        // assert mode
                        formatter = formatter.Substring(0, formatterQA).Trim();
                        dictionary.addFormatter(name + ".query-assert", formatter);
                } else
                {
                    // both query/assert
                    dictionary.addFormatter(name + ".format-assert", englishFormatter);
                    dictionary.addFormatter(name + ".format-query", englishFormatter);
                }
            }
            englishFormatter = RTPBot.GetAttribValue(myNode, "assert,format-assert", null);
            if (englishFormatter != null)
            {
                dictionary.addFormatter(name + ".format-assert", englishFormatter);
            }
            englishFormatter = RTPBot.GetAttribValue(myNode, "query,format-query", null);
            if (englishFormatter != null)
            {
                dictionary.addFormatter(name + ".format-query", englishFormatter);
            }
            englishFormatter = RTPBot.GetAttribValue(myNode, "whword", null);
            if (englishFormatter != null)
            {
                dictionary.addFormatter(name + ".format-whword", englishFormatter);
            }

            bool dictcontainsLocalCalled = dict.containsLocalCalled(name);

            if (updateOrAddOrDefualt == "add")
            {
                if (!overwriteExisting)
                {
                    if (dictcontainsLocalCalled)
                    {
                        return;
                    }
                }
                if (onlyIfUnknown && dictcontainsLocalCalled)
                {
                    var old = dict.grabSetting(name);
                    if (!Unifiable.IsUnknown(old))
                    {
                        return;
                    }
                }
                bool wasTracing = dict.IsTraced;
                dict.addSetting(name, new StringUnifiable(value));
                dict.IsTraced = wasTracing;
            }
            else
            {
                bool inherited = !dictcontainsLocalCalled && dict.containsSettingCalled(name);
                // update only
                var old = dict.grabSetting(name);
                if (inherited && onlyIfUnknown)
                {
                    if (!Unifiable.IsUnknown(old))
                    {
                        return;
                    }
                }
                if (onlyIfUnknown && dictcontainsLocalCalled)
                {
                    if (!Unifiable.IsUnknown(old))
                    {
                        return;
                    }
                }
                bool wasTracing = dict.IsTraced;
                dict.updateSetting(name, new StringUnifiable(value));
                dict.IsTraced = wasTracing;
            }
        }

        static public void loadSettingNode(ISettingsDictionary dict, XmlNode myNode, bool overwriteExisting, bool onlyIfUnknown, Request request)
        {
            lock (dict)
            {
                loadSettingNode0(dict, myNode, overwriteExisting, onlyIfUnknown, request);
            }
        }
        static public void loadSettingNode0(ISettingsDictionary dict, XmlNode myNode, bool overwriteExisting, bool onlyIfUnknown, Request request)
        {

            if (myNode == null) return;
            if (myNode.NodeType == XmlNodeType.Comment) return;
            if (myNode.NodeType == XmlNodeType.Attribute)
            {
                // attribues should not overwrite existing? 
                loadNameValueSetting(dict, myNode.Name, myNode.Value, "add", myNode, overwriteExisting, onlyIfUnknown, request);
                return;
            }
            int atcount = 0;
            if (myNode.Attributes != null)
            {
                atcount = myNode.Attributes.Count;
                if (myNode.Attributes["xmlns"] != null) atcount = atcount - 1;
            }
            if (myNode.NodeType == XmlNodeType.XmlDeclaration)
            {
                loadSettingNode(dict, myNode.Attributes, false, onlyIfUnknown, request);
                loadSettingNode(dict, myNode.ChildNodes, overwriteExisting, onlyIfUnknown, request);
                return;
            }
            string lower = myNode.Name.ToLower();

            if (myNode.NodeType == XmlNodeType.Document || lower == "#document")
            {
                loadSettingNode(dict, myNode.Attributes, false, onlyIfUnknown, request);
                loadSettingNode(dict, myNode.ChildNodes, overwriteExisting, onlyIfUnknown, request);
                return;
            }
            if (lower == "substitutions")
            {
                //loadSettingNode(dict, myNode.Attributes, false, true, request);
                SettingsDictionary substDict = ToSettingsDictionary(dict);
                string substName = RTPBot.GetAttribValue(myNode, "name,dict,value", "input");
                var chdict = request.GetSubstitutions(substName, true);
                foreach (XmlNode n in myNode.ChildNodes)
                {
                    substName = n.Name.ToLower();
                    /// ProgramQ            ProgramD
                    if (substName != "substitution" && substName != "substitute" && !IsNameValueTag(substName))
                    {
                        chdict = request.GetSubstitutions(substName, false);
                        try
                        {
                            if (chdict == null)
                            {
                                chdict = request.GetSubstitutions(substName, true);
                                substDict.writeToLog("Creating substitutions: " + chdict);
                            }
                            loadSettingNode(chdict, n.ChildNodes, overwriteExisting, onlyIfUnknown, request);
                            continue;
                        }
                        catch (Exception e)
                        {
                            substDict.writeToLog("ERROR {0}", e);
                            //continue;
                            throw;
                        }
                    }
                    else
                    {
                        /// ProgramD shoukd nbot actually be here
                        loadSettingNode(chdict, n, overwriteExisting, onlyIfUnknown, request);
                    }
                }
                return;
            }
            if (myNode.NodeType == XmlNodeType.Element)
            {
                string href = RTPBot.GetAttribValue(myNode, "href", null);
                if (href != null && href.Length > 0)
                {
                    string name = RTPBot.GetAttribValue(myNode, "id", myNode.Name);
                    loadNameValueSetting(dict, name, href, "add", myNode, false, true, request);
                    return;
                }
            }

            if (lower == "bot")
            {
                var p = myNode.ParentNode;
                if (p != null && p.Name.ToLower() == "bots")
                {
                    loadSettingNode(dict, myNode.ChildNodes, overwriteExisting, onlyIfUnknown, request);
                    loadSettingNode(dict, myNode.Attributes, false, false, request);
                    return;
                }
            }

            if (lower == "root" || lower == "vars" || lower == "items" || lower == "properties"
                || lower == "bots" || lower == "testing" || lower == "predicates")
            {
                loadSettingNode(dict, myNode.ChildNodes, overwriteExisting, onlyIfUnknown, request);
                loadSettingNode(dict, myNode.Attributes, false, false, request);
                return;
            }
            if ((lower == "include"))
            {
                string path = RTPBot.GetAttribValue(myNode, "path", myNode.InnerText);

                overwriteExisting =
                    Boolean.Parse(RTPBot.GetAttribValue(myNode, "overwriteExisting", "" + overwriteExisting));

                onlyIfUnknown =
                    Boolean.Parse(RTPBot.GetAttribValue(myNode, "onlyIfKnown", "" + onlyIfUnknown));

                loadSettings(ToSettingsDictionary(dict), path, overwriteExisting, onlyIfUnknown, request);
                return;
            }
            SettingsDictionary settingsDict = ToSettingsDictionary(dict);
            if ((lower == "parent" || lower == "override" || lower == "fallback" || lower == "listener"
                || lower == "provider" || lower == "syncon" || lower == "synchon" || lower == "prefixes"
                || lower == "settingtypes" || lower == "formatter"))
            {
                string name = RTPBot.GetAttribValue(myNode, "value,dict,name", null);
                if (!string.IsNullOrEmpty(name))
                {
                    ParentProvider pp = settingsDict.FindDictionary(name, null);
                    if (pp == null || pp() == null)
                    {
                        settingsDict.writeToLog("DEBUG9 Cannot ResolveToObject settings line {0} in {1}", name, settingsDict);
                        return;
                    }
                    switch (lower)
                    {
                        case "provider":
                            settingsDict.InsertProvider(pp);
                            return;
                        case "parent":
                            settingsDict.InsertFallback(pp);
                            return;
                        case "syncon":
                        case "listener":
                            settingsDict.InsertListener(pp);
                            return;
                        case "fallback":
                            settingsDict.InsertFallback(pp);
                            return;
                        case "override":
                            settingsDict.InsertOverrides(pp);
                            return;
                        case "settingtypes":
                            settingsDict.InsertSettingReturnTypes(pp);
                            return;
                        case "formatter":
                            settingsDict.InsertGetSetFormatter(pp);
                            return;
                        case "prefixes":
                            settingsDict.AddChild(RTPBot.GetAttribValue(myNode, "prefix,name,dict,value", name), pp);
                            return;
                        default:
                            settingsDict.writeToLog("ERROR cannot make a name/v from " + AIMLLoader.TextAndSourceInfo(myNode));
                            return;
                    }
                    return;
                }
            }
            if ((lower == "maskedvar"))
            {
                string name = RTPBot.GetAttribValue(myNode, "name", "");
                if (name == "")
                {
                }
                if (settingsDict == null)
                {
                    ///warned already
                    return;
                }
                settingsDict.maskSetting(name);
                return;

            }
            //<predicates><predicate name="failed" default="" set-return="value"/>
            //<properties><property name="name" value="YourBot"/>
            //<vars><set name="accountability" >gnucash</set>
            //<properties><entry key="programd.aiml-schema.namespace-uri">http://alicebot.org/2001/AIML-1.0.1</entry>
            //<param name="PrintStackTraces" value="false"/>
            //<parameter name="channel" value="#some-channel"/>
            //<input><substitute find="=reply" replace=""/>
            //<substitution><old>:\)</old><new> smile </new></substitution>
            if (IsNameValueTag(lower))
            {

                string name = RTPBot.GetAttribValue(myNode, "name,var,old,key,find,param", null);
                if (name == null)
                {
                    XmlNode holder = AIMLLoader.FindNode("name,var,old,key,find", myNode, null);
                    if (holder == null)
                    {
                        settingsDict.SettingsLog("ERROR cannot make a name/v from " + AIMLLoader.TextAndSourceInfo(myNode));
                        return;
                    }
                    name = holder.InnerText;
                }
                string value = RTPBot.GetAttribValue(myNode, "value,href,default,replace,new,enabled", null);
                if (value == null)
                {
                    XmlNode holder = AIMLLoader.FindNode("value,default,replace,new", myNode, null);
                    if (holder != null)
                    {
                        value = holder.InnerXml;
                    }
                }
                if (value == null)
                {
                    string maybe = myNode.InnerXml.Trim();
                    if (maybe != null) value = maybe;
                }
                if (value == null)
                    settingsDict.writeToLog("ERROR cannot make a n/value from " + AIMLLoader.TextAndSourceInfo(myNode));

                loadNameValueSetting(dict, name, value, RTPBot.GetAttribValue(myNode, "type", "add"), myNode,
                            overwriteExisting, onlyIfUnknown, request);
                return;
            }
            if (lower == "learn" || lower == "srai" || lower == "aiml" || lower == "that" || lower == "category" || lower == "topic")
            {
                request.Loader.loadAIMLNode(myNode, request.LoadOptions, request);
                return;
            }
            if (myNode.NodeType == XmlNodeType.Element && atcount == 0)
            {
                int cs = myNode.ChildNodes.Count;
                string value = myNode.InnerXml.Trim();
                string itext = myNode.InnerText.Trim();
                if (itext == value)
                {
                    string name = myNode.Name;
                    loadNameValueSetting(dict, name, value, "add", myNode, false, true, request);
                    return;
                }

            }
            {
                settingsDict.writeToLog("-DICTRACE: ERROR unknow settings line {0} in {1}", AIMLLoader.TextAndSourceInfo(myNode), dict);
            }
        }

        private static bool IsNameValueTag(string lower)
        {
            return lower == "item" || lower == "set" || lower == "entry" || lower == "predicate" || lower == "property" ||
            lower == "substitution" || lower == "param" || lower == "parameter" || lower == "substitute";
        }

        public ParentProvider FindDictionary(string name, ParentProvider fallback)
        {
            var rtpbotobjCol = MushDLR223.ScriptEngines.ScriptManager.ResolveToObject(this, name);
            if (rtpbotobjCol == null || rtpbotobjCol.Count == 0)
            {
                var botGetDictionary = bot.GetDictionary(name);
                if (botGetDictionary != null) return ToParentProvider(botGetDictionary);
                writeToLog("DEBUG9 Cannot ResolveToObject settings line {0} in {1}", name, this);
                return fallback;
            }
            //if (tr)
            ParentProvider pp = ToParentProvider(rtpbotobjCol);
            if (pp == null)
            {
                ///warned already
                return fallback;
            }
            return pp;
        }

        public static SettingsDictionary ToSettingsDictionary(ISettingsDictionary dictionary)
        {
            if (dictionary == null)
            {
                RTPBot.writeDebugLine("-DICTRACE: Warning ToSettingsDictionary got NULL");
                return null;
            }
            if (dictionary is SubQuery) dictionary = ((SubQuery)dictionary).TargetSettings;
            if (dictionary is User) dictionary = ((User)dictionary).Predicates;
            SettingsDictionary sd = dictionary as SettingsDictionary;
            if (sd != null) return sd;
            RTPBot.writeDebugLine("-DICTRACE: Warning ToSettingsDictionary got type={0} '{1}'",
                                  dictionary.GetType(),
                                  dictionary);
            return null;
        }

        public static ParentProvider ToParentProvider(object dictionary)
        {
            if (dictionary == null)
            {
                RTPBot.writeDebugLine("-DICTRACE: Warning ToParentProvider got NULL");
                return null;
            }
            ParentProvider sd = dictionary as ParentProvider;
            if (sd != null) return sd;
            if (dictionary is ISettingsDictionary)
            {
                return (() => (ISettingsDictionary)dictionary);
            }
            if (dictionary is IEnumerable)
            {
                foreach (var VARIABLE in dictionary as IEnumerable)
                {
                    ParentProvider e = ToParentProvider(VARIABLE);
                    if (e != null) return e;
                }
            }
            RTPBot.writeDebugLine("-DICTRACE: Warning ToParentProvider got type={0} '{1}'",
                                  dictionary.GetType(),
                                  dictionary);
            return null;
        }

        public void SaveTo(string dir, string rootname, string filename)
        {
            HostSystem.CreateDirectory(dir);
            string tofile = HostSystem.Combine(dir, filename);
            if (fromFile == null) fromFile = tofile;
            HostSystem.BackupFile(tofile);
            XmlDocument xmldoc;
            lock (orderedKeys)
            {
                var restore = NameSpace;
                try
                {
                    NameSpace = rootname;
                    xmldoc = DictionaryAsXML;
                }
                finally
                {
                    NameSpace = restore;
                }
            }
            xmldoc.Save(tofile);
        }

        /// <summary>
        /// Adds a bespoke setting to the Settings class (accessed via the grabSettings(string name)
        /// method.
        /// </summary>
        /// <param name="name">The name of the new setting</param>
        /// <param name="value">The value associated with this setting</param>
        public bool addSetting(string name, Unifiable value)
        {
            bool found = true;
            lock (orderedKeys)
            {
                name = TransformName(name);
                string normalizedName = TransformKey(name);
                if (normalizedName == "ISSUBSTS")
                {
                    IsSubsts = Unifiable.IsTrue(value);
                }
                if (makedvars.Contains(normalizedName))
                {
                    SettingsLog("ERROR MASKED ADD SETTING '" + name + "'=" + str(value) + " ");
                    return false;
                }
                value = TransformValue(value);
                if (normalizedName.Length > 0)
                {
                    SettingsLog("ADD LOCAL '" + name + "'=" + str(value) + " ");
                    found = this.removeSetting(name);
                    updateListeners(name, value, true, !found);
                    this.orderedKeys.Add(name);
                    this.settingsHash.Add(normalizedName, value);
                }
                else
                {
                    SettingsLog("ERROR ADD Setting Local '" + name + "'=" + str(value) + " ");
                }
            }
            return !found;
        }

        private void updateListeners(string name, Unifiable value, bool locally, bool addedNew)
        {
            foreach (var list in _listeners)
            {
                var l = list();
                if (addedNew) l.addSetting(name, value);
                else
                    l.updateSetting(name, value);
            }
        }

        private Unifiable TransformValue(Unifiable value)
        {
            if (value == null)
            {
                writeToLog("ERROR " + value + " NULL");
                return " NULL ";

            }
            string v = value.AsString();
            if (v.Contains("<") || v.Contains("&"))
            {
                writeToLog("!@ERROR BAD INPUT? " + value);
            }
            return value;
        }

        public bool addListSetting(string name, Unifiable value)
        {
            lock (orderedKeys)
            {
                name = TransformName(name);
                string normalizedName = TransformKey(name);
                if (normalizedName.Length > 0)
                {
                    this.removeSetting(name);
                    this.orderedKeys.Add(name);
                    this.settingsHash.Add(normalizedName, value);
                }
            }
            return true;
        }

        private string TransformName(string name)
        {
            string nn = name;
            name = name.ToUpper();
            int len = name.Length;
            if (IsSubsts)
            {
                name = name.Replace("\\b", " ").Trim();
            }
            else
            {
                name = name.Replace("FAVORITE", "FAV");
            }
            if (name == nn) return nn;
            return name;
        }

        /// <summary>
        /// Removes the named setting from this class
        /// </summary>
        /// <param name="name">The name of the setting to remove</param>
        public bool removeSetting(string name)
        {
            lock (orderedKeys)
            {
                name = TransformName(name);
                string normalizedName = TransformKey(name);
                bool ret = orderedKeys.Contains(name);
                this.orderedKeys.Remove(name);
                // shouldnt need this next one (but just in case)
                this.orderedKeys.Remove(normalizedName);
                this.removeFromHash(name);
                if (ret)
                {
                    //maskedettings.Remove(normalizedName);
                }
                return ret;
            }
        }

        public string TransformKey(string name)
        {
            if (TrimKeys) name = name.Trim();

            name = name.ToUpper();
            if (true) name = name.Replace("FAVORITE", "FAV");

            if (false) foreach (var k in new string[] { "FAVORITE", "FAV" })
                {

                    if (name.StartsWith(k))
                    {
                        if (name.Length > k.Length) name = name.Substring(k.Length);
                    }
                }
            else
                if (false) foreach (var k in new string[] { "FAVORITE", "FAV" })
                    {

                        if (name.StartsWith(k))
                        {
                            if (name.Length > k.Length) name = name.Substring(k.Length);
                        }
                    }
            return name;
            //return MakeCaseInsensitive.TransformInput(name);
        }

        /// <summary>
        /// Removes a named setting from the Dictionary<,>
        /// </summary>
        /// <param name="name">the key for the Dictionary<,></param>
        private void removeFromHash(string name)
        {
            lock (orderedKeys)
            {
                name = TransformName(name);
                string normalizedName = TransformKey(name);
                this.settingsHash.Remove(normalizedName);
            }
        }

        /// <summary>
        /// Updates the named setting with a new value whilst retaining the position in the
        /// dictionary
        /// </summary>
        /// <param name="name">the name of the setting</param>
        /// <param name="value">the new value</param>
        public bool updateSetting(string name, Unifiable value)
        {
            bool overriden = false;
            foreach (var parent in _overides)
            {
                var p = parent();
                if (p.updateSetting(name, value))
                {
                    SettingsLog("OVERRIDDEN UPDATE " + p + " '" + name + "'=" + str(value));
                    overriden = true;
                }
            }
            if (overriden)
            {
                return true;
            }
            lock (orderedKeys)
            {
                name = TransformName(name);
                string normalizedName = TransformKey(name);
                if (this.settingsHash.ContainsKey(normalizedName))
                {
                    var old = this.settingsHash[normalizedName];

                    if (makedvars.Contains(normalizedName))
                    {
                        SettingsLog("MASKED Not Update Local '" + name + "'=" + str(value) + " keeped " + str(old));
                        return false;
                    }
                    updateListeners(name, value, true, false);
                    this.removeFromHash(name);
                    SettingsLog("UPDATE Setting Local '" + name + "'=" + str(value));
                    this.settingsHash.Add(normalizedName, value);
                    return true;
                }

                // before fallbacks
                if (makedvars.Contains(normalizedName))
                {
                    SettingsLog("MASKED NOT UPDATE FALLBACKS '" + name + "'=" + str(value));
                    return false;
                }
            }
            foreach (var parent in Fallbacks)
            {
                if (parent.updateSetting(name, value))
                {
                    SettingsLog("PARENT UPDATE " + parent + " '" + name + "'=" + str(value));
                    return true;
                }
            }
            return false;
        }

        static string str(Unifiable unifiable)
        {
            return "'" + Unifiable.ToVMString(unifiable) + "'";
        }

        /// <summary>
        /// Clears the dictionary to an empty state
        /// </summary>
        public void clearSettings()
        {
            lock (orderedKeys)
            {
                this.orderedKeys.Clear();
                this.settingsHash.Clear();
            }
        }
        public void clearHierarchy()
        {
            lock (orderedKeys)
            {
                _overides.Clear();
                _fallbacks.Clear();
                _fallbacks.Add(() => prefixProvideer);
                makedvars.Clear();
            }
        }

        public void clearSyncs()
        {
            _listeners.Clear();
            _listeners.Add(() => prefixProvideer);
        }

        private HashSet<string> makedvars = new HashSet<string>();
        public void maskSetting(string name)
        {
            name = TransformName(name);
            name = TransformKey(name);
            writeToLog("MASKING: " + name);
            lock (orderedKeys) makedvars.Add(name);
        }

        /// <summary>
        /// Returns the value of a setting given the name of the setting
        /// </summary>
        /// <param name="name">the name of the setting whose value we're interested in</param>
        /// <returns>the value of the setting</returns>
        public Unifiable grabSetting(string name)
        {
#if debug
            var v = grabSetting0(name);
            if (Unifiable.IsNullOrEmpty(v))
            {
                writeToLog("DICT '{0}'=null", null);
            }
            return v;
#else
            try
            {
                name = TransformName(name);
                return grabSetting0(name);
            }
            catch (Exception e)
            {
                writeToLog("ERROR {0}", e);

                return null;
            }
#endif
        }

        public Unifiable grabSetting0(string name)
        {
            foreach (ParentProvider overide in _overides)
            {
                ISettingsDictionary dict = overide();
                if (dict.containsSettingCalled(name))
                {
                    Unifiable v = dict.grabSetting(name);
                    SettingsLog("OVERRIDE '" + name + "'=" + str(v));
                    return v;
                }
            }
            lock (orderedKeys)
            {
                string normalizedName = TransformKey(name);

                if (this.settingsHash.ContainsKey(normalizedName))
                {
                    Unifiable v = this.settingsHash[normalizedName];
                    if (makedvars.Contains(normalizedName))
                    {
                        SettingsLog("MASKED RETURNLOCAL '" + name + "=NULL instead of" + str(v));
                        return null;
                    }
                    SettingsLog("LOCALRETURN '" + name + "'=" + str(v));
                    return v;
                }
                else if (Fallbacks.Count > 0)
                {
                    foreach (var list in Fallbacks)
                    {
                        list.IsTraced = false;
                        if (list.containsSettingCalled(name))
                        {
                            Unifiable v = list.grabSetting(name);
                            if (makedvars.Contains(normalizedName))
                            {
                                SettingsLog("MASKED PARENT '" + name + "=NULL instead of" + str(v));
                                return null;
                            }
                            SettingsLog("RETURN FALLBACK '" + name + "'=" + str(v));
                            if (v != null && !Unifiable.IsFalse(v)) return v;
                        }
                    }
                    var v0 = Fallbacks[0].grabSetting(name);
                    if (!Unifiable.IsNull(v0))
                    {
                        SettingsLog("RETURN FALLBACK0 '" + name + "'=" + str(v0));
                        return v0;
                    }
                }
                SettingsLog("MISSING '" + name + "'");
                return Unifiable.NULL;

            }
        }

        public void SettingsLog(string message, params object[] args)
        {
            if (message.Contains("ERROR") && !message.Contains("ERROR: The requ"))
            {
                IsTraced = true;
            }
            if (!IsTraced) return;
            var fc = new StackTrace().FrameCount;
            writeToLog("DICTLOG: " + NameSpace + " (" + fc + ")   " + message, args);
            if (fc > 200)
            {
                //throw new 
                writeToLog("ERROR DICTLOG OVERFLOWING: " + NameSpace + " (" + fc + ")   " + message, args);
                //Console.ReadLine();
            }
        }

        /// <summary>
        /// Checks to see if a setting of a particular name exists
        /// </summary>
        /// <param name="name">The setting name to check</param>
        /// <returns>Existential truth value</returns>
        public bool containsLocalCalled(string name)
        {
            lock (orderedKeys)
            {
                name = TransformName(name);
                string normalizedName = TransformKey(name);

                if (makedvars.Contains(normalizedName)) return true;

                if (normalizedName.Length > 0)
                {
                    return settingsHash.ContainsKey(normalizedName);

                    if (!this.settingsHash.ContainsKey(normalizedName))
                    {
                        if (!this.orderedKeys.Contains(name))
                        {
                            if (!this.orderedKeys.Contains(name.ToUpper()))
                            {
                                writeToLog("Missing odered key " + name);
                            }
                        }
                        return true;
                    }
                    return false;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool containsSettingCalled(string name)
        {
            var value = grabSettingNoDebug(name);
            return !Unifiable.IsNullOrEmpty(value);
        }

        /// <summary>
        /// Returns a collection of the names of all the settings defined in the dictionary
        /// </summary>
        /// <returns>A collection of the names of all the settings defined in the dictionary</returns>
        public IEnumerable<string> SettingNames(int depth)
        {
            //       get
            {
                lock (orderedKeys)
                {
                    var list = prefixProvideer.SettingNames(depth) as List<String>;
                    if (list != null && list.Count > 0)
                    {
                        list.AddRange(orderedKeys);
                        return list.ToArray();
                    }
                    string[] result = new string[this.orderedKeys.Count];
                    this.orderedKeys.CopyTo(result, 0);
                    return result;
                }
            }
        }

        public List<ISettingsDictionary> Fallbacks
        {
            get
            {
                return ProvidersFrom(_fallbacks);
            }
        }



        public Unifiable this[string name]
        {
            get { return SettingsDictionary.IndexGet(this, name); }
            set { SettingsDictionary.IndexSet(this, name, value); }
        }

        public static void IndexSet(ISettingsDictionary dictionary, string name, Unifiable value)
        {
            dictionary.addSetting(name, value);
        }

        public static Unifiable IndexGet(ISettingsDictionary dictionary, string name)
        {
            return dictionary.grabSetting(name);
        }

        public static IEnumerable<string> NO_SETTINGS = new string[0];
        public static IEnumerable<string> TOO_DEEP = new string[0];
        public bool IsSubsts;


        public void InsertSettingReturnTypes(ParentProvider pp)
        {
            AddSettingToCollection(pp, SetReturnProviders);
        }

        public void InsertGetSetFormatter(ParentProvider pp)
        {
            AddSettingToCollection(pp, GetSetFormatters);
        }

        public void AddSettingToCollection(ParentProvider pp, List<ParentProvider> cols)
        {
            AddSettingToCollection(null, pp, cols);
        }
        public void AddSettingToCollection(ISettingsDictionary dictionary, List<ParentProvider> cols)
        {
            AddSettingToCollection(dictionary, null, cols);
        }


        public void AddSettingToCollection(ISettingsDictionary dictionary, ParentProvider pp, List<ParentProvider> cols)
        {
            if (dictionary == null)
            {
                if (pp == null)
                {
                    writeToLog("ERROR: should not place NULL inside self");
                    return;
                }
                dictionary = pp();
                if (dictionary == null)
                {
                    writeToLog("WARN: NULL inside pp");
                }
            }
            if (dictionary == this)
            {
                writeToLog("ERROR: should not place inside self");
                return;
            }
            lock (cols)
            {
                foreach (var deep in cols)
                {
                    if (deep == pp)
                    {
                        return;
                    }
                    var inner = deep();
                    if (inner == null)
                    {
                        writeToLog("WARN: NULL Parent IDictionary " + dictionary);
                    }
                    if (inner == dictionary)
                    {
                        writeToLog("WARN: alread contains IDictionary " + dictionary);
                        return;
                    }
                }
                if (pp == null) pp = () => dictionary;
                cols.Insert(0, pp);
            }
        }

        public List<ParentProvider> GetSetFormatters = new List<ParentProvider>();
        /// <summary>
        /// //"$bot feels $value emotion towards $user";
        /// </summary>
        public String DefaultFormatter = "$subject $relation is $value";  //default
        /// <summary>
        /// $user $relation $value   $robot  $dict
        ///   1      2        3       4       5 
        /// </summary>
        /// <param name="relation"></param>
        /// <returns></returns>
        public string GetFormatter(string relation)
        {
            string realName;
            return WithProviders(this, relation, out realName,
                                 GetSetFormatters,
                                 (realName0) => DefaultFormatter.Replace("$relation", realName0));
        }

        public static Unifiable WithProviders(ISettingsDictionary dictionary, string name, out string realName, 
            IEnumerable<ParentProvider> providers, Func<string, Unifiable> Else)
        {
            //SettingsDictionary dictionary = ToSettingsDictionary(dictionary0);
            realName = name;
            foreach (var provider in ProvidersFrom(providers, dictionary))
            {
                var v = provider.grabSetting(name);
                if (!Unifiable.IsNull(v)) return v;
            }
            if (name.Contains(","))
            {
                foreach (string name0 in name.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries))
                {
                    var un = WithProviders(dictionary, name0, out realName, providers, Else);
                    if (!Unifiable.IsNull(un))
                    {
                        return un;
                    }
                    if (dictionary.containsLocalCalled(name))
                    {
                        realName = name;
                    }
                }
                return Else(realName);
            }
            return null;
        }

        public string Preposition = "";
        public List<ParentProvider> SetReturnProviders = new List<ParentProvider>();
        public Unifiable GetSetReturn(string name, out string realName)
        {
            return WithProviders(this, name, out realName, SetReturnProviders,
                                 realname =>
                                 {
                                     string prep = Preposition;
                                     return (string.IsNullOrEmpty(prep) ? "" : prep + " ")
                                            + realname;
                                 });
        }

        private List<ISettingsDictionary> ProvidersFrom(IEnumerable<ParentProvider> providers)
        {
            return ProvidersFrom(providers, this);
        }

        static List<ISettingsDictionary> ProvidersFrom(IEnumerable<ParentProvider> providers, object exceptFor)
        {
            var found = new List<ISettingsDictionary>();
            lock (providers)
            {
                foreach (var list in providers)
                {
                    var res = list();
                    if (res == null)
                    {
                        //writeToLog("NULL provider " + list);
                        continue;
                    }
                    if (res == exceptFor)
                    {
                        //writeToLog("ERROR: Circular provider " + list);
                        continue;
                    }
                    found.Add(res);
                }
            }
            return found;
        }

        private void addSetReturn(string name, string value)
        {
            AddToProviders(name + ".set-return", value, SetReturnProviders);
        }

        public void addFormatter(string name, string value)
        {
            AddToProviders(name, value, GetSetFormatters);
        }

        private void AddToProviders(string name, string value, IEnumerable<ParentProvider> providers)
        {
            foreach (ISettingsDictionary s in ProvidersFrom(providers))
            {
                var v = s.grabSetting(name);
                if (v == null) s.addSetting(name, value);
            }
        }

        /// <summary>
        /// Copies the values in the current object into the SettingsDictionary passed as the target
        /// </summary>
        /// <param name="target">The target to recieve the values from this SettingsDictionary</param>
        public void Clone(ISettingsDictionary target)
        {
            var dt = ToSettingsDictionary(target);
            lock (SetReturnProviders) foreach (var pp in SetReturnProviders)
                {
                    dt.InsertSettingReturnTypes(pp);
                }
            lock (GetSetFormatters) foreach (var pp in GetSetFormatters)
                {
                    dt.InsertGetSetFormatter(pp);
                }
            lock (orderedKeys)
            {
                foreach (string name in this.orderedKeys)
                {
                    target.addSetting(name, this.grabSetting(name));
                }
            }
        }
        /// <summary>
        /// Copies the values in the current object into the SettingsDictionary passed as the target
        /// If the keys are missing
        /// </summary>
        /// <param name="target">The target to recieve the values from this SettingsDictionary</param>
        public void AddMissingKeys(ISettingsDictionary target)
        {
            lock (orderedKeys)
            {
                foreach (string name in this.orderedKeys)
                {
                    if (target.containsLocalCalled(name)) continue;
                    target.addSetting(name, this.grabSetting(name));
                }
            }
        }
        #endregion

        public void addObjectFields(Object obj)
        {
            foreach (var hash in obj.GetType().GetProperties())
            {
                addObjectProperty(obj, hash);
            }
        }

        public void AddObjectProperty(object o, String name)
        {
            addGetSet(new ObjectPropertyDictionary(name, name, o));
        }


        private void addObjectProperty(object o, PropertyInfo info)
        {
            string name = info.Name;
            addGetSet(new ObjectPropertyDictionary(name, name, o));
        }

        private void addGetSet(ObjectPropertyDictionary o)
        {
            InsertProvider(() => { return o; });
        }

        public void InsertFallback(ParentProvider provider)
        {
            AddSettingToCollection(provider, _fallbacks);
        }

        public void InsertListener(ParentProvider provider)
        {
            AddSettingToCollection(provider, _listeners);
        }

        public void InsertOverrides(ParentProvider provider)
        {
            AddSettingToCollection(provider, _overides);
        }

        public void InsertProvider(ParentProvider provider)
        {
            AddSettingToCollection(provider, _listeners);
            AddSettingToCollection(provider, _fallbacks);
        }

        public void AddChild(string prefix, ParentProvider dict)
        {
            ISettingsDictionary sdict = dict();
            prefixProvideer.AddChild(prefix, dict);
        }

        public void AddGetSetProperty(string topic, GetUnifiable getter, Action<Unifiable> setter)
        {
            var prov = new GetSetDictionary(topic, new GetSetProperty(getter, setter));
            InsertProvider(() => prov);
        }

        internal void AddGetSetProperty(string p, CollectionProperty v)
        {
            GetSetDictionary prov = new GetSetDictionary(p, v.GetProvider());
            InsertProvider(() => prov);
        }

        public Unifiable grabSettingNoDebug(string name)
        {
            lock (orderedKeys)
            {
                if (!IsTraced) return grabSetting(name);
                IsTraced = true;
                try
                {
                    var v = grabSetting(name);
                    return v;
                }
                finally
                {
                    IsTraced = false;
                }
            }
        }

        public static Unifiable grabSettingDefault(ISettingsDictionary dictionary, string name, out string realName, SubQuery query)
        {
            bool succeed;
            return NamedValuesFromSettings.GetSettingForType(dictionary.NameSpace, query, dictionary, name, 
                out realName, name, null, out succeed, null);
        }
        public static Unifiable grabSettingDefaultDict(ISettingsDictionary dictionary, string name, out string realName)
        {
            realName = name;
            var un = dictionary.grabSetting(name);
            if (Unifiable.IsNull(un))
            {
                if (name.Contains(","))
                    foreach (string name0 in name.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        un = grabSettingDefaultDict(dictionary, name0, out realName);
                        if (!Unifiable.IsNull(un))
                        {
                            return un;
                        }
                    }

                string[] chops = new string[] { "favorite.", "favorite", "fav" };
                foreach (var chop in chops)
                {
                    if (name.StartsWith(chop))
                    {
                        string newName = name.Substring(chop.Length);
                        return grabSettingDefaultDict(dictionary, newName, out newName);
                    }
                }
                foreach (var chop in chops)
                {

                    realName = chop + name;
                    if (dictionary is SettingsDictionary)
                    {
                        SettingsDictionary sd = (SettingsDictionary)dictionary;
                        un = sd.grabSettingNoDebug(realName);
                    }
                    else
                    {
                        un = dictionary.grabSetting(realName);
                    }
                    if (!Unifiable.IsNull(un))
                    {
                        return un;
                    }
                }
            }
            return un;
        }
    }
}
