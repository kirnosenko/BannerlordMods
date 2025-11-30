using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.Pages;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using Common;

namespace Telepathy
{
	[ViewModelMixin("RefreshValues")]
	internal sealed class EncyclopediaHeroPageVMMixin : BaseViewModelMixin<EncyclopediaHeroPageVM>
	{
		private readonly Hero hero;
		private readonly string btnText;

		public EncyclopediaHeroPageVMMixin(EncyclopediaHeroPageVM vm)
			: base(vm)
		{
			this.hero = (vm.Obj as Hero);
			this.btnText = new TextObject("{=Telepathy_Talk_To_Me}Talk to me!", null).ToString();
			vm.RefreshValues();
		}

		[DataSourceMethod]
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
