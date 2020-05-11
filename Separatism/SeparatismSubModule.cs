using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using HarmonyLib;
using Common;

namespace Separatism
{
    public class SeparatismSubModule : ModSubModule
	{
		protected override void OnSubModuleLoad()
		{
			base.OnSubModuleLoad();

			SeparatismConfig.Load(BasePath.Name + "Modules/Separatism/ModuleData/config.xml");
		}

		protected override void AddBehaviours(CampaignGameStarter gameInitializer)
		{
			gameInitializer.AddBehavior(new SeparateBehaviour());
		}

		protected override void PrintPatchingError()
		{
			base.PrintPatchingError();
		}
	}

	[HarmonyPatch(typeof(SeparatismSubModule), "PrintPatchingError")]
	public class SeparatismSubModulePatch
	{
		public static bool Prefix()
		{
			return false;
		}
	}
}
