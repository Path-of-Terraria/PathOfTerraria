using PathOfTerraria.Common.Subworlds;
using ReLogic.Content;
using ReLogic.Graphics;
using SubworldLibrary;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI.Chat;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.UI;

internal static class SubworldLoadingScreen
{
	public static Asset<Texture2D> HideGradientTex = null;

	private static int _walkTimer = 0;
	private static string _tip = "";
	private static string _fadingInTip = "";
	private static int _tipTime = 0;

	internal static void DrawLoading(MappingWorld mappingWorld)
	{
		string statusText = Main.statusText;
		GenerationProgress progress = WorldGenerator.CurrentGenerationProgress;

		Main.spriteBatch.End();
		Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);

		DrawWalkingBackground(mappingWorld);

		Main.spriteBatch.End();
		Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);

		if (SubworldSystem.Current is not null)
		{
			DrawStringCentered(Language.GetTextValue("Mods.PathOfTerraria.Subworlds.Entering"), Color.LightGray, new Vector2(0, -360), 0.4f, true);
			DrawStringCentered(mappingWorld.SubworldName.Value, Color.White, new Vector2(0, -310), 1.1f, true);
			DrawStringCentered(mappingWorld.SubworldDescription.Value, Color.White, new Vector2(0, -250), 0.5f, true);
		}
		else
		{
			DrawStringCentered(Language.GetTextValue("Mods.PathOfTerraria.Subworlds.Exiting"), Color.White, new Vector2(0, -310), 0.9f, true);
		}

		DrawWalkingPlayer();

		if (WorldGen.gen && progress is not null)
		{
			DrawStringCentered(progress.Message, Color.LightGray, new Vector2(0, 150), 0.6f);
			double percentage = progress.Value / progress.CurrentPassWeight * 100f;
			DrawStringCentered($"{percentage:#0.##}%", Color.LightGray, new Vector2(0, 190), 0.7f);
		}

		DrawStringCentered(statusText, Color.White, new Vector2(0, 90));

		if (_tip == "")
		{
			SetTip();
			_tipTime = 0;
		}

		_tipTime++;

		const int TipOffset = 438;

		DrawStringCentered(Language.GetTextValue("Mods.PathOfTerraria.UI.Tips.Title"), Color.White, new Vector2(0, TipOffset - 38), 0.8f);

		if (_tipTime > 300)
		{
			float factor = (_tipTime - 300) / 60f;
			DrawStringCentered(_tip, Color.White * (1 - factor), new Vector2(0, TipOffset), 0.5f);
			DrawStringCentered(_fadingInTip, Color.White * factor, new Vector2(0, TipOffset), 0.5f);

			if (_tipTime == 360)
			{
				SetTip(_fadingInTip);
				_tipTime = 0;
			}
		}
		else
		{
			DrawStringCentered(_tip, Color.White, new Vector2(0, TipOffset), 0.5f);
		}
	}

	private static void DrawWalkingBackground(MappingWorld world)
	{
		HideGradientTex ??= ModContent.Request<Texture2D>("PathOfTerraria/Assets/UI/SubworldLoadScreens/HideGradient");

		_walkTimer++;

		var position = new Vector2(-_walkTimer * 1.3f, Main.screenHeight / 2 + 14);
		var originMod = new Vector2(0, 1);

		if (SubworldSystem.Current is null)
		{
			position = new Vector2(_walkTimer * 1.3f, Main.screenHeight / 2 + 14);
			originMod = new Vector2(1, 1);
		}

		if (world.LoadingBackgrounds.Length == 0)
		{
			return;
		}

		if (world.ScrollingBackgroundCount == 1)
		{
			Texture2D tex = world.LoadingBackgrounds[0].Value;
			Main.spriteBatch.Draw(tex, position, new Rectangle(0, 0, tex.Width * 300, tex.Height), Color.White, 0f, tex.Size() * originMod, 1f, SpriteEffects.None, 0);
		}
		else
		{
			int xOff = 0;

			for (int i = 0; i < 10; ++i)
			{
				Texture2D texture = world.LoadingBackgrounds[i % world.ScrollingBackgroundCount].Value;
				Main.spriteBatch.Draw(texture, position, null, Color.White, 0f, texture.Size() * originMod, 1f, SpriteEffects.None, 0);
				xOff += texture.Width;
			}
		}

		Texture2D hide = HideGradientTex.Value;
		var hideSrc = new Rectangle(0, 0, hide.Width * 300, hide.Height);
		Main.spriteBatch.Draw(hide, position with { X = 0 }, hideSrc, Color.White, 0f, hide.Size() * new Vector2(0, 1), 1f, SpriteEffects.None, 0);
	}

	private static void DrawWalkingPlayer()
	{
		Player plr = Main.LocalPlayer;
		using var _currentPlr = new Main.CurrentPlayerOverride(plr);

		plr.direction = SubworldSystem.Current is not null ? 1 : -1;
		plr.ResetEffects();
		plr.ResetVisibleAccessories();
		plr.UpdateMiscCounter();
		plr.UpdateDyes();
		plr.PlayerFrame();

		int num = (int)(Main.GlobalTimeWrappedHourly / 0.07f) % 14 + 6;
		plr.bodyFrame.Y = (plr.legFrame.Y = (plr.headFrame.Y = num * 56));
		plr.wings = 0;

		if (plr.mount.Active)
		{
			plr.QuickMount();
		}

		plr.WingFrame(wingFlap: false);

		Item item = plr.inventory[plr.selectedItem];
		plr.inventory[plr.selectedItem] = new Item(ItemID.None);
		Main.PlayerRenderer.DrawPlayer(Main.Camera, plr, new Vector2(Main.screenWidth / 2f, Main.screenHeight / 2 - 126) + Main.screenPosition, 0f, Vector2.Zero, 0f, 1f);
		plr.inventory[plr.selectedItem] = item;
	}

	/// <summary>
	/// Sets the tip to <paramref name="text"/>, or if <paramref name="text"/> is null, any random tip.
	/// </summary>
	private static void SetTip(string text = null)
	{
		const int MaxTips = 31;

		_tip = text ?? Language.GetTextValue("Mods.PathOfTerraria.UI.Tips." + Main.rand.Next(MaxTips));

		do
		{
			_fadingInTip = Language.GetTextValue("Mods.PathOfTerraria.UI.Tips." + Main.rand.Next(MaxTips));
		} while (_tip == _fadingInTip);
	}

	private static void DrawStringCentered(string test, Color color, Vector2 position = default, float scale = 1f, bool outlined = false)
	{
		Vector2 screenCenter = new Vector2(Main.screenWidth, Main.screenHeight) / 2f + position;
		DynamicSpriteFont font = FontAssets.DeathText.Value;
		Vector2 halfSize = font.MeasureString(test) / 2f * scale;

		if (!outlined)
		{
			Main.spriteBatch.DrawString(font, test, screenCenter - halfSize, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0);
		}
		else
		{
			const int Offset = 6;

			Color shadowColor = Color.Black;
			Color textColor = Color.White;

			Vector2 drawPos = screenCenter - halfSize;
			ChatManager.DrawColorCodedStringShadow(Main.spriteBatch, font, test, drawPos - new Vector2(Offset, 0), shadowColor, 0f, Vector2.Zero, new Vector2(scale));
			ChatManager.DrawColorCodedStringShadow(Main.spriteBatch, font, test, drawPos - new Vector2(0, Offset), shadowColor, 0f, Vector2.Zero, new Vector2(scale));
			ChatManager.DrawColorCodedStringShadow(Main.spriteBatch, font, test, drawPos + new Vector2(Offset, 0), shadowColor, 0f, Vector2.Zero, new Vector2(scale));
			ChatManager.DrawColorCodedStringShadow(Main.spriteBatch, font, test, drawPos + new Vector2(0, Offset), shadowColor, 0f, Vector2.Zero, new Vector2(scale));
			ChatManager.DrawColorCodedString(Main.spriteBatch, font, test, drawPos, textColor, 0f, Vector2.Zero, new Vector2(scale));
		}
	}
}
