using PathOfTerraria.Core.Items;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.WarShields;

internal class LeadBattleBulwark : WarShield
{
	public override ShieldData Data => new(15, 100, 12, DustID.Lead);

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.MinDropItemLevel = 12;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.damage = 14;
		Item.Size = new(28);
	}
}
