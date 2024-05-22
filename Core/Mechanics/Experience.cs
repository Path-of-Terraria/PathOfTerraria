using PathOfTerraria.API.GraphicsLib;
using PathOfTerraria.Core.Systems;
using System.Collections.Generic;

namespace PathOfTerraria.Core.Mechanics
{
	public sealed class Experience{
		public static class Sizes{
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

		private readonly int Value;

		public Vector2 Center;
		private Vector2 Velocity;

		private readonly int Target;

		private readonly Queue<Vector2> _oldCenters;
		private Color[] _collectedTrail;

		public bool Active;
		public bool Collected;
		private bool _oldCollected;

		private const int ExtraUpdates = 7;

		public readonly float Rotation;

		public Experience(int xp, Vector2 startPosition, Vector2 startVelocity, int targetPlayer){
			Value = xp;

			Vector2 size = GetSize();
			if(size == Vector2.Zero)
				throw new Exception("Invalid xp count: " + xp);

			Center = startPosition;
			Velocity = startVelocity;

			Target = targetPlayer;

			_oldCenters = new Queue<Vector2>();

			Active = true;

			Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
		}

		public Vector2 GetSize(){
			return Value switch
			{
				Sizes.OrbSmallYellow or Sizes.OrbSmallGreen or Sizes.OrbSmallBlue => new Vector2(6),
				Sizes.OrbMediumYellow or Sizes.OrbMediumGreen or Sizes.OrbMediumBlue => new Vector2(8),
				Sizes.OrbLargeYellow or Sizes.OrbLargeGreen or Sizes.OrbLargeBlue => new Vector2(10),
				_ => Vector2.Zero
			};
		}

		private Color GetTrailColor(){
			return Value switch
			{
				Sizes.OrbSmallYellow or Sizes.OrbMediumYellow or Sizes.OrbLargeYellow => Color.Yellow,
				Sizes.OrbSmallGreen or Sizes.OrbMediumGreen or Sizes.OrbLargeGreen => Color.LimeGreen,
				Sizes.OrbSmallBlue or Sizes.OrbMediumBlue or Sizes.OrbLargeBlue => Color.Blue,
				_ => Color.Transparent
			};
		}

		public Rectangle GetSourceRectangle()
			=> Value switch{
				Sizes.OrbSmallYellow =>  new Rectangle( 0,  0,  6,  6),
				Sizes.OrbSmallGreen =>   new Rectangle( 8,  0,  6,  6),
				Sizes.OrbSmallBlue =>    new Rectangle(16,  0,  6,  6),
				Sizes.OrbMediumYellow => new Rectangle( 0,  8,  8,  8),
				Sizes.OrbMediumGreen =>  new Rectangle(10,  8,  8,  8),
				Sizes.OrbMediumBlue =>   new Rectangle(20,  8,  8,  8),
				Sizes.OrbLargeYellow =>  new Rectangle( 0, 18, 10, 10),
				Sizes.OrbLargeGreen =>   new Rectangle(12, 18, 10, 10),
				Sizes.OrbLargeBlue =>    new Rectangle(24, 18, 10, 10),
				_ => Rectangle.Empty
			};

		public void Update(){
			if(!Active) return;

			_oldCollected = Collected;

			//Home in on the player, unless they've disconnected
			Player player = Main.player[Target];

			if(!player.active)
				Collected = true;

			if(Collected){
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

		private void InnerUpdate(){
			Player player = Main.player[Target];

			Vector2 direction = player.DirectionFrom(Center);

			switch (player.dead)
			{
				case false when !Collected:
					{
						if (Velocity != Vector2.Zero)
						{
							Velocity = Velocity.RotateTowards(direction, MathHelper.ToRadians(270) / 60f / (ExtraUpdates + 1));
						}

						if(Vector2.DistanceSquared(Center, player.Center) >= Vector2.DistanceSquared(Center + Velocity / (ExtraUpdates + 1), player.Center)) {
							Velocity += Vector2.Normalize(Velocity) * 5f / 60f / (ExtraUpdates + 1);

							const float vel = 30 * 16;
							if (Velocity.LengthSquared() > vel * vel)
							{
								Velocity = Vector2.Normalize(Velocity) * vel;
							}
						}
						else
						{
							Velocity *= 1f - 3.57f / 60f / (ExtraUpdates + 1);
						}

						break;
					}
				case true:
					{
						//Slow down
						Velocity *= 1f - 0.37f / 60f;

						if (Velocity.LengthSquared() < 0.5f * 0.5f)
						{
							Velocity = Vector2.Zero;
						}
						break;
					}
			}

			if (_oldCenters.Count < 30 * (ExtraUpdates + 1))
			{
				_oldCenters.Enqueue(Center);
			} else {
				_oldCenters.Dequeue();
				_oldCenters.Enqueue(Center);
			}

			Center += Velocity / (ExtraUpdates + 1);

			if(!Collected && !player.dead && player.Hitbox.Contains(Center.ToPoint())){
				ExpModPlayer statPlayer = player.GetModPlayer<ExpModPlayer>();
				statPlayer.Exp += Value;
				CombatText.NewText(player.Hitbox, new Color(145, 255, 160), $"+{Value}");
				Collected = true;
			}

			//Draw calls happen less often than Update calls during lag... which causes the arrays to not be the same size
			//Therefore, the data must be set here instead of in DrawTrail
			Color color = GetTrailColor();
			int trailColorCount = _oldCenters.Count + 1;
			var colors = new Color[trailColorCount];

			if (_oldCollected && _collectedTrail is not null) return;
			colors[^1] = color;
			int i = 0;
			foreach(Vector2 _ in _oldCenters){
				colors[i] = Color.Lerp(color, Color.Transparent, 1f - i / (float)trailColorCount);
				i++;
			}

			_collectedTrail = colors;
		}

		public void DrawTrail(){
			if(!Active) return;

			Vector2 size = GetSize();
			if(size == Vector2.Zero) return;

			//No trail yet
			if (_oldCenters.Count == 0) return;

			Color color = GetTrailColor();
			int trailColorCount = _oldCenters.Count + 1;
			var colors = new Color[trailColorCount];

			if(!_oldCollected){
				//Manually set the colors
				colors[^1] = color;
				int i = 0;
				foreach(Vector2 _ in _oldCenters){
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
}