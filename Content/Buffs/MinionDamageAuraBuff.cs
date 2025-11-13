namespace PathOfTerraria.Content.Buffs;

public sealed class MinionDamageAuraBuff : ModBuff
{
	public override void SetStaticDefaults()
	{
		Main.buffNoSave[Type] = true;
		Main.buffNoTimeDisplay[Type] = true;
	}
}