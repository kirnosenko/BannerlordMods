using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using HarmonyLib;

namespace Separatism
{
    public class SeparatismBase : MBSubModuleBase
	{
		private SeparatismConfig config;
		private SeparateBehaviour separateBehaviour;

		protected override void OnSubModuleLoad()
		{
			base.OnSubModuleLoad();

			new Harmony(nameof(Separatism)).PatchAll();
			config = SeparatismConfig.Load(BasePath.Name + "Modules/Separatism/ModuleData/config.xml");
		}

		protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
		{
			base.OnGameStart(game, gameStarterObject);

			Campaign campaign = game.GameType as Campaign;
			if (campaign == null) return;
			CampaignGameStarter gameInitializer = (CampaignGameStarter)gameStarterObject;
			AddBehaviours(gameInitializer);
		}

		private void AddBehaviours(CampaignGameStarter gameInitializer)
		{
			separateBehaviour = new SeparateBehaviour(config);
			gameInitializer.AddBehavior(separateBehaviour);
		}
	}
}
