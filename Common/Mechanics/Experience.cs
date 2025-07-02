using System.Collections.Generic;
using System.Linq;
using PathOfTerraria.Common.Systems.ModPlayers;

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

	private const int ExtraUpdates = 1;

	public readonly float Rotation;
	public readonly int Target;

	private readonly List<Vector2> _oldCenters;
	private readonly float _magnitude;

	public Vector2 Center;
	public bool Active;
	public bool Collected;

	private Vector2 _velocity;
	private int _value;
	private byte _canMergeTimer = 255;
	private float _modMagnitude = 0;

	public Experience(int xp, Vector2 startPosition, Vector2 startVelocity, int targetPlayer)
	{
		_value = xp;

		GetSize(); // Used only to throw an ArgumentException if it fails

		Center = startPosition;
		Active = true;
		Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
		Target = targetPlayer;

		_velocity = startVelocity;
		_magnitude = startVelocity.Length();
		_oldCenters = [.. Enumerable.Repeat(Center, 8)];
		_canMergeTimer = 120;
		_modMagnitude = _magnitude;
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

		Player player = Main.player[Target];

		if (!player.active) // Clear if the player isn't active
		{
			Collected = true;
			return;
		}

		if (Collected)
		{
			if (_oldCenters.Count == 0) // Make the trail shrink
			{
				Active = false;
			}
			else
			{
				_oldCenters.RemoveAt(0);
			}

			return;
		}

		MergeFunctionality(who, experienceList);

		for (int i = 0; i < ExtraUpdates + 1; i++)
		{
			InnerUpdate();
		}
	}

	private void MergeFunctionality(int who, Experience[] experienceList)
	{
		if (_canMergeTimer > 0)
		{
			_canMergeTimer--;
		}

		if (!Collected && who != experienceList.Length - 1 && _canMergeTimer == 0)
		{
			for (int i = who + 1; i < experienceList.Length; i++)
			{
				Experience exp = experienceList[i];

				if (exp != null && !exp.Collected && exp.Target == Target && (long)exp._value + _value < int.MaxValue)
				{
					float dist = Center.DistanceSQ(exp.Center);

					if (dist < 5 * 5)
					{
						exp.Collected = true;
						_value += exp._value;
					}
					else if (dist < 24 * 24)
					{
						exp._velocity += exp.Center.DirectionTo(Center) * 0.7f;
						exp._modMagnitude = exp._magnitude * 1.25f;
					}
				}
			}
		}
	}

	private void InnerUpdate()
	{
		Player player = Main.player[Target];

		_velocity += player.DirectionFrom(Center) * 0.2f; // Lazily home towards player

		if (player.DistanceSQ(Center) < 120 * 120) // Decay velocity a bit so the exp doesn't forever dance around the player
		{
			_velocity *= 0.99f;
		}

		if (_velocity.LengthSquared() > _modMagnitude * _modMagnitude) // Cap speed to original magnitude
		{
			_velocity = Vector2.Normalize(_velocity) * _modMagnitude;
		}

		_modMagnitude = MathHelper.Lerp(_modMagnitude, _magnitude, 0.1f);

		if (Main.GameUpdateCount % 2 == 0) // Increase spacing for trails
		{
			if (_oldCenters.Count >= 8 * (ExtraUpdates + 1))
			{
				_oldCenters.RemoveAt(0);
			}

			_oldCenters.Add(Center);
		}

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
	}

	public override string ToString()
	{
		return $"{Active}: Collected: {Collected} Value: {_value}";
	}

	internal void Draw(SpriteBatch batch, Texture2D texture, bool isTrail = false, Vector2? centerOverride = null, float opacity = 1f)
	{
		if (!Active)
		{
			return;
		}

		if (!isTrail)
		{
			DrawTrail(batch, texture);
		}

		Vector2 size = GetSize() switch
		{
			Sizes.OrbSmallYellow or Sizes.OrbSmallGreen or Sizes.OrbSmallBlue => new Vector2(6),
			Sizes.OrbMediumYellow or Sizes.OrbMediumGreen or Sizes.OrbMediumBlue => new Vector2(8),
			Sizes.OrbLargeYellow or Sizes.OrbLargeGreen or Sizes.OrbLargeBlue => new Vector2(10),
			_ => Vector2.Zero
		};

		if (size == Vector2.Zero)
		{
			return;
		}

		Rectangle source = GetSourceRectangle();
		float scale = GetScale();

		batch.Draw(texture, (centerOverride ?? Center) - Main.screenPosition, source, Color.White * opacity, Rotation, size / 2f, scale, SpriteEffects.None, 0);
	}

	internal void DrawTrail(SpriteBatch batch, Texture2D texture)
	{
		for (int i = 0; i < _oldCenters.Count; i++)
		{
			Vector2 item = _oldCenters[i];
			Draw(batch, texture, true, item, i / (float)_oldCenters.Count * 0.5f);
		}
	}
}