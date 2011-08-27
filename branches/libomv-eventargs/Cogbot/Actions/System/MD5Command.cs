using System;
using OpenMetaverse;

namespace cogbot.Actions
{
    public class MD5Command : Command, SystemApplicationCommand
    {
        public MD5Command(BotClient testClient)
        {
            Name = "md5";
            Description = "Creates an MD5 hash from a given password. Usage: md5 [password]";
            Category = CommandCategory.Other;
        }

        public override CmdResult Execute(string[] args, UUID fromAgentID, OutputDelegate WriteLine)
        {
            if (args.Length == 1)
                return Success(Utils.MD5(args[0]));
            else
                return ShowUsage();// " md5 [password]";
        }
    }
}