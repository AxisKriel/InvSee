using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InvSee.Extensions;
using TShockAPI;
using TShockAPI.DB;
using Terraria;

namespace InvSee
{
	internal class Commands
	{
		public static void DoInvSee(CommandArgs args)
		{
			if (!Main.ServerSideCharacter)
			{
				args.Player.SendErrorMessage("ServerSideCharacters must be enabled.");
				return;
			}

			PlayerInfo info = args.Player.GetPlayerInfo();

			if (args.Parameters.Count < 1)
			{
				bool restored = info.Restore(args.Player);

				if (restored)
					args.Player.SendSuccessMessage("[InvSee] Restored your inventory.");
				else
				{
					args.Player.SendInfoMessage("[InvSee] You are currently not seeing anyone's inventory.");
					args.Player.SendInfoMessage("[InvSee] Use '{0}invsee <player name>' to begin.", TShock.Config.CommandSpecifier);
				}
			}
			else
			{
				string playerName = string.Join(" ", args.Parameters);
				var playerList = TShock.Utils.FindPlayer(playerName);

				if (playerList == null || playerList.Count < 1)
					args.Player.SendErrorMessage("Invalid player!");
				else if (playerList.Count > 1)
					TShock.Utils.SendMultipleMatchError(args.Player, playerList);
				else
				{
					try
					{
						// Setting up backup data
						if (info.Backup == null)
						{
							info.Backup = new PlayerData(args.Player);
							info.Backup.CopyCharacter(args.Player);
						}

						TSPlayer player = playerList[0];
						player.PlayerData.RestoreCharacter(args.Player);
						args.Player.SendSuccessMessage("[InvSee] Copied {0}'s inventory.", player.Name);
					}
					catch (Exception ex)
					{
						// In case it fails, everything is restored
						if (info.Backup != null)
						{
							info.Backup.RestoreCharacter(args.Player);
							info.Backup = null;
						}
						Log.ConsoleError(ex.ToString());
						args.Player.SendErrorMessage("[InvSee] Something went wrong... restored your inventory.");
					}
				}
			}
		}
	}
}
