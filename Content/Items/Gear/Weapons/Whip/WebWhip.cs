using PathOfTerraria.Content.Projectiles.Whip;
using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Whip;

internal class WebWhip : Whip
{
	public override WhipDrawData DrawData => new(new(14, 22), new(0, 22, 14, 16), new(0, 38, 14, 16), new(0, 54, 14, 16), new(0, 70, 14, 18), false);

	public override WhipSettings WhipSettings => new()
	{
		Segments = 30,
		RangeMultiplier = 1f,
	};

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.MinDropItemLevel = 9;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.DefaultToWhip(ModContent.ProjectileType<WhipBaseProjectile>(), 9, 2, 4);
		Item.channel = true;
	}
}