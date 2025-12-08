using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Content.Passives;

internal class ReturningProjectilesMastery : Passive
{
	internal class ReturningProjectile : GlobalProjectile
	{
		public override bool InstancePerEntity => true;

		private int _timer = 0;
		private Vector2 _spawnPosition = Vector2.Zero;
		private float _spawnMagnitude = 0f;
		private byte _timeToReturn = 0;

		public override void OnSpawn(Projectile projectile, IEntitySource source)
		{
			if (source is EntitySource_ItemUse_WithAmmo ammo && ammo.Player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<ReturningProjectilesMastery>(out float value) 
				&& ammo.Item.CountsAsClass(DamageClass.Ranged) && ammo.Item.useAmmo == AmmoID.Bullet)
			{
				_spawnPosition = projectile.Center;
				_spawnMagnitude = projectile.velocity.Length();
				_timeToReturn = (byte)value;
			}
		}

		public override bool PreAI(Projectile projectile)
		{
			_timer++;

			if (_timer > _timeToReturn && _timeToReturn != 0)
			{
				projectile.velocity = Vector2.SmoothStep(projectile.velocity, projectile.DirectionTo(_spawnPosition) * _spawnMagnitude, 0.08f);

				if (projectile.DistanceSQ(_spawnPosition) <= _spawnMagnitude * _spawnMagnitude)
				{
					projectile.Kill();
				}
			}

			return true;
		}
	}

	public override string DisplayTooltip => Language.GetText($"Mods.PathOfTerraria.Passives.{Name}.Tooltip").Format((Value / 60f).ToString("#.0#"));
}