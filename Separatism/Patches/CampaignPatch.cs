using HarmonyLib;
using TaleWorlds.CampaignSystem;

namespace Separatism.Patches
{
	[HarmonyPatch(typeof(Campaign), "DeletePeriodicEvent")]
	public static class CampaignPatch
	{
		public static bool Prefix(MBCampaignEvent campaignEvent)
		{
			return campaignEvent != null;
		}
	}
}
