using System.Linq;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors.BarterBehaviors;

namespace Separatism.Patches
{
	[HarmonyPatch(typeof(DiplomaticBartersBehavior), "ConsiderClanJoin")]
	public static class ConsiderClanJoinPatch
	{
		public static bool Prefix(Clan clan, Kingdom kingdom)
		{
			if (clan.Leader.IsEnemy(kingdom.Leader))
			{
				return false;
			}

			return true;
		}
	}

	[HarmonyPatch(typeof(DiplomaticBartersBehavior), "ConsiderClanLeaveKingdom")]
	public static class ConsiderClanLeaveKingdomPatch
	{
		public static bool Prefix(Clan clan)
		{
			if (clan.Leader == clan.Kingdom.Leader ||
				(clan.Settlements.Count() > 0 && clan.Leader.HasGoodRelationWith(clan.Kingdom.Leader)))
			{
				return false;
			}

			return true;
		}
	}

	[HarmonyPatch(typeof(DiplomaticBartersBehavior), "ConsiderDefection")]
	public static class ConsiderDefectionPatch
	{
		public static bool Prefix(Clan clan1, Kingdom kingdom)
		{
			if (clan1.Leader == clan1.Kingdom.Leader ||
				((clan1.Settlements.Count() > 0) &&
					(clan1.Kingdom == kingdom ||
					clan1.Leader.HasGoodRelationWith(clan1.Kingdom.Leader) ||
					clan1.Leader.IsEnemy(kingdom.Leader) ||
					!clan1.CloseKingdoms().Contains(kingdom))))
			{
				return false;
			}

			return true;
		}
	}
}
