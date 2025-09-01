using PathOfTerraria.Content.Buffs.ShrineBuffs;
using PathOfTerraria.Content.Projectiles.Utility;
using Terraria.ID;

namespace PathOfTerraria.Content.Tiles.Maps;

public class HasteShrine : BaseShrine
{
	public override int AoE => ModContent.ProjectileType<HasteAoE>();
}

public class HasteAoE : ShrineAoE
{
	public override Color Tint => Color.Green;
	public override int BuffId => ModContent.BuffType<HasteBuff>();
}