using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using HarmonyLib;
using Common;

namespace Telepathy
{
	public class TelepathySubModule : ModSubModule
	{
		protected override void OnSubModuleLoad()
		{
			base.OnSubModuleLoad();

			TelepathyConfig.Load(BasePath.Name + "Modules/Telepathy/ModuleData/config.xml");
		}

		protected override void AddBehaviours(CampaignGameStarter gameInitializer)
		{
			gameInitializer.AddBehavior(new TelepathyBehaviour());
		}

		protected override void PrintPatchingError()
		{
			base.PrintPatchingError();
		}
	}

	[HarmonyPatch(typeof(TelepathySubModule), "PrintPatchingError")]
	public class TelepathySubModulePatch
	{
		public static bool Prefix()
		{
			return false;
		}
	}
}
