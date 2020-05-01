using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;

namespace Telepathy
{
	public class HeroViewModelEx : HeroViewModel
	{
		private readonly Hero hero;

		public HeroViewModelEx(Hero hero, CharacterViewModel.StanceTypes stance = CharacterViewModel.StanceTypes.None)
			: base(stance)
		{
			this.hero = hero;
		}

		public void CallToTalk()
		{
			TelepathyBehaviour.CallToTalk(hero);
		}

		[DataSourceProperty]
		public bool IsAlive
		{
			get
			{
				return this.hero.IsAlive;
			}
			set
			{
			}
		}
	}
}
