using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;

namespace LifeIsShort.Patches
{
	[HarmonyPatch(typeof(AgingCampaignBehavior), "DailyTick")]
	public class DailyTickPatch
	{
		private static void Postfix()
		{
			bool updateDeathProbabilities = ((int)Campaign.Current.CampaignStartTime.ElapsedDaysUntilNow % LifeIsShortConfig.Instance.OneYearOfHeroLifeInDays) == 0;
			float timeShiftDays = 1 * LifeIsShortConfig.Instance.AgeMultiplier - 1;

			if (timeShiftDays == 0 && !updateDeathProbabilities)
			{
				return;
			}

			foreach (var hero in Hero.All)
			{
				if (timeShiftDays > 0)
				{
					TimeShift(hero, timeShiftDays);
				}
				if (updateDeathProbabilities && !hero.IsDead && hero.Age > (float)Campaign.Current.Models.AgeModel.BecomeOldAge)
				{
					hero.ProbabilityOfDeath = Campaign.Current.Models.HeroDeathProbabilityCalculationModel.CalculateHeroDeathProbability(hero, null);
				}
			}
		}

		private static void TimeShift(Hero hero, float timeShiftDays)
		{
			hero.BirthDay = CampaignTime.Days((float)hero.BirthDay.ToDays - timeShiftDays);
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
