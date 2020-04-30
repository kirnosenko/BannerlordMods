using TaleWorlds.CampaignSystem;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection;

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
	}
}
