using PathOfTerraria.Common.Enums;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones.Swords;

internal class Starfury : VanillaClone
{
	protected override short VanillaItemId => ItemID.Starfury;

	public override void Defaults()
	{
		ItemType = ItemType.Melee;
		base.Defaults();
	}

	public override void MeleeEffects(Player player, Rectangle box)
	{
		if (Main.rand.NextBool(5))
		{
			Dust.NewDust(new Vector2(box.X, box.Y), box.Width, box.Height, DustID.Enchanted_Pink, 0f, 0f, 150, default, 1.2f);
		}

		if (Main.rand.NextBool(10))
		{
			Gore.NewGore(player.GetSource_ItemUse(Item), new Vector2(box.X, box.Y), default, Main.rand.Next(16, 18));
		}
	}

	public override bool Shoot(Player plr, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		Vector2 targetY = Main.MouseWorld;
		Vector2 direction = (position - Main.MouseWorld).SafeNormalize(new Vector2(0f, -1f));

		while (targetY.Y > position.Y && WorldGen.SolidTile(targetY.ToTileCoordinates()))
		{
			targetY += direction * 16f;
		}

		Projectile.NewProjectile(source, position, velocity, type, damage, knockback, Main.myPlayer, 0f, targetY.Y);

		return false;
	}
}
