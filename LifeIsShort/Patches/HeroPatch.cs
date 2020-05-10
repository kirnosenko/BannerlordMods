using HarmonyLib;
using TaleWorlds.CampaignSystem;

namespace LifeIsShort.Patches
{
	[HarmonyPatch(typeof(Hero), "Init")]
	public class HeroInitPatch
	{
		private static void Postfix(Hero __instance)
		{
			HeroCollection.AddHero(__instance);
		}
	}

	[HarmonyPatch(typeof(Hero), "AfterLoad")]
	public class HeroAfterLoadPatch
	{
		private static void Postfix(Hero __instance)
		{
			HeroCollection.AddHero(__instance);
		}
	}
}
