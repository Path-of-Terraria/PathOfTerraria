using PathOfTerraria.Common.Config;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Projectiles.Summoner;
using ReLogic.Content;
using Terraria.Localization;

namespace PathOfTerraria.Content.Passives.Summon.Masteries;

internal class HealingGrimoireMastery : Passive
{
	internal class HealingGrimoireProjectile : GlobalProjectile
	{
		/// <summary>
		/// Amount of time before the healing timer resets.
		/// </summary>
		private const int MaxHealTime = HealTimeInSeconds * 60;

		/// <summary>
		/// The amount of time until the projectile actually does the heal pulse.
		/// </summary>
		private const int ActualHealTime = MaxHealTime - 20;

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

				if (_healTimer == ActualHealTime)
				{
					foreach (Player player in Main.ActivePlayers)
					{
						if (player.DistanceSQ(projectile.Center) < PoTMod.NearbyDistanceSq)
						{
							player.Heal((int)value);
						}
					}
				}

				if (_healTimer == MaxHealTime)
				{
					_healTimer = 0;
				}
			}

			return true;
		}

		public override bool PreDraw(Projectile projectile, ref Color lightColor)
		{
			if (_healTimer >= ActualHealTime && ModContent.GetInstance<GameplayConfig>().NearbyAuras)
			{
				int modTimer = _healTimer - ActualHealTime - 20;
				Texture2D tex = Aura.Value;
				float alpha = modTimer / 10f;
				Vector2 pos = projectile.Center - Main.screenPosition;

				if (_healTimer >= ActualHealTime + 10)
				{
					alpha = Utils.GetLerpValue(MaxHealTime, ActualHealTime + 10, _healTimer, true);
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

	public const int HealTimeInSeconds = 5;

	public override string DisplayTooltip => Language.GetText($"Mods.PathOfTerraria.Passives.{Name}.Tooltip").Format(Value, HealTimeInSeconds);
}