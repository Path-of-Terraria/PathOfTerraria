using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Buffs;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Content.Passives;

internal class TracerShotsMastery : Passive
{
	internal class TracerShotProjectile : GlobalProjectile
	{
		public override bool InstancePerEntity => true;

		private bool _tracer = false;

		public override void OnSpawn(Projectile projectile, IEntitySource source)
		{
			if (source is EntitySource_ItemUse_WithAmmo ammo && ammo.Player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<TracerShotsMastery>(out float value)
				&& ammo.AmmoItemIdUsed != AmmoID.None)
			{
				_tracer = true;
				projectile.velocity *= 1 + (value / 100f);

				if (projectile.velocity.LengthSquared() > 16 * 16)
				{
					projectile.velocity /= 2f;
					projectile.extraUpdates++;
				}
			}
		}

		public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (_tracer && target.TryGetGlobalNPC(out IdentifiedDebuff.IdentifiedDebuffNPC shotNpc))
			{
				target.AddBuff(ModContent.BuffType<IdentifiedDebuff>(), 3 * 60);
			}
		}
	}

	public override string DisplayTooltip => Language.GetText($"Mods.PathOfTerraria.Passives.{Name}.Tooltip").Format(Value, 3);
}