using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using Helpers;

namespace LifeIsShort.Patches
{
	[HarmonyPatch(typeof(HeroHelper), "GetRandomBirthDayForAge")]
	public class HeroHelperPatch
	{
		public static void Postfix(float age, ref CampaignTime __result)
		{
			float num = MBRandom.RandomFloatRanged(1f, 84f);
			float num2 = (float)CampaignTime.Now.GetYear - age;
			if (num > (float)CampaignTime.Now.GetDayOfYear)
			{
				num2 -= 1f;
			}
			__result = CampaignTime.Days(num) + CampaignTime.Years(num2);
		}
	}
}
