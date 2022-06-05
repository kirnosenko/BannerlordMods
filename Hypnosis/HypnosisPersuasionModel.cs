using TaleWorlds.CampaignSystem.Conversation.Persuasion;
using TaleWorlds.CampaignSystem.GameComponents;

namespace Hypnosis
{
	public class HypnosisPersuasionModel : DefaultPersuasionModel
	{
		public override void GetChances(PersuasionOptionArgs optionArgs, out float successChance, out float critSuccessChance, out float critFailChance, out float failChance, float difficultyMultiplier)
		{
			successChance = 1;
			critSuccessChance = 0;
			failChance = 0;
			critFailChance = 0;
		}
		public override void GetEffectChances(PersuasionOptionArgs option, out float moveToNextStageChance, out float blockRandomOptionChance, float difficultyMultiplier)
		{
			moveToNextStageChance = 1;
			blockRandomOptionChance = 0;
		}
	}
}
