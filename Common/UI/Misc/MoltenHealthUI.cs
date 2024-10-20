using System.Collections.Generic;
using PathOfTerraria.Content.Buffs;
using PathOfTerraria.Core.UI.SmartUI;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.Misc;

internal class MoltenHealthUI : SmartUiState
{
	public override bool Visible => true;

	public override int InsertionIndex(List<GameInterfaceLayer> layers)
	{
		return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars"));
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		MoltenShieldBuff.MoltenShieldPlayer shieldPlayer = Main.LocalPlayer.GetModPlayer<MoltenShieldBuff.MoltenShieldPlayer>();

		if (shieldPlayer.HealthBuffer <= 0)
		{
			return;
		}

		Texture2D moltenShield = ModContent.Request<Texture2D>("PathOfTerraria/Assets/Skills/MoltenShieldUI").Value;
		int max = (int)Math.Ceiling(shieldPlayer.HealthBuffer / 5f);

		for (int i = 0; i < max; ++i)
		{
			Color color = Color.White;

			if (i == max - 1)
			{
				color = Color.White * ((shieldPlayer.HealthBuffer - 5 * i) / 5f);
			}

			spriteBatch.Draw(moltenShield, new Vector2(Main.screenWidth - 296 + i * 26, 24), null, color, 0f, moltenShield.Size() / 2f, 1f, SpriteEffects.None, 0);
		}
	}
}