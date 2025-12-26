using PathOfTerraria.Common;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Projectiles.Utility;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

namespace PathOfTerraria.Content.Passives.Melee.Masteries;

#nullable enable

internal class SpoolbreakerMastery : Passive
{
	internal class SpoolbreakerProjectile : GlobalProjectile
	{
		public override bool InstancePerEntity => true;

		private float _originalScale = 1f;
		private Vector2 _originalSize = Vector2.One;
		private int _timeSinceLastHit = 0;

		public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
		{
			return entity.aiStyle == ProjAIStyleID.Yoyo;
		}

		public override void OnSpawn(Projectile projectile, IEntitySource source)
		{
			if (source is EntitySource_ItemUse_WithAmmo)
			{
				_originalScale = projectile.scale;
				_originalSize = projectile.Size;
			}
		}

		public override void AI(Projectile projectile)
		{
			_timeSinceLastHit++;

			if (_timeSinceLastHit > 60)
			{
				projectile.scale = MathHelper.Lerp(projectile.scale, _originalScale, 0.01f);
			}
			
			Vector2 siz = _originalSize * projectile.scale;
			projectile.Resize((int)siz.X, (int)siz.Y);
		}

		public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (!projectile.TryGetOwner(out Player? plr) || !plr.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<SpoolbreakerMastery>(out float value))
			{
				return;
			}

			projectile.scale += 0.1f;

			_timeSinceLastHit = 0;

			if (projectile.scale > 3f)
			{
				ExplosionHitbox.QuickSpawn(projectile.GetSource_Death(), target, (int)(damageDone * value / 100f), projectile.owner, new Vector2(200));
				projectile.scale = 0f;
				projectile.GetOwner().channel = false;
			}
		}

		public override bool PreDraw(Projectile projectile, ref Color lightColor)
		{
			if (projectile.scale > _originalScale)
			{
				Texture2D tex = TextureAssets.Projectile[projectile.type].Value;
				Main.spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, lightColor, projectile.rotation, tex.Size() / 2f, projectile.scale, SpriteEffects.None, 0);
				return false;
			}

			return true;
		}
	}
}
