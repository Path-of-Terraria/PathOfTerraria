using Terraria.ID;

namespace PathOfTerraria.Content.Buffs;

public sealed class SacrificeReviveCooldown : ModBuff
{
	public override void SetStaticDefaults()
	{
		Main.debuff[Type] = true;
		BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
	}
}