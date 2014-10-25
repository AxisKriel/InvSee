using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TShockAPI;

namespace InvSee
{
	public class PlayerInfo
	{
		public PlayerData Backup { get; set; }

		public PlayerInfo()
		{
			Backup = null;
		}

		public bool Restore(TSPlayer player)
		{
			if (Backup == null)
				return false;

			Backup.RestoreCharacter(player);
			Backup = null;
			return true;
		}
	}
}
