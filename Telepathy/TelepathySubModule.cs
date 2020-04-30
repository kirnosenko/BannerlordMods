using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using HarmonyLib;

namespace Telepathy
{
	public class TelepathySubModule : MBSubModuleBase
	{
		protected override void OnSubModuleLoad()
		{
			base.OnSubModuleLoad();

			new Harmony(nameof(Telepathy)).PatchAll();
			System.Threading.Thread.Sleep(50);
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
			gameInitializer.AddBehavior(new TelepathyBehaviour());
		}
	}
}
