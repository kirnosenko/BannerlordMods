using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using HarmonyLib;
using Common;

namespace Separatism
{
    public class SeparatismSubModule : ModSubModule
	{
		private SeparatismConfig config;
		private SeparateBehaviour separateBehaviour;

		protected override void OnSubModuleLoad()
		{
			base.OnSubModuleLoad();

			config = SeparatismConfig.Load(BasePath.Name + "Modules/Separatism/ModuleData/config.xml");
		}

		protected override void AddBehaviours(CampaignGameStarter gameInitializer)
		{
			separateBehaviour = new SeparateBehaviour(config);
			gameInitializer.AddBehavior(separateBehaviour);
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
