using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Passives;

internal class HairTriggerMastery : Passive
{
	internal class HairTriggerProjectile : GlobalProjectile
	{
		public override void OnSpawn(Projectile projectile, IEntitySource source)
		{
			if (source is EntitySource_ItemUse_WithAmmo ammo && ammo.Player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<HairTriggerMastery>(out float value)
				&& ammo.AmmoItemIdUsed != AmmoID.None)
			{
				projectile.velocity = projectile.velocity.RotatedByRandom(value / 150f);
			}
		}
	}

	public override void BuffPlayer(Player player)
	{
		player.GetAttackSpeed(DamageClass.Ranged) += Value / 100f;
	}
}