using System;
using System.Linq;
using System.Text.RegularExpressions;
using InvSee.Extensions;
using Terraria;
using TShockAPI;
using TShockAPI.DB;

namespace InvSee
{
	internal class Commands
	{
		private static readonly string _cp = TShockAPI.Commands.Specifier;

		public static void DoInvSee(CommandArgs args)
		{
			if (!Main.ServerSideCharacter)
			{
				args.Player.PluginErrorMessage("ServerSideCharacters must be enabled.");
				return;
			}

			PlayerInfo info = args.Player.GetPlayerInfo();

			if (args.Parameters.Count < 1)
			{
				if (args.Player.Dead)
				{
					args.Player.PluginErrorMessage("You cannot restore your inventory while dead.");
					return;
				}
				bool restored = info.Restore(args.Player);
				if (restored)
					args.Player.PluginErrorMessage("Inventory has been restored.");
				else
				{
					args.Player.PluginInfoMessage("You are currently not seeing anyone's inventory.");
					args.Player.PluginInfoMessage($"Use '{_cp}invsee <player name>' to begin.");
				}
			}
			else
			{
				Regex regex = new Regex(@"^\w+ (?:-(?<Saving>s(?:ave)?)|""?(?<Name>.+?)""?)$");
				Match match = regex.Match(args.Message);
				if (!String.IsNullOrWhiteSpace(match.Groups["Saving"].Value))
				{
					if (!args.Player.Group.HasPermission(Permissions.InvSeeSave))
					{
						args.Player.PluginErrorMessage("You don't have the permission to change player inventories!");
						return;
					}

					if (info.Backup == null || String.IsNullOrWhiteSpace(info.CopyingUserName))
						args.Player.PluginErrorMessage("You are not copying any user!");
					else
					{
						User user = TShock.Users.GetUserByName(info.CopyingUserName);
						TSPlayer player;
						if (user == null)
						{
							args.Player.PluginErrorMessage("Invalid user!");
							return;
						}
						else if ((player = TShock.Utils.FindPlayer(info.CopyingUserName).FirstOrDefault()) != null)
						{
							// Fixes Invsee Saving on Active Players
							args.Player.PlayerData.CopyCharacter(args.Player);
							args.Player.PlayerData.RestoreCharacter(player);
							TShock.Log.ConsoleInfo("[Online] {0} has modified {1}'s ({2}) inventory.",
								args.Player.Name, info.CopyingUserName, info.UserID);
						}
						else
						{
							try
							{
								// Only replace inventory, ignore character looks.
								// We copy our character to make sure inventory is up to date before sending it.
								args.Player.PlayerData.CopyCharacter(args.Player);
								PlayerData playerData = args.Player.PlayerData;

								string query = @"UPDATE tsCharacter
												 SET Health = @0, MaxHealth = @1, Mana = @2, MaxMana = @3,
													 Inventory = @4
												 WHERE Account = @5;";
								TShock.CharacterDB.database.Query(query, playerData.health, playerData.maxHealth,
									playerData.mana, playerData.maxMana, String.Join("~", playerData.inventory),
									info.UserID);
								TShock.Log.ConsoleInfo("[Offline] {0} has modified {1}'s ({2}) inventory.",
									args.Player.Name, info.CopyingUserName, info.UserID);
							}
							catch (Exception ex)
							{
								args.Player.PluginErrorMessage("Unable to save the player's inventory.");
								TShock.Log.Error(ex.ToString());
								return;
							}
						}
						args.Player.PluginInfoMessage($"Saved changes made to {user.Name}'s inventory.");
					}
				}
				else
				{
					string playerName = match.Groups["Name"].Value;

					PlayerData data;
					string name = "";
					int userid = 0;
					var players = TShock.Utils.FindPlayer(playerName);
					if (players.Count == 0)
					{
						if (!args.Player.Group.HasPermission(Permissions.InvSeeUser))
						{
							args.Player.PluginErrorMessage("You can't copy users!");
							return;
						}

						User user = TShock.Users.GetUserByName(playerName);
						if (user == null)
						{
							args.Player.PluginErrorMessage($"Invalid player or account '{playerName}'!");
							return;
						}
						else
						{
							data = TShock.CharacterDB.GetPlayerData(args.Player, user.ID);
							name = user.Name;
							userid = user.ID;
						}
					}
					else if (players.Count > 1)
					{
						TShock.Utils.SendMultipleMatchError(args.Player, players.Select(p => p.Name));
						return;
					}
					else
					{
						if(players[0].User == null)
						{
							args.Player.PluginErrorMessage($"Invalid player or account '{playerName}'!");
							return;
						}
						userid = players[0].User.ID;
						players[0].PlayerData.CopyCharacter(players[0]);
						data = players[0].PlayerData;
						name = players[0].User?.Name ?? "";
					}
					try
					{
						if (data == null)
						{
							args.Player.PluginErrorMessage($"{name}'s data not found!");
							return;
						}

						// Setting up backup data
						if (info.Backup == null)
						{
							info.Backup = new PlayerData(args.Player);
							info.Backup.CopyCharacter(args.Player);
						}

						info.CopyingUserName = name;
						info.UserID = userid;
						data.RestoreCharacter(args.Player);
						args.Player.PluginSuccessMessage($"Copied {name}'s inventory.");
					}
					catch (Exception ex)
					{
						// In case it fails, everything is restored
						if (info.Backup != null)
						{
							info.CopyingUserName = "";
							info.Backup.RestoreCharacter(args.Player);
							info.Backup = null;
						}
						TShock.Log.ConsoleError(ex.ToString());
						args.Player.PluginErrorMessage("Something went wrong... restored your inventory.");
					}
				}
			}
		}
	}
}