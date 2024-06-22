using ReLogic.Content;
using System.Collections.Generic;
using Terraria.UI;

namespace PathOfTerraria.Core.Systems;

public class AltUseSystem : ModSystem
{
	public static AltUseSystem Instance => ModContent.GetInstance<AltUseSystem>();

	private static Asset<Texture2D> AltBar = null;

	float _fadeBar = 0;

	public override void Load()
	{
		AltBar = ModContent.Request<Texture2D>("PathOfTerraria/Assets/GUI/AltBar");
	}

	public override void Unload()
	{
		AltBar.Dispose();
		AltBar = null;
	}

	public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
	{
		int resourceBarIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars"));

		if (resourceBarIndex != -1)
		{
			layers.Insert(resourceBarIndex, new LegacyGameInterfaceLayer(
				"PathOfTerraria: Alt Bar UI",
				delegate
				{
					DrawChargeBar();
					return true;
				},
				InterfaceScaleType.UI)
			);
		}
	}

	private void DrawChargeBar()
	{
		Player player = Main.LocalPlayer;
		AltUsePlayer alt = player.GetModPlayer<AltUsePlayer>();

		if (player.GetModPlayer<AltUsePlayer>().AltFunctionCooldown > 0)
		{
			_fadeBar = MathHelper.Lerp(_fadeBar, 1, 0.2f);
		}
		else
		{
			_fadeBar *= 0.95f;
		}

		if (_fadeBar <= 0)
		{
			return;
		}

		float factor = alt.AltFunctionCooldown / (float)alt.MaxAltCooldown;
		Vector2 center = new Vector2(Main.screenWidth, Main.screenHeight) / 2f + Vector2.UnitY * player.height;

		Main.spriteBatch.Draw(AltBar.Value, center, new Rectangle(0, 0, 52, 14), Color.White * _fadeBar, 0f, new Vector2(26, 7), 1f, SpriteEffects.None, 0);

		Rectangle barSrc = new Rectangle(4, 14, (int)(44 * factor), 14);
		Main.spriteBatch.Draw(AltBar.Value, center + new Vector2(4, 0), barSrc, Color.White * _fadeBar, 0f, new Vector2(26, 7), 1f, SpriteEffects.None, 0);
	}
}