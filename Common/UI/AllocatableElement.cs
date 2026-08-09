using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Core.Sounds;
using PathOfTerraria.Core.UI.SmartUI;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;

#nullable enable

namespace PathOfTerraria.Common.UI;

/// <summary> Methods needed to properly render dynamic edge connections. </summary>
internal interface IConnectedAllocatableNode
{
	Vector2 GetCenter();
	bool AppearsAsAllocated(Allocatable? nodeOverride = null);
	bool AppearsAsCanBeAllocated(Allocatable? nodeOverride = null);
}

internal abstract class AllocatableElement : SmartUiElement, IConnectedAllocatableNode
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
	
	/// <summary> Walks up the parent chain and returns the zoom level of the enclosing <see cref="AllocatableInnerPanel"/>, or 1 if none is found. </summary>
	protected float GetZoom()
	{
		UIElement current = Parent;
		while (current is not null)
		{
			if (current is AllocatableInnerPanel panel)
			{
				return panel.Zoom;
			}

			current = current.Parent;
		}

		return 1f;
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		base.DrawSelf(spriteBatch);

		float zoom = GetZoom();
		Vector2 center = GetDimensions().Center();
		DrawNode(Node, spriteBatch, center, zoom);
		DrawOnto(spriteBatch, center);

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

	public virtual void DrawNode(Allocatable node, SpriteBatch spriteBatch, Vector2 center, float scale = 1f)
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

		center = center.Floor();
		spriteBatch.Draw(tex, center, null, color, 0, node.Size * 0.5f, scale, 0, 0);
		
		if (node.MaxLevel > 1)
		{
			Utils.DrawBorderString(spriteBatch, $"{node.Level}/{node.MaxLevel}", center + node.Size * 0.5f * scale, color, scale, 0.5f, 0.5f);
		}
	}

	/// <summary> Draws the name and tooltip of <see cref="Node"/> when hovered over. </summary>
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
			name += $"\n{passive.TreePos} | ID: {passive.ReferenceId}";
		}
#endif
	
		string subtitle = node.DisplayTooltip;

		if (node.RequiredAllocatedEdges > 1)
		{
			string requiredNodesLine = Language.GetTextValue("Mods.PathOfTerraria.UI.MasteryRequiredNodes", node.RequiredAllocatedEdges);
			requiredNodesLine = "[c/888888:" + requiredNodesLine + "]"; // Light grey color
			subtitle = string.IsNullOrEmpty(subtitle) ? requiredNodesLine : subtitle + "\n" + requiredNodesLine;
		}

		Tooltip.Create(new TooltipDescription
		{
			Identifier = GetType().Name,
			SimpleTitle = name,
			SimpleSubtitle = subtitle,
		});
	}

	public override void SafeClick(UIMouseEvent evt)
	{
		if (CheckMouseContained() && Node.CanAllocate(Main.LocalPlayer))
		{
			Allocate(Node, 1);
		}
	}

	public virtual Vector2 GetCenter()
	{
		return GetDimensions().Center();
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