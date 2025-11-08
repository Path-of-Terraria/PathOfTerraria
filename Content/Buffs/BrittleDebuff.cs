namespace PathOfTerraria.Content.Buffs;

public class BrittleDebuff : ModBuff
{
	private class BrittleNPC : GlobalNPC
	{
		public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
		{
			if (npc.HasBuff<BrittleDebuff>())
			{
				modifiers.FinalDamage += 0.25f;
			}
		}
	}

	public override void SetStaticDefaults()
	{
		Main.buffNoSave[Type] = true;
		Main.debuff[Type] = false;
	}
}