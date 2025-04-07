using PathOfTerraria.Content.Buffs.ShrineBuffs;
using PathOfTerraria.Content.Projectiles.Utility;

namespace PathOfTerraria.Content.Tiles.Maps;

public class DefenseShrine : BaseShrine
{
	public override int AoE => ModContent.ProjectileType<DefenseAoE>();
}

public class DefenseAoE : ShrineAoE
{
	public override Color Tint => Color.White;
	public override int BuffId => ModContent.BuffType<HardinessBuff>();
}
