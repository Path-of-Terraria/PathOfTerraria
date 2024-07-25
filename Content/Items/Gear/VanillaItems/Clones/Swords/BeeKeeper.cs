using PathOfTerraria.Common.Enums;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones.Swords;

internal class BeeKeeper : VanillaClone
{
	protected override short VanillaItemId => ItemID.BeeKeeper;

	public override void Defaults()
	{
		ItemType = ItemType.Melee;
		base.Defaults();
	}

	public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
	{
		if (!target.CanBeChasedBy())
		{
			return;
		}

		int beeCount = Main.rand.Next(1, 4);

		if (player.strongBees && Main.rand.NextBool(3))
		{
			beeCount++;
		}

		Rectangle itemRectangle = SwordCommon.GetItemRectangle(player, Item);

		for (int j = 0; j < beeCount; j++)
		{
			float velX = player.direction * 2 + Main.rand.Next(-35, 36) * 0.02f;
			float velY = Main.rand.Next(-35, 36) * 0.02f;
			velX *= 0.2f;
			velY *= 0.2f;

			int x = itemRectangle.X + itemRectangle.Width / 2;
			int y = itemRectangle.Y + itemRectangle.Height / 2;
			int proj = Projectile.NewProjectile(player.GetSource_OnHit(target), x, y, velX, velY, player.beeType(), player.beeDamage(Item.damage / 3), player.beeKB(0f), player.whoAmI);
			Main.projectile[proj].DamageType = DamageClass.Melee;
		}
	}
}
