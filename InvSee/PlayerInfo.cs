using TShockAPI;

namespace InvSee
{
	public class PlayerInfo
	{
		public const string KEY = "InvSee_Data";

		public PlayerData Backup { get; set; }

		public string CopyingUserName { get; set; }

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
			CopyingUserName = "";
			return true;
		}
	}
}
