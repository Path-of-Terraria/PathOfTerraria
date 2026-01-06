using PathOfTerraria.Common.Buffs;
using PathOfTerraria.Common.Systems.ModPlayers;
using Terraria.DataStructures;
using Terraria.Localization;

namespace PathOfTerraria.Content.Buffs;

public class RageStacksBuff : ModBuff
{
	public override void SetStaticDefaults()
	{
		Main.buffNoSave[Type] = true;
		Main.buffNoTimeDisplay[Type] = false;
		Main.debuff[Type] = false;
	}

	public override bool ReApply(Player player, int time, int buffIndex)
	{
		return true;
	}

	public override void Update(Player player, ref int buffIndex)
	{
		player.buffTime[buffIndex]++;

		if (player.GetModPlayer<RagePlayer>().Rage <= 0f)
		{
			player.DelBuff(buffIndex);
			buffIndex--;
		}
	}
	
	public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare)
	{
		Player player = Main.LocalPlayer;
		RagePlayer plr = player.GetModPlayer<RagePlayer>();

		buffName = DisplayName.Value;

		if (plr.Rage > 1)
		{
			buffName += $" ({(int)plr.Rage})";
		}

		tip = Language.GetTextValue(Description.Key, (int)plr.Rage);
	}

	public override void PostDraw(SpriteBatch spriteBatch, int buffIndex, BuffDrawParams drawParams)
	{
		Player player = Main.LocalPlayer;
		RagePlayer plr = player.GetModPlayer<RagePlayer>();
		
		if (plr.Rage <= 1)
		{
			return; // Don't draw if 1 charges
		}

		BuffUtils.DrawNumberOverBuff(drawParams, ((int)plr.Rage).ToString());
	}
}