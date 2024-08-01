using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Core.Events;

namespace PathOfTerraria.Core.Systems.Affixes.ItemTypes;

internal class HealOnKillingBurningEnemiesAffix : ItemAffix
{
	public override void OnLoad()
	{
		PathOfTerrariaNpcEvents.OnHitByProjectileEvent += ApplyLifeStealOnBurningEnemies;
	}

	private void ApplyLifeStealOnBurningEnemies(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
	{
		Player owner = Main.player[projectile.owner];
		float value = owner.GetModPlayer<AffixPlayer>().StrengthOf<HealOnKillingBurningEnemiesAffix>();
		owner.Heal((int)(value * 2));
	}
}
