using System.Xml;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;

namespace Telepathy
{
	[PrefabExtension("EncyclopediaHeroPage", "descendant::TextWidget[@Text='@SkillsText']")]
	public class EncyclopediaHeroPagePrefabExtension : PrefabExtensionInsertPatch
	{
		private readonly XmlDocument document;

		public override InsertType Type => 0;

		public EncyclopediaHeroPagePrefabExtension()
		{
			document = new XmlDocument();
			document.LoadXml("<EncyclopediaHeroPageInject />");
		}

		[PrefabExtensionInsertPatch.PrefabExtensionXmlDocumentAttribute(false)]
		public XmlDocument GetPrefabExtension()
		{
			return document;
		}
	}
}
