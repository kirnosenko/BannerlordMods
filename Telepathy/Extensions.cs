using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;

namespace Telepathy
{
	public static class Extensions
	{
		public static bool CanTalkTo(this Hero hero)
		{
			return hero.IsAlive && hero != Hero.MainHero;
		}
	}
}
