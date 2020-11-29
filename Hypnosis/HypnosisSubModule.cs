using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Conversation.Persuasion;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace Hypnosis
{
	public class HypnosisSubModule : MBSubModuleBase
	{
		protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
		{
			if (game.GameType is Campaign)
			{
				gameStarterObject.AddModel(new HypnosisPersuasionModel());
			}
		}
	}
}
