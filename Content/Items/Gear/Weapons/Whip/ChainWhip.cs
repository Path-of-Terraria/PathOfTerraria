using PathOfTerraria.Content.Projectiles.Whip;
using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Whip;

internal class ChainWhip : Whip
{
	public override WhipDrawData DrawData => new(new(14, 22), new(0, 22, 14, 12), new(0, 34, 14, 12), new(0, 46, 14, 12), new(0, 52, 14, 14), false);
	
	public override WhipSettings WhipSettings => new()
	{
		Segments = 18,
		RangeMultiplier = 1.05f,
	};

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.MinDropItemLevel = 15;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.DefaultToWhip(ModContent.ProjectileType<WhipBaseProjectile>(), 13, 2, 4);
		Item.channel = true;
	}
}