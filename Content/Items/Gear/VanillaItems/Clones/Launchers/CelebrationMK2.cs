using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones.Miscellaneous;

internal class CelebrationMK2 : VanillaClone
{
	protected override short VanillaItemId => ItemID.Celeb2;

	public override void Load()
	{
		On_Projectile.AI += HijackTypeDuringAI;
	}

	private void HijackTypeDuringAI(On_Projectile.orig_AI orig, Projectile self)
	{
		// The vanilla projectile chooses ammo based on the held weapon.
		// This is an issue for the clone, which is not the normal Celeb2 weapon.
		// This makes it Celeb2 during AI, so it fires properly, then unsets it.

		bool isHoldingClone = self.type == ProjectileID.Celeb2Weapon && Main.player[self.owner].HeldItem.type == ModContent.ItemType<CelebrationMK2>();

		if (isHoldingClone)
		{
			Main.player[self.owner].HeldItem.type = ItemID.Celeb2;
		}

		orig(self);

		if (isHoldingClone)
		{
			Main.player[self.owner].HeldItem.type = ModContent.ItemType<CelebrationMK2>();
		}
	}

	public override void SetDefaults()
	{
		ItemType = Core.ItemType.Melee;
	}

	public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		int subId = 5 * Main.rand.Next(0, 20);
		Projectile.NewProjectile(source, position.X, position.Y, velocity.X, velocity.Y, ProjectileID.Celeb2Weapon, damage, knockback, player.whoAmI, subId);
		return false;
	}

	public override bool CanConsumeAmmo(Item ammo, Player player)
	{
		return Main.rand.NextBool();
	}
}
