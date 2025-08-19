using PathOfTerraria.Common.Systems.Charges;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Localization;

namespace PathOfTerraria.Content.Buffs;

public class AegisChargeBuff : ModBuff
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
	
	public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare)
	{
		Player player = Main.LocalPlayer;
		var chargePlayer = player.GetModPlayer<AegisChargePlayer>();
    
		buffName = Language.GetTextValue("Mods.PathOfTerraria.Buffs.AegisChargeBuff.DisplayName");

		if (chargePlayer.Charges > 1)
		{
			buffName += $" ({chargePlayer.Charges})";
		}

		tip = Language.GetTextValue("Mods.PathOfTerraria.Buffs.AegisChargeBuff.Description", chargePlayer.Charges * 20, chargePlayer.Charges * 5);
	}

	public override void PostDraw(SpriteBatch spriteBatch, int buffIndex, BuffDrawParams drawParams)
	{
		Player player = Main.LocalPlayer;
		var chargePlayer = player.GetModPlayer<AegisChargePlayer>();
		
		if (chargePlayer.Charges <= 1) return; // Don't draw if 1 charges
		
		string chargeText = chargePlayer.Charges.ToString();
		Vector2 textSize = FontAssets.MouseText.Value.MeasureString(chargeText);
		
		//Top middle of buff icon
		Vector2 textPosition = new Vector2(
			drawParams.Position.X + 22 - textSize.X - 2,
			drawParams.Position.Y + 20 - textSize.Y - 2 
		);
		
		Utils.DrawBorderString(spriteBatch, chargeText, textPosition, Color.White, 1f);
	}
}