using PathOfTerraria.Core.Mechanics;
using System.Collections.Generic;
using Terraria.ID;

namespace PathOfTerraria.Core.Systems
{
	// ReSharper disable once ClassNeverInstantiated.Global
	public class ExperienceTracker : ModSystem{
		private static Experience[] _trackedExp;

		public override void OnWorldLoad(){
			_trackedExp = new Experience[1000];
		}

		public override void PostUpdateNPCs()
		{
			foreach (Experience t in _trackedExp) t?.Update();
		}

		public override void PostDrawTiles(){
			SpriteBatch batch = Main.spriteBatch;
			batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

			//Draw the orbs
			Texture2D texture = ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/Experience").Value;
			foreach (Experience xp in _trackedExp)
			{
				if(xp is null || !xp.Active) continue;

				Vector2 size = xp.GetSize();
				
				if(size == Vector2.Zero || xp.Collected) continue;

				Rectangle source = xp.GetSourceRectangle();

				batch.Draw(texture, xp.Center - Main.screenPosition, source, Color.White, xp.Rotation, size / 2f, 1f, SpriteEffects.None, 0);
			}
			batch.End();

			//Draw the trails
			foreach (Experience t in _trackedExp) t?.DrawTrail();
		}

		public static int[] SpawnExperience(int xp, Vector2 location, float velocityLength, int targetPlayer){
			if (xp <= 0) return [];

			List<Experience> spawned = [];
			int totalLeft = xp;
			while(totalLeft > 0){
				int toSpawn;
				switch (totalLeft)
				{
					case >= Experience.Sizes.OrbLargeBlue:
						toSpawn = Experience.Sizes.OrbLargeBlue;
						totalLeft -= Experience.Sizes.OrbLargeBlue;
						break;
					case >= Experience.Sizes.OrbLargeGreen:
						toSpawn = Experience.Sizes.OrbLargeGreen;
						totalLeft -= Experience.Sizes.OrbLargeGreen;
						break;
					case >= Experience.Sizes.OrbLargeYellow:
						toSpawn = Experience.Sizes.OrbLargeYellow;
						totalLeft -= Experience.Sizes.OrbLargeYellow;
						break;
					case >= Experience.Sizes.OrbMediumBlue:
						toSpawn = Experience.Sizes.OrbMediumBlue;
						totalLeft -= Experience.Sizes.OrbMediumBlue;
						break;
					case >= Experience.Sizes.OrbMediumGreen:
						toSpawn = Experience.Sizes.OrbMediumGreen;
						totalLeft -= Experience.Sizes.OrbMediumGreen;
						break;
					case >= Experience.Sizes.OrbMediumYellow:
						toSpawn = Experience.Sizes.OrbMediumYellow;
						totalLeft -= Experience.Sizes.OrbMediumYellow;
						break;
					case >= Experience.Sizes.OrbSmallBlue:
						toSpawn = Experience.Sizes.OrbSmallBlue;
						totalLeft -= Experience.Sizes.OrbSmallBlue;
						break;
					case >= Experience.Sizes.OrbSmallGreen:
						toSpawn = Experience.Sizes.OrbSmallGreen;
						totalLeft -= Experience.Sizes.OrbSmallGreen;
						break;
					default:
						toSpawn = Experience.Sizes.OrbSmallYellow;
						totalLeft--;
						break;
				}

				var thing = new Experience(toSpawn, location, Vector2.UnitX.RotatedByRandom(MathHelper.Pi) * velocityLength, targetPlayer);
				spawned.Add(thing);
			}

			int[] indices = new int[spawned.Count];
			for(int i = 0; i < indices.Length; i++)
				indices[i] = InsertExperience(spawned[i]);

			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				Networking.Networking.SendSpawnExperienceOrbs(Main.myPlayer, targetPlayer, xp, location, velocityLength);	
			}

			return indices;
		}

		private static int InsertExperience(Experience expNew){
			for(int i = 0; i < _trackedExp.Length; i++){
				Experience exp = _trackedExp[i];

				if (exp is not null && exp.Active) continue;
				_trackedExp[i] = expNew;
				return i;
			}

			int index = _trackedExp.Length;
			Array.Resize(ref _trackedExp, _trackedExp.Length * 2);

			_trackedExp[index] = expNew;
			return index;
		}
	}
}