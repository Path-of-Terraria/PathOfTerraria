using PathOfTerraria.Core.Items;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Javelins;

internal class IronPilum : Javelin
{
	public override Vector2 ItemSize => new(78);
	public override int DeathDustType => DustID.Iron;

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.MinDropItemLevel = 5;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.damage = 8;
	}
}
