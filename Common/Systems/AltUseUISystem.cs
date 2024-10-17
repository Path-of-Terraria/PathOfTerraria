using System.Collections.Generic;
using ReLogic.Content;
using Terraria.UI;

namespace PathOfTerraria.Common.Systems;

public class AltUseUISystem : ModSystem
{
	public static AltUseUISystem Instance => ModContent.GetInstance<AltUseUISystem>();

	private static Asset<Texture2D> _altBar;

	float _fadeBar = 0;

	public override void Load()
	{
		_altBar = ModContent.Request<Texture2D>("PathOfTerraria/Assets/UI/AltBar");
	}

	public override void Unload()
	{
		_altBar = null;
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
				})
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

		// Check for NaN when the max alt cooldown is 0 (i.e. x/0).
		if (float.IsNaN(factor))
		{
			return;
		}

		Vector2 center = player.Center - Main.screenPosition + Vector2.UnitY * player.height;
		
		Main.spriteBatch.Draw(_altBar.Value, center, new Rectangle(0, 0, 52, 14), Color.White * _fadeBar, 0f, new Vector2(26, 7), 1f, SpriteEffects.None, 0);

		Rectangle barSrc = new Rectangle(4, 14, (int)(44 * factor), 14);
		Main.spriteBatch.Draw(_altBar.Value, center + new Vector2(4, 0), barSrc, Color.White * _fadeBar, 0f, new Vector2(26, 7), 1f, SpriteEffects.None, 0);
	}
}
