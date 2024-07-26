using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones.Swords;

internal class IceBlade : VanillaClone
{
	protected override short VanillaItemId => ItemID.IceBlade;

	public override void SetDefaults()
	{
		ItemType = Core.ItemType.Melee;
		
		Item.shootsEveryUse = true;
	}

	public override void MeleeEffects(Player player, Rectangle hitbox)
	{
		Rectangle rect = SwordCommon.GetItemRectangle(player, Item);
		int dust = Dust.NewDust(new Vector2(rect.X, rect.Y), rect.Width, rect.Height, DustID.IceRod, player.velocity.X * 0.2f + player.direction * 3, 
			player.velocity.Y * 0.2f, 90, default, 1.5f);
		Main.dust[dust].noGravity = true;
		Main.dust[dust].velocity *= 0.2f;
	}
}