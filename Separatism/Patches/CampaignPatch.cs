using HarmonyLib;
using TaleWorlds.CampaignSystem;

namespace Separatism.Patches
{
	[HarmonyPatch(typeof(Campaign), "InitializeTypes")]
	public static class CampaignPatch
	{
		public static void Postfix(Campaign __instance)
		{
			__instance.RemoveEmptyKingdoms();
		}
	}
}
