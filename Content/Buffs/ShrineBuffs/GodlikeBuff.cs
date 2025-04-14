using Terraria.ID;

namespace PathOfTerraria.Content.Buffs.ShrineBuffs;

internal class GodlikeBuff : ModBuff
{
	public override void SetStaticDefaults()
	{
		// This allows for otherwise buff immune NPCs to have this effect
		BuffID.Sets.IsATagBuff[Type] = true;
	}

	public override void Update(NPC npc, ref int buffIndex)
	{
		if (npc.buffTime[buffIndex] >= 1)
		{
			npc.dontTakeDamage = true;
		}
		else
		{
			npc.dontTakeDamage = ContentSamples.NpcsByNetId[npc.type].dontTakeDamage;
		}
	}

	public class GodlikePlayer : ModPlayer
	{
		public override bool FreeDodge(Player.HurtInfo info)
		{
			return Player.HasBuff<GodlikeBuff>();
		}
	}
}
