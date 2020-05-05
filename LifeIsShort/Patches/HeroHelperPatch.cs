using System;
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
			float ageYears = Math.Max(age - 1, 0);
			float ageDays = MBRandom.RandomFloatRanged(1f, LifeIsShortConfig.Instance.OneYearOfHeroLifeInDays);

			float birthDaysAgo = ageYears * LifeIsShortConfig.Instance.OneYearOfHeroLifeInDays + ageDays;
			__result = CampaignTime.Days((float)CampaignTime.Now.ToDays - birthDaysAgo);
		}
	}
}
