using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
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
	}
}
