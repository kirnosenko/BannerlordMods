using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.Pages;
using TaleWorlds.Core.ViewModelCollection;

namespace Telepathy.Patches
{
	[HarmonyPatch(typeof(EncyclopediaHeroPageVM), "Refresh")]
	public class EncyclopediaHeroPageVMPatch
	{
		public static bool Prefix(EncyclopediaHeroPageVM __instance)
		{
			var hero = __instance.Obj as Hero;
			if (hero.CanTalkTo())
			{
				__instance.HeroCharacter = new HeroViewModelEx(hero, CharacterViewModel.StanceTypes.EmphasizeFace);
			}
			
			return true;
		}
	}
}
