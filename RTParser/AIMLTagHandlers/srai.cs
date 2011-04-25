using System;
using System.Collections.Generic;
using System.Threading;
using System.Xml;
using System.Text;
using System.IO;
using AIMLbot;
using MushDLR223.ScriptEngines;
using RTParser.Utils;

namespace RTParser.AIMLTagHandlers
{
    /// <summary>
    /// The srai element instructs the AIML interpreter to pass the result of processing the contents 
    /// of the srai element to the AIML matching loop, as if the input had been produced by the user 
    /// (this includes stepping through the entire input normalization process). The srai element does 
    /// not have any attributes. It may contain any AIML template elements. 
    /// 
    /// As with all AIML elements, nested forms should be parsed from inside out, so embedded srais are 
    /// perfectly acceptable. 
    /// </summary>
    public class srai : RTParser.Utils.AIMLTagHandler
    {
        public static bool UseSraiLimiters = false;
        RTParser.RTPBot mybot;
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="bot">The bot involved in this request</param>
        /// <param name="user">The user making the request</param>
        /// <param name="query">The query that originated this node</param>
        /// <param name="request">The request inputted into the system</param>
        /// <param name="result">The result to be passed to the user</param>
        /// <param name="templateNode">The node to be processed</param>
        public srai(RTParser.RTPBot bot,
                        RTParser.User user,
                        RTParser.Utils.SubQuery query,
                        RTParser.Request request,
                        RTParser.Result result,
                        XmlNode templateNode)
            : base(bot, user, query, request, result, templateNode)
        {
            mybot = bot;
            IsDeterministic = false;
        }

        public override void writeToLog(string unifiable, params object[] objs)
        {
            if (Parent != null && Parent != this)
            {
                Parent.writeToLog(unifiable, objs);
            }
            else
            {
                base.writeToLog(unifiable, objs);
            }
            if (false && SafeFormat(unifiable,objs).Contains("&"))
            {
                base.writeToLog("!ERRROR");
            }
        }

        public static bool UseOriginalProcess = false;
        public bool KnowsCanProcess;

#if false
        // unsuesd/... just code refernce
        protected string OriginalProcessChange()
        {
            if (this.templateNode.Name.ToLower() == "srai")
            {
                if (this.templateNode.InnerText.Length > 0)
                {
                    Request subRequest = new AIMLbot.MasterRequest(this.templateNode.InnerText, this.user,
                                                                   (AIMLbot.Bot) this.Proc, null, null);
                    subRequest.StartedOn = this.request.StartedOn; // make sure we don't keep adding time to the request
                    Result subQuery = this.Proc.ChatWithRequest(subRequest);
                    this.request.WhyComplete = subRequest.WhyComplete;
                    subRequest.IsToplevelRequest = false;
                    return subQuery.Output;
                }
            }
            return string.Empty;
        }
#endif

        protected override Unifiable ProcessChange()
        {
            if (RecurseResultValid) return RecurseResult;
            if (InUnify)
            {
                return Unifiable.INCOMPLETE;
            }
            IsStarted = true;
            if (!RecurseResultValid)
            {
                try
                {
                    user.Enter(this);
                   //int userDepth = user.depth;
                    var sraiDepth = request.SraiDepth;
                    if (request.SraiDepth.IsOverMax)
                    {
                        query.prefix = string.Format("{0}: SRAIDEPTH({1})", request.Graph, sraiDepth);
                        writeToLog("WARNING Depth pretty deep " + templateNode);
                        if (UseSraiLimiters)
                        {
                            writeToLog(" returning " + RecurseResult);
                            return RecurseResult;
                        }
                    }
                    var vv = /*UseOriginalProcess  ? (Unifiable)OriginalProcessChange() : */ ProcessChange0();
                    if (!IsNullOrEmpty(vv))
                    {
                        RecurseResult = vv;
                        templateNode.InnerXml = MushDLR223.Utilities.StaticXMLUtils.XmlValueSettable(vv);
                        return vv;
                    }
                    if (!IsNull(vv))
                    {
                        if (IsEMPTY(vv))
                        {
                            vv = GetTemplateNodeInnerText();
                            return vv;
                        }
                        RecurseResult = vv;
                        templateNode.InnerXml = MushDLR223.Utilities.StaticXMLUtils.XmlValueSettable(vv);
                        return vv;
                    }
                    return vv;
                }
                catch (ChatSignal ex)
                {
                    throw;
                }
                catch (Exception e)
                {
                    writeToLogWarn("" + e);
                }
                finally
                {
                    user.Exit(this); 
                }
            }
            return RecurseResult;
        }

        private const bool ProcessChange12 = true;
        public override Unifiable CompleteProcess()
        {
            if (RecurseResultValid) return RecurseResult;
            if (InUnify)
            {
                return Unifiable.INCOMPLETE;
            }
            // ReSharper disable ConditionIsAlwaysTrueOrFalse
            var sraiResult = ProcessChange12 ? ProcessChange0() : ProcessAimlChange();
            // ReSharper restore ConditionIsAlwaysTrueOrFalse
            if (IsNull(sraiResult))
            {
                return sraiResult;
                ResetValues(true);
                sraiResult = base.CompleteProcess();
                if (RecurseResultValid)
                {
                    return RecurseResult;
                }
                writeToLogWarn("srai.CompleteProcess() == NULL!");
                return sraiResult;
            }
            return sraiResult;
            // return base.CompleteProcess();
        }

        public override string Transform()
        {
            if (RecurseResultValid) return RecurseResult;
            var vv = ProcessChange0();
            return vv;
        }/*
        public override Unifiable RecurseProcess()
        {
            return ProcessChange();
        }
        */
        protected Unifiable ProcessChange0()
        {
            string s;
            if (RecurseResultValid) return RecurseResult;
            if (CheckNode("srai"))
            {
                bool chatTraced = Proc.chatTrace;
                Proc.chatTrace = false;
                try
                {
                    var templateNodeInnerValue = Recurse();
                    if (!KnowsCanProcess)
                    {
                        KnowsCanProcess = true;
                        string toUpper = MakeMatchable(templateNodeInnerValue);
                        var rp = request.ParentRequest;
                        if (rp != null && !rp.CanProcess(toUpper)) return null;
                    }
                    if (false &&  IsNull(templateNodeInnerValue))
                    {
                        templateNodeInnerValue = Recurse();
                    }
                    TemplateInfo queryTemplate = query.CurrentTemplate;
                    if (queryTemplate != null)
                    {
                        if (!result.CanResultUseTemplate(queryTemplate))
                        {
                            writeToLogWarn("!result.CanResultUseTemplate " + queryTemplate);
                            return Unifiable.INCOMPLETE;
                        }
                        if (!request.CanUseRequestTemplate(queryTemplate))
                        {
                            writeToLogWarn("!request.CanUseRequestTemplate " + queryTemplate);
                            return Unifiable.INCOMPLETE;
                        }
                    }
                    templateNodeInnerValue = Proc.CleanupCyc(templateNodeInnerValue);
                    var vv = ProcessChangeSrai(request, query, templateNodeInnerValue, templateNode, initialString, writeToLog);
                    if (!Unifiable.IsNullOrEmpty(vv))
                    {
                        return vv;
                    }
                    if (Unifiable.IsNull(vv))
                    {
                        writeToLogWarn("NULL SRAI!?!");
                    }
                    else
                    {
                        if (IsSilentTag(templateNode.ChildNodes))
                        {
                            return Unifiable.Empty;
                        }
                        return vv; // Empty
                    }
                    if (ProcessChange12) return null;
                    if (Unifiable.IsNull(vv))
                    {
                        vv = GetTemplateNodeInnerText();
                        return FAIL;
                    }
                    return vv;
                }
                catch (ChatSignal ex)
                {
                    throw;
                }
                catch (Exception e)
                {
                    writeToLog("ERROR: " + e);
                    throw;
                }
                finally
                {
                    Proc.chatTrace = chatTraced;
                }
            }
            return Unifiable.Empty;
        }

        static internal Unifiable ProcessChangeSrai(Request request, SubQuery query,
            Unifiable templateNodeInnerValue, XmlNode templateNode, string initialString, OutputDelegate writeToLog)
        {
            if (IsNullOrEmpty(templateNodeInnerValue))
            {
                writeToLog("ERROR BAD REQUEST " + request);
                return templateNodeInnerValue;
            }
            var salientRequest = MasterRequest.GetOriginalSalientRequest(request);
            try
            {
                Unifiable prevResult;
                var CurrentTemplate = query.CurrentTemplate;
                if (!salientRequest.EnterSalientSRAI(templateNodeInnerValue, out prevResult))
                {
                    writeToLog("ERROR EnterSailentSRAI: " + prevResult);
                    if (true)
                    {
                        var disable = CurrentTemplate;
                        if (disable != null && disable.IsDisabled)
                        {
                            request.CurrentResult.ResultTemplates.Add(disable);
                            disable.IsDisabled = true;
                            request.AddUndo("undisable loop " + disable.ToFileString(request.Requester.PrintOptions), () => { disable.IsDisabled = false; });
                        }
                    }
                    //return null;
                }
                int depth = request.SraiDepth.Current;
                if (!UseSraiLimiters) depth = 0;
                if (CurrentTemplate != null)
                {
                    if (CurrentTemplate.IsHighlyUsefull)
                    {
                        writeToLog("IsHighlyUsefull: " + CurrentTemplate.ToFileString(request.Requester.PrintOptions));
                        request.SuspendSearchLimits = true;
                        request.depth = 0;
                    }
                }
                Unifiable subResultOutput = null;
                writeToLog = writeToLog ?? DEVNULL;
                RTPBot mybot = request.TargetBot;
                User user = request.Requester;
                var thisrequest = request;
                var thisresult = request.CurrentResult;
                /*
                writeToLog("WARNING Depth pretty deep " + templateNode + " returning empty");
                */
                //string s;
                //if (ResultReady(out s)) return s;
                /*
                int d = request.GetCurrentDepth();
                if (d > request.SraiDepth.Max)
                {
                    writeToLog(prefix + " WARNING Depth pretty deep " + templateNode + " returning empty");
                    return Unifiable.Empty;
                }
                 */
                //if (CheckNode("srai"))
                {
                    string prefix =
                        query.prefix =
                        SafeFormat("ProcessChangeSrai: {0}: \"{1}\"\n", request.Graph, Unifiable.DescribeUnifiable(templateNodeInnerValue));

                    if (request.SraiDepth.IsOverMax && UseSraiLimiters)
                    {
                        string sss = prefix + " request.SraiDepth.IsOverMax '" + request.SraiDepth.Current + "'";
                        writeToLog(sss);
                        if (!request.SuspendSearchLimits)
                        {
                            throw new ChatSignalOverBudget(request, sss);
                            return Unifiable.INCOMPLETE;
                        }
                    }
                    string why = request.WhyComplete;
                    if (why != null && UseSraiLimiters)
                    {
                        string sss = prefix + " " + why;
                        writeToLog(sss);
                        if (!request.SuspendSearchLimits)
                        {
                            throw new ChatSignalOverBudget(request, sss);
                            return Unifiable.INCOMPLETE;
                        }
                    }
                    //Unifiable templateNodeInnerValue = Recurse();
                    try
                    {
                        Request subRequest = request.CreateSubRequest(templateNodeInnerValue, null);


                        string requestGraphSrai = request.SraiGraph;
                        subRequest.Graph = request.GetGraph(requestGraphSrai);

                        var ti = query.CurrentTemplate;
                        // make sure we veto later use of this template
                        if (ti != null) subRequest.RequestTemplates.Add(ti);

                        // make sure we don't keep adding time to the request
                        bool showDebug = DebugSRAIs;
                        string subRequestrawInput = subRequest.rawInput;
                        if (showDebug && subRequestrawInput.Contains("SYSTEMANIM") ||
                            subRequestrawInput.Contains("HANSANIM"))
                        {
                            showDebug = false;
                        }

                        if (showDebug)
                            writeToLog(prefix + " CALLING '" + subRequestrawInput + "'");


                        if (mybot.chatTrace)
                        {
                            //  mybot.bot.writeChatTrace("\"L{0}\" -> \"S{1}\" ;\n", depth - 1, depth-1);
                            // mybot.bot.writeChatTrace("\"L{0}\" -> \"S{1}\" ;\n", depth, depth);
                            //mybot.bot.writeChatTrace("\"S{0}\" -> \"SRC:{1}\" ;\n", depth, CatTextInfo());

                            //mybot.bot.writeChatTrace("\"S{0}\" -> \"S{1}\" ;\n", depth - 1, depth);
                            //mybot.bot.writeChatTrace("\"S{0}\" -> \"SIN:{1}\" ;\n", depth, subRequestrawInput);
                            //mybot.bot.writeChatTrace("\"SIN:{0}\" -> \"LN:{1}\" ;\n", subRequestrawInput, CatTextInfo());
                        }
                        if (depth > 200 && UseSraiLimiters)
                        {
                            writeToLog(prefix + " FAILING TOOOO DEEEEP '" + request + "'");
                            return Unifiable.INCOMPLETE;
                        }

                        AIMLbot.MasterResult subResult;
                        string subQueryRawOutputText;
                        subResult = GetSubResult(prefix, request, user, mybot, (MasterRequest) subRequest, showDebug,
                                                 out subResultOutput,
                                                 out subQueryRawOutputText, writeToLog);


                        string whyComplete = thisrequest.WhyComplete = subRequest.WhyComplete;
                        string subResultOutputTrace = Unifiable.DescribeUnifiable(subResultOutput);
                        if (Unifiable.IsNull(subResultOutput))
                        {
                            if (showDebug)
                            {
                                writeToLog(prefix + "MISSING RETURN " + whyComplete);
                            }
                            subResult = (MasterResult)mybot.ChatFor1Result(subRequest, subResult);
                            subResultOutput = subResult.Output;
                            subResultOutputTrace = Unifiable.DescribeUnifiable(subResultOutput);
                            //subQueryRawOutput = subResult.RawOutput.Trim();
                            if (!IsNullOrEmpty(subResultOutput))
                            {
                                writeToLog(prefix + "RESCUED RETURN " + subResultOutput);
                                //  subQueryRawOutput = "" + subResultOutput;
                            }
                            // This is the failure cases
                            if (Unifiable.IsNull(subResultOutput))
                            {
                                if (mybot.chatTrace)
                                {
                                    ShowChatTrace(subRequestrawInput, mybot, depth, subResult, templateNode);
                                }
                                request.IsTraced = true;
                                why = subRequest.WhyComplete ?? "ERRORY";
                                writeToLog("{0} NULL?! RETURN {1} {2} '{3}'", why + ": " + prefix, subRequestrawInput,
                                           subResult.Score, subResultOutputTrace);
                                return subResultOutput;
                            }
                        }
                        if (Unifiable.IsEMPTY(subResultOutput))
                        {
                            request.IsTraced = true;                   
                            why = subRequest.WhyComplete ?? "ERRORY";
                            writeToLog("{0} EMPTY?! RETURN {1} {2} '{3}'", why + ": " + prefix, subRequestrawInput,
                                       subResult.Score, subResultOutputTrace);
                            return subResultOutput;
                        }
                        {

                            if (subRequestrawInput.Contains("STDCATCHALL STDCATCHALL"))
                            {
                                // @TODO @debug this
                                throw new InvalidCastException("loop STDCATCHALL STDCATCHALL");
                            }
                            // ReSharper disable ConditionIsAlwaysTrueOrFalse
                            if (query != null)
                            // ReSharper restore ConditionIsAlwaysTrueOrFalse
                            {
                                double before = query.Request.TopLevelScore;
                                if (query.CurrentTemplate != null)
                                {
                                    query.Request.TopLevelScore *= (subResult.Score*query.CurrentTemplate.TemplateRating);
                                    if (showDebug)
                                    {
                                        writeToLog("SCORE {0}*{1}*{2}->{3} ",
                                                   before, subResult.Score, query.CurrentTemplate.TemplateRating,
                                                   query.Request.TopLevelScore);
                                        writeToLog("{0} SUCCESS RETURN {1}  {2} '{3}'", prefix, subRequestrawInput,
                                                   subResult.Score, subResultOutputTrace);
                                    }

                                }
                            }
                            thisrequest.AddSubResult(thisresult);
                            thisrequest.AddSubResult(subResult);
                        }

                        if (mybot.chatTrace)
                        {
                            ShowChatTrace(subRequestrawInput, mybot, depth, subResult, templateNode);
                        }
                        //salientRequest.ExitSalientSRAI(templateNodeInnerValue, subResultOutput);
                        return subResultOutput;
                    }
                    finally
                    {
                        if (subResultOutput != null)
                        {
                            salientRequest.ExitSalientSRAI(templateNodeInnerValue, subResultOutput);
                        }
                        // @TODO @HACK @BUG stops loops?
                        request.DisableTemplateUntilFinished(CurrentTemplate);
                    }
                }
                return Unifiable.Empty;
            }
            finally
            {
                //depth--;
            }
        }

        private static void ShowChatTrace(string subRequestrawInput, RTPBot mybot, int depth, MasterResult subResult, XmlNode templateNode)
        {
            mybot.writeChatTrace("\"L{0}\" -> \"S{1}\" ;\n", depth, depth);
            mybot.writeChatTrace("\"S{0}\" -> \"SIN:{1}\" ;\n", depth, subRequestrawInput);
            //mybot.writeChatTrace("\"SIN:{0}\" -> \"LN:{1}\" ;\n", subRequestrawInput, CatTextInfo());
            mybot.writeChatTrace("\"SIN:{0}\" -> \"PATH:{1}\" [label=\"{2}\"] ;\n",
                                 subRequestrawInput, depth, subResult.NormalizedPaths);
            mybot.writeChatTrace("\"PATH:{0}\" -> \"LN:{1}\" [label=\"{2}\"] ;\n", depth, depth,
                                 AIMLLoader.TextAndSourceInfo(templateNode));

            mybot.writeChatTrace("\"LN:{0}\" -> \"RPY:MISSING({1})\" ;\n", depth, depth);
        }

        static MasterResult GetSubResult(String prefix, Request prevRequest, User user, RTPBot mybot, MasterRequest subRequest, bool showDebug, out Unifiable subResultOutput, out  string subQueryRawOutput1, OutputDelegate writeToLog)
        {
            var prev = subRequest.GraphsAcceptingUserInput;
            var prevSO = user.SuspendAddResultToUser;
            MasterResult subResult = subRequest.CreateResult(subRequest);
            try
            {
                Dictionary<Unifiable, Unifiable> sraiMark = null;
                var originalSalientRequest = MasterRequest.GetOriginalSalientRequest(prevRequest);
                if (UseSraiLimiters)
                {
                    sraiMark = originalSalientRequest.CreateSRAIMark();
                }
                subRequest.GraphsAcceptingUserInput = true;
                //var newresult = new AIMLbot.Result(request.user, Proc, request);
                //subRequest.result = newresult;
                user.SuspendAddResultToUser = true;
                if (prevRequest.IsTraced) subRequest.IsTraced = !showDebug;
                subRequest.IsTraced = true;
                subResult = (MasterResult) mybot.ChatWithToplevelResults(subRequest,subResult);
                subResultOutput = subResult.RawOutput;
                int resultCount = subResult.OutputSentences.Count;
                if (RTPBot.BE_COMPLETE_NOT_FAST && resultCount == 0)
                {
                    subRequest.ResetValues(false);
                    if (UseSraiLimiters) originalSalientRequest.ResetSRAIResults(sraiMark);
                    if (Unifiable.IsNullOrEmpty(subResultOutput))
                    {
                        subResult = (MasterResult)mybot.ChatFor1Result(subRequest, subResult); 
                        subResultOutput = subResult.Output;
                        if (!IsNullOrEmpty(subResultOutput))
                        {
                            writeToLog(prefix + "RESCUED RETURN " + subResultOutput);
                          //  subQueryRawOutput = "" + subResultOutput;
                        }
                    }
                }
            }
            finally
            {
                user.SuspendAddResultToUser = prevSO;
                subRequest.GraphsAcceptingUserInput = prev;
            }
            subQueryRawOutput1 = subResultOutput;//.Trim();
            return subResult;
        }
    }
}