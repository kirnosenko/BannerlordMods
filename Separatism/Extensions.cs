using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Library;

namespace Separatism
{
	public static class Extensions
	{
		public static bool HasGoodRelationWith(this Hero hero1, Hero hero2)
		{
			return (hero1.IsFriend(hero2) || (!hero1.IsEnemy(hero2) && hero1.Culture.GetCultureCode() == hero2.Culture.GetCultureCode()));
		}
		public static Kingdom[] CloseKingdoms(this Clan clan)
		{
			Vec2 clanPosition = Vec2.Zero;
			int counter = 0;
			foreach (var spos in clan.Settlements.Where(x => x.IsTown || x.IsCastle).Select(x => x.Position2D))
			{
				clanPosition += spos;
				counter++;
			}
			if (counter == 0)
			{
				return Kingdom.All.ToArray();
			}
			clanPosition *= ((float)1 / counter);

			var kingdomDistance = Kingdom.All
				.Select(k => (k, k.FactionMidPoint.Distance(clanPosition)))
				.ToArray();
			var average = kingdomDistance.Average(x => x.Item2);
			return kingdomDistance
				.Where(x => x.Item2 <= average)
				.OrderBy(x => x.Item2)
				.Select(x => x.k)
				.ToArray();
		}
		public static bool IsInsideKingdomTeritory(this Kingdom kingdom1, Kingdom kingdom2)
		{
			var positions1 = kingdom1.Settlements.Where(x => x.IsTown || x.IsCastle)
				.Select(x => x.Position2D).ToArray();
			var positions2 = kingdom2.Settlements.Where(x => x.IsTown || x.IsCastle)
				.Select(x => x.Position2D).ToArray();

			return positions1.Max(p => p.X) <= positions2.Max(p => p.X) &&
				positions1.Max(p => p.Y) <= positions2.Max(p => p.Y) &&
				positions1.Min(p => p.X) >= positions2.Min(p => p.X) &&
				positions1.Min(p => p.Y) >= positions2.Min(p => p.Y);
		}
		public static float Distance(this Vec2 value1, Vec2 value2)
		{
			float v1 = value1.X - value2.X, v2 = value1.Y - value2.Y;
			return (float)Math.Sqrt((v1 * v1) + (v2 * v2));
		}


		public static void AddKingdom(this Campaign campaign, Kingdom kingdom)
		{
			ModifyKingdomList(campaign, kingdoms =>
			{
				if (!kingdoms.Contains(kingdom))
				{
					kingdoms.Add(kingdom);
					return kingdoms;
				}

				return null;
			});
		}

		public static void RemoveKingdom(this Campaign campaign, Kingdom kingdom)
		{
			ModifyKingdomList(campaign, kingdoms =>
			{
				if (kingdoms.RemoveAll(k => k == kingdom) > 0)
				{
					return kingdoms;
				}
				
				return null;
			});
		}

		public static void RemoveEmptyKingdoms(this Campaign campaign, Action<Kingdom> callBack = null)
		{
			ModifyKingdomList(campaign, kingdoms =>
			{
				var emptyKingdomsToRemove = kingdoms
					.Where(k => 
						k.Clans.Where(x => x.Leader.IsAlive).Count() == 0 &&
						(!SeparatismConfig.Instance.KeepEmptyKingdoms || k.RulingClan?.GetClanKingdomId() == k.StringId))
					.ToArray();
				if (emptyKingdomsToRemove.Length > 0)
				{
					foreach (var kingdom in emptyKingdomsToRemove)
					{
						callBack?.Invoke(kingdom);
						DestroyKingdomAction.Apply(kingdom);
					}
					return kingdoms.Except(emptyKingdomsToRemove).ToList();
				}

				return null;
			});
		}

		public static string GetClanKingdomId(this Clan clan)
		{
			return $"{clan.Name.ToString().ToLower()}_kingdom";
		}

		private static void ModifyKingdomList(this Campaign campaign, Func<List<Kingdom>, List<Kingdom>> modificator)
		{
			List<Kingdom> kingdoms = campaign.Kingdoms.ToList();
			kingdoms = modificator(kingdoms);
			if (kingdoms != null)
			{
				AccessTools.Field(Campaign.Current.GetType(), "_kingdoms").SetValue(Campaign.Current, new MBReadOnlyList<Kingdom>(kingdoms));
			}
		}
	}
}
