using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using PathOfTerraria.Core.Items;
using System.Collections.Generic;

namespace PathOfTerraria.Content.Items.Gear.Armor.Helmet;

[AutoloadEquip(EquipType.Head)]
internal sealed class YryothsCrown : Helmet
{
	public override string Texture => $"{PoTMod.ModName}/Content/Items/Gear/Armor/Helmet/SpectralVeil";

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();
		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = null;
		staticData.IsUnique = true;
	}

	public override List<ItemAffix> GenerateAffixes() =>
	[
		(ItemAffix)Affix.CreateAffix<ExtraColdDamage>(30),
		(ItemAffix)Affix.CreateAffix<BaseLifeAffix>(50),
		(ItemAffix)Affix.CreateAffix<CannotBeChilledAffix>(1),
	];

	public override void PostRoll()
	{
		Item.defense = 16;
	}
}
