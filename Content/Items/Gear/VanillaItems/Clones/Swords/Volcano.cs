using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones.Swords;

internal class FieryGreatsword : VanillaClone
{
	protected override short VanillaItemId => ItemID.FieryGreatsword;

	private bool spawnBurstFlag = false;

	public override void Defaults()
	{
		ItemType = Core.ItemType.Melee;
		base.Defaults();
	}

	public override bool? UseItem(Player player)
	{
		spawnBurstFlag = true;
		return true;
	}

	public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
	{
		if (!target.CanBeChasedBy() || !spawnBurstFlag)
		{
			return;
		}

		spawnBurstFlag = false;

		Vector2 center = target.Center;
		Projectile.NewProjectile(player.GetSource_OnHit(target), center.X, center.Y, 0f, -1f * player.gravDir, 978, damageDone, Item.knockBack, player.whoAmI, 0f, 2);
	}

	public override void MeleeEffects(Player player, Rectangle box)
	{
		for (int i = 0; i < 2; i++)
		{
			float scale = player.GetAdjustedItemScale(Item);
			SwordCommon.GetPointOnSwungItemPath(player, 70f, 70f, 0.2f + 0.8f * Main.rand.NextFloat(), scale, out Vector2 location, out Vector2 outwardDirection);
			Vector2 vector = outwardDirection.RotatedBy((float)Math.PI / 2f * player.direction * player.gravDir);
			Dust.NewDustPerfect(location, 6, vector * 4f, 100, default, 2.5f).noGravity = true;
		}
	}
}
