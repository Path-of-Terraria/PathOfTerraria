using PathOfTerraria.Common.Systems;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Core.UI.SmartUI;
using ReLogic.Content;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.PlayerStats;

internal class PlayerStatInnerPanel : SmartUiElement
{
	public static Asset<Texture2D> ChainTex = null;
	public static Asset<Texture2D> BackTex = null;

	private UIElement Panel => Parent;

	public override string TabName => "PlayerStats";

	private UICharacter _drawDummy = null;
	private int _offset = 0;

	public PlayerStatInnerPanel()
	{
		ChainTex ??= ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/PlayerStatBackChain");
		BackTex ??= ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/PlayerStatBack");
	}

	public override void SafeMouseOver(UIMouseEvent evt)
	{
		Main.blockMouse = true;
		Main.isMouseLeftConsumedByUI = true;
		Main.LocalPlayer.mouseInterface = true;
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		_offset = 0;
		
		DrawBack(spriteBatch);
		SetAndDrawPlayer(spriteBatch);

		PotionSystem potionPlayer = Main.LocalPlayer.GetModPlayer<PotionSystem>();
		ExpModPlayer expPlayer = Main.LocalPlayer.GetModPlayer<ExpModPlayer>();
		string playerLine = Main.LocalPlayer.name;
		Utils.DrawBorderStringBig(spriteBatch, playerLine, GetRectangle().Center() + new Vector2(0, -60), Color.White, 0.7f, 0.5f, 0.35f);

		float expPercent = expPlayer.Exp / (float)expPlayer.NextLevel * 100;
		DrawSingleStat(spriteBatch, $"{GetStatLocalization("Level")}: {expPlayer.Level}");
		DrawSingleStat(spriteBatch, $"{GetStatLocalization("Experience")}: {expPlayer.Exp}/{expPlayer.NextLevel} ({expPercent:#0.##}%)");
		float lifePercent = Main.LocalPlayer.statLife / Main.LocalPlayer.statLifeMax2 * 100;
		DrawSingleStat(spriteBatch, $"{GetStatLocalization("Life")}: {Main.LocalPlayer.statLife}/{Main.LocalPlayer.statLifeMax2} ({lifePercent:#0.##}%)");
		float manaPercent = Main.LocalPlayer.statMana / Main.LocalPlayer.statManaMax * 100;
		DrawSingleStat(spriteBatch, $"{GetStatLocalization("Mana")}: {Main.LocalPlayer.statMana}/{Main.LocalPlayer.statManaMax2} ({manaPercent:#0.##}%)");
		DrawSingleStat(spriteBatch, $"{GetStatLocalization("HealthPotions")}: {potionPlayer.HealingLeft}/{potionPlayer.MaxHealing}");
		DrawSingleStat(spriteBatch, $"{GetStatLocalization("ManaPotions")}: {potionPlayer.ManaLeft}/{potionPlayer.MaxMana}");
		DrawSingleStat(spriteBatch, $"{GetStatLocalization("DamageReduction")}: {Main.LocalPlayer.endurance:#0.##}%");

#if DEBUG
		GUIDebuggingTools.DrawGuiBorder(spriteBatch, this, Color.LavenderBlush);
#endif

		// Shorthand for localization here
		static string GetStatLocalization(string type)
		{
			return Language.GetTextValue($"Mods.{PoTMod.ModName}.UI.StatUI." + type);
		}
	}

	private void DrawBack(SpriteBatch spriteBatch)
	{
		Texture2D chain = ChainTex.Value;
		Texture2D tex = BackTex.Value;

		for (int i = 0; i < 9; ++i)
		{
			Color color = Color.White;
			float yOff = i * chain.Height + tex.Height / 2.2f;

			if (i > 4)
			{
				color *= 1 - (i - 4) / 5f;
			}

			spriteBatch.Draw(chain, GetRectangle().Center() - new Vector2(180, yOff), null, color, 0f, chain.Size() / 2f, 1f, SpriteEffects.None, 0);
			spriteBatch.Draw(chain, GetRectangle().Center() - new Vector2(-180, yOff), null, color, 0f, chain.Size() / 2f, 1f, SpriteEffects.None, 0);
		}

		spriteBatch.Draw(tex, GetRectangle().Center(), null, Color.White, 0f, tex.Size() / 2f, 1f, SpriteEffects.None, 0);
	}

	private void SetAndDrawPlayer(SpriteBatch spriteBatch)
	{
		if (_drawDummy == null)
		{
			_drawDummy = new UICharacter(Main.LocalPlayer, true, true, 1, true)
			{
				Width = StyleDimension.FromPixels(60),
				Height = StyleDimension.FromPixels(60),
				HAlign = 0.5f,
				Top = StyleDimension.FromPixels(60)
			};
			Append(_drawDummy);
			Recalculate();
		}

		// The correct approach at correctly rendering the player in the stat
		// panel would be the following (with drawingPlayer defined):
		/*private sealed class StatPanelRendererPlayer : ModPlayer
		{
			public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
			{
				base.DrawEffects(drawInfo, ref r, ref g, ref b, ref a, ref fullBright);

				// Force fullBright if rendering the player so hair draws
				// correctly.
				if (drawingPlayer)
				{
					fullBright = true;
				}
			}
		}*/
		// We aren't so lucky, though. Instead, we have to trick
		// Lighting::GetColor into thinking we're in the game menu so it always
		// returns White instead of the real color. This emulates full-bright
		// (ignores in-world lighting) until we can correctly set the
		// `fullBright` parameter in Player::DrawEffects (see: TML-4317).

		bool origGameMenu = Main.gameMenu;
		Main.gameMenu = true;
		_drawDummy.Draw(spriteBatch);
		Main.gameMenu = origGameMenu;
	}

	private void DrawSingleStat(SpriteBatch spriteBatch, string text)
	{
		Utils.DrawBorderStringBig(spriteBatch, text, GetRectangle().Center() + new Vector2(0, -20 + 30 * _offset), Color.White, 0.5f, 0.5f, 0.35f);
		_offset++;
	}

	private Rectangle GetRectangle()
	{
		return Panel.GetDimensions().ToRectangle();
	}
}