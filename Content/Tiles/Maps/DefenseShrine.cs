using PathOfTerraria.Content.Projectiles.Utility;
using Terraria.ID;

namespace PathOfTerraria.Content.Tiles.Maps;

public class DefenseShrine : BaseShrine
{
	public override int AoE => ModContent.ProjectileType<UnstoppableAoE>();
}

public class DefenseAoE : ShrineAoE
{
	public override Color Tint => Color.Gray;
	public override int BuffId => BuffID.Ironskin;
}
