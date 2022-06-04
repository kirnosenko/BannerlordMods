using TaleWorlds.CampaignSystem.GameComponents;

namespace Separatism
{
	public class SeparatismSettlementLoyaltyModel : DefaultSettlementLoyaltyModel
	{
		public override int RebellionStartLoyaltyThreshold
		{
			get
			{
				return SeparatismConfig.Settings.SettlementRebellionStartLoyaltyThreshold;
			}
		}
	}
}
