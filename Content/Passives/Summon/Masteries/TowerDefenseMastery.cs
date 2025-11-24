using PathOfTerraria.Common.Config;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using ReLogic.Content;
using Terraria.DataStructures;

namespace PathOfTerraria.Content.Passives.Summon.Masteries;

internal class TowerDefenseMastery : Passive
{
	internal class TowerDefenseProjectile : GlobalProjectile
	{
		private static Asset<Texture2D> DamageAura = null;

		public override bool InstancePerEntity => true;

		private int? _originalDamage = 0;
		private int _timer = 0;

		public override void Load()
		{
			DamageAura = ModContent.Request<Texture2D>("PathOfTerraria/Assets/Misc/VFX/SentryDamageAura");
		}

		public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
		{
			return entity.sentry;
		}

		public override void OnSpawn(Projectile projectile, IEntitySource source)
		{
			if (projectile.sentry && source is EntitySource_ItemUse_WithAmmo { Player: Player player, Item: Item item } 
				&& player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<TowerDefenseMastery>(out float value))
			{
				_originalDamage = item.damage;
			}
		}

		public override void AI(Projectile p)
		{
			if (!_originalDamage.HasValue || !p.TryGetOwner(out Player o) || !o.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<TowerDefenseMastery>(out float value))
			{
				return;
			}

			_timer++;

			if (_timer >= 60 * value)
			{
				foreach (NPC npc in Main.ActiveNPCs)
				{
					if (npc.CanBeChasedBy() && npc.DistanceSQ(p.Center) < PoTMod.NearbyDistanceSq)
					{
						npc.SimpleStrikeNPC(_originalDamage.Value, 0);
					}
				}

				_timer = 0;
			}
		}

		public override bool PreDraw(Projectile proj, ref Color lightColor)
		{
			if (!proj.TryGetOwner(out Player owner) || !owner.GetModPlayer<PassiveTreePlayer>().HasNode<TowerDefenseMastery>() || !proj.sentry
				|| !ModContent.GetInstance<GameplayConfig>().NearbyAuras)
			{
				return true;
			}

			Vector2 pos = proj.Center - Main.screenPosition;
			float opacity = 0.1f;

			if (_timer < 60)
			{
				opacity = (1 - _timer / 60f) * 0.4f;
			}

			opacity = MathF.Max(opacity, 0.1f);

			float rotation = Main.GameUpdateCount * 0.025f;
			Main.spriteBatch.Draw(DamageAura.Value, pos, null, Color.White * opacity, rotation, DamageAura.Size() / 2f, 1f, SpriteEffects.None, 0);
			return true;
		}
	}
}