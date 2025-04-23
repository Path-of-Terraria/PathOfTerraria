using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Content.SkillPassives;
using PathOfTerraria.Core.Sounds;
using PathOfTerraria.Core.UI.SmartUI;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.ID;
using Terraria.UI;

namespace PathOfTerraria.Common.UI;

internal class AllocatableElement : SmartUiElement
{
	public static Asset<Texture2D> GlowAlpha;
	public static Asset<Texture2D> StarAlpha;

	public readonly Allocatable Node;

	private int _flashTimer;
	private int _redFlashTimer;

	public static void LoadAssets()
	{
		if (GlowAlpha?.IsLoaded is not true)
		{
			GlowAlpha = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/GlowAlpha");
		}

		if (StarAlpha?.IsLoaded is not true)
		{
			StarAlpha = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/StarAlpha");
		}
	}

	public AllocatableElement(Allocatable node)
	{
		var size = node.Size.ToPoint();

		Width.Set(size.X, 0);
		Height.Set(size.Y, 0);

		if (Node is Anchor)
		{
			(Node as SkillPassive).Level = 1;
		}

		LoadAssets();
		Node = node;
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		Node.Draw(spriteBatch, GetDimensions().Center());
		DrawOnto(spriteBatch, GetDimensions().Center());

		if (_flashTimer > 0)
		{
			Texture2D glow = GlowAlpha.Value;
			Texture2D star = StarAlpha.Value;

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
			Texture2D glow = GlowAlpha.Value;
			Texture2D star = StarAlpha.Value;

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
			DrawHoverTooltip();
		}

		Recalculate();
	}

	/// <summary> Draws the name and tooltip of <see cref="Node"/> when hovered over. </summary>
	public virtual void DrawHoverTooltip()
	{
		string name = Node.DisplayName;

		if (Node is ILevel level)
		{
			(int, int) range = level.LevelRange;

			if (range.Item2 > 1)
			{
				name += $" ({range.Item1}/{range.Item2})";
			}
		}

		Tooltip.SetName(name);
		Tooltip.SetTooltip(Node.DisplayTooltip);
	}

	public override void SafeClick(UIMouseEvent evt)
	{
		Player p = Main.LocalPlayer;

		if (CheckMouseContained() && Node.CanAllocate(p))
		{
			Node.OnAllocate(p);
			_flashTimer = 20;

			if (Node is ILevel level)
			{
				(int, int) range = level.LevelRange;
				TreeSoundEngine.PlaySoundForTreeAllocation(range.Item2, range.Item1);
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

		if (CheckMouseContained() && Node.CanDeallocate(p))
		{
			Node.OnDeallocate(p);
			_redFlashTimer = 20;
			SoundEngine.PlaySound(SoundID.DD2_WitherBeastDeath);
		}
	}
}