using TaleWorlds.CampaignSystem.SandBox.GameComponents;

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
		
		public override int RebellionEndLoyaltyThreshold
		{
			get
			{
				return SeparatismConfig.Settings.SettlementRebellionEndLoyaltyThreshold;
			}
		}
	}
}
