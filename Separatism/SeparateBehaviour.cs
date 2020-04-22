using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using HarmonyLib;

namespace Separatism
{
	public class SeparateBehaviour : CampaignBehaviorBase
	{
		private static Random rng = new Random();

		public override void RegisterEvents()
		{
			CampaignEvents.DailyTickClanEvent.AddNonSerializedListener(this, OnClanTick);
		}

		public override void SyncData(IDataStore dataStore)
		{
		}

		private void OnClanTick(Clan clan)
		{
			var kingdom = clan.Kingdom;
			if (kingdom == null || clan == Clan.PlayerClan) return;
			var ruler = kingdom.Ruler;

			if (clan.Leader.IsAlive
				&& !clan.Leader.IsFactionLeader
				&& !clan.Leader.IsPrisoner
				&& clan.Leader.IsEnemy(ruler)
				&& clan.Settlements.Where(x => x.IsCastle || x.IsTown).Count() >= 2)
			{
				var colors = BannerManager.ColorPalette.Values.Select(x => x.Color).ToList();
				uint color1 = TakeColor(colors);
				uint color2 = color1;
				while (colors.Count > 0 && ColorDiff(color1, color2) < 0.3)
				{
					color2 = TakeColor(colors);
				}

				clan.Banner.ChangePrimaryColor(color1);
				clan.Banner.ChangeIconColors(color2);
				var newKingdom = CreateKingdom(clan);
				ChangeKingdomAction.ApplyByLeaveWithRebellionAgainstKingdom(clan, newKingdom, true);
				
				GameLog.Warn($"Clan {clan.Name.ToString()} leave kingdom {kingdom}.");
			}
		}

		private uint TakeColor(List<uint> colors)
		{
			int index = rng.Next(colors.Count);
			uint color = colors[index];
			colors.RemoveAt(index);

			return color;
		}

		private double ColorDiff(uint color1, uint color2)
		{
			var gray1 = (0.2126 * (color1 >> 16 & 0xFF) + 0.7152 * (color1 >> 8 & 0xFF) + 0.0722 * (color1 & 0xFF)) / 255;
			var gray2 = (0.2126 * (color2 >> 16 & 0xFF) + 0.7152 * (color2 >> 8 & 0xFF) + 0.0722 * (color2 & 0xFF)) / 255;

			return Math.Abs(gray1 - gray2);
		}

		private Kingdom CreateKingdom(Clan clan)
		{
			Kingdom kingdom = MBObjectManager.Instance.CreateObject<Kingdom>($"{clan.Name.ToString().ToLower()}_kingdom");
			TextObject textObject = new TextObject("{=72pbZgQL}{CLAN_NAME}", null);
			textObject.SetTextVariable("CLAN_NAME", clan.Name);
			TextObject textObject2 = new TextObject("{=EXp18CLD}Kingdom of the {CLAN_NAME}", null);
			textObject2.SetTextVariable("CLAN_NAME", clan.Name);
			kingdom.InitializeKingdom(textObject2, textObject, clan.Culture, clan.Banner, clan.Color, clan.Color2, clan.InitialPosition);
			ChangeKingdomAction.ApplyByJoinToKingdom(clan, kingdom, true);
			kingdom.RulingClan = clan;

			if (!Kingdom.All.Contains(clan.MapFaction))
			{
				List<Kingdom> curkingdoms = new List<Kingdom>(Campaign.Current.Kingdoms.ToList());
				curkingdoms.Add(kingdom);
				AccessTools.Field(Campaign.Current.GetType(), "_kingdoms").SetValue(Campaign.Current, new MBReadOnlyList<Kingdom>(curkingdoms));
			}

			return kingdom;
		}
	}
}
