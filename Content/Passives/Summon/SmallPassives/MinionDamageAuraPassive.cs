using PathOfTerraria.Common.Config;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Buffs;
using ReLogic.Content;

namespace PathOfTerraria.Content.Passives;

internal class MinionDamageAuraPassive : Passive
{
	public class MinionDamageAuraProjectile : GlobalProjectile
	{
		private static Asset<Texture2D> Aura = null;

		public override void Load()
		{
			Aura = ModContent.Request<Texture2D>("PathOfTerraria/Assets/Misc/VFX/MinionDamageAura");
		}

		public override bool PreAI(Projectile proj)
		{
			if (!proj.TryGetOwner(out Player owner) || !owner.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<MinionDamageAuraPassive>(out float value)
				|| !proj.minion)
			{
				return true;
			}

			foreach (Player player in Main.ActivePlayers)
			{
				if (player.DistanceSQ(proj.Center) < PoTMod.NearbyDistanceSq && !player.HasBuff<MinionDamageAuraBuff>())
				{
					player.GetDamage(DamageClass.Generic) += value / 100f;
					player.AddBuff(ModContent.BuffType<MinionDamageAuraBuff>(), 2);
				}
			}

			return true;
		}

		public override bool PreDraw(Projectile proj, ref Color lightColor)
		{
			if (!proj.TryGetOwner(out Player owner) || !owner.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<MinionDamageAuraPassive>(out float value) || !proj.sentry
				|| !ModContent.GetInstance<GameplayConfig>().NearbyAuras)
			{
				return true;
			}

			Vector2 pos = proj.Center - Main.screenPosition;
			Main.spriteBatch.Draw(Aura.Value, pos, null, Color.White * proj.Opacity * 0.05f, Main.GameUpdateCount * 0.03f, Aura.Size() / 2f, 1f, SpriteEffects.None, 0);
			return true;
		}
	}
}