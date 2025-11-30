using Bannerlord.UIExtenderEx;
using TaleWorlds.CampaignSystem;
using HarmonyLib;
using Common;

namespace Telepathy
{
	public class TelepathySubModule : ModSubModule
	{
		protected override void OnSubModuleLoad()
		{
			base.OnSubModuleLoad();
			
			UIExtender uiextender = UIExtender.Create(nameof(Telepathy));
			uiextender.Register(typeof(TelepathySubModule).Assembly);
			uiextender.Enable();
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
