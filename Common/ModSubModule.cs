using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using HarmonyLib;

namespace Common
{
	public abstract class ModSubModule : MBSubModuleBase
	{
		protected override void OnSubModuleLoad()
		{
			base.OnSubModuleLoad();

			// Harmony.DEBUG = true;
			var harmony = new Harmony(this.GetType().Namespace);
			harmony.PatchAll(this.GetType().Assembly);
			// For a strange reason sometimes it is necessary 
			// to wait here a bit to prevent patching problems...
			System.Threading.Thread.Sleep(50);
		}

		protected override void OnBeforeInitialModuleScreenSetAsRoot()
		{
			base.OnBeforeInitialModuleScreenSetAsRoot();
			PrintPatchingError();
		}

		protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
		{
			base.OnGameStart(game, gameStarterObject);

			Campaign campaign = game.GameType as Campaign;
			if (campaign == null) return;
			CampaignGameStarter gameInitializer = (CampaignGameStarter)gameStarterObject;
			AddBehaviours(gameInitializer);
		}

		protected virtual void AddBehaviours(CampaignGameStarter gameInitializer)
		{
		}

		protected virtual void PrintPatchingError()
		{
			GameLog.Warn($"Error while loading {this.GetType().Namespace}! Try to change mod loading order.");
		}
	}
}
