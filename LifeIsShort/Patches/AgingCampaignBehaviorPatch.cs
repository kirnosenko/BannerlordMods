using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;

namespace LifeIsShort.Patches
{
	[HarmonyPatch(typeof(AgingCampaignBehavior), "DailyTickHero")]
	public class DailyTickPatch
	{
		private static void Prefix(Hero hero)
		{
			HeroCollection.DailyUpdate();
		}
	}
}
