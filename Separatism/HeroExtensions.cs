using TaleWorlds.CampaignSystem;

namespace Separatism
{
	public static class HeroExtensions
	{
		public static bool HasGoodRelationWith(this Hero hero1, Hero hero2)
		{
			return (hero1.IsFriend(hero2) || (!hero1.IsEnemy(hero2) && hero1.Culture == hero2.Culture));
		}
	}
}
