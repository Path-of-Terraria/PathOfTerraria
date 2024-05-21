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

		public readonly int value;

		public Vector2 center;
		public Vector2 velocity;

		public readonly int target;

		private readonly Queue<Vector2> oldCenters;
		private Color[] collectedTrail;

		public bool active;
		public bool collected;
		private bool oldCollected;

		public const int ExtraUpdates = 7;

		public readonly float rotation;

		public Experience(int xp, Vector2 startPosition, Vector2 startVelocity, int targetPlayer){
			value = xp;

			Vector2 size = GetSize();
			if(size == Vector2.Zero)
				throw new Exception("Invalid xp count: " + xp);

			center = startPosition;
			velocity = startVelocity;

			target = targetPlayer;

			oldCenters = new();

			active = true;

			rotation = Main.rand.NextFloat(MathHelper.TwoPi);
		}

		public Vector2 GetSize(){
			return value switch
			{
				Sizes.OrbSmallYellow or Sizes.OrbSmallGreen or Sizes.OrbSmallBlue => new Vector2(6),
				Sizes.OrbMediumYellow or Sizes.OrbMediumGreen or Sizes.OrbMediumBlue => new Vector2(8),
				Sizes.OrbLargeYellow or Sizes.OrbLargeGreen or Sizes.OrbLargeBlue => new Vector2(10),
				_ => Vector2.Zero
			};
		}

		public Color GetTrailColor(){
			if(value is Sizes.OrbSmallYellow or Sizes.OrbMediumYellow or Sizes.OrbLargeYellow)
				return Color.Yellow;
			if(value is Sizes.OrbSmallGreen or Sizes.OrbMediumGreen or Sizes.OrbLargeGreen)
				return Color.LimeGreen;
			if(value is Sizes.OrbSmallBlue or Sizes.OrbMediumBlue or Sizes.OrbLargeBlue)
				return Color.Blue;
			return Color.Transparent;
		}

		public Rectangle GetSourceRectangle()
			=> value switch{
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
			if(!active)
				return;

			oldCollected = collected;

			//Home in on the player, unless they've disconnected
			Player player = Main.player[target];

			if(!player.active)
				collected = true;

			if(collected){
				//Make the trail shrink
				if (oldCenters.Count == 0)
				{
					active = false;	
				}
				else
				{
					for (int i = 0; i < ExtraUpdates + 1; i++)
					{
						oldCenters.Dequeue();	
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
			Player player = Main.player[target];

			Vector2 direction = player.DirectionFrom(center);

			switch (player.dead)
			{
				case false when !collected:
					{
						if(velocity != Vector2.Zero)
							velocity = velocity.RotateTowards(direction, MathHelper.ToRadians(270) / 60f / (ExtraUpdates + 1));

						if(Vector2.DistanceSquared(center, player.Center) >= Vector2.DistanceSquared(center + velocity / (ExtraUpdates + 1), player.Center)){
							velocity += Vector2.Normalize(velocity) * 5f / 60f / (ExtraUpdates + 1);

							const float vel = 30 * 16;
							if(velocity.LengthSquared() > vel * vel)
								velocity = Vector2.Normalize(velocity) * vel;
						}else
							velocity *= 1f - 3.57f / 60f / (ExtraUpdates + 1);

						break;
					}
				case true:
					{
						//Slow down
						velocity *= 1f - 0.37f / 60f;

						if(velocity.LengthSquared() < 0.5f * 0.5f)
							velocity = Vector2.Zero;
						break;
					}
			}

			if(oldCenters.Count < 30 * (ExtraUpdates + 1))
				oldCenters.Enqueue(center);
			else{
				oldCenters.Dequeue();
				oldCenters.Enqueue(center);
			}

			center += velocity / (ExtraUpdates + 1);

			if(!collected && !player.dead && player.Hitbox.Contains(center.ToPoint())){
				ExpModPlayer statPlayer = player.GetModPlayer<ExpModPlayer>();
				statPlayer.Exp += value;
				CombatText.NewText(player.Hitbox, new Color(145, 255, 160), $"+{value}");
				collected = true;
			}

			//Draw calls happen less often than Update calls during lag... which causes the arrays to not be the same size
			//Therefore, the data must be set here instead of in DrawTrail
			Color color = GetTrailColor();
			int trailColorCount = oldCenters.Count + 1;
			var colors = new Color[trailColorCount];

			if (oldCollected && collectedTrail is not null) return;
			colors[^1] = color;
			int i = 0;
			foreach(Vector2 old in oldCenters){
				colors[i] = Color.Lerp(color, Color.Transparent, 1f - i / (float)trailColorCount);
				i++;
			}

			collectedTrail = colors;
		}

		public void DrawTrail(){
			if(!active)
				return;

			Vector2 size = GetSize();
			if(size == Vector2.Zero)
				return;

			//No trail yet
			if(oldCenters.Count == 0)
				return;

			Color color = GetTrailColor();
			int trailColorCount = oldCenters.Count + 1;
			var colors = new Color[trailColorCount];

			if(!oldCollected){
				//Manually set the colours
				colors[^1] = color;
				int i = 0;
				foreach(Vector2 old in oldCenters){
					colors[i] = Color.Lerp(color, Color.Transparent, 1f - i / (float)trailColorCount);
					i++;
				}
			}else{
				//If collected, just grab colors from the old array
				Array.Copy(collectedTrail, collectedTrail.Length - trailColorCount, colors, 0, trailColorCount);
			}

			var points = new Vector2[trailColorCount];
			points[^1] = center;
			oldCenters.CopyTo(points, 0);

			PrimitiveDrawing.DrawLineStrip(points, colors);
		}
	}
}