using TaleWorlds.CampaignSystem;
using Common;

namespace Telepathy
{
	public class TelepathySubModule : ModSubModule
	{
		protected override void AddBehaviours(CampaignGameStarter gameInitializer)
		{
			gameInitializer.AddBehavior(new TelepathyBehaviour());
		}
	}
}
