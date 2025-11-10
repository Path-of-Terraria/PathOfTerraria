namespace PathOfTerraria.Content.Buffs;

public sealed class ArcaneSurgeBuff : ModBuff
{
	public override void SetStaticDefaults()
	{
		Main.buffNoSave[Type] = true;
	}

	public override void Update(Player player, ref int buffIndex)
	{
		player.GetDamage(DamageClass.Magic) += 0.15f;
		player.manaRegenBonus += 2;
	}
}