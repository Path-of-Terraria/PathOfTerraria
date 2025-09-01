using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems;
using PathOfTerraria.Common.Systems.Skills;
using ReLogic.Content;
using Terraria.ID;

namespace PathOfTerraria.Content.SkillPassives.RainOfArrowsPassives;

internal class CreepingVines(SkillTree tree) : SkillPassive(tree)
{
	public class VineNPC : GlobalNPC
	{
		public override bool InstancePerEntity => true;

		private static Asset<Texture2D> VineTex = null;

		public Vector2? Vined = null;

		public override void Load()
		{
			VineTex = ModContent.Request<Texture2D>("PathOfTerraria/Assets/SkillPassives/RainOfArrowsPassives/CreepingVineVine");
		}

		public override bool PreAI(NPC npc)
		{
			if (Vined.HasValue)
			{
				Vector2 anchor = Vined.Value;
				float sqDist = npc.DistanceSQ(anchor);

				if (sqDist < 180 * 180)
				{
					float mod = Utils.GetLerpValue(120 * 120, 180 * 180, sqDist, true) * 0.4f;
					npc.GetGlobalNPC<SlowDownNPC>().SpeedModifier += 0.1f + mod;
				}
				else
				{
					float dist = npc.Distance(anchor) / 10f;
					
					for (int i = 0; i < dist; ++i)
					{
						Dust.NewDustPerfect(Vector2.Lerp(npc.Center, anchor, i / dist), DustID.Grass, Main.rand.NextVector2Circular(3, 3));
					}

					Vined = null;
				}
			}

			return true;
		}

		public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			if (Vined.HasValue)
			{
				Vector2 anchor = Vined.Value;
				Vector2 target = npc.Center + new Vector2(0, npc.gfxOffY);
				float scale = 1f;
				int texHeight = VineTex.Height();
				float dist = anchor.Distance(target);

				if (dist > texHeight)
				{
					scale = dist / texHeight;
					target += Main.rand.NextVector2CircularEdge(scale - 1, scale - 1) * 6;
				}

				float rotation = anchor.AngleTo(target) - MathHelper.PiOver2;
				float vineLength = MathF.Min(dist, texHeight) / texHeight;
				var src = new Rectangle(0, 0, 14, (int)(vineLength * texHeight));
				Color color = Lighting.GetColor(anchor.ToTileCoordinates());
				
				spriteBatch.Draw(VineTex.Value, anchor - Main.screenPosition, src, color, rotation, new Vector2(7, 0), new Vector2(2 - scale, scale), SpriteEffects.None, 0);
			}

			return true;
		}
	}
}
