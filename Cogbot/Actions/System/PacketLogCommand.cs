using System;
using OpenMetaverse;

namespace cogbot.Actions.System
{
    public class PacketLogCommand : Command, BotSystemCommand
    {
        public PacketLogCommand(BotClient testClient)
        {
            Name = "packetlog";
            Description = "Logs a given number of packets to an xml file. Usage: packetlog 10 tenpackets.xml";
            Category = CommandCategory.BotClient;
        }

        public override CmdResult Execute(string[] args, UUID fromAgentID, OutputDelegate WriteLine)
        {
            if (args.Length != 2)
                return ShowUsage();// " packetlog 10 tenpackets.xml";

            return Success("This function is currently unimplemented");
        }
    }
}
