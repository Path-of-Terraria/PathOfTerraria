using PathOfTerraria.Common.Buffs;
using PathOfTerraria.Content.Passives.Magic.Masteries;
using Terraria.DataStructures;

namespace PathOfTerraria.Content.Buffs;

public class SingleFocusBuff : ModBuff
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
		ChannelingFunctionalityPlayer plr = player.GetModPlayer<ChannelingFunctionalityPlayer>();
		int stacks = Math.Min(plr.ChannelTimer / 30, 10);

		if (plr.ChannelTimer <= 1)
		{
			return;
		}

		BuffUtils.DrawNumberOverBuff(drawParams, stacks.ToString(), (BuffDrawParams _, ref Vector2 positiom, ref Color color, ref float scale, ref string str) =>
		{
			if (stacks == 10)
			{
				color = Color.Yellow;
				scale = 1.15f + MathF.Sin(Main.GameUpdateCount * 0.05f) * 0.15f;
			}
		});
	}
}