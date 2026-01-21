namespace PathOfTerraria.Content.Buffs;

public class PrismaticOrbBuff : ModBuff
{
	public override void SetStaticDefaults()
	{
		Main.buffNoSave[Type] = true; 
		Main.buffNoTimeDisplay[Type] = false; 
	}

	public override void Update(Player player, ref int buffIndex)
	{
		// Buff effect is handled in the projectile code
	}
}
