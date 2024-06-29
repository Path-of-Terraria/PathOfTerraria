using PathOfTerraria.Core.Loaders.UILoading;
using PathOfTerraria.Core.Systems;
using PathOfTerraria.Core.Systems.ModPlayers;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace PathOfTerraria.Content.GUI.PlayerStats;

internal class PlayerStatInnerPanel : SmartUIElement
{
	private UIElement Panel => Parent;

	public override string TabName => "PlayerStats";

	private UICharacter _drawDummy = null;
	private int _offset = 0;

	public override void Draw(SpriteBatch spriteBatch)
	{
		_offset = 0;
		
		DrawBack(spriteBatch);
		SetAndDrawPlayer(spriteBatch);

		PotionSystem potionPlayer = Main.LocalPlayer.GetModPlayer<PotionSystem>();
		ExpModPlayer expPlayer = Main.LocalPlayer.GetModPlayer<ExpModPlayer>();
		string playerLine = $"{Main.LocalPlayer.name}";
		Utils.DrawBorderStringBig(spriteBatch, playerLine, GetRectangle().Center() + new Vector2(0, -60), Color.White, 0.7f, 0.5f, 0.35f);

		float expPercent = expPlayer.Exp / (float)expPlayer.NextLevel * 100;
		DrawSingleStat(spriteBatch, $"Level: {expPlayer.Level}");
		DrawSingleStat(spriteBatch, $"Exp: {expPlayer.Exp}/{expPlayer.NextLevel} ({expPercent:#0.##}%)");
		float lifePercent = Main.LocalPlayer.statLife / Main.LocalPlayer.statLifeMax2 * 100;
		DrawSingleStat(spriteBatch, $"Life: {Main.LocalPlayer.statLife}/{Main.LocalPlayer.statLifeMax2} ({lifePercent:#0.##}%)");
		float manaPercent = Main.LocalPlayer.statMana / Main.LocalPlayer.statManaMax * 100;
		DrawSingleStat(spriteBatch, $"Mana: {Main.LocalPlayer.statMana}/{Main.LocalPlayer.statManaMax2} ({manaPercent:#0.##}%)");
		DrawSingleStat(spriteBatch, $"Health Potions: {potionPlayer.HealingLeft}/{potionPlayer.MaxHealing}");
		DrawSingleStat(spriteBatch, $"Mana Potions: {potionPlayer.ManaLeft}/{potionPlayer.MaxMana}");
		DrawSingleStat(spriteBatch, $"Damage Reduction: {Main.LocalPlayer.endurance:#0.##}%");
	}

	private void DrawBack(SpriteBatch spriteBatch)
	{
		Texture2D chain = ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/GUI/PlayerStatBackChain").Value;
		Texture2D tex = ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/GUI/PlayerStatBack").Value;

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

		_drawDummy.Draw(spriteBatch);
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