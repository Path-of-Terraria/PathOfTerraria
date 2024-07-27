using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core.Items;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Sword;

internal class WoodenSword : Sword
{
	public int ItemLevel
	{
		get => 1;
		set => this.GetInstanceData().RealLevel = value; // Technically preserves previous behavior.
	}

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;

		GearAlternatives.Register(Type, ItemID.WoodenSword);
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.Size = new(38);
		Item.damage = 4;
		Item.UseSound = SoundID.Item1;

		PoTInstanceItemData data = this.GetInstanceData();
		data.ItemType = ItemType.Sword;
	}
}