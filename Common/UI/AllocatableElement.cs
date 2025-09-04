using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Core.Sounds;
using PathOfTerraria.Core.UI.SmartUI;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.ID;
using Terraria.UI;

namespace PathOfTerraria.Common.UI;

internal abstract class AllocatableElement : SmartUiElement
{
	public static Asset<Texture2D> GlowAlpha = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/GlowAlpha");
	public static Asset<Texture2D> StarAlpha = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/StarAlpha");

	public Allocatable Node { get; }

	private int _flashTimer;
	private int _redFlashTimer;

	public AllocatableElement(Allocatable node)
	{
		var size = node.Size.ToPoint();

		Width.Set(size.X, 0);
		Height.Set(size.Y, 0);

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

		if (IsMouseHovering && GetElementAt(Main.MouseScreen) == this)
		{
			DrawHoverTooltip();
		}

		Recalculate();
	}

	/// <summary> Draws the name and tooltip of <see cref="Node"/> when hovered over. </summary>
	public virtual void DrawHoverTooltip()
	{
		string name = Node.DisplayName;

		if (Node.MaxLevel > 1)
		{
			name += $" ({Node.Level}/{Node.MaxLevel})";
		}

#if DEBUG
		if (Node is Systems.PassiveTreeSystem.Passive passive)
		{
			name += $" -- {passive.ReferenceId}";
		}
#endif

		Tooltip.Create(new TooltipDescription
		{
			Identifier = GetType().Name,
			SimpleTitle = name,
			SimpleSubtitle = Node.DisplayTooltip,
		});
	}

	public override void SafeClick(UIMouseEvent evt)
	{
		Player p = Main.LocalPlayer;

		if (CheckMouseContained() && Node.CanAllocate(p))
		{
			Allocate(Node, 1);
		}
	}

	public override void SafeRightClick(UIMouseEvent evt)
	{
		Player p = Main.LocalPlayer;

		if (CheckMouseContained() && Node.CanDeallocate(p))
		{
			Deallocate(Node, 1);
		}
	}

	protected virtual void Allocate(Allocatable node, int usedCost)
	{
		_flashTimer = 20;

		node.OnAllocate(Main.LocalPlayer);
		TreeSoundEngine.PlaySoundForTreeAllocation(Node.Level, Node.MaxLevel);
	}

	protected virtual void Deallocate(Allocatable node, int usedCost)
	{
		_redFlashTimer = 20;

		node.OnDeallocate(Main.LocalPlayer);
		SoundEngine.PlaySound(SoundID.DD2_WitherBeastDeath);
	}
}