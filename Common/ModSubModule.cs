using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using HarmonyLib;

namespace Common
{
	public class ModSubModule : MBSubModuleBase
	{
		protected override void OnSubModuleLoad()
		{
			base.OnSubModuleLoad();

			new Harmony(this.GetType().Namespace).PatchAll();
			// For a strange reason we have to wait here
			// to prevent patching problem...
			// Remove it and you will see an error.
			System.Threading.Thread.Sleep(42);
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

		private void PrintPatchingError()
		{
			GameLog.Warn($"Error while loading {this.GetType().Namespace}! Try to change mod loading order.");
		}
	}

	[HarmonyPatch(typeof(ModSubModule), "PrintPatchingError")]
	public class ModSubModulePatch
	{
		public static bool Prefix()
		{
			return false;
		}
	}
}
