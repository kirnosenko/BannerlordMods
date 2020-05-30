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
			// prevent joining to enemy
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
			// prevent stupid things
			if (clan.Leader == clan.Kingdom.Leader)
			{
				return false;
			}
			// prevent unwise leaving for lord with fiefs 
			if (clan.Settlements.Count() > 0 && clan.Leader.HasGoodRelationWith(clan.Kingdom.Leader))
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
			// prevent stupid things
			if (clan1.Leader == clan1.Kingdom.Leader || clan1.Kingdom == kingdom)
			{
				return false;
			}
			// prevent unwise defection for lord with fiefs 
			if ((clan1.Settlements.Count() > 0) &&
				(clan1.Leader.HasGoodRelationWith(clan1.Kingdom.Leader) ||
				clan1.Leader.IsEnemy(kingdom.Leader) ||
				!clan1.CloseKingdoms().Contains(kingdom)))
			{
				return false;
			}
			// prevent defection of supporting clan while kingdom have a chance
			if (clan1.Leader.HasGoodRelationWith(clan1.Kingdom.Leader) &&
				clan1.Kingdom.Settlements.Count() > 0 &&
				clan1.Kingdom.Clans.Where(x => !x.IsUnderMercenaryService).Count() <= 2)
			{
				return false;
			}

			return true;
		}
	}
}
