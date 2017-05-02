using System;
using InvSee.Extensions;
using Microsoft.Xna.Framework;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace InvSee
{
	[ApiVersion(2, 1)]
	public class PMain : TerrariaPlugin
	{
		public override string Author => "Enerdy";

		public override string Description => "Utilizes SSC technology to temporarily copy a player's inventory.";

		public override string Name => "InvSee";

		public override Version Version => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

		public static string Tag => TShock.Utils.ColorTag("InvSee:", Color.Teal);

		public PMain(Main game) : base(game)
		{
			// A lower order ensures commands are replaced properly
			Order--;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				ServerApi.Hooks.GameInitialize.Deregister(this, OnInitialize);
				ServerApi.Hooks.ServerLeave.Deregister(this, OnLeave);
				PlayerHooks.PlayerLogout -= OnLogout;
			}
		}

		public override void Initialize()
		{
			ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);
			ServerApi.Hooks.ServerLeave.Register(this, OnLeave);
			PlayerHooks.PlayerLogout += OnLogout;
		}

		void OnInitialize(EventArgs e)
		{
			Action<Command> Add = (command) =>
			{
				TShockAPI.Commands.ChatCommands.RemoveAll(c =>
				{
					foreach (string s in c.Names)
					{
						if (command.Names.Contains(s))
							return true;
					}
					return false;
				});
				TShockAPI.Commands.ChatCommands.Add(command);
			};

			Add(new Command(Permissions.InvSee, Commands.DoInvSee, "invsee")
			{
				HelpDesc = new[]
				{
					"Replaces own inventory with target player's inventory.",
					$"Use '{TShockAPI.Commands.Specifier}invsee' to reset your inventory."
				}
			});
		}

		void OnLeave(LeaveEventArgs e)
		{
			if (e.Who < 0 || e.Who > Main.maxNetPlayers)
				return;

			TSPlayer player = TShock.Players[e.Who];
			if (player != null)
			{
				PlayerInfo info = player.GetPlayerInfo();
				info.Restore(player);
			}
		}

		void OnLogout(PlayerLogoutEventArgs e)
		{
			if (e.Player == null || !e.Player.Active || !e.Player.RealPlayer)
				return;

			e.Player.GetPlayerInfo().Restore(e.Player);
		}
	}
}
