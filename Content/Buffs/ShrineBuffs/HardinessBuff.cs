using PathOfTerraria.Common.Systems.BlockSystem;
using PathOfTerraria.Content.Tiles.Maps;
using Terraria.ID;

namespace PathOfTerraria.Content.Buffs.ShrineBuffs;

internal class HardinessBuff : ShrineBuff
{
	public override int AoEType => ModContent.ProjectileType<DefenseAoE>();

	public override void Update(NPC npc, ref int buffIndex)
	{
		npc.defense = npc.defDefense + 10;
	}

	public override void Update(Player player, ref int buffIndex)
	{
		player.statDefense += 10;
		player.GetModPlayer<BlockPlayer>().AddBlockChance(0.2f);
	}
}
