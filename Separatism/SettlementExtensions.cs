using System.Collections.Generic;
using TaleWorlds.CampaignSystem.Settlements;

namespace Separatism
{
	public static class SettlementExtensions
	{
		public static IEnumerable<Settlement> FindSettlementsAround(this Settlement settlement, float radius)
		{
			var search = Settlement.StartFindingLocatablesAroundPosition(settlement.Position.ToVec2(), radius);
			yield return Settlement.FindNextLocatable(ref search);
		}
	}
}
