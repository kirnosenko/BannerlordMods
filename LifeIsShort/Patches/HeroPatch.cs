using HarmonyLib;
using TaleWorlds.CampaignSystem;

namespace LifeIsShort.Patches
{
	[HarmonyPatch(typeof(Hero), "Age", MethodType.Getter)]
	public class HeroPatch
	{
		public static void Postfix(Hero __instance, ref float __result)
		{
			__result = __instance.Age * ((float)84 / LifeIsShortConfig.Instance.OneYearOfHeroLifeInDays);
		}
	}
}
