using System.Diagnostics;
using System.Linq;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using Terraria.UI;

#nullable enable

namespace PathOfTerraria.Common.UI.PassiveTree;

/// <summary> Offers a choice of one of multiple passives contained within. Every hidden child node is considered an inner node. </summary>
internal class MultiPassiveElement : PassiveElement
{
	public int AnimationTime { get; set; }
	public int AnimationTimeMax => 20;
	public Passive[] InnerPassives { get; }
	public Passive? ActivePassive => InnerPassives.FirstOrDefault(p => p.Level > 0);

	public MultiPassiveElement(Passive passive) : base(passive)
	{
		PassiveTreePlayer passiveTreeSystem = Main.LocalPlayer.GetModPlayer<PassiveTreePlayer>();
		InnerPassives = passiveTreeSystem.Edges.Where(e => e.Start == Passive && e.End.IsHidden).Select(e => (Passive)e.End).ToArray();
	}

	public override void SafeUpdate(GameTime gameTime)
	{
		base.SafeUpdate(gameTime);

		bool showsRadials = Children.Any();
		bool shouldHaveRadials = ActivePassive == null && Passive.CanAllocate(Main.LocalPlayer);
		bool shouldShowRadials = shouldHaveRadials || AnimationTime > 0;

		AnimationTime = shouldHaveRadials ? Math.Min(AnimationTimeMax, AnimationTime + 1) : Math.Max(0, AnimationTime - 1);

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

	public override void Draw(SpriteBatch spriteBatch)
	{
		base.Draw(spriteBatch);

		if (ActivePassive is { } inner)
		{
			inner.Draw(spriteBatch, GetDimensions().Center());
		}
	}

	public override void SafeClick(UIMouseEvent evt)
	{
		Player player = Main.LocalPlayer;

		if (evt?.Target is PassiveRadialElement radial && ActivePassive == null && Passive.CanAllocate(player))
		{
			// The inner CanAllocate will likely check for our current node's level, so we must falsify the evidence.
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
				Allocate(Passive);
				Allocate(radial.Passive);
			}
		}
	}

	public override void SafeRightClick(UIMouseEvent evt)
	{
		if (ActivePassive is Passive active)
		{
			Deallocate(active);
			Deallocate(Passive);
		}
	}

	// Repair clicks on the unpredictably moving radials.
	public override bool ContainsPoint(Vector2 point)
	{
		return base.ContainsPoint(point) || Children.Any(e => e.ContainsPoint(point));
	}

	private void AddRadials()
	{
		int index = 0;

		foreach (Passive outgoing in InnerPassives)
		{
			Append(new PassiveRadialElement(outgoing, Vector2.Zero, index, InnerPassives.Length));
			index++;
		}

		RecalculateChildren();
	}
}

internal class PassiveRadialElement : PassiveElement
{
	private readonly int _index;
	private readonly int _numRadials;

	public MultiPassiveElement? Handler => Parent is MultiPassiveElement e ? e : null;

	private float Progress => Handler is { } handler ? (float)Handler.AnimationTime / Handler.AnimationTimeMax : 0f;

	public PassiveRadialElement(Passive passive, Vector2 origin, int index, int numRadials) : base(passive)
	{
		var size = passive.Texture.Size().ToPoint();

		Debug.Assert(size.X > 1 && size.Y > 1);

		_index = index;
		_numRadials = numRadials;

		Width.Set(size.X, 0);
		Height.Set(size.Y, 0);
		Left.Set(origin.X - Width.Pixels / 2, 0.5f);
		Top.Set(origin.Y - Height.Pixels / 2, 0.5f);
	}

	public override void SafeUpdate(GameTime gameTime)
	{
		base.SafeUpdate(gameTime);

		Vector2 origin = Vector2.Zero;
		float distance = (Height.Pixels + 22) * Progress + (float)Math.Sin(Progress * Math.PI) * 30; // The total distance to move
		float angle = MathHelper.TwoPi * (_index / (float)_numRadials);
		var newPos = (origin - (Vector2.UnitY * distance).RotatedBy(angle)).ToPoint();

		Left.Set(newPos.X - Width.Pixels / 2, 0.5f);
		Top.Set(newPos.Y - Height.Pixels / 2, 0.5f);
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