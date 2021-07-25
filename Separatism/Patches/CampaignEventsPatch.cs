using HarmonyLib;
using TaleWorlds.CampaignSystem;

namespace Separatism.Patches
{
	[HarmonyPatch(typeof(CampaignEvents), "DeletePeriodicEvent")]
	public static class CampaignEventsPatch
	{
		public static bool Prefix(MBCampaignEvent campaignEvent)
		{
			return campaignEvent != null;
		}
	}
}
