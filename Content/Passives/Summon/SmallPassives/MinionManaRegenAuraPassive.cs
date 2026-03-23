using PathOfTerraria.Common.Config;
using PathOfTerraria.Common.Projectiles;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Buffs;
using ReLogic.Content;

namespace PathOfTerraria.Content.Passives;

internal class MinionManaRegenAuraPassive : Passive
{
	public class MinionManaRegenAuraProjectile : GlobalProjectile
	{
		private static Asset<Texture2D> Aura = null;

		public override void Load()
		{
			Aura = ModContent.Request<Texture2D>("PathOfTerraria/Assets/Misc/VFX/MinionManaRegenAura");
		}

		public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
		{
			return entity.minion && !CustomProjectileSets.MultisegmentMinionProjectiles[entity.type];
		}

		public override bool PreAI(Projectile proj)
		{
			if (!proj.TryGetOwner(out Player plr) || !plr.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<MinionManaRegenAuraPassive>(out float value) || !AppliesToEntity(proj, true))
			{
				return true;
			}

			foreach (Player player in Main.ActivePlayers)
			{
				if (player.DistanceSQ(proj.Center) < PoTMod.NearbyDistanceSq && !player.HasBuff<MinionManaRegenAuraBuff>())
				{
					player.manaRegen += (int)(value / 100f);
					player.AddBuff(ModContent.BuffType<MinionManaRegenAuraBuff>(), 2);
				}
			}

			return true;
		}

		public override bool PreDraw(Projectile proj, ref Color lightColor)
		{
			if (!proj.TryGetOwner(out Player owner) || !owner.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<MinionManaRegenAuraPassive>(out float value)
				|| !ModContent.GetInstance<GameplayConfig>().NearbyAuras || !AppliesToEntity(proj, true))
			{
				return true;
			}

			Vector2 pos = proj.Center - Main.screenPosition;
			Main.spriteBatch.Draw(Aura.Value, pos, null, Color.White * proj.Opacity * 0.15f, Main.GameUpdateCount * 0.03f, Aura.Size() / 2f, 1f, SpriteEffects.None, 0);
			return true;
		}
	}
}