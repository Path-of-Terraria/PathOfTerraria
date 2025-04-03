using PathOfTerraria.Common.Systems.BlockSystem;
using Terraria.ID;

namespace PathOfTerraria.Content.Buffs.ShrineBuffs;

internal class HardinessBuff : ModBuff
{
	public override void SetStaticDefaults()
	{
		// This allows for otherwise buff immune NPCs to have this effect
		BuffID.Sets.IsATagBuff[Type] = true;
	}

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
