using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using PathOfTerraria.Core.Items;
using System.Collections.Generic;

namespace PathOfTerraria.Content.Items.Gear.Armor.Leggings;

[AutoloadEquip(EquipType.Legs)]
internal sealed class RimeboundTreads : Leggings
{
	public override string Texture => $"{PoTMod.ModName}/Content/Items/Gear/Armor/Legs/SpectralStriders";

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();
		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = null;
		staticData.IsUnique = true;
	}

	public override List<ItemAffix> GenerateAffixes() =>
	[
		(ItemAffix)Affix.CreateAffix<ColdConversionDamage>(20),
		(ItemAffix)Affix.CreateAffix<ExtraColdDamage>(20),
		(ItemAffix)Affix.CreateAffix<MovementSpeedAffix>(20),
		(ItemAffix)Affix.CreateAffix<CannotBeChilledAffix>(1),
	];

	public override void PostRoll()
	{
		Item.defense = 8;
	}
}
