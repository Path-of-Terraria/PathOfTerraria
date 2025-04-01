using PathOfTerraria.Content.Projectiles.Utility;
using Terraria.ID;

namespace PathOfTerraria.Content.Tiles.Maps;

public class UnstoppableShrine : BaseShrine
{
	public override int AoE => ModContent.ProjectileType<UnstoppableAoE>();
}

public class UnstoppableAoE : ShrineAoE
{
	public override Color Tint => Color.Orange;
	public override int BuffId => BuffID.Ironskin;
}