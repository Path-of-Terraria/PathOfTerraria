using PathOfTerraria.Common.Enums;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones.Swords;

internal class Muramasa : VanillaClone
{
	protected override short VanillaItemId => ItemID.Muramasa;

	private bool spawnSlashFlag = false;

	public override void Defaults()
	{
		ItemType = ItemType.Melee;
		base.Defaults();
	}

	public override bool? UseItem(Player player)
	{
		spawnSlashFlag = true;
		return true;
	}

	public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
	{
		if (!target.CanBeChasedBy() || !spawnSlashFlag)
		{
			return;
		}

		spawnSlashFlag = false;

		Rectangle hitbox = target.Hitbox;
		hitbox.Inflate(30, 16);
		hitbox.Y -= 8;

		Vector2 randomPointInBox = Main.rand.NextVector2FromRectangle(hitbox);
		var center = hitbox.Center.ToVector2();
		Vector2 spinPoint = (center - randomPointInBox).SafeNormalize(new Vector2(player.direction, player.gravDir)) * 8f;
		float rotationBase = (Main.rand.Next(2) * 2 - 1) * ((float)Math.PI / 5f + (float)Math.PI * 4f / 5f * Main.rand.NextFloat()) * 0.5f;
		spinPoint = spinPoint.RotatedBy(MathHelper.PiOver4);

		// I don't know what these consts mean exactly but they sure are variables
		const int num9 = 3;
		const int num10 = 10 * num9;
		const int num11 = 5;
		const int num2 = num11 * num9;

		randomPointInBox = center;

		for (int k = 0; k < num2; k++)
		{
			randomPointInBox -= spinPoint;
			spinPoint = spinPoint.RotatedBy((0f - rotationBase) / num10);
		}

		randomPointInBox += target.velocity * num11;
		Projectile.NewProjectile(player.GetSource_OnHit(target), randomPointInBox, spinPoint, ProjectileID.Muramasa, (int)(damageDone * 0.5f), 0f, player.whoAmI, rotationBase);
	}
}
