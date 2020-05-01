using TaleWorlds.CampaignSystem;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using System;
using TaleWorlds.Core;
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

			InformationManager.DisplayMessage(new InformationMessage($"name {hero.Name}", Color.White));
			InformationManager.DisplayMessage(new InformationMessage($"party {hero.PartyBelongedTo}", Color.White));
			InformationManager.DisplayMessage(new InformationMessage($"minor faction {hero.IsMinorFactionHero}", Color.White));
			InformationManager.DisplayMessage(new InformationMessage($"map faction {hero.MapFaction.Name}", Color.White));
			InformationManager.DisplayMessage(new InformationMessage($"special {hero.IsSpecial}", Color.White));
		}

		public void CallToTalk()
		{
			TelepathyBehaviour.CallToTalk(hero);
		}
	}
}
