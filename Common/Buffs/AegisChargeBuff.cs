using PathOfTerraria.Common.Systems.Charges;
using Terraria.DataStructures;
using Terraria.GameContent;

namespace PathOfTerraria.Common.Buffs;

public class AegisChargeBuff : ModBuff
{
	public override string Texture => "PathOfTerraria/Assets/Buffs/EnduranceChargeBuff";

	public override void SetStaticDefaults()
	{
		Main.buffNoSave[Type] = true;
		Main.buffNoTimeDisplay[Type] = false;
		Main.debuff[Type] = false;
	}

	public override bool ReApply(Player player, int time, int buffIndex)
	{
		// Refresh the buff duration when reapplied
		player.buffTime[buffIndex] = time;
		return false;
	}
	
	public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare)
	{
		Player player = Main.LocalPlayer;
		var chargePlayer = player.GetModPlayer<AegisChargePlayer>();
    
		buffName = "Aegis Charges";
		if (chargePlayer.Charges > 1)
		{
			buffName += $" ({chargePlayer.Charges})";
		}
    
		tip = $"Increases max health by {chargePlayer.Charges * 20} and defense by {chargePlayer.Charges * 5}";
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