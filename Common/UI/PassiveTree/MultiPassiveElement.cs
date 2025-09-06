using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using Terraria.UI;

#nullable enable

namespace PathOfTerraria.Common.UI.PassiveTree;

/// <summary> Offers a choice of one of multiple passives contained within. Every hidden child node is considered an inner node. </summary>
internal class MultiPassiveElement : PassiveElement
{
	private readonly Edge<AllocatableElement>[]_extraEdges;

	public int AnimationTime { get; set; }
	public int AnimationTimeMax => 60;
	public Passive[] InnerPassives { get; }
	public Passive? ActivePassive => InnerPassives.FirstOrDefault(p => p.Level > 0);

	public MultiPassiveElement(Passive passive) : base(passive)
	{
		PassiveTreePlayer passiveTreeSystem = Main.LocalPlayer.GetModPlayer<PassiveTreePlayer>();
		InnerPassives = passiveTreeSystem.Edges.Where(e => e.Start == Passive && e.End.IsHidden).Select(e => (Passive)e.End).ToArray();
		_extraEdges = new Edge<AllocatableElement>[InnerPassives.Length];
	}

	public override void SafeUpdate(GameTime gameTime)
	{
		base.SafeUpdate(gameTime);

		bool showsRadials = Children.Any();
		bool shouldHaveRadials = ActivePassive == null && Passive.CanAllocate(Main.LocalPlayer);
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

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		base.DrawSelf(spriteBatch);

		if (ActivePassive is { } inner)
		{
			DrawNode(inner, spriteBatch, GetDimensions().Center());
		}
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		// Not calling base on purpose.

		if (Children.Any())
		{
			AllocatableInnerPanel.DrawEdgeConnections(spriteBatch, _extraEdges);
		}

		DrawChildren(spriteBatch);

		// Draw self above children.
		DrawSelf(spriteBatch);
	}

	public override void SafeClick(UIMouseEvent evt)
	{
		Player player = Main.LocalPlayer;

		if (evt?.Target is PassiveRadialElement radial && ActivePassive == null && Passive.CanAllocate(player))
		{
			bool canAllocateInner;
			try
			{
				Passive.Level++;
				canAllocateInner = radial.Passive.CanAllocate(player);
			}
			finally
			{
				Passive.Level--;
			}

			if (canAllocateInner)
			{
				Allocate(Passive, usedCost: 0);
				Allocate(radial.Passive, usedCost: 1);
			}
		}
	}

	public override void SafeRightClick(UIMouseEvent evt)
	{
		if (ActivePassive is Passive active)
		{
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
			_extraEdges[i] = new Edge<AllocatableElement>(this, element, Flags: 0);
		}

		RecalculateChildren();
	}
}

internal class PassiveRadialElement : PassiveElement
{
	private readonly Vector2 _startOffset;
	private readonly Vector2 _targetOffset;

	public MultiPassiveElement? Handler => Parent is MultiPassiveElement e ? e : null;

	private float Progress => Handler is { } handler ? (float)Handler.AnimationTime / Handler.AnimationTimeMax : 0f;

	public PassiveRadialElement(Passive passive, Vector2 startOffset, Vector2 targetOffset) : base(passive)
	{
		var size = passive.Texture.Size().ToPoint();

		Debug.Assert(size.X > 1 && size.Y > 1);

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

		static float EaseOutElastic(float x)
		{
			// https://easings.net/#easeOutElastic
			const float c4 = (2f * MathF.PI) / 3f;
			return x <= 0f ? 0f : (x >= 1f ? 1f : (MathF.Pow(2f, -10f * x) * MathF.Sin((x * 10f - 0.75f) * c4) + 1f));
		}

		float animationProgress = Progress;
		float lerpStep = EaseOutElastic(animationProgress);
		var newPos = Vector2.Lerp(_startOffset, _targetOffset, lerpStep);

		Left.Set(newPos.X - Width.Pixels * 0.5f, 0.5f);
		Top.Set(newPos.Y - Height.Pixels * 0.5f, 0.5f);
	}

	public override bool AppearsAsCanBeAllocated(Allocatable? nodeOverride = null)
	{
		return Handler?.AppearsAsCanBeAllocated(Handler.Node) == true;
	}

	public override void DrawHoverTooltip(Allocatable node)
	{
		// Prevent tooltip overlaps as the radials appear.
		if (Progress >= 0.25f)
		{
			base.DrawHoverTooltip(node);
		}
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		base.Draw(spriteBatch);
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