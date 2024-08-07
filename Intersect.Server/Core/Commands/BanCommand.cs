﻿using Intersect.Server.Database.PlayerData;
using Intersect.Server.Localization;
using Intersect.Server.Networking;

namespace Intersect.Server.Core.Commands
{

    internal sealed partial class BanCommand : ModeratorActionCommand
    {

        public BanCommand() : base(
            Strings.Commands.Ban, Strings.Commands.Arguments.TargetBan, Strings.Commands.Arguments.DurationBan,
            Strings.Commands.Arguments.IpBan, Strings.Commands.Arguments.ReasonBan
        )
        {
        }

        protected override void HandleClient(ServerContext context, Client target, int duration, bool ip, string reason)
        {
            if (target.Entity == null)
            {
                Console.WriteLine($@"    {Strings.Player.Offline}");

                return;
            }

            // TODO: Refactor the global/console messages into ModeratorActionCommand
            var name = target.Entity.Name;
            if (string.IsNullOrEmpty(Ban.CheckBan(target.User, "")))
            {
                Ban.Add(target, duration, reason, Strings.Commands.banuser, ip ? target.Ip : "");
                target.Disconnect();
                PacketSender.SendGlobalMsg(Strings.Account.Banned.ToString(name));
                Console.WriteLine($@"    {Strings.Account.Banned.ToString(name)}");
            }
            else
            {
                Console.WriteLine($@"    {Strings.Account.AlreadyBanned.ToString(name)}");
            }
        }

    }

}
