using PathOfTerraria.Content.Buffs.ShrineBuffs;
using PathOfTerraria.Content.Projectiles.Utility;

namespace PathOfTerraria.Content.Tiles.Maps;

public class RestorationShrine : BaseShrine
{
	public override int AoE => ModContent.ProjectileType<RestorationAoE>();
}

public class RestorationAoE : ShrineAoE
{
	public override Color Tint => Color.Purple;
	public override int BuffId => ModContent.BuffType<RestorationBuff>();

	public override void SetDefaults()
	{
		base.SetDefaults();

		Projectile.Opacity = 0.1f;
	}
}