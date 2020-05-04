using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using Helpers;
using HarmonyLib;
using Common;

namespace Separatism
{
	public class SeparateBehaviour : CampaignBehaviorBase
	{
		private SeparatismConfig config;

		public SeparateBehaviour(SeparatismConfig config)
		{
			this.config = config;
		}

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
			if (kingdom == null
				|| clan == Clan.PlayerClan
				|| !clan.Leader.IsAlive
				|| clan.Leader.IsPrisoner
				|| clan.Parties.Any(x => x.MapEvent != null))
			{
				return;
			}
			var ruler = kingdom.Ruler;

			if (clan.Leader != ruler)
			{
				var hasReason = !clan.Leader.HasGoodRelationWith(ruler);
				var kingdomFiefs = kingdom.Settlements.Sum(x => x.IsTown ? 2 : x.IsCastle ? 1 : 0);
				var clanFiefs = clan.Settlements.Sum(x => x.IsTown ? 2 : x.IsCastle ? 1 : 0);
				var hasEnoughFiefs = 100 * (double)clanFiefs / kingdomFiefs >= config.MinimalKingdomFiefsPercentToRebel;

				var rebelRightNow = config.DailyChanceToRebelWhenHaveAReason == 100 ||
					(MBRandom.RandomFloat * 100 <= config.DailyChanceToRebelWhenHaveAReason);

				if (hasReason && hasEnoughFiefs && rebelRightNow)
				{
					var rebelKingdom = GoRebelKingdom(clan);
					GameLog.Warn($"The {clan.Name} have broken from {kingdom} to found the {rebelKingdom}");
				}
			}
			else
			{
				if (kingdom.Clans.Where(x => x.Leader.IsAlive).Count() == 1) // kingdom has the only one ruler clan
				{
					if (clan.Settlements.Count() == 0) // no any fiefs
					{
						ClanChangeKingdom(clan, null, false);
						GameLog.Warn($"The {kingdom} has been destroyed and the {clan.Name} are in search of a new sovereign.");
					}
					else // look for ally
					{
						var enemies = FactionManager.GetEnemyKingdoms(kingdom).ToArray();
						var potentialAllies = enemies
							.SelectMany(x => FactionManager.GetEnemyKingdoms(x))
							.Distinct()
							.Except(enemies)
							.Intersect(clan.CloseKingdoms())
							.Where(x => x != kingdom)
							.ToArray();
						foreach (var pa in potentialAllies)
						{
							if (kingdom.Leader.HasGoodRelationWith(pa.Leader) &&
								pa.Leader.Clan.Tier >= clan.Tier)
							{
								var commonEnemies = FactionManager.GetEnemyKingdoms(pa)
									.Intersect(enemies)
									.ToArray();
								foreach (var enemy in commonEnemies)
								{
									var kingdomDistance = enemy.FactionMidPoint.Distance(kingdom.FactionMidPoint);
									var paDistance = enemy.FactionMidPoint.Distance(pa.FactionMidPoint);
									var allianceDistance = kingdom.FactionMidPoint.Distance(pa.FactionMidPoint);

									if (allianceDistance <= Math.Sqrt(kingdomDistance * kingdomDistance + paDistance * paDistance) ||
										(kingdom.IsInsideKingdomTeritory(enemy) && pa.IsInsideKingdomTeritory(enemy)))
									{
										ClanChangeKingdom(clan, pa, false);
										GameLog.Warn($"The {kingdom} has joined to the {pa} to fight against common enemy the {enemy}.");
										return;
									}
								}
							}
						}
					}
				}
			}
		}

		private (uint,uint) GetRebelKingdomColors()
		{
			var colors = BannerManager.ColorPalette.Values.Select(x => x.Color).Distinct().ToList();
			uint color1 = colors.Max();
			uint color2 = colors.Min();

			if (!config.OneColorForAllRebels)
			{
				colors = colors.Except(Kingdom.All.Select(x => x.Color)).ToList();
				color1 = TakeRandomColor(colors);
				color2 = color1;
				while (colors.Count > 0 && ColorDiff(color1, color2) < 0.3)
				{
					color2 = TakeRandomColor(colors);
				}
			}

			return (color1, color2);
		}

		private uint TakeRandomColor(List<uint> colors)
		{
			int index = MBRandom.RandomInt(colors.Count);
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

		private string GetClanKingdomId(Clan clan)
		{
			return $"{clan.Name.ToString().ToLower()}_kingdom";
		}

		private Kingdom GoRebelKingdom(Clan clan)
		{
			string kingdomId = GetClanKingdomId(clan);
			var kingdom = Kingdom.All.SingleOrDefault(x => x.StringId == kingdomId);

			if (kingdom == null)
			{
				// create a rebel kingdom and set its name
				kingdom = MBObjectManager.Instance.CreateObject<Kingdom>(kingdomId);
				TextObject textObject = new TextObject("{=72pbZgQL}{CLAN_NAME}", null);
				textObject.SetTextVariable("CLAN_NAME", clan.Name);
				var kingdomName = "Kingdom of {CLAN_NAME}";
				switch (clan.Culture.GetCultureCode())
				{
					case CultureCode.Aserai:
						kingdomName = "Sultanate of {CLAN_NAME}";
						break;
					case CultureCode.Khuzait:
						kingdomName = "Khanate of {CLAN_NAME}";
						break;
					case CultureCode.Sturgia:
						kingdomName = "Principality of {CLAN_NAME}";
						break;
					case CultureCode.Empire:
						kingdomName = "Empire of {CLAN_NAME}";
						break;
					default:
						break;
				}
				TextObject textObject2 = new TextObject("{=EXp18CLD}" + kingdomName, null);
				textObject2.SetTextVariable("CLAN_NAME", clan.Name);

				// set colors for a rebel kingdom and the ruler clan
				var (color1,color2) = GetRebelKingdomColors();
				if (!config.KeepRebelBannerColors)
				{
					clan.Banner.ChangePrimaryColor(color1);
					clan.Banner.ChangeIconColors(color2);
					clan.Color = color1;
					clan.Color2 = color2;
				}
				kingdom.InitializeKingdom(textObject2, textObject, clan.Culture, clan.Banner, color1, color2, clan.InitialPosition);
				AccessTools.Property(typeof(Kingdom), "AlternativeColor").SetValue(kingdom, color1);
				AccessTools.Property(typeof(Kingdom), "AlternativeColor2").SetValue(kingdom, color2);
				AccessTools.Property(typeof(Kingdom), "LabelColor").SetValue(kingdom, clan.Kingdom.LabelColor);

				// apply policies from original kingdom
				foreach (var policy in clan.Kingdom.ActivePolicies)
				{
					kingdom.AddPolicy(policy);
				}

				kingdom.RulingClan = clan;
			}

			ClanChangeKingdom(clan, kingdom, true);
			if (!Kingdom.All.Contains(kingdom))
			{
				ModifyKingdomList(kingdoms => kingdoms.Add(kingdom));
			}

			return kingdom;
		}

		private void ModifyKingdomList(Action<List<Kingdom>> modificator)
		{
			List<Kingdom> kingdoms = new List<Kingdom>(Campaign.Current.Kingdoms.ToList());
			modificator(kingdoms);
			AccessTools.Field(Campaign.Current.GetType(), "_kingdoms").SetValue(Campaign.Current, new MBReadOnlyList<Kingdom>(kingdoms));
		}

		private void ClanChangeKingdom(Clan clan, Kingdom newKingdom, bool rebellion)
		{
			Kingdom oldKingdom = clan.Kingdom;
			
			if (newKingdom != null)
			{
				foreach (Kingdom k in Kingdom.All)
				{
					if (k == newKingdom || !newKingdom.IsAtWarWith(k))
					{
						FactionHelper.FinishAllRelatedHostileActionsOfFactionToFaction(clan, k);
						FactionHelper.FinishAllRelatedHostileActionsOfFactionToFaction(k, clan);
					}
				}
				foreach (Clan c in Clan.All)
				{
					if (c != clan && c.Kingdom == null && !newKingdom.IsAtWarWith(c))
					{
						FactionHelper.FinishAllRelatedHostileActions(clan, c);
					}
				}
			}

			StatisticsDataLogHelper.AddLog(StatisticsDataLogHelper.LogAction.ChangeKingdomAction, new object[]
			{
				clan,
				oldKingdom,
				newKingdom,
				newKingdom == null || rebellion
			});
			clan.IsUnderMercenaryService = false;
			clan.ClanLeaveKingdom(false);
			if (newKingdom != null)
			{
				clan.ClanJoinFaction(newKingdom);
				NotifyClanChangedKingdom(clan, oldKingdom, newKingdom, rebellion, true);

				if (rebellion)
				{
					foreach (Clan c in oldKingdom.Clans)
					{
						int relationChange = 0;
						if (c.Leader == oldKingdom.Leader)
						{
							relationChange = -20;
						}
						else if (c.Leader.IsFriend(oldKingdom.Leader))
						{
							relationChange = -10;
						}
						else if (c.Leader.IsEnemy(oldKingdom.Leader))
						{
							relationChange = +10;
						}

						ChangeRelationAction.ApplyRelationChangeBetweenHeroes(clan.Leader, c.Leader, relationChange, true);
					}

					InheritWarsFromKingdom(oldKingdom, newKingdom);
					DeclareWarAction.Apply(oldKingdom, newKingdom);
				}
				else
				{
					ChangeRelationAction.ApplyRelationChangeBetweenHeroes(clan.Leader, newKingdom.Leader, +20, true);
					InheritWarsFromKingdom(oldKingdom, newKingdom);
				}
			}
			if (oldKingdom.Clans.Count == 0) // old kingdom is empty so we destroy it
			{
				if (newKingdom == null)
				{
					NotifyClanChangedKingdom(clan, oldKingdom, null, false, true);
				}
				DestroyKingdomAction.Apply(oldKingdom);
				ModifyKingdomList(kingdoms => kingdoms.RemoveAll(x => x == oldKingdom));
			}

			CheckIfPartyIconIsDirty(clan, oldKingdom);
		}

		private void InheritWarsFromKingdom(Kingdom src, Kingdom dest)
		{
			if (config.KeepOriginalKindomWars)
			{
				var oldKingdomEnemies = FactionManager.GetEnemyKingdoms(src).ToArray();
				foreach (var enemy in oldKingdomEnemies)
				{
					DeclareWarAction.Apply(enemy, dest);
				}
			}
		}

		private void NotifyClanChangedKingdom(Clan clan, Kingdom oldKingdom, Kingdom newKingdom, bool byRebellion, bool showNotification = true)
		{
			AccessTools.Method(CampaignEventDispatcher.Instance.GetType(), "OnClanChangedKingdom")
				.Invoke(CampaignEventDispatcher.Instance, new object[] { clan, oldKingdom, newKingdom, byRebellion, showNotification });
		}

		private void CheckIfPartyIconIsDirty(Clan clan, Kingdom oldKingdom)
		{
			IFaction faction;
			if (clan.Kingdom == null)
			{
				faction = clan;
			}
			else
			{
				IFaction kingdom = clan.Kingdom;
				faction = kingdom;
			}
			IFaction faction2 = faction;
			IFaction faction3 = (IFaction)oldKingdom ?? clan;
			foreach (MobileParty mobileParty in MobileParty.All)
			{
				if (mobileParty.IsVisible && ((mobileParty.Party.Owner != null && mobileParty.Party.Owner.Clan == clan) || (clan == Clan.PlayerClan && ((!FactionManager.IsAtWarAgainstFaction(mobileParty.MapFaction, faction2) && FactionManager.IsAtWarAgainstFaction(mobileParty.MapFaction, faction3)) || (FactionManager.IsAtWarAgainstFaction(mobileParty.MapFaction, faction2) && !FactionManager.IsAtWarAgainstFaction(mobileParty.MapFaction, faction3))))))
				{
					mobileParty.Party.Visuals.SetMapIconAsDirty();
				}
			}
			foreach (Settlement settlement in clan.Settlements)
			{
				settlement.Party.Visuals.SetMapIconAsDirty();
			}
		}
	}
}
