namespace PathOfTerraria.Content.Buffs;

internal class IdentifiedDebuff : ModBuff
{
	internal class IdentifiedDebuffNPC : GlobalNPC
	{
		public override Color? GetAlpha(NPC npc, Color drawColor)
		{
			return npc.HasBuff<IdentifiedDebuff>() ? Color.Orange : null;
		}
	}

	public override void SetStaticDefaults()
	{
		Main.debuff[Type] = true;
	}
}
