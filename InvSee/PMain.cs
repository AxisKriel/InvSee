using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InvSee.Extensions;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace InvSee
{
	[ApiVersion(1, 17)]
    public class PMain : TerrariaPlugin
    {
		public override string Author
		{
			get { return "Enerdy"; }
		}

		public override string Description
		{
			get { return "Utilizes SSC technology to temporarily copy a player's inventory."; }
		}

		public override string Name
		{
			get { return "InvSee"; }
		}

		public override Version Version
		{
			get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version; }
		}

		public PMain(Main game)
			: base(game)
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
			}
		}

		public override void Initialize()
		{
			ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);
			ServerApi.Hooks.ServerLeave.Register(this, OnLeave);
		}

		void OnInitialize(EventArgs e)
		{
			Action<Command> Add = (command) =>
				{
					TShockAPI.Commands.ChatCommands.RemoveAll(c =>
						c.Name.Equals(command.Name, StringComparison.OrdinalIgnoreCase));
					TShockAPI.Commands.ChatCommands.Add(command);
				};

			Add(new Command(Permissions.InvSee, Commands.DoInvSee, "invsee")
				{
					HelpDesc = new[]
					{
						"Replaces own inventory with target player's inventory.",
						"Use '{0}invsee' to reset your inventory.".SFormat(TShock.Config.CommandSpecifier)
					}
				});
		}

		void OnLeave(LeaveEventArgs e)
		{
			if (e.Who < 0 || e.Who > Main.maxPlayers)
				return;

			TSPlayer player = TShock.Players[e.Who];
			if (player != null)
			{
				PlayerInfo info = player.GetPlayerInfo();
				info.Restore(player);
			}
		}
    }
}
