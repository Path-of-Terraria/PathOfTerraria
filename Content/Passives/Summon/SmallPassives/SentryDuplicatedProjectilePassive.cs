using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Passives;

internal class SentryDuplicatedProjectilePassive : Passive
{
	public sealed class SentryDuplicatedProjectileGlobal : GlobalProjectile
	{
		public override void OnSpawn(Projectile projectile, IEntitySource source)
		{
			if (source is not EntitySource_Parent and not EntitySource_ItemUse and not EntitySource_ItemUse_WithAmmo) { return; }
			
			if (!ProjectileID.Sets.SentryShot[projectile.type] || projectile.owner < 0 || projectile.owner >= Main.maxPlayers) { return; }

			Player player = Main.player[projectile.owner];
			float passiveValues = player.GetModPlayer<PassiveTreePlayer>().GetCumulativeValue<SentryDuplicatedProjectilePassive>();
			if (passiveValues <= 0) { return; }

			float chance = passiveValues / 100.0f;
			if (Main.rand.NextFloat() >= chance) { return; }
			
			EntitySource_Misc duplicateSource = new("SentryDuplication");

			float angleOffset = Main.rand.NextFloat(-0.1f, 0.1f);
			Vector2 duplicateVelocity = projectile.velocity.RotatedBy(angleOffset);
			
			Projectile.NewProjectile(duplicateSource, projectile.position, duplicateVelocity, projectile.type, projectile.damage, projectile.knockBack, projectile.owner);
		}
	}
}
