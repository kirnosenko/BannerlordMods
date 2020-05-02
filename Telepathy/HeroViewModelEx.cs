using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using Common;

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
			GameLog.Info($"{hero.Name} will talk to you soon.");
			base.OnPropertyChanged(nameof(WillNotTalk));
		}

		[DataSourceProperty]
		public bool CanTalkTo
		{
			get
			{
				return this.hero.CanTalkTo();
			}
		}

		[DataSourceProperty]
		public bool WillNotTalk
		{
			get
			{
				return !TelepathyBehaviour.CalledToTalk(hero);
			}
		}
	}
}
