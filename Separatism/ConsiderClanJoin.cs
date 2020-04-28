using System;
using System.Linq;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors.BarterBehaviors;

namespace Separatism
{
	[HarmonyPatch(typeof(DiplomaticBartersBehavior), "ConsiderClanJoin")]
	public class ConsiderClanJoin
	{
		public static bool Prefix(Clan clan, Kingdom kingdom)
		{
			if (clan.Settlements.Count() > 0 && clan.Leader.IsEnemy(kingdom.Leader))
			{
				return false;
			}
			
			return true;
		}
	}
}
