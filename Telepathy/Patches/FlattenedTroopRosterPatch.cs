using HarmonyLib;
using TaleWorlds.CampaignSystem;

namespace Telepathy.Patches
{
	[HarmonyPatch(typeof(FlattenedTroopRoster), "GenerateUniqueNoFromParty")]
	public class FlattenedTroopRosterPatch
	{
		public static bool Prefix(MobileParty party, int troopIndex, ref int __result)
		{
			__result = ((party?.Party?.Index ?? 1) * 999983 + troopIndex * 100003) % 616841;

			return false;
		}
	}
}
