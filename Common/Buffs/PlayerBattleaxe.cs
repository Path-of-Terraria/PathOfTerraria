using PathOfTerraria.Content.Buffs;
using Terraria.DataStructures;

namespace PathOfTerraria.Common.Buffs;

public sealed class BattleaxePlayer : ModPlayer
{
	public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
	{
		if (!Player.HasBuff(ModContent.BuffType<Battleaxe>()))
		{
			return;
		}
		
		drawInfo.colorArmorBody = Color.Red;
		drawInfo.colorArmorLegs = Color.Red;
		drawInfo.colorArmorHead = Color.Red;
		drawInfo.colorHair = Color.Red;
		drawInfo.colorEyeWhites = Color.Red;
		drawInfo.colorEyes = Color.Red;
		drawInfo.colorHead = Color.Red;
		drawInfo.colorBodySkin = Color.Red;
		drawInfo.colorLegs = Color.Red;
	}
}
