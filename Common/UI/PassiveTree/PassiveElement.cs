using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Common.Systems.TreeSystem;
using PathOfTerraria.Content.Passives;
using PathOfTerraria.Core.Sounds;
using PathOfTerraria.Core.UI.SmartUI;
using Terraria.Audio;
using Terraria.ID;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.PassiveTree;

internal class PassiveElement : SmartUiElement
{
	private static TreeState UiTreeState => SmartUiLoader.GetUiState<TreeState>();

	private readonly Passive _passive;

	private int _flashTimer;
	private int _redFlashTimer;

	public PassiveElement(Passive passive)
	{
		float halfSizeX = passive.Size.X / 2;
		float halfSizeY = passive.Size.Y / 2;

		if (passive.TreePos.X - halfSizeX < UiTreeState.TopLeftTree.X)
		{
			UiTreeState.TopLeftTree.X = passive.TreePos.X - halfSizeX;
		}
		
		if (passive.TreePos.Y - halfSizeY < UiTreeState.TopLeftTree.Y)
		{
			UiTreeState.TopLeftTree.Y = passive.TreePos.Y - halfSizeY;
		}

		if (passive.TreePos.X + halfSizeX > UiTreeState.BotRightTree.X)
		{
			UiTreeState.BotRightTree.X = passive.TreePos.X + halfSizeX;
		}
		
		if (passive.TreePos.Y + halfSizeY > UiTreeState.BotRightTree.Y)
		{
			UiTreeState.BotRightTree.Y = passive.TreePos.Y + halfSizeY;
		}

		_passive = passive;
		Left.Set(passive.TreePos.X - halfSizeX, 0.5f);
		Top.Set(passive.TreePos.Y - halfSizeY, 0.5f);
		Width.Set(passive.Size.X, 0);
		Height.Set(passive.Size.Y, 0);

		// Anchor passive should always be "unlocked", thus this hardcoding
		if (_passive is AnchorPassive)
		{
			_passive.Level = 1;
		}
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		_passive.Draw(spriteBatch, GetDimensions().Center());
		DrawOnto(spriteBatch, GetDimensions().Center());

		if (_flashTimer > 0)
		{
			Texture2D glow = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/GlowAlpha").Value;
			Texture2D star = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/StarAlpha").Value;

			float prog = _flashTimer / 20f;

			var glowColor = new Color(255, 230, 150)
			{
				A = 0
			};

			glowColor *= prog * 0.5f;

			spriteBatch.Draw(glow, GetDimensions().Center(), null, glowColor, 0, glow.Size() / 2f, 1 + (1f - prog), 0, 0);
			spriteBatch.Draw(star, GetDimensions().Center(), null, glowColor * 0.5f, 0, star.Size() / 2f, 1 + (1f - prog), 0, 0);

			_flashTimer--;
		}

		if (_redFlashTimer > 0)
		{
			Texture2D glow = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/GlowAlpha").Value;
			Texture2D star = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/StarAlpha").Value;

			float prog = _redFlashTimer / 20f;

			var glowColor = new Color(255, 60, 60)
			{
				A = 0
			};

			glowColor *= prog * 0.5f;

			spriteBatch.Draw(glow, GetDimensions().Center(), null, glowColor, 0, glow.Size() / 2f, 1 + (1f - prog), 0, 0);
			spriteBatch.Draw(star, GetDimensions().Center(), null, glowColor * 0.5f, 0, star.Size() / 2f, 1 + prog, 0, 0);

			_redFlashTimer--;
		}

		if (IsMouseHovering)
		{
			string name = _passive.DisplayName;

			if (_passive.MaxLevel > 1)
			{
				name += $" ({_passive.Level}/{_passive.MaxLevel})";
			}
			
#if DEBUG
			name += $" -- {_passive.ReferenceId}";
#endif

			Tooltip.SetName(name);
			Tooltip.SetTooltip(_passive.DisplayTooltip);
		}

		Recalculate();
	}

	public override void SafeClick(UIMouseEvent evt)
	{
		if (!_passive.CanAllocate(Main.LocalPlayer) || !CheckMouseContained())
		{
			return;
		}

		_passive.Level++;
		Main.LocalPlayer.GetModPlayer<PassiveTreePlayer>().Points--;

		_flashTimer = 20;

		TreeSoundEngine.PlaySoundForTreeAllocation(_passive.MaxLevel, _passive.Level);
	}

	public override void SafeRightClick(UIMouseEvent evt)
	{
		if (!_passive.CanDeallocate(Main.LocalPlayer) || !CheckMouseContained())
		{
			return;
		}

		_passive.Level--;
		Main.LocalPlayer.GetModPlayer<PassiveTreePlayer>().Points++;

		_redFlashTimer = 20;

		SoundEngine.PlaySound(SoundID.DD2_WitherBeastDeath);
	}
}
