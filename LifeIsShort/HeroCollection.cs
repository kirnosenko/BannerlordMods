﻿using System.Collections.Generic;
using TaleWorlds.CampaignSystem;

namespace LifeIsShort
{
	public static class HeroCollection
	{
		private static HashSet<Hero> heroes = new HashSet<Hero>();
		private static int timestamp;

		public static void Clear()
		{
			heroes.Clear();
			timestamp = 0;
		}
		public static void AddHero(Hero hero)
		{
			heroes.Add(hero);
		}
		public static void DailyUpdate()
		{
			var newTimestamp = (int)CampaignTime.Now.ToDays;
			if (newTimestamp == timestamp)
			{
				return;
			}
			timestamp = newTimestamp;

			float timeShiftDays = 1 * LifeIsShortConfig.Instance.AgeMultiplier - 1;

			if (timeShiftDays == 0)
			{
				return;
			}

			foreach (var hero in heroes)
			{
				if (timeShiftDays > 0)
				{
					TimeShift(hero, timeShiftDays);
				}
			}
		}

		private static void TimeShift(Hero hero, float timeShiftDays)
		{
			hero.SetBirthDay(CampaignTime.Days((float)hero.BirthDay.ToDays - timeShiftDays));
		}
	}
}
