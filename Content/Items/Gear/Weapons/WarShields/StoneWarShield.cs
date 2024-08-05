using PathOfTerraria.Core.Items;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.WarShields;

internal class StoneWarShield : WarShield
{
	public override ShieldData Data => new(18, 100, 12f, DustID.Stone);

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.MinDropItemLevel = 5;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.damage = 13;
		Item.Size = new(20, 24);
	}
}
