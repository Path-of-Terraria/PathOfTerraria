using Terraria.ID;

namespace PathOfTerraria.Content.Buffs;

internal class ToxicSmogImmunityBuff : ModBuff
{
	public override void SetStaticDefaults()
	{
		Main.debuff[Type] = true;

		BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
	}
}
