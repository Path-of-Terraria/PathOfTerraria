using PathOfTerraria.Core.Items;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Bow;

internal class BasicWoodenBow : Bow
{
	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;

		GearAlternatives.Register(Type, ItemID.WoodenBow);
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.damage = 8;
		Item.Size = new Vector2(24, 42);
	}
}