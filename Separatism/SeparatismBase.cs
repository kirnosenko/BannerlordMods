using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.SaveSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.InputSystem;

namespace Separatism
{
    public class SeparatismBase : MBSubModuleBase
	{
		private SeparateBehaviour separateBehaviour;

		protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
		{
			Campaign campaign = game.GameType as Campaign;
			if (campaign == null) return;
			CampaignGameStarter gameInitializer = (CampaignGameStarter)gameStarterObject;
			AddBehaviours(gameInitializer);
		}

		private void AddBehaviours(CampaignGameStarter gameInitializer)
		{
			separateBehaviour = new SeparateBehaviour();
			gameInitializer.AddBehavior(separateBehaviour);
		}
	}
}
