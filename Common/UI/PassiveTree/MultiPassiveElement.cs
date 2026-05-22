using System.Linq;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using Terraria.UI;

#nullable enable

namespace PathOfTerraria.Common.UI.PassiveTree;

/// <summary> Offers a choice of one of multiple passives contained within. Every hidden child node is considered an inner node. </summary>
internal class MultiPassiveElement : PassiveElement
{
	private readonly Edge<IConnectedAllocatableNode>[] _extraEdges;

	private AllocatableInnerPanel _parentPanel = null!;
	public int AnimationTime { get; set; }
	public int AnimationTimeMax => 60;
	public Passive[] InnerPassives { get; }
	public Passive? ActivePassive => InnerPassives.FirstOrDefault(p => p.Level > 0);

	public MultiPassiveElement(Passive passive) : base(passive)
	{
		PassiveTreePlayer passiveTreeSystem = Main.LocalPlayer.GetModPlayer<PassiveTreePlayer>();
		InnerPassives = passiveTreeSystem.Edges.Where(e => e.Start == Passive && e.End.IsHidden).Select(e => (Passive)e.End).ToArray();
		_extraEdges = new Edge<IConnectedAllocatableNode>[InnerPassives.Length];
	}

	public override void SafeUpdate(GameTime gameTime)
	{
		base.SafeUpdate(gameTime);

		bool showsRadials = Children.Any();
		bool shouldHaveRadials = (ActivePassive == null && Passive.CanAllocate(Main.LocalPlayer)) || GetDimensions().Center().DistanceSQ(Main.MouseScreen) < GetRadius();
		bool shouldShowRadials = shouldHaveRadials || AnimationTime > 0;

		AnimationTime = shouldHaveRadials ? Math.Min(AnimationTimeMax, AnimationTime + 1) : Math.Max(0, AnimationTime - 1);
		// Speed up closing animation.
		AnimationTime = !shouldHaveRadials ? Math.Min(AnimationTime, AnimationTimeMax / 4) : AnimationTime;

		if (showsRadials != shouldShowRadials)
		{
			if (shouldHaveRadials)
			{
				AddRadials();
			}
			else
			{
				RemoveAllChildren();
			}
		}
	}

	private float GetRadius()
	{
		float distance = 0f;
		Vector2 center = Node.TreePos;

		foreach (Passive line in InnerPassives)
		{
			Vector2 target = line.TreePos;
			distance = MathF.Max(distance, center.DistanceSQ(target));
		}

		_parentPanel ??= (Parent as AllocatableInnerPanel)!;
		float zoom = _parentPanel?.Zoom ?? 1f;
		return (distance + 120 * 120) * zoom * zoom;	
	}

	public override bool AppearsAsAllocated(Allocatable? nodeOverride = null)
	{
		// MasteryPassive.OnAllocate intentionally skips Level increment, so Node.Allocated is always false.
		// Use the active inner choice as the authoritative "is this mastery allocated" signal instead.
		return nodeOverride is not null ? base.AppearsAsAllocated(nodeOverride) : ActivePassive is not null;
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		base.DrawSelf(spriteBatch);

		if (ActivePassive is { } inner)
		{
			DrawNode(inner, spriteBatch, GetDimensions().Center(), GetZoom());
		}
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		// Not calling base on purpose.

		if (Children.Any())
		{
			AllocatableInnerPanel.DrawEdgeConnections(spriteBatch, _extraEdges, GetZoom());
		}

		DrawChildren(spriteBatch);

		// Draw self above children.
		DrawSelf(spriteBatch);
	}

	/// <summary>
	/// Checks if a mastery with the same internal identifier is already allocated elsewhere on the tree.
	/// </summary>
	public bool IsMasteryTypeAlreadyAllocated(string masteryIdentifier)
	{
		Player player = Main.LocalPlayer;
		PassiveTreePlayer passiveTreePlayer = player.GetModPlayer<PassiveTreePlayer>();
		
		return passiveTreePlayer.ActiveNodes
			.Where(passive => passive.Level > 0 && passive.Name == masteryIdentifier)
			.Any();
	}

	public override void SafeClick(UIMouseEvent evt)
	{
		Player player = Main.LocalPlayer;

		if (evt?.Target is PassiveRadialElement radial && ActivePassive == null && Passive.CanAllocate(player))
		{
			// Check if this mastery type is already allocated elsewhere
			if (IsMasteryTypeAlreadyAllocated(radial.Passive.Name))
			{
				// TODO: Some kind of feedback here maybe?
				return;
			}

			// Skip the inner CanAllocate check for masteries - if the hub can be allocated, the inner can too
			Allocate(Passive, usedCost: 0);
			Allocate(radial.Passive, usedCost: 1);
		}
	}

	public override void SafeRightClick(UIMouseEvent evt)
	{
		Player player = Main.LocalPlayer;
		if (ActivePassive is Passive active)
		{
			// Ensure that removing this mastery won't leave any node disconnected from the tree.
			if (!player.GetModPlayer<PassiveTreePlayer>().FullyLinkedWithout(Passive))
			{
				return;
			}
			Deallocate(active, usedCost: 1);
			Deallocate(Passive, usedCost: 0);
		}
	}

	// Repair clicks on the unpredictably moving radials.
	public override bool ContainsPoint(Vector2 point)
	{
		return base.ContainsPoint(point) || Children.Any(e => e.ContainsPoint(point));
	}

	public override void DrawHoverTooltip(Allocatable node)
	{
		base.DrawHoverTooltip(ActivePassive is { } active ? active : node);
	}

	private void AddRadials()
	{
		for (int i = 0; i < InnerPassives.Length; i++)
		{
			Passive inner = InnerPassives[i];
			var element = new PassiveRadialElement(inner, Vector2.Zero, inner.TreePos - Passive.TreePos);
			Append(element);
			_extraEdges[i] = new Edge<IConnectedAllocatableNode>(this, element, Flags: 0);
		}

		RecalculateChildren();
	}
}

internal class PassiveRadialElement : PassiveElement
{
	private readonly Vector2 _startOffset;
	private readonly Vector2 _targetOffset;
	private readonly Vector2 _origSize;
	
	public MultiPassiveElement? Handler => Parent is MultiPassiveElement e ? e : null;

	private float Progress => Handler is { } handler ? (float)Handler.AnimationTime / Handler.AnimationTimeMax : 0f;

	public PassiveRadialElement(Passive passive, Vector2 startOffset, Vector2 targetOffset) : base(passive)
	{
		var size = passive.Texture.Size().ToPoint();

		Debug.Assert(size.X > 1 && size.Y > 1);

		_origSize = size.ToVector2();
		_startOffset = startOffset;
		_targetOffset = targetOffset;

		Width.Set(size.X, 0);
		Height.Set(size.Y, 0);
		Left.Set(_startOffset.X - Width.Pixels * 0.5f, 0.5f);
		Top.Set(_startOffset.Y - Height.Pixels * 0.5f, 0.5f);
	}

	public override void SafeUpdate(GameTime gameTime)
	{
		base.SafeUpdate(gameTime);
		
		float zoom = (Handler?.Parent as AllocatableInnerPanel)?.Zoom ?? 1f;

		// Keep element bounds in sync with zoom so hit detection is accurate.
		Width.Pixels = _origSize.X * zoom;
		Height.Pixels = _origSize.Y * zoom;

		static float EaseOutElastic(float x)
		{
			// https://easings.net/#easeOutElastic
			const float c4 = (2f * MathF.PI) / 3f;
			return x <= 0f ? 0f : (x >= 1f ? 1f : (MathF.Pow(2f, -10f * x) * MathF.Sin((x * 10f - 0.75f) * c4) + 1f));
		}

		float animationProgress = Progress;
		float lerpStep = EaseOutElastic(animationProgress);
		var newPos = Vector2.Lerp(_startOffset, _targetOffset * zoom, lerpStep);
		
		Left.Set(newPos.X - Width.Pixels * 0.5f, 0.5f);
		Top.Set(newPos.Y - Height.Pixels * 0.5f, 0.5f);
	}

	public override bool AppearsAsCanBeAllocated(Allocatable? nodeOverride = null)
	{
		bool canHandlerBeAllocated = Handler?.AppearsAsCanBeAllocated(Handler.Node) == true;
		if (!canHandlerBeAllocated)
		{
			return false;
		}

		// Check if this specific mastery type is already allocated elsewhere
		if (Handler != null && Handler.IsMasteryTypeAlreadyAllocated(Passive.Name))
		{
			return false;
		}

		return true;
	}

	public override void DrawHoverTooltip(Allocatable node)
	{
		// Prevent tooltip overlaps as the radials appear.
		if (Progress >= 0.25f)
		{
			// Check if this mastery is already allocated and show appropriate tooltip
			if (Handler != null && Handler.IsMasteryTypeAlreadyAllocated(node.Name))
			{
				string name = node.DisplayName + " (Already Allocated)";
				string subtitle = "[c/FF6666:This mastery is already allocated elsewhere on the tree.]";
				
				Tooltip.Create(new TooltipDescription
				{
					Identifier = GetType().Name,
					SimpleTitle = name,
					SimpleSubtitle = subtitle,
				});
			}
			else
			{
				base.DrawHoverTooltip(node);
			}
		}
	}

	public override void DrawNode(Allocatable node, SpriteBatch spriteBatch, Vector2 center, float scale = 1f)
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

		// Dim the color if this mastery type is already allocated elsewhere
		if (Handler != null && Handler.IsMasteryTypeAlreadyAllocated(node.Name))
		{
			color = Color.Gray * 0.4f;
		}

		center = center.Floor();
		spriteBatch.Draw(tex, center, null, color, 0, node.Size * 0.5f, scale, 0, 0);
		
		if (node.MaxLevel > 1)
		{
			Utils.DrawBorderString(spriteBatch, $"{node.Level}/{node.MaxLevel}", center + node.Size * 0.5f * scale, color, scale, 0.5f, 0.5f);
		}
	}

	public override void SafeClick(UIMouseEvent evt)
	{
		Handler?.SafeClick(evt);
	}

	public override void SafeRightClick(UIMouseEvent evt)
	{
		Handler?.SafeRightClick(evt);
	}
}