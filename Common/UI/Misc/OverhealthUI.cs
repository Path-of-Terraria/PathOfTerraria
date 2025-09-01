using System.Collections.Generic;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Core.UI.SmartUI;
using Terraria.GameContent.UI.ResourceSets;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.Misc;

internal class OverhealthUI : SmartUiState
{
	public override bool Visible => true;

	public override int InsertionIndex(List<GameInterfaceLayer> layers)
	{
		return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars"));
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		OverhealthPlayer overhealthPlayer = Main.LocalPlayer.GetModPlayer<OverhealthPlayer>();

		if (overhealthPlayer.Overhealth <= 0)
		{
			return;
		}

		var texture = (Texture2D)ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/Heart_Fill_Purple");
		int max = (int)Math.Ceiling(overhealthPlayer.Overhealth / 20f);

		for (int i = 0; i < max; ++i)
		{
			Color color = Color.White;
			float scale = 1f;

			if (i == max - 1)
			{
				scale = overhealthPlayer.Overhealth % 20 / 20f;
				color = Color.White * scale;

				scale += MathF.Abs(MathF.Sin(Main.GameUpdateCount * 0.02f) * 0.25f);
			}

			int y = 28 * (int)(i / 10f + 1);
			spriteBatch.Draw(texture, new Vector2(Main.screenWidth - 292 + i % 10f * 24, y), null, color, 0f, texture.Size() / 2f, scale, SpriteEffects.None, 0);
		}
	}
}