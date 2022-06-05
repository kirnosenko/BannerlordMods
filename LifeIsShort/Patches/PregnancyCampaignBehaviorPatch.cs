using System.Collections.Generic;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;

namespace LifeIsShort.Patches
{
	[HarmonyPatch(typeof(PregnancyCampaignBehavior), "RegisterEvents")]
	public class RegisterEventsPatch
	{
		private static void Postfix(PregnancyCampaignBehavior __instance)
		{
			CampaignEvents.DailyTickEvent.AddNonSerializedListener(__instance, () =>
			{
				var pregnancies = AccessTools.Field(typeof(PregnancyCampaignBehavior), "_heroPregnancies")
					.GetValue(__instance) as IReadOnlyList<object>;
				float timeShiftDays = 1 * LifeIsShortConfig.Instance.AgeMultiplier - 1;

				if (timeShiftDays > 0)
				{
					foreach (var pregnancy in pregnancies)
					{
						var dateField = pregnancy.GetType().GetField("DueDate");
						var date = (CampaignTime)dateField.GetValue(pregnancy);
						var newDate = CampaignTime.Days((float)date.ToDays - timeShiftDays);
						dateField.SetValue(pregnancy, newDate);
					}
				}
			});
		}
	}
}
