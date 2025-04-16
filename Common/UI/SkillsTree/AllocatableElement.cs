using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Content.SkillPassives;
using PathOfTerraria.Core.Sounds;
using PathOfTerraria.Core.UI.SmartUI;
using Terraria.Audio;
using Terraria.ID;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.SkillsTree;

internal class AllocatableElement : SmartUiElement
{
	private static TreeState UiTreeState => SmartUiLoader.GetUiState<TreeState>();

	private readonly Allocatable _node;

	private int _flashTimer;
	private int _redFlashTimer;
	
	public AllocatableElement(Vector2 origin, Allocatable node)
	{
		var size = node.Texture.Size().ToPoint();
		_node = node;

		Left.Set(origin.X - size.X / 2, 0.5f);
		Top.Set(origin.Y - size.Y / 2, 0.5f);
		Width.Set(size.X, 0);
		Height.Set(size.Y, 0);

		if (_node is SkillPassiveAnchor)
		{
			(_node as SkillPassive).Level = 1;
		}
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		_node.Draw(spriteBatch, GetDimensions().Center());
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
			string name = _node.Name;

			if (_node is SkillPassive passive && passive.MaxLevel > 1)
			{
				name += $" ({passive.Level}/{passive.MaxLevel})";
			}

			Tooltip.SetName(name);
			Tooltip.SetTooltip(_node.Tooltip);
		}

		Recalculate();
	}

	public override void SafeClick(UIMouseEvent evt)
	{
		Player p = Main.LocalPlayer;

		if (CheckMouseContained() && _node.CanAllocate(p))
		{
			_node.OnAllocate(p);
			_flashTimer = 20;
			
			if (_node is SkillPassive passive)
			{
				TreeSoundEngine.PlaySoundForTreeAllocation(passive.MaxLevel, passive.Level);
			}
			else
			{
				TreeSoundEngine.PlaySoundForTreeAllocation(1, 1);
			}
		}
	}

	public override void SafeRightClick(UIMouseEvent evt)
	{
		Player p = Main.LocalPlayer;

		if (CheckMouseContained() && _node.CanDeallocate(p))
		{
			_node.OnDeallocate(p);
			_redFlashTimer = 20;
			SoundEngine.PlaySound(SoundID.DD2_WitherBeastDeath);
		}
	}
}
