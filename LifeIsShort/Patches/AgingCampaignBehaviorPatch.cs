using HarmonyLib;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;

namespace LifeIsShort.Patches
{
	[HarmonyPatch(typeof(AgingCampaignBehavior), "DailyTick")]
	public class DailyTickPatch
	{
		private static void Postfix()
		{
			HeroCollection.DailyUpdate();
		}
	}

	[HarmonyPatch(typeof(AgingCampaignBehavior), "WeeklyTick")]
	public class WeeklyTickPatch
	{
		private static bool Prefix()
		{
			return false;
		}
	}
}
