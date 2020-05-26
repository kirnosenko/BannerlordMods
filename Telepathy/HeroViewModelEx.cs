using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using Common;

namespace Telepathy
{
	public class HeroViewModelEx : HeroViewModel
	{
		private readonly Hero hero;
		private readonly string btnText;

		public HeroViewModelEx(Hero hero, CharacterViewModel.StanceTypes stance = CharacterViewModel.StanceTypes.None)
			: base(stance)
		{
			this.hero = hero;
			this.btnText = new TextObject("{=Telepathy_Talk_To_Me}Talk to me!", null).ToString();
		}

		public void CallToTalk()
		{
			TelepathyBehaviour.CallToTalk(hero);
			var textObject = new TextObject("{=Telepathy_Hero_Will_Talk}{HeroName} will talk to you soon...", null);
			textObject.SetTextVariable("HeroName", hero.Name);
			GameLog.Info(textObject.ToString());
			base.OnPropertyChanged(nameof(WillNotTalk));
		}

		[DataSourceProperty]
		public string CallToTalkText
		{
			get
			{
				return btnText;
			}
		}

		[DataSourceProperty]
		public bool CanTalkTo
		{
			get
			{
				return hero.CanTalkTo();
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
