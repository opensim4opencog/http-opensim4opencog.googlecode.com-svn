using System;
using System.Collections.Generic;
using System.Threading;
using OpenMetaverse;
using OpenMetaverse.Packets;
using System.Text;

namespace cogbot.Actions
{
    public class InviteGroupCommand : Command
    {
        public InviteGroupCommand(BotClient testClient)
        {
            Name = "invitegroup";
            Description = "invite an avatar into a group. Usage: invitegroup AvatarUUID GroupUUID RoleUUID*";
            Category = CommandCategory.Groups;
        }

        public override CmdResult Execute(string[] args, UUID fromAgentID, OutputDelegate WriteLine)
        {
            if (args.Length < 2)
                return Failure(Description);

            UUID avatar = UUID.Zero;
            UUID group = UUID.Zero;
            UUID role = UUID.Zero;
            List<UUID> roles = new List<UUID>();

            if (!UUIDTryParse(args[0], out avatar))
                    return Failure( "parse error avatar UUID");
            if (!UUIDTryParse(args[1], out group))
                    return Failure( "parse error group UUID");
            for (int i = 2; i < args.Length; i++)
                if (UUID.TryParse(args[i], out role))
                    roles.Add(role);
                
            Client.Groups.Invite(group, roles, avatar);

            return Success("invited "+avatar+" to "+group);
        }
    }
}
