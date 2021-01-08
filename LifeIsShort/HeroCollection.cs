using System.Collections.Generic;
using TaleWorlds.CampaignSystem;

namespace LifeIsShort
{
	public static class HeroCollection
	{
		private static HashSet<Hero> heroes = new HashSet<Hero>();

		public static void Clear()
		{
			heroes.Clear();
		}
		public static void AddHero(Hero hero)
		{
			heroes.Add(hero);
		}
		public static void DailyUpdate()
		{
			bool updateDeathProbabilities = ((int)Campaign.Current.CampaignStartTime.ElapsedDaysUntilNow % LifeIsShortConfig.Instance.OneYearOfHeroLifeInDays) == 0;
			float timeShiftDays = 1 * LifeIsShortConfig.Instance.AgeMultiplier - 1;

			if (timeShiftDays == 0 && !updateDeathProbabilities)
			{
				return;
			}

			foreach (var hero in heroes)
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
			hero.SetBirthDay(CampaignTime.Days((float)hero.BirthDay.ToDays - timeShiftDays));
		}
	}
}
