using System.Runtime.CompilerServices;
using TShockAPI;

namespace InvSee.Extensions
{
	public static class TSPlayerExtensions
	{
		private static ConditionalWeakTable<TSPlayer, PlayerInfo> data = new ConditionalWeakTable<TSPlayer, PlayerInfo>();

		public static PlayerInfo GetPlayerInfo(this TSPlayer player)
		{
			return data.GetOrCreateValue(player);
		}
	}
}
