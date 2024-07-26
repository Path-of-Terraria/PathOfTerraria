using PathOfTerraria.Core.Items;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones.Swords;

internal class Bladetongue : VanillaClone
{
	protected override short VanillaItemId => ItemID.Bladetongue;

	public override void SetDefaults()
	{
		PoTInstanceItemData data = this.GetInstanceData();
		data.ItemType = Core.ItemType.Melee;
	}

	public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
	{
		Vector2 vel = Vector2.Normalize(new Vector2(player.direction * 100 + Main.rand.Next(-25, 26), Main.rand.Next(-75, 76))) * Main.rand.Next(30, 41) * 0.1f;
		Rectangle itemRectangle = SwordCommon.GetItemRectangle(player, Item);
		var pos = new Vector2(itemRectangle.X + Main.rand.Next(itemRectangle.Width), itemRectangle.Y + Main.rand.Next(itemRectangle.Height));
		pos = (pos + player.Center * 2f) / 3f;
		Projectile.NewProjectile(player.GetSource_OnHit(target), pos.X, pos.Y, vel.X, vel.Y, 524, (int)(damageDone * 0.7f), Item.knockBack * 0.7f, player.whoAmI);
	}
}