using PathOfTerraria.Common.Buffs;
using PathOfTerraria.Content.Passives;
using Terraria.DataStructures;
using Terraria.Localization;

namespace PathOfTerraria.Content.Buffs;

public class WideSwingsBuff : ModBuff
{
	public override void Load()
	{
		On_Player.AddBuff += OnAddBuff;
	}

	private void OnAddBuff(On_Player.orig_AddBuff orig, Player self, int type, int timeToAdd, bool quiet, bool foodHack)
	{
		orig(self, type, timeToAdd, quiet, foodHack);

		if (type == ModContent.BuffType<WideSwingsBuff>() && self.TryGetModPlayer(out WideSwingsMastery.WideSwingsPlayer plr) && plr.StacksCount < WideSwingsMastery.MaxStacks)
		{
			plr.AddStack(timeToAdd);
		}
	}

	public override void SetStaticDefaults()
	{
		Main.buffNoTimeDisplay[Type] = true;
	}

	public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare)
	{
		WideSwingsMastery.WideSwingsPlayer plr = Main.LocalPlayer.GetModPlayer<WideSwingsMastery.WideSwingsPlayer>();

		buffName = DisplayName.Value;
    
		if (plr.StacksCount > 1)
		{
			buffName += $" ({plr.StacksCount})";
		}
    
		tip = Language.GetTextValue(Description.Key, plr.StacksCount * 5);
	}

	public override void PostDraw(SpriteBatch spriteBatch, int buffIndex, BuffDrawParams drawParams)
	{
		WideSwingsMastery.WideSwingsPlayer plr = Main.LocalPlayer.GetModPlayer<WideSwingsMastery.WideSwingsPlayer>();
		
		if (plr.StacksCount <= 1)
		{
			return; // Don't draw if 1 charges
		}

		BuffUtils.DrawNumberOverBuff(drawParams, plr.StacksCount.ToString());
	}
}