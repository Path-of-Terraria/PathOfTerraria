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
		Main.buffNoTimeDisplay[Type] = true;
		Main.debuff[Type] = false;
	}

	public override bool ReApply(Player player, int time, int buffIndex)
	{
		return true;
	}

	public override void Update(Player player, ref int buffIndex)
	{
		if (player.GetModPlayer<RagePlayer>().Rage > 0f)
		{
			player.buffTime[buffIndex] = 2;
		}
		else
		{
			player.DelBuff(buffIndex);
			buffIndex--;
		}
	}
	
	public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare)
	{
		Player player = Main.LocalPlayer;
		RagePlayer plr = player.GetModPlayer<RagePlayer>();
		int displayedRage = GetDisplayedRage(plr);

		buffName = $"{DisplayName.Value} ({displayedRage})";
		tip = Language.GetTextValue(Description.Key, displayedRage);
	}

	public override void PostDraw(SpriteBatch spriteBatch, int buffIndex, BuffDrawParams drawParams)
	{
		Player player = Main.LocalPlayer;
		RagePlayer plr = player.GetModPlayer<RagePlayer>();
		
		if (plr.Rage <= 0)
		{
			return;
		}

		BuffUtils.DrawNumberOverBuff(drawParams, GetDisplayedRage(plr).ToString());
	}

	private static int GetDisplayedRage(RagePlayer player)
	{
		return Math.Max(1, (int)player.Rage);
	}
}
