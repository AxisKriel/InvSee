using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InvSee.Extensions;
using TShockAPI;
using TShockAPI.DB;
using Terraria;
using System.Text.RegularExpressions;

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
				Regex regex = new Regex(@"^\w+ (.+)$");
				Match match = regex.Match(args.Message);
				string playerName = match.Groups[1].Value;

				int acctid = 0;
				string name = "";
				var players = TShock.Utils.FindPlayer(playerName);
				if (players.Count == 0)
				{
					if (!args.Player.Group.HasPermission(Permissions.InvSeeUser))
					{
						args.Player.SendErrorMessage("You can't copy users!");
						return;
					}

					User user = TShock.Users.GetUserByName(playerName);
					if (user == null)
					{
						args.Player.SendErrorMessage("Invalid player or account '{0}'!", playerName);
						return;
					}
					else
					{
						acctid = user.ID;
						name = user.Name;
					}
				}
				else if (players.Count > 1)
				{
					TShock.Utils.SendMultipleMatchError(args.Player, players);
					return;
				}
				else
				{
					acctid = players[0].UserID;
					name = players[0].Name;
				}

				try
				{
					// Setting up backup data
					if (info.Backup == null)
					{
						info.Backup = new PlayerData(args.Player);
						info.Backup.CopyCharacter(args.Player);
					}

					PlayerData data = TShock.CharacterDB.GetPlayerData(args.Player, acctid);
					if (data == null)
					{
						args.Player.SendErrorMessage("{0}'s data not found!", name);
						return;
					}
					data.RestoreCharacter(args.Player);
					args.Player.SendSuccessMessage("[InvSee] Copied {0}'s inventory.", name);
				}
				catch (Exception ex)
				{
					// In case it fails, everything is restored
					if (info.Backup != null)
					{
						info.Backup.RestoreCharacter(args.Player);
						info.Backup = null;
					}
					TShock.Log.ConsoleError(ex.ToString());
					args.Player.SendErrorMessage("[InvSee] Something went wrong... restored your inventory.");
				}
			}
		}
	}
}
