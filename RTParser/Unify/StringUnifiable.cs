using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using RTParser.AIMLTagHandlers;
using RTParser.Database;
using RTParser.Utils;
using Console=System.Console;

namespace RTParser
{
    public class StringUnifiable : Unifiable
    {

        protected void SpoilCache()
        {
            splitted = null;
            valueCache = null;
            rest = null;
            upper = null;
        }

        public string str;
        protected Unifiable[] splitted = null;
        protected Unifiable rest = null;
        public string upper;
        private object valueCache;

        protected string _str
        {
            set
            {
                Flags = FlagsForString(value);
                str = value;
            }
            get { return str; }
        }
        public override string ToUpper()
        {
            if (upper == null)
            {
                upper = AsString().ToUpper().Trim().Replace("  ", " ");
            }
            return upper;
        }

        protected StringUnifiable()
        {
            str = "";
        }

        public StringUnifiable(string value)
        {
            _str = value;
        }

        static UFlags FlagsForString(string str)
        {
            if (str == null) return UFlags.IS_NULL;
            else
            {
                int len = str.Length;
                if (len == 0)
                    return UFlags.IS_EMPTY | UFlags.IS_EXACT | UFlags.IS_FALSE;
                else
                {
                    UFlags Flags = UFlags.NO_FLAGS;
                    char c;
                    str = str.Trim();
                    if (str == "") return UFlags.IS_EMPTY | UFlags.IS_FALSE;
                    c = str[0];

                    switch (c)
                    {
                        case '_':
                            Flags |= UFlags.SHORT_WILDCARD;
                            break;
                        case 'N':
                            Flags |= UFlags.IS_FALSE | UFlags.IS_EXACT;
                            break;
                        case 'F':
                            Flags |= UFlags.IS_FALSE | UFlags.IS_EXACT;
                            break;
                        case 'T':
                            if (len > 3)
                            {
                                if (str.StartsWith("TAG-")) Flags |= UFlags.IS_TAG;
                            }
                            Flags |= UFlags.IS_TRUE | UFlags.IS_EXACT;
                            break;
                        case '*':
                            Flags |= UFlags.LONG_WILDCARD;
                            break;
                        case '~':
                            Flags |= UFlags.REG_CLASS | UFlags.NO_BINDS_STARS;
                            break;
                        case '^':
                            Flags |= UFlags.REG_CLASS;
                            break;
                        case '#':
                            Flags |= UFlags.REG_CLASS | UFlags.IS_TRUE | UFlags.IS_EXACT;
                            break;
                        default:
                            if (len > 3)
                            {
                                if (c == '<')
                                {
                                    Flags |= UFlags.LAZY_XML;

                                    if (UFlags.LAZY_XML == Flags)
                                    {
                                        if (str.Contains("_")) Flags |= UFlags.SHORT_WILDCARD;
                                        else if (str.Contains("*")) Flags |= UFlags.LONG_WILDCARD;
                                        else
                                        {
                                            Flags |= UFlags.NO_BINDS_STARS;
                                        }
                                    }
                                }
                                else
                                {
                                    int wh = str.IndexOf(' ');
                                    if (wh > 1) Flags |= UFlags.MORE_THAN_ONE;
                                    else Flags |= UFlags.ONLY_ONE;
                                    Flags |= UFlags.IS_EXACT;
                                }
                            }
                            break;

                    }


                    c = str[str.Length - 1];
                    if (c == '_')
                        Flags |= UFlags.SHORT_WILDCARD | UFlags.BINDS_STARS;
                    else if (c == '*')
                        Flags |= UFlags.LONG_WILDCARD | UFlags.BINDS_STARS;
                    else if (Flags == UFlags.NO_FLAGS)
                    {
                        if (char.IsLetter(c))
                        {
                            return UFlags.IS_EXACT;
                        }
                        else if (char.IsNumber(c) || c == '%')
                        {
                            return UFlags.IS_EXACT;
                        }
                        else if (char.IsPunctuation(c) || c == '%')
                        {
                            return UFlags.IS_PUNCT;
                        }
                        return UFlags.IS_EXACT;
                    }
                    return Flags;
                }

            }
        }

        //public int Length
        //{
        //    get
        //    {
        //        if (str == null)
        //        {
        //            return 0;
        //        }
        //        return str.Length;
        //    }
        //}




        //public Unifiable Replace(object marker, object param1)
        //{
        //    return str.Replace(astr(marker), astr(param1));
        //}

        public static string astr(object param1)
        {
            return "" + param1;
        }

        public override Unifiable Trim()
        {
            string str2 = str.Trim().Replace("  ", " ").Replace("  ", " ");
            if (str2 == str) return this;
            return str.Trim();
        }

        public override string AsString()
        {
            return str;
        }

        public override Unifiable ToCaseInsenitive()
        {
            return Create(str.ToUpper());
        }

        public virtual char[] ToCharArray()
        {
            return str.ToCharArray();
        }

        public override bool Equals(object obj)
        {
            if (obj is Unifiable) return ((Unifiable)obj) == this;
            var os = astr(obj);
            if (str == os) return true;
            if (str.ToLower() == os.ToLower())
            {
                return true;
            }
            return false;

        }

        public override object AsNodeXML()
        {
            return str;
        }

        public override string ToString()
        {
            if (str == null)
            {
                writeToLog("ToSTring=NULL");
                return null;
            }
            return str;
        }

        public override int GetHashCode()
        {
            if (IsWildCard()) return -1;
            return str.GetHashCode();
        }

        //public override Unifiable[] Split(Unifiable[] tokens, StringSplitOptions stringSplitOptions)
        //{
        //    return arrayOf(str.Split(FromArrayOf(tokens), stringSplitOptions));
        //}

        public override object Raw
        {
            get { return str; }
        }

        protected override bool IsFalse()
        {
            if (IsFlag(UFlags.IS_FALSE)) return true;
            if (IsFlag(UFlags.IS_TRUE)) return false;
            if (String.IsNullOrEmpty(str))
            {
                Flags |= UFlags.IS_FALSE;
                return true;
            }

            string found = str.Trim().ToUpper();
            if (found == "" || found == "NIL" || found == "()" || found == "FALSE" || found == "NO" || found == "OFF")
            {
                Flags |= UFlags.IS_FALSE;
                return true;
            }
            return false;
        }

        public override bool IsWildCard()
        {
            if (string.IsNullOrEmpty(str)) return false;
            char c = str[str.Length - 1];
            return c == '_' || c == '*';
            return false;

            if (str == "*" || str == "_")
            {
                return true;
            }
            if (str.Contains("*") || str.Contains("_")) return true;
            //if (IsMarkerTag()) return false;
            if (str.StartsWith("<"))
            {
                return IsLazyStar();
            }
            return false;
        }

        public override Unifiable[] ToArray()
        {
            if (splitted != null)
            {
                return splitted;
            }
            if (splitted == null) splitted = Splitter(str);
            return splitted;
        }

        public static Unifiable[] Splitter(string str)
        {
            string strTrim = str.Trim().Replace("  ", " ").Replace("  ", " ");
            if (!strTrim.Contains("<"))
                return arrayOf(strTrim.Split(BRKCHARS, StringSplitOptions.RemoveEmptyEntries));
            XmlDocument doc = new XmlDocument();
            List<Unifiable> u = new List<Unifiable>();

            try
            {
                doc.LoadXml("<node>" + strTrim + "</node>");
                foreach (XmlNode node in doc.FirstChild.ChildNodes)
                {
                    if (node.NodeType == XmlNodeType.Comment) continue;
                    if (node.NodeType == XmlNodeType.Whitespace) continue;
                    if (node.NodeType == XmlNodeType.Text)
                    {
                        string splitMe = node.Value.Trim();
                        u.AddRange(Splitter(splitMe));
                    }
                    else if (node.NodeType == XmlNodeType.Element)
                    {
                        string splitMe = node.OuterXml.Trim();
                        u.Add(splitMe);
                    }
                    else
                    {
                        string splitMe = node.OuterXml.Trim();
                        u.Add(splitMe);
                    }
                }
                return u.ToArray();
            }
            catch (Exception e)
            {
                RTPBot.writeDebugLine("" + e.Message + ": " + strTrim);
            }
            return arrayOf(strTrim.Split(BRKCHARS, StringSplitOptions.RemoveEmptyEntries));
        }

        public override bool IsTag(string that)
        {
            return str == "TAG-" + that || str.StartsWith("<" + that.ToLower());
        }

        public override void Append(Unifiable p)
        {
            throw new Exception("this " + AsString() + " cannot be appended with " + p);
        }

        public override void Append(string part)
        {
            throw new NotImplementedException();
        }

        public override Unifiable Frozen(SubQuery subquery)
        {
            return Create(str);
        }

        public override Unifiable ToPropper()
        {
            int len = str.Length;

            if (len == 0) return this;
            string newWord = str.Substring(0, 1).ToUpper();
            if (len == 1)
            {
                if (newWord == str) return this;
            }
            newWord += str.Substring(1).ToLower();
            return newWord;
        }

        public override Unifiable Rest()
        {

            splitted = ToArray();
            if (rest == null) 
             return Join(" ", splitted, 1, splitted.Length - 1);
            return rest;

            if (String.IsNullOrEmpty(this.str)) return Unifiable.Empty;
            int i = str.IndexOfAny(BRKCHARS);
            if (i == -1) return Empty;
            rest = str.Substring(i + 1);
            return Create(rest.Trim());
        }

        readonly static char[] BRKCHARS = " \r\n\t".ToCharArray();

        public override Unifiable First()
        {
            if (String.IsNullOrEmpty(str)) return Unifiable.Empty;
            //int i = str.IndexOfAny(BRKCHARS);
            //if (i == -1) return Create(str);
            var s = ToArray();
            if (s == null) return null;
            if (s.Length < 1) return Empty;
            return s[0];
            //string rest = str.Substring(0, i - 1);
            //return Create(rest.Trim());
        }

        public override bool IsShort()
        {
            if (str == "_") return true;
            // if (this.IsMarkerTag()) return false; // tested by the next line
            if (IsLazyStar()) return false;
            if (IsLazy()) return true;
            return false;
        }

        public override bool IsFiniteWildCard()
        {
            if (str == "_") return true;
            // if (this.IsMarkerTag()) return false; // tested by the next line
            if (IsLazyStar()) return false;
            if (IsLazy()) return true;
            return false;
        }

        public override bool IsLongWildCard()
        {
            return str == "*";
            return IsFlag(UFlags.LONG_WILDCARD);
            if (str == ("*")) return true;
            if (str == ("^")) return true;
            if (this.IsMarkerTag()) return false;
            if (IsLazyStar()) return true;
            return false;
        }

        public override bool IsLazy()
        {
            if (this.IsMarkerTag()) return false;
            if (str == "") return false;
            if (str[0] == '~')
            {
                return true;
            }
            return str.StartsWith("<");
        }

        public override bool IsLitteral()
        {
            if (this.IsLazy()) return false;
            if (this.IsMarkerTag()) return true;
            if (this.IsWildCard()) return false;
            return true;
        }

        public virtual bool IsMarkerTag()
        {
            return IsFlag(UFlags.IS_TAG);
        }

        override public bool StoreWildCard()
        {
            return !str.StartsWith("~");
        }

        public override bool IsAnyWord()
        {
            return str == "_";
        }

        override public bool ConsumeFirst(Unifiable fullpath, out Unifiable left, out Unifiable right, SubQuery query)
        {
            left = Unifiable.Empty;
            right = fullpath;
            return false;
            Unifiable[] array = fullpath.ToArray();
            int len = array.Length;
            if (len == 0) return false;
            if (str == "_")
            {
                if (len > 1)
                {

                }
                return false;
            }
            Unifiable[] myA = ToArray();
            int upTo = myA.Length;
            // if (upTo == 0) return false;
            int min = 1;
            Unifiable matchMe = this;
            if (!IsLazy())
            {
                upTo = matchMe.ToUpper().Split(new char[] { ' ' }).Length;
                min = upTo;
            }
            else
            {
                matchMe = ToValue(query);
                upTo = matchMe.ToUpper().Split(new char[] { ' ' }).Length;
                min = upTo;
            }
            if (upTo > len)
            {
                upTo = len;
            }
            //if (upTo > 1) writeToLog("ConsumeFirst Try: " + fullpath);

            for (int j = min; j <= upTo; j++)
            {
                left = Join(" ", array, 0, j);
                if (matchMe.WillUnify(left, query))
                {
                    if (j > 1) writeToLog("ConsumeFirst Success!: " + fullpath);
                    rest = Join(" ", array, j, len - j);
                    return true;
                }
            }
            return false;
        }

        public override bool ConsumePath(string[] strings, out string fw, out int rw, SubQuery query)
        {
            rw = strings.Length;
            if (rw == 0)
            {
                fw = "";
                return IsEmpty;
            }
            fw = strings[0];
            rw = 1;
            string fws = fw.ToUpper();
            string su = ToUpper();
            if (su == fws)
            {
                return true;
            }
            int minLen = LengthMin;
            if (minLen > strings.Length)
            {
                return false;
            }
            if (fws == "NOTHING") return false;
            Unifiable ovs = fws;
            if (ovs.IsFlag(UFlags.IS_TAG | UFlags.IS_EMPTY)) return false;
            if (ovs.IsFlag(UFlags.BINDS_STARS)) return false;
            if (str.StartsWith("<"))
            {
                return UnifyTagHandler(ovs, query);
            }
            if (minLen > 1)
            {
                writeToLog("MinLen=" + minLen);
            }
            return false;
        }

        protected int LengthMin
        {
            get { return ToArray().Length; }
        }

        public override float Unify(Unifiable other, SubQuery query)
        {
            if (Object.ReferenceEquals(this, other)) return UNIFY_TRUE;
            if (Object.ReferenceEquals(null, other)) return UNIFY_FALSE;

            string su = ToUpper();
            string ou = other.ToUpper();
            if (su == ou) return UNIFY_TRUE;
            bool otherIsLitteral = other.IsLitteral();
            if (IsLitteral())
            {
                if (!otherIsLitteral)
                {
                    return other.Unify(this, query);
                }
                string sv = ToValue(query);
                string ov = other.ToValue(query);
                if (IsStringMatch(sv, ov))
                {
                    writeToLog("IsStringMatch({0}, {1})", sv, ov);
                    return UNIFY_TRUE;
                }
                return UNIFY_FALSE;
            }
            if (su == "*")
            {
                writeToLog("CALL CALL/WILL UNIFY");
                return !other.IsEmpty ? UNIFY_TRUE : UNIFY_FALSE;
            }
            else if (su == "_")
            {
                writeToLog("CALL CALL/WILL UNIFY");
                return other.IsShort() ? UNIFY_TRUE : UNIFY_FALSE;
            }
            else
            {
                string sv = ToValue(query);
                string ov = other.ToValue(query);
                if (IsStringMatch(sv, ov))
                {
                    writeToLog("IsStringMatch({0}, {1})", sv, ov);
                    return UNIFY_TRUE;
                }
                if (IsLazy())
                {
                    return UnifyTagHandler(other, query) ? UNIFY_TRUE : UNIFY_FALSE;
                }
                if (IsWildCard())
                {
                    writeToLog("UnifyLazy SUCCESS: " + other + " in " + query);
                    return UNIFY_TRUE;
                }
                writeToLog("UnifyLazy FALSE: " + other + " in " + query);
                return UNIFY_FALSE;
            }
        }

        private bool UnifyTagHandler(Unifiable ov, SubQuery query)
        {
            try
            {
                var flags = ov.Flags;
                ///bot.GetTagHandler(templateNode, subquery, request, result, request.user);
                if (IsCachedMatch(ov,query))
                {
                    writeToLog("UnifyLazy: CACHED" + ov + " in " + query);
                    return true;
                }
                valueCache = ToValue(query);
                if (ov == valueCache)
                {
                    writeToLog("UnifyLazy: SUCCEED" + ov + " in " + query);
                    return true;
                }

                var tagHandler = GetTagHandler(query);
                if (tagHandler.CanUnify(ov) == UNIFY_TRUE)
                {
                    writeToLog("UnifyLazy: SUCCEED" + ov + " in " + query);
                    return true;
                }
                Unifiable outputSentence = tagHandler.CompleteProcess();
                string value = outputSentence.AsString();
                if (ov.ToUpper() == value.ToUpper())
                {
                    writeToLog("UnifyLazy: SUCCEED" + ov + " in " + query);
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                writeToLog("UnifyLazy ERROR! " + e);
                return false;
            }
        }

        private bool IsCachedMatch(Unifiable unifiable, SubQuery query)
        {
            return false;
        }

        private void GetPos()
        {
            throw new NotImplementedException();
        }

        //private SubQuery savedSQ;
        //AIMLTagHandler savedTagHandler;
        //public XmlNode node;
        public AIMLTagHandler GetTagHandler(SubQuery subquery)
        {
            if (valueCache is AIMLTagHandler) return (AIMLTagHandler)valueCache;
            return subquery.GetTagHandler(GetNode());
        }
        public virtual XmlNode GetNode()
        {
            if (valueCache is XmlNode) return (XmlNode)valueCache;
            try
            {
                return AIMLTagHandler.getNode(str);
            }
            catch (Exception e)
            {
                return AIMLTagHandler.getNode("<template>" + str + "</template>");
            }
        }

        public override bool IsEmpty
        {
            get
            {
                string s = str;
                if (string.IsNullOrEmpty(s)) return true;
                s = s.Trim();
                if (s.Length != 0) return false;
                writeToLog("IsEmpty: " + str);
                return true;
            }
        }

        public override void Clear()
        {
            throw new IndexOutOfRangeException();
            _str = "";
        }

        sealed public override string ToValue(SubQuery query)
        {
            if (valueCache==null) valueCache = ToValue0(query);
            return "" + valueCache;
        }
       protected string ToValue0(SubQuery query)
        {
            if (IsLitteral()) return str;
            if (str.Length < 2) return str;
            if (IsLazy())
            {
                //todo 
                if (query == null) return AsString();
                AIMLTagHandler tagHandler = GetTagHandler(query);
                Unifiable outputSentence = tagHandler.CompleteProcess();
                if (!outputSentence.IsEmpty) return outputSentence.AsString();
                writeToLog("Failed Eval " + str);
                ///bot.GetTagHandler(templateNode, subquery, request, result, request.user);
            }
            return AsString();
        }


        public override bool IsLazyStar()
        {
            if (!IsLazy()) return false;
            if (!str.StartsWith("<")) return false;
            if (str.Contains("star") || str.Contains("match="))
            {
                return true;
            }
            if (str.Contains("name="))
            {
                return true;
            }
            return false;
        }
    }
}