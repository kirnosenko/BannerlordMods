using TaleWorlds.CampaignSystem.SandBox.GameComponents;

namespace LifeIsShort
{
	public class ShortPregnancyModel : DefaultPregnancyModel
	{
		public override float PregnancyDurationInDays
		{
			get
			{
				return base.PregnancyDurationInDays / LifeIsShortConfig.Instance.AgeMultiplier;
			}
		}
	}
}
