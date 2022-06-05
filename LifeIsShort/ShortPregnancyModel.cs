using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;

namespace LifeIsShort
{
	public class ShortPregnancyModel : DefaultPregnancyModel
	{
		public override float GetDailyChanceOfPregnancyForHero(Hero hero)
		{
			var p = base.GetDailyChanceOfPregnancyForHero(hero);
			return 1 - (float)Math.Pow(1 - p, LifeIsShortConfig.Instance.AgeMultiplier);
		}
	}
}
