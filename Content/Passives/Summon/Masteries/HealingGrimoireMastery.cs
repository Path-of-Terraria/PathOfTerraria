using PathOfTerraria.Common.Config;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Projectiles.Summoner;
using ReLogic.Content;

namespace PathOfTerraria.Content.Passives.Summon.Masteries;

internal class HealingGrimoireMastery : Passive
{
	internal class HealingGrimioreProjectile : GlobalProjectile
	{
		public override bool InstancePerEntity => true;

		private static Asset<Texture2D> Aura = null;

		private short _healTimer = 0;
		private float _healScale = 0;

		public override void Load()
		{
			Aura = ModContent.Request<Texture2D>("PathOfTerraria/Assets/Misc/VFX/HealingPulseAura");
		}

		public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
		{
			return entity.ModProjectile is GrimoireSummon;
		}

		public override bool PreAI(Projectile projectile)
		{
			if (projectile.TryGetOwner(out Player owner) && owner != null && owner.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<HealingGrimoireMastery>(out float value))
			{
				_healTimer++;

				if (_healTimer == 280)
				{
					foreach (Player player in Main.ActivePlayers)
					{
						if (player.DistanceSQ(projectile.Center) < PoTMod.NearbyDistanceSq)
						{
							player.Heal((int)value);
						}
					}
				}

				if (_healTimer == 300)
				{
					_healTimer = 0;
				}
			}

			return true;
		}

		public override bool PreDraw(Projectile projectile, ref Color lightColor)
		{
			if (_healTimer >= 280 && ModContent.GetInstance<GameplayConfig>().NearbyAuras)
			{
				int modTimer = _healTimer - 260;
				Texture2D tex = Aura.Value;
				float alpha = modTimer / 10f;
				Vector2 pos = projectile.Center - Main.screenPosition;

				if (_healTimer >= 290f)
				{
					alpha = Utils.GetLerpValue(300f, 290f, _healTimer, true);
				}

				_healScale = MathHelper.Lerp(_healScale, 1.5f, 0.1f);
				Color col = Color.Lerp(lightColor, Color.White, 0.33f) * alpha * 0.6f;
				Main.spriteBatch.Draw(tex, pos, null, col, Main.GameUpdateCount * 0.03f, tex.Size() / 2f, _healScale, SpriteEffects.None, 0);
			}
			else
			{
				_healScale = 1;
			}

			return true;
		}
	}
}