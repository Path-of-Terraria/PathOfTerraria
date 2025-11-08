using PathOfTerraria.Common.Buffs;
using PathOfTerraria.Content.Passives.Magic.Masteries;
using Terraria.DataStructures;

namespace PathOfTerraria.Content.Buffs;

public class ElectrostaticBuff : ModBuff
{
	public override void SetStaticDefaults()
	{
		Main.buffNoSave[Type] = true;
		Main.buffNoTimeDisplay[Type] = false;
		Main.debuff[Type] = false;
	}

	public override void PostDraw(SpriteBatch spriteBatch, int buffIndex, BuffDrawParams drawParams)
	{
		Player player = Main.LocalPlayer;
		ElectrostaticAttractionMastery.ElectrostaticPlayer plr = player.GetModPlayer<ElectrostaticAttractionMastery.ElectrostaticPlayer>();
		
		if (plr.StackCount <= 1)
		{
			return;
		}

		BuffUtils.DrawNumberOverBuff(drawParams, plr.StackCount.ToString(), (BuffDrawParams _, ref Vector2 positiom, ref Color color, ref float scale, ref string str) =>
		{
			if (plr.StackCount == 10)
			{
				color = Color.Yellow;
				scale = 1.15f + MathF.Sin(Main.GameUpdateCount * 0.05f) * 0.15f;
			}
		});
	}
}