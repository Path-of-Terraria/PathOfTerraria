using PathOfTerraria.Core.Items;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones.Miscellaneous;

internal class Gatligator : VanillaClone
{
	protected override short VanillaItemId => ItemID.Gatligator;

	public override void SetDefaults()
	{
		PoTInstanceItemData data = this.GetInstanceData();
		data.ItemType = Core.ItemType.Melee;
	}

	public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		float speed = velocity.Length();

		if (float.IsNaN(velocity.X) && float.IsNaN(velocity.Y) || velocity.X == 0f && velocity.Y == 0f)
		{
			velocity.X = player.direction;
			velocity.Y = 0f;
			speed = Item.shootSpeed;
		}
		else
		{
			speed = Item.shootSpeed / speed;
		}

		velocity.X += Main.rand.Next(-50, 51) * 0.03f / speed;
		velocity.Y += Main.rand.Next(-50, 51) * 0.03f / speed;

		Projectile.NewProjectile(source, position.X, position.Y, velocity.X, velocity.Y, type, damage, knockback, player.whoAmI);

		return false;
	}
}
