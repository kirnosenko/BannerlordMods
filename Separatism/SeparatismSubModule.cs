using TaleWorlds.CampaignSystem;
using HarmonyLib;
using Common;
using Separatism.Behaviours;

namespace Separatism
{
    public class SeparatismSubModule : ModSubModule
	{
		protected override void AddBehaviours(CampaignGameStarter gameInitializer)
		{
			gameInitializer.AddBehavior(new UtilityBehaviour());
			gameInitializer.AddBehavior(new LordRebellionBehaviour());
			gameInitializer.AddBehavior(new NationalRebellionBehaviour());
			gameInitializer.AddBehavior(new AnarchyRebellionBehaviour());
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
