using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones.Swords;

internal class LightsBane : VanillaClone
{
	protected override short VanillaItemId => ItemID.LightsBane;

	public override void Defaults()
	{
		ItemType = Core.ItemType.Melee;
		base.Defaults();
	}

	public override void MeleeEffects(Player player, Rectangle hitbox)
	{
		Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustID.Demonite, player.direction * 2, 0f, 150, default, 1.3f);
	}

	public override bool Shoot(Player plr, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		Vector2 vector48 = new Vector2(plr.direction, plr.gravDir * 4f).SafeNormalize(Vector2.UnitY).RotatedBy((float)Math.PI * 2f * Main.rand.NextFloatDirection() * 0.05f);
		Vector2 searchCenter = plr.MountedCenter + new Vector2(70f, -40f) * plr.Directions + vector48 * -10f;

		if (SwordCommon.GetZenithTarget(plr, searchCenter, 50f, out int npcTargetIndex2))
		{
			NPC nPC3 = Main.npc[npcTargetIndex2];
			searchCenter = nPC3.Center + Main.rand.NextVector2Circular(nPC3.width / 2, nPC3.height / 2);
		}
		else
		{
			searchCenter += Main.rand.NextVector2Circular(20f, 20f);
		}

		float damageMultiplier = 1f;
		if (Main.rand.Next(100) < plr.GetCritChance(DamageClass.Melee))
		{
			damageMultiplier = 2f;
			damage *= 2;
		}

		Projectile.NewProjectile(source, searchCenter, vector48 * 0.001f, type, (int)(damage * 0.5), knockback, plr.whoAmI, damageMultiplier);
		NetMessage.SendData(MessageID.PlayerControls, -1, -1, null, plr.whoAmI);

		return true;
	}
}
