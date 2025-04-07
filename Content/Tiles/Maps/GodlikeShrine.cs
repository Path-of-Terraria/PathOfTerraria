using PathOfTerraria.Content.Buffs.ShrineBuffs;
using PathOfTerraria.Content.Projectiles.Utility;

namespace PathOfTerraria.Content.Tiles.Maps;

public class GodlikeShrine : BaseShrine
{
	public override int AoE => ModContent.ProjectileType<GodlikeAoE>();
}

public class GodlikeAoE : ShrineAoE
{
	public override Color Tint => Color.Yellow;
	public override int BuffId => ModContent.BuffType<GodlikeBuff>();
}