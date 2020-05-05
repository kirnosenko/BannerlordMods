using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using HarmonyLib;
using Common;

namespace LifeIsShort
{
	public class LifeIsShortSubModule : ModSubModule
	{
		protected override void OnSubModuleLoad()
		{
			base.OnSubModuleLoad();

			LifeIsShortConfig.Load(BasePath.Name + "Modules/LifeIsShort/ModuleData/config.xml");
		}

		protected override void PrintPatchingError()
		{
			base.PrintPatchingError();
		}
	}

	[HarmonyPatch(typeof(LifeIsShortSubModule), "PrintPatchingError")]
	public class LifeIsShortSubModulePatch
	{
		public static bool Prefix()
		{
			return false;
		}
	}
}
