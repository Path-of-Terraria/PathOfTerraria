using PathOfTerraria.Core.Items;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.WarShields;

internal class GoldenBattleBulwark : WarShield
{
	public override ShieldData Data => new(13, 120, 13f, DustID.Gold);

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.MinDropItemLevel = 20;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.damage = 26;
		Item.Size = new(26);
		Item.knockBack = 9;
	}
}
