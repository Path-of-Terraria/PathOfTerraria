using Terraria.ID;

namespace PathOfTerraria.Content.Buffs.ShrineBuffs;

internal abstract class ShrineBuff : ModBuff
{
	public abstract int AoEType { get; }

	public override void SetStaticDefaults()
	{
		// This allows for otherwise buff immune NPCs to have this effect
		BuffID.Sets.IsATagBuff[Type] = true;
	}
}
