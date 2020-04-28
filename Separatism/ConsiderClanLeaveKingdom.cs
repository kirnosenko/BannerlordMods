using System;
using System.Linq;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors.BarterBehaviors;

namespace Separatism
{
	[HarmonyPatch(typeof(DiplomaticBartersBehavior), "ConsiderClanLeaveKingdom")]
	public class ConsiderClanLeaveKingdom
	{
		public static bool Prefix(Clan clan)
		{
			if (clan.Settlements.Count() > 0)
			{
				if (clan.Leader == clan.Kingdom.Leader ||
					clan.Leader.HasGoodRelationWith(clan.Kingdom.Leader))
				{
					return false;
				}
			}
			
			return true;
		}
	}
}
