using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Core.Sounds;
using PathOfTerraria.Core.UI.SmartUI;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.ID;
using Terraria.UI;

#nullable enable

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

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		base.DrawSelf(spriteBatch);

		DrawNode(Node, spriteBatch, GetDimensions().Center());
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
			DrawHoverTooltip(Node);
		}
	}

	public virtual void DrawNode(Allocatable node, SpriteBatch spriteBatch, Vector2 center)
	{
		Texture2D tex = node.Texture.Value;
		Color color = Color.Gray;

		if (AppearsAsCanBeAllocated(node))
		{
			color = Color.Lerp(Color.Gray, Color.White, (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.5f + 0.5f);
		}

		if (AppearsAsAllocated(node))
		{
			color = Color.White;
		}

		spriteBatch.Draw(tex, center, null, color, 0, node.Size * 0.5f, 1, 0, 0);

		if (node.MaxLevel > 1)
		{
			Utils.DrawBorderString(spriteBatch, $"{node.Level}/{node.MaxLevel}", center + node.Size * 0.5f, color, 1, 0.5f, 0.5f);
		}
	}

	/// <summary> Draws the name and tooltip of <see cref="Node"/> when hovered over. </summary>
	public virtual void DrawHoverTooltip(Allocatable node)
	{
		string name = node.DisplayName;

		if (node.MaxLevel > 1)
		{
			name += $" ({node.Level}/{node.MaxLevel})";
		}

#if DEBUG
		if (node is Systems.PassiveTreeSystem.Passive passive)
		{
			name += $" -- {passive.ReferenceId}";
		}
#endif

		Tooltip.Create(new TooltipDescription
		{
			Identifier = GetType().Name,
			SimpleTitle = name,
			SimpleSubtitle = node.DisplayTooltip,
		});
	}

	public override void SafeClick(UIMouseEvent evt)
	{
		if (CheckMouseContained() && Node.CanAllocate(Main.LocalPlayer))
		{
			Allocate(Node, 1);
		}
	}

	public virtual bool AppearsAsCanBeAllocated(Allocatable? nodeOverride = null)
	{
		return (nodeOverride ?? Node).CanAllocate(Main.LocalPlayer);
	}

	public virtual bool AppearsAsAllocated(Allocatable? nodeOverride = null)
	{
		return (nodeOverride ?? Node).Allocated;
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