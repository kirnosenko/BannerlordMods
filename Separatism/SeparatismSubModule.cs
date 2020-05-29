using TaleWorlds.CampaignSystem;
using HarmonyLib;
using Common;

namespace Separatism
{
    public class SeparatismSubModule : ModSubModule
	{
		protected override void AddBehaviours(CampaignGameStarter gameInitializer)
		{
			gameInitializer.AddBehavior(new SeparateBehaviour());
			gameInitializer.AddModel(new SeparatismSettlementLoyaltyModel());
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
