using PathOfTerraria.Common.Systems.ElementalDamage;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Buffs.ElementalBuffs;
using PathOfTerraria.Content.Projectiles.PassiveProjectiles;

namespace PathOfTerraria.Content.Passives.Magic.Masteries;

internal class BludgeoningBlowMastery : Passive
{
	public class BludgeoningBlowPlayer : ModPlayer, ElementalPlayerHooks.IPostElementHitPlayer
	{
		public void PostElementalHit(NPC target, ElementalContainer container, ElementalContainer other, int finalDamage, NPC.HitInfo info, Item item = null)
		{
			if (target.life < 0 && target.HasBuff<FreezeDebuff>() && Player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<BludgeoningBlowMastery>(out float value))
			{
				for (int i = 0; i < 4; ++i)
				{
					Vector2 velocity = new Vector2(0, Main.rand.NextFloat(5, 8)).RotatedByRandom(MathHelper.TwoPi) + target.velocity * 0.25f;
					int type = ModContent.ProjectileType<BlastIcicle>();
					int proj = Projectile.NewProjectile(Player.GetSource_OnHit(target), target.Center, velocity, type, finalDamage / 4, 4, Player.whoAmI);
					ref ElementalDamage mod = ref Main.projectile[proj].GetGlobalProjectile<ElementalProjectile>().Container[ElementType.Cold].DamageModifier;
					mod = mod.ApplyOverride(0, 1);
				}
			}
		}
	}
}
