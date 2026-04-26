using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Projectiles.PassiveProjectiles;
using ReLogic.Content;
using Terraria.DataStructures;

namespace PathOfTerraria.Content.Passives.Magic.Masteries;

internal class StarcallerMastery : Passive
{
	internal class StarcallerHeadLayer : PlayerDrawLayer
	{
		private static Asset<Texture2D> StarcallerHead = null;

		public override void Load()
		{
			StarcallerHead = ModContent.Request<Texture2D>("PathOfTerraria/Assets/Misc/VFX/Starcaller_Head");
		}

		public override Position GetDefaultPosition()
		{
			return new AfterParent(PlayerDrawLayers.FinchNest);
		}

		protected override void Draw(ref PlayerDrawSet drawInfo)
		{
			Player player = drawInfo.drawPlayer;

			if (drawInfo.headOnlyRender || player.dead || Main.gameMenu || !player.GetModPlayer<PassiveTreePlayer>().HasNode<StarcallerMastery>())
			{
				return;
			}

			Vector2 bob = Main.OffsetsPlayerHeadgear[player.bodyFrame.Y / player.bodyFrame.Height];
			float heightAdj = player.mount.Active ? player.mount.HeightBoost : 0;
			Vector2 position = drawInfo.Position - Main.screenPosition - new Vector2(-10, 2 - bob.Y - heightAdj);
			SpriteEffects effect = player.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

			if (effect == SpriteEffects.FlipHorizontally)
			{
				position.X -= 12;
			}

			drawInfo.DrawDataCache.Add(new DrawData(StarcallerHead.Value, position.Floor(), null, Color.White, 0f, Vector2.Zero, 1f, effect, 0));
		}
	}

	internal class StarcallerPlayer : ModPlayer
	{
		public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (!hit.DamageType.CountsAsClass(DamageClass.Magic))
			{
				return;
			}

			int type = ModContent.ProjectileType<StarcallerStar>();

			if (proj.type != type && Player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<StarcallerMastery>(out float value) && hit.Crit)
			{
				IEntitySource src = Player.GetSource_OnHit(target);
				Vector2 pos = target.Center - new Vector2(0, 800);
				Projectile.NewProjectile(src, pos, new Vector2(0, 8), type, (int)(damageDone), 2, Player.whoAmI, target.whoAmI);
			}
		}

		public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (hit.DamageType.CountsAsClass(DamageClass.Magic) && Player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<StarcallerMastery>(out float value) && hit.Crit)
			{
				IEntitySource src = Player.GetSource_OnHit(target);
				Vector2 pos = target.Center - new Vector2(0, 800);
				int type = ModContent.ProjectileType<StarcallerStar>();
				Projectile.NewProjectile(src, pos, new Vector2(0, 8), type, (int)(damageDone * value / 100f), 2, Player.whoAmI, target.whoAmI);
			}
		}
	}
}
