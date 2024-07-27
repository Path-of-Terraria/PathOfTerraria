using System.Collections.Generic;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Core.Graphics;

namespace PathOfTerraria.Common.Mechanics;

public sealed class Experience
{
	public static class Sizes
	{
		public const int OrbSmallYellow =      1;
		public const int OrbSmallGreen =       5;
		public const int OrbSmallBlue =       10;
		public const int OrbMediumYellow =    50;
		public const int OrbMediumGreen =    100;
		public const int OrbMediumBlue =     500;
		public const int OrbLargeYellow =   1000;
		public const int OrbLargeGreen =    5000;
		public const int OrbLargeBlue =    10000;
	}

	private const int ExtraUpdates = 7;

	public readonly float Rotation;

	private readonly Queue<Vector2> _oldCenters;
	private readonly int _value;
	private readonly float _magnitude;
	private readonly int _target;

	public Vector2 Center;
	public bool Active;
	public bool Collected;

	private Vector2 _velocity;
	private Color[] _collectedTrail;
	private bool _oldCollected;

	public Experience(int xp, Vector2 startPosition, Vector2 startVelocity, int targetPlayer)
	{
		_value = xp;

		Vector2 size = GetSize();
		if (size == Vector2.Zero)
		{
			throw new Exception("Invalid xp count: " + xp);
		}

		Center = startPosition;
		Active = true;
		Rotation = Main.rand.NextFloat(MathHelper.TwoPi);

		_velocity = startVelocity;
		_target = targetPlayer;
		_magnitude = startVelocity.Length();
		_oldCenters = new Queue<Vector2>();
	}

	public Vector2 GetSize()
	{
		return _value switch
		{
			Sizes.OrbSmallYellow or Sizes.OrbSmallGreen or Sizes.OrbSmallBlue => new Vector2(6),
			Sizes.OrbMediumYellow or Sizes.OrbMediumGreen or Sizes.OrbMediumBlue => new Vector2(8),
			Sizes.OrbLargeYellow or Sizes.OrbLargeGreen or Sizes.OrbLargeBlue => new Vector2(10),
			_ => Vector2.Zero
		};
	}

	private Color GetTrailColor()
	{
		return _value switch
		{
			Sizes.OrbSmallYellow or Sizes.OrbMediumYellow or Sizes.OrbLargeYellow => Color.Yellow,
			Sizes.OrbSmallGreen or Sizes.OrbMediumGreen or Sizes.OrbLargeGreen => Color.LimeGreen,
			Sizes.OrbSmallBlue or Sizes.OrbMediumBlue or Sizes.OrbLargeBlue => Color.Blue,
			_ => Color.Transparent
		};
	}

	public Rectangle GetSourceRectangle()
	{
		return _value switch
		{
			Sizes.OrbSmallYellow => new Rectangle(0, 0, 6, 6),
			Sizes.OrbSmallGreen => new Rectangle(8, 0, 6, 6),
			Sizes.OrbSmallBlue => new Rectangle(16, 0, 6, 6),
			Sizes.OrbMediumYellow => new Rectangle(0, 8, 8, 8),
			Sizes.OrbMediumGreen => new Rectangle(10, 8, 8, 8),
			Sizes.OrbMediumBlue => new Rectangle(20, 8, 8, 8),
			Sizes.OrbLargeYellow => new Rectangle(0, 18, 10, 10),
			Sizes.OrbLargeGreen => new Rectangle(12, 18, 10, 10),
			Sizes.OrbLargeBlue => new Rectangle(24, 18, 10, 10),
			_ => Rectangle.Empty
		};
	}

	public void Update()
	{
		if (!Active)
		{
			return;
		}

		_oldCollected = Collected;

		//Home in on the player, unless they've disconnected
		Player player = Main.player[_target];

		if (!player.active)
		{
			Collected = true;
		}

		if (Collected)
		{
			//Make the trail shrink
			if (_oldCenters.Count == 0)
			{
				Active = false;
			}
			else
			{
				for (int i = 0; i < ExtraUpdates + 1; i++)
				{
					_oldCenters.Dequeue();
				}
			}

			return;
		}

		for (int i = 0; i < ExtraUpdates + 1; i++)
		{
			InnerUpdate();
		}
	}

	private void InnerUpdate()
	{
		Player player = Main.player[_target];

		// Lazily home towards player
		_velocity += player.DirectionFrom(Center) * 0.1f;

		if (_velocity.LengthSquared() > _magnitude * _magnitude) // Cap speed to original magnitude
		{
			_velocity = Vector2.Normalize(_velocity) * _magnitude;
		}

		if (_oldCenters.Count >= 30 * (ExtraUpdates + 1))
		{
			_oldCenters.Dequeue();
		}

		_oldCenters.Enqueue(Center);

		Center += _velocity / (ExtraUpdates + 1);

		if (!Collected && !player.dead && player.Hitbox.Contains(Center.ToPoint()))
		{
			ExpModPlayer statPlayer = player.GetModPlayer<ExpModPlayer>();
			statPlayer.Exp += _value;
			CombatText.NewText(player.Hitbox, new Color(145, 255, 160), $"+{_value}");
			Collected = true;
		}

		//Draw calls happen less often than Update calls during lag... which causes the arrays to not be the same size
		//Therefore, the data must be set here instead of in DrawTrail
		Color color = GetTrailColor();
		int trailColorCount = _oldCenters.Count + 1;
		var colors = new Color[trailColorCount];

		if (_oldCollected && _collectedTrail is not null)
		{
			return;
		}

		colors[^1] = color;
		int i = 0;
		foreach (Vector2 _ in _oldCenters)
		{
			colors[i] = Color.Lerp(color, Color.Transparent, 1f - i / (float)trailColorCount);
			i++;
		}

		_collectedTrail = colors;
	}

	public void DrawTrail()
	{
		if (!Active)
		{
			return;
		}

		Vector2 size = GetSize();
		if (size == Vector2.Zero)
		{
			return;
		}

		//No trail yet
		if (_oldCenters.Count == 0)
		{
			return;
		}

		Color color = GetTrailColor();
		int trailColorCount = _oldCenters.Count + 1;
		var colors = new Color[trailColorCount];

		if (!_oldCollected)
		{
			//Manually set the colors
			colors[^1] = color;
			int i = 0;
			foreach (Vector2 _ in _oldCenters)
			{
				colors[i] = Color.Lerp(color, Color.Transparent, 1f - i / (float)trailColorCount);
				i++;
			}
		}
		else
		{
			Array.Copy(_collectedTrail, _collectedTrail.Length - trailColorCount, colors, 0, trailColorCount);
		}

		var points = new Vector2[trailColorCount];
		points[^1] = Center;
		_oldCenters.CopyTo(points, 0);

		PrimitiveDrawing.DrawLineStrip(points, colors);
	}
}