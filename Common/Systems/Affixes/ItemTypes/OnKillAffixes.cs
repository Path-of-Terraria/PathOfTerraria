using PathOfTerraria.Common.Systems.Affixes;
using Terraria.ID;

namespace PathOfTerraria.Core.Systems.Affixes.ItemTypes;

internal class HealOnKillingBurningEnemiesAffix : ItemAffix
{
	public override void OnLoad()
	{
		PathOfTerrariaNpcEvents.OnHitByProjectileEvent += ApplyLifeStealOnDeadBurningEnemies;
	}

	private void ApplyLifeStealOnDeadBurningEnemies(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
	{
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
