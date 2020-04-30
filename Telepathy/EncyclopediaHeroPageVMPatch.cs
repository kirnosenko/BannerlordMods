using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;

namespace Telepathy
{
	[HarmonyPatch(typeof(EncyclopediaHeroPageVM), "Refresh")]
	public class EncyclopediaHeroPageVMPatch
	{
		public static bool Prefix(EncyclopediaHeroPageVM __instance)
		{
			var hero = __instance.Obj as Hero;
			__instance.HeroCharacter = new HeroViewModelEx(hero, CharacterViewModel.StanceTypes.EmphasizeFace);
			
			return true;
		}
	}
}
