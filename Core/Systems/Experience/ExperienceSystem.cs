using PathOfTerraria.Core.Mechanics;
using System.Collections.Generic;
using Terraria.ID;

namespace PathOfTerraria.Core.Systems
{
	public class ExperienceTracker : ModSystem{
		private static Experience[] trackedEXP;

		public override void OnWorldLoad(){
			trackedEXP = new Experience[1000];
		}

		public override void PostUpdateNPCs(){
			for(int i = 0; i < trackedEXP.Length; i++)
				trackedEXP[i]?.Update();
		}

		public override void PostDrawTiles(){
			var batch = Main.spriteBatch;
			batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

			//Draw the orbs
			var texture = ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/Experience").Value;
			for(int i = 0; i < trackedEXP.Length; i++){
				var xp = trackedEXP[i];
				if(xp is null || !xp.active)
					continue;

				var size = xp.GetSize();
				if(size == Vector2.Zero || xp.collected)
					continue;

				var source = xp.GetSourceRectangle();

				batch.Draw(texture, xp.center - Main.screenPosition, source, Color.White, xp.rotation, size / 2f, 1f, SpriteEffects.None, 0);
			}
			batch.End();

			//Draw the trails
			foreach (Experience t in trackedEXP)
				t?.DrawTrail();
		}

		public static int[] SpawnExperience(int xp, Vector2 location, float velocityLength, int targetPlayer){
			if (xp <= 0) return [];

			List<Experience> spawned = [];
			int totalLeft = xp;
			while(totalLeft > 0){
				int toSpawn;
				if(totalLeft >= Experience.Sizes.OrbLargeBlue){
					toSpawn = Experience.Sizes.OrbLargeBlue;
					totalLeft -= Experience.Sizes.OrbLargeBlue;
				}else if(totalLeft >= Experience.Sizes.OrbLargeGreen){
					toSpawn =Mechanics. Experience.Sizes.OrbLargeGreen;
					totalLeft -= Experience.Sizes.OrbLargeGreen;
				}else if(totalLeft >= Experience.Sizes.OrbLargeYellow){
					toSpawn = Experience.Sizes.OrbLargeYellow;
					totalLeft -= Experience.Sizes.OrbLargeYellow;
				}else if(totalLeft >= Experience.Sizes.OrbMediumBlue){
					toSpawn = Experience.Sizes.OrbMediumBlue;
					totalLeft -= Experience.Sizes.OrbMediumBlue;
				}else if(totalLeft >= Experience.Sizes.OrbMediumGreen){
					toSpawn = Experience.Sizes.OrbMediumGreen;
					totalLeft -= Experience.Sizes.OrbMediumGreen;
				}else if(totalLeft >= Experience.Sizes.OrbMediumYellow){
					toSpawn = Experience.Sizes.OrbMediumYellow;
					totalLeft -= Experience.Sizes.OrbMediumYellow;
				}else if(totalLeft >= Experience.Sizes.OrbSmallBlue){
					toSpawn = Experience.Sizes.OrbSmallBlue;
					totalLeft -= Experience.Sizes.OrbSmallBlue;
				}else if(totalLeft >= Experience.Sizes.OrbSmallGreen){
					toSpawn = Experience.Sizes.OrbSmallGreen;
					totalLeft -= Experience.Sizes.OrbSmallGreen;
				}else{
					toSpawn = Experience.Sizes.OrbSmallYellow;
					totalLeft--;
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
			for(int i = 0; i < trackedEXP.Length; i++){
				Experience exp = trackedEXP[i];

				if (exp is not null && exp.active) continue;
				trackedEXP[i] = expNew;
				return i;
			}

			int index = trackedEXP.Length;
			Array.Resize(ref trackedEXP, trackedEXP.Length * 2);

			trackedEXP[index] = expNew;
			return index;
		}
	}
}