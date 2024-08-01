using Terraria.ID;

namespace PathOfTerraria.Common.Systems.Affixes.ItemTypes;

internal class HealOnKillingBurningEnemiesAffix : ItemAffix
{
	private sealed class HealOnKillingBurningEnemiesAffixGlobalNpc : GlobalNPC
	{
		public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
		{
			base.OnHitByProjectile(npc, projectile, hit, damageDone);
			
			if (!npc.HasBuff(BuffID.OnFire) && !npc.HasBuff(BuffID.OnFire3) || npc.life > 0)
			{
				return;
			}

			Player owner = Main.player[projectile.owner];
			float value = owner.GetModPlayer<AffixPlayer>().StrengthOf<HealOnKillingBurningEnemiesAffix>();

			if (value != 0)
			{
				owner.Heal((int)(value * 2));
			}
		}
	}
}
