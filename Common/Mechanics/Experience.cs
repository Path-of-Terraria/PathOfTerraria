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
	public readonly int Target;

	private readonly Queue<Vector2> _oldCenters;
	private readonly float _magnitude;

	public Vector2 Center;
	public bool Active;
	public bool Collected;

	private Vector2 _velocity;
	private int _value;
	private Color[] _collectedTrail;
	private bool _oldCollected;

	public Experience(int xp, Vector2 startPosition, Vector2 startVelocity, int targetPlayer)
	{
		_value = xp;

		GetSize(); // Used only to throw an ArgumentException if it fails

		Center = startPosition;
		Active = true;
		Rotation = Main.rand.NextFloat(MathHelper.TwoPi);

		_velocity = startVelocity;
		Target = targetPlayer;
		_magnitude = startVelocity.Length();
		_oldCenters = new Queue<Vector2>();
	}

	public int GetSize()
	{
		return _value switch
		{
			>= Sizes.OrbLargeBlue => Sizes.OrbLargeBlue,
			>= Sizes.OrbLargeGreen => Sizes.OrbLargeGreen,
			>= Sizes.OrbLargeYellow => Sizes.OrbLargeYellow,
			>= Sizes.OrbMediumBlue => Sizes.OrbMediumBlue,
			>= Sizes.OrbMediumGreen => Sizes.OrbMediumGreen,
			>= Sizes.OrbMediumYellow => Sizes.OrbMediumYellow,
			>= Sizes.OrbSmallBlue => Sizes.OrbSmallBlue,
			>= Sizes.OrbSmallGreen => Sizes.OrbSmallGreen,
			>= Sizes.OrbSmallYellow => Sizes.OrbSmallYellow,
			_ => throw new ArgumentException("value is equal to or less than 0. This should never happen.")
		};
	}

	public float GetScale()
	{
		float scale = 1f;

		if (GetSize() == Sizes.OrbLargeBlue)
		{
			scale += MathF.Sqrt((_value - Sizes.OrbLargeBlue) * 0.00001f);
		}

		return scale;
	}

	private Color GetTrailColor()
	{
		return GetSize() switch
		{
			Sizes.OrbSmallYellow or Sizes.OrbMediumYellow or Sizes.OrbLargeYellow => Color.Yellow,
			Sizes.OrbSmallGreen or Sizes.OrbMediumGreen or Sizes.OrbLargeGreen => Color.LimeGreen,
			Sizes.OrbSmallBlue or Sizes.OrbMediumBlue or Sizes.OrbLargeBlue => Color.Blue,
			_ => Color.Transparent
		};
	}

	public Rectangle GetSourceRectangle()
	{
		return GetSize() switch
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

	public void Update(int who, Experience[] experienceList)
	{
		if (!Active)
		{
			return;
		}

		_oldCollected = Collected;

		//Home in on the player, unless they've disconnected
		Player player = Main.player[Target];

		if (!player.active)
		{
			Collected = true;
			return;
		}

		if (!Collected && who != experienceList.Length - 1)
		{
			for (int i = who + 1; i < experienceList.Length; i++)
			{
				Experience exp = experienceList[i];

				if (exp != null && !exp.Collected && exp.Target == Target && Center.DistanceSQ(exp.Center) < 18 * 18 && (long)exp._value + _value < int.MaxValue)
				{
					exp.Collected = true;
					exp.Active = false;
					_value += exp._value;
				}
			}
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
		Player player = Main.player[Target];

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

	public override string ToString()
	{
		return $"{Active}: Collected: {Collected} Value: {_value}";
	}
}