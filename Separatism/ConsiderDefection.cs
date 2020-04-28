using System;
using System.Linq;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors.BarterBehaviors;

namespace Separatism
{
	[HarmonyPatch(typeof(DiplomaticBartersBehavior), "ConsiderDefection")]
	public class ConsiderDefection
	{
		public static bool Prefix(Clan clan1, Kingdom kingdom)
		{
			if (clan1.Settlements.Count() > 0)
			{
				if (clan1.Kingdom == kingdom ||
					clan1.Leader == clan1.Kingdom.Leader ||
					clan1.Leader.HasGoodRelationWith(clan1.Kingdom.Leader) ||
					clan1.Leader.IsEnemy(kingdom.Leader) ||
					!clan1.CloseKingdoms().Contains(kingdom))
				{
					return false;
				}
			}
			
			return true;
		}
	}
}
