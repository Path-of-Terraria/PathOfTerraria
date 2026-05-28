using System.Collections.Generic;
using PathOfTerraria.Common.Systems.EnergyShield;
using PathOfTerraria.Core.UI.SmartUI;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.Misc;

internal class EnergyShieldUI : SmartUiState
{
	public override bool Visible => true;

	public override int InsertionIndex(List<GameInterfaceLayer> layers)
	{
		return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars"));
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		Player player = Main.LocalPlayer;
		EnergyShieldPlayer energyShield = player.GetModPlayer<EnergyShieldPlayer>();

		if (energyShield.MaximumEnergyShield <= 0)
		{
			return;
		}

		Texture2D emptyTexture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/Heart_Fill_Vanilla").Value;
		Texture2D fillTexture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/Heart_Fill_Replace").Value;

		int max = (int)Math.Ceiling(energyShield.MaximumEnergyShield / 20f);
		int lifeRows = Math.Max(1, (int)Math.Ceiling(player.statLifeMax2 / 200f));
		Color emptyColor = new Color(20, 70, 110) * 0.65f;
		Color fillColor = new Color(92, 210, 255);

		for (int i = 0; i < max; ++i)
		{
			Vector2 position = new(Main.screenWidth - 292 + i % 10f * 24, 28 * (lifeRows + i / 10 + 1));
			spriteBatch.Draw(emptyTexture, position, null, emptyColor, 0f, emptyTexture.Size() / 2f, 1f, SpriteEffects.None, 0);

			float remaining = energyShield.CurrentEnergyShield - i * 20f;
			if (remaining <= 0)
			{
				continue;
			}

			float scale = MathHelper.Clamp(remaining / 20f, 0f, 1f);
			spriteBatch.Draw(fillTexture, position, null, fillColor * scale, 0f, fillTexture.Size() / 2f, scale, SpriteEffects.None, 0);
		}
	}
}
