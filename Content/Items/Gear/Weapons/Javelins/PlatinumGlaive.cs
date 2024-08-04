using PathOfTerraria.Core.Items;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Javelins;

internal class PlatinumGlaive : Javelin
{
	public override Vector2 ItemSize => new(98);
	public override int DeathDustType => DustID.Platinum;

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.MinDropItemLevel = 21;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.damage = 16;
	}
}
