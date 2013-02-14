using System;
using System.Xml;
using System.Text;
using AltAIMLParser;
using AltAIMLbot;

namespace RTParser.AIMLTagHandlers
{
    /// <summary>
    /// The space element instructs the AIML interpreter to perform all usual processing of its 
    /// contents, but to not return any value, regardless of whether the contents produce output.
    /// 
    /// The space element has no attributes. It may contain any AIML template elements.
    /// </summary>
    public class space : RTParser.Utils.AIMLTagHandler
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="bot">The bot involved in this request</param>
        /// <param name="user">The user making the request</param>
        /// <param name="query">The query that originated this node</param>
        /// <param name="request">The request inputted into the system</param>
        /// <param name="result">The result to be passed to the user</param>
        /// <param name="templateNode">The node to be processed</param>
        public space(RTParser.AltBot bot,
                        RTParser.User user,
                        RTParser.Utils.SubQuery query,
                        Request request,
                        Result result,
                        XmlNode templateNode)
            : base(bot, user, query, request, result, templateNode)
        {
        }

        protected override bool ExpandingSearchWillYieldNoExtras { get { return true; } }
        protected override Unifiable ProcessChange()
        {
            return " ";
        }
    }
}
