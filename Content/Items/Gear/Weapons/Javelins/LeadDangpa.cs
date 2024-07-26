using PathOfTerraria.Core.Items;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Javelins;

internal class LeadDangpa : Javelin
{
	public override Vector2 ItemSize => new(70);
	public override int DeathDustType => DustID.Lead;

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.MinDropItemLevel = 12;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.damage = 10;
	}
}
