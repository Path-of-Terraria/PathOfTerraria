using Terraria.ID;

namespace PathOfTerraria.Content.Buffs.ElementalBuffs;

internal class FreezeDebuff : ModBuff
{
	public override void SetStaticDefaults()
	{
		Main.debuff[Type] = true;
		BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
	}

	public override void Update(NPC npc, ref int buffIndex)
	{
		npc.GetGlobalNPC<FreezeNPC>().Frozen = true;
	}
}

internal class FreezeNPC : GlobalNPC
{
	public override bool InstancePerEntity => true;

	public bool Frozen = false;

	public override void ResetEffects(NPC npc)
	{
		Frozen = false;
	}

	public override bool PreAI(NPC npc)
	{
		if (Frozen)
		{
			npc.position -= npc.velocity;
			return false;
		}

		return true;
	}

	public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		if (Frozen && !FrozenNPCBatching.Drawing)
		{
			FrozenNPCBatching.CachedNPCs.Enqueue(npc.whoAmI);
			return false;
		}

		return Frozen == FrozenNPCBatching.Drawing;
	}
}
