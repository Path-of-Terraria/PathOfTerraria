using Terraria.ID;

namespace PathOfTerraria.Content.SkillPassives.RainOfArrowsTree;

internal class SporeDebuff : ModBuff
{
	public override void SetStaticDefaults()
	{
		Main.debuff[Type] = true;
		BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
	}
}

internal class SporeNPC : GlobalNPC
{
	public override bool InstancePerEntity => true;

	public int MaxTime = 0;
	public int? Debuff = null;
	public float DoT = 0;
	public float VisualDoT = 0;

	public static void AddSporeDebuff(NPC npc, int damage, int time, bool noReApply)
	{
		if (noReApply && npc.HasBuff<SporeDebuff>())
		{
			return;
		}

		npc.AddBuff(ModContent.BuffType<SporeDebuff>(), time);
		SporeNPC spore = npc.GetGlobalNPC<SporeNPC>();
		spore.Debuff = damage;
		spore.MaxTime = time;
	}

	public override bool PreAI(NPC npc)
	{
		if (npc.HasBuff<SporeDebuff>())
		{
			float newDot = Debuff.Value * 0.1f * (1f / MaxTime);
			DoT += newDot;
			VisualDoT += newDot;
			int dot = (int)DoT;
			npc.life -= dot;
			npc.checkDead();
			DoT -= dot;

			if (VisualDoT > 5)
			{
				CombatText.NewText(npc.Hitbox, new Color(150, 120, 0), (int)VisualDoT);
				VisualDoT -= 5;
			}
		}
		else
		{
			if (VisualDoT > 0)
			{
				CombatText.NewText(npc.Hitbox, new Color(150, 120, 0), (int)VisualDoT);
			}

			Debuff = null;
			DoT = 0;
			VisualDoT = 0;
		}

		return true;
	}
}