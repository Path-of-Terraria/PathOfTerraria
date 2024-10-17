using PathOfTerraria.Core.Items;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.WarShields;

internal class SteelWarShield : WarShield
{
	public override ShieldData Data => new(16, 110, 15, DustID.t_SteampunkMetal);

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.MinDropItemLevel = 25;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.damage = 30;
		Item.Size = new(26);
		Item.knockBack = 9;
	}
}
