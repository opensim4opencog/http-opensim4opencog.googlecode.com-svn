using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;
using OpenMetaverse.Packets;

using MushDLR223.ScriptEngines;

namespace cogbot.Actions.Communication
{
    public class ShoutCommand : Command, BotPersonalCommand
    {
        public ShoutCommand(BotClient testClient)
        {
            Name = "shout";
            Description = Name + " something.";
            Usage = Htmlize.Usage(Name + " (optional channel) whatever", Name + "s whatever (optionally to channel)");
            Category = CommandCategory.Communication;
            Parameters =
                NamedParam.CreateParams(
                            NamedParam.Optional("channel", typeof(int), "the optional channel in which the message goes out"),
                            "message", typeof(string), "what you output to the simulator");
            ResultMap = NamedParam.CreateParams(
                "message", typeof(string), "if success was false, the reason why",
                "success", typeof(bool), "true if command was successful");
        }

        public override CmdResult Execute(string[] args, UUID fromAgentID, OutputDelegate WriteLine)
        {
            int channel = 0;
            int startIndex = 0;
            string message = String.Empty;
            if (args.Length < 1)
            {
                return ShowUsage();// " shout (optional channel) whatever";
            }
            else if (args.Length > 1 && (args[0].StartsWith("/") || args[0].StartsWith("#")))
            {
                if (Int32.TryParse(args[0].Substring(1), out channel))
                    startIndex = 1;
            }

            for (int i = startIndex; i < args.Length; i++)
            {
                message += args[i] + " ";
            }

            Client.Self.Chat(message, channel, ChatType.Shout);

            return Success("Shouted " + message);
        }
    }
}
