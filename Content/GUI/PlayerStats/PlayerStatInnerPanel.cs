using Microsoft.Xna.Framework.Graphics;
using PathOfTerraria.Core.Loaders.UILoading;
using PathOfTerraria.Core.Systems;
using PathOfTerraria.Core.Systems.ModPlayers;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.UI;

namespace PathOfTerraria.Content.GUI.PlayerStats;

internal class PlayerStatInnerPanel : SmartUIElement
{
	private UIElement Panel => Parent;
	private Player Player => Main.LocalPlayer;

	public override string TabName => "PlayerStats";

	private UICharacter _drawDummy = null;
	private int _offset = 0;

	public override void Draw(SpriteBatch spriteBatch)
	{
		if (_drawDummy == null)
		{
			_drawDummy = new UICharacter(Main.LocalPlayer, true, true, 1, true)
			{
				Width = StyleDimension.FromPixels(60),
				Height = StyleDimension.FromPixels(60),
				HAlign = 0.5f,
				Top = StyleDimension.FromPixels(20)
			};
			Append(_drawDummy);
		}
		else
		{
			//_drawDummy.Left = Parent.Left;
			//_drawDummy.Top = Parent.Top;
			_drawDummy.Draw(spriteBatch);
		}

		_offset = 0;

		PotionSystem potionPlayer = Main.LocalPlayer.GetModPlayer<PotionSystem>();
		ExpModPlayer expPlayer = Main.LocalPlayer.GetModPlayer<ExpModPlayer>();
		string playerLine = $"{Main.LocalPlayer.name} lvl:{expPlayer.Level}";
		Utils.DrawBorderStringBig(spriteBatch, playerLine, GetRectangle().Top() + new Vector2(10, 160), Color.White, 0.75f, 0.5f, 0.35f);

		float expPercent = expPlayer.Exp / (float)expPlayer.NextLevel * 100;
		DrawSingleStat(spriteBatch, $"Exp: {expPlayer.Exp}/{expPlayer.NextLevel} ({expPercent:#0.##}%)");
		float lifePercent = Main.LocalPlayer.statLife / Main.LocalPlayer.statLifeMax2 * 100;
		DrawSingleStat(spriteBatch, $"Life: {Main.LocalPlayer.statLife}/{Main.LocalPlayer.statLifeMax2} ({lifePercent:#0.##}%)");
		float manaPercent = Main.LocalPlayer.statMana / Main.LocalPlayer.statManaMax * 100;
		DrawSingleStat(spriteBatch, $"Mana: {Main.LocalPlayer.statMana}/{Main.LocalPlayer.statManaMax2} ({manaPercent:#0.##}%)");
		DrawSingleStat(spriteBatch, $"Health Potions: {potionPlayer.HealingLeft}/{potionPlayer.MaxHealing}");
		DrawSingleStat(spriteBatch, $"Mana Potions: {potionPlayer.ManaLeft}/{potionPlayer.MaxMana}");
		DrawSingleStat(spriteBatch, $"Endurance: {Main.LocalPlayer.endurance:#0.##}%");
	}

	private void DrawSingleStat(SpriteBatch spriteBatch, string text)
	{
		Utils.DrawBorderStringBig(spriteBatch, text, GetRectangle().Top() + new Vector2(10, 200 + 30 * _offset), Color.White, 0.5f, 0.5f, 0.35f);
		_offset++;
	}

	private Rectangle GetRectangle()
	{
		return Panel.GetDimensions().ToRectangle();
	}

	//private void UpdateAnim()
	//{
	//	int num = (int)(Main.GlobalTimeWrappedHourly / 0.07f) % 14 + 6;
	//	Player.bodyFrame.Y = (Player.legFrame.Y = (Player.headFrame.Y = num * 56));
	//	Player.WingFrame(wingFlap: false);
	//}

	//protected override void DrawSelf(SpriteBatch spriteBatch)
	//{
	//	// Override the current player reference until the end of this method.
	//	using var _currentPlr = new Main.CurrentPlayerOverride(Player);

	//	CalculatedStyle dimensions = GetDimensions();
	//	spriteBatch.Draw(Main.Assets.Request<Texture2D>("Images/UI/PlayerBackground").Value, dimensions.Position(), Color.White);

	//	UpdateAnim();
	//	DrawPets(spriteBatch);
	//	Vector2 playerPosition = GetPlayerPosition(ref dimensions);
	//	Item item = Player.inventory[Player.selectedItem];
	//	Player.inventory[Player.selectedItem] = _blankItem;
	//	Main.PlayerRenderer.DrawPlayer(Main.Camera, Player, playerPosition + Main.screenPosition, 0f, Vector2.Zero, 0f, _characterScale);
	//	Player.inventory[Player.selectedItem] = item;
	//}

	//private Vector2 GetPlayerPosition(ref CalculatedStyle dimensions)
	//{
	//	Vector2 result = dimensions.Position() + new Vector2(dimensions.Width * 0.5f - (float)(Player.width >> 1), dimensions.Height * 0.5f - (float)(Player.height >> 1));
	//	if (_petProjectiles.Length != 0)
	//		result.X -= 10f;

	//	return result;
	//}

	//public void DrawPets(SpriteBatch spriteBatch)
	//{
	//	CalculatedStyle dimensions = GetDimensions();
	//	Vector2 playerPosition = GetPlayerPosition(ref dimensions);
	//	for (int i = 0; i < _petProjectiles.Length; i++)
	//	{
	//		Projectile projectile = _petProjectiles[i];
	//		Vector2 vector = playerPosition + new Vector2(0f, Player.height) + new Vector2(20f, 0f) + new Vector2(0f, -projectile.height);
	//		projectile.position = vector + Main.screenPosition;
	//		projectile.velocity = new Vector2(0.1f, 0f);
	//		projectile.direction = 1;
	//		projectile.owner = Main.myPlayer;
	//		ProjectileID.Sets.CharacterPreviewAnimations[projectile.type].ApplyTo(projectile, _animated);
	//		Player player = Main.player[Main.myPlayer];
	//		Main.player[Main.myPlayer] = Player;
	//		Main.instance.DrawProjDirect(projectile);
	//		Main.player[Main.myPlayer] = player;
	//	}

	//	spriteBatch.End();
	//	spriteBatch.Begin(SpriteSortMode.Immediate, spriteBatch.GraphicsDevice.BlendState, spriteBatch.GraphicsDevice.SamplerStates[0], spriteBatch.GraphicsDevice.DepthStencilState, spriteBatch.GraphicsDevice.RasterizerState, null, Main.UIScaleMatrix);
	//}
}