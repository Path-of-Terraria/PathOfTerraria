using PathOfTerraria.Core.Items;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones.Swords;

internal class Meowmere : VanillaClone
{
	protected override short VanillaItemId => ItemID.Meowmere;

	public override void SetDefaults()
	{
		PoTInstanceItemData data = this.GetInstanceData();
		data.ItemType = Core.ItemType.Melee;
	}

	public override void MeleeEffects(Player player, Rectangle hitbox)
	{
		Rectangle rect = SwordCommon.GetItemRectangle(player, Item);
		int dust = Dust.NewDust(rect.TopLeft(), rect.Width, rect.Height, DustID.RainbowTorch, 0f, 0f, 150, Color.Transparent, 0.85f);
		Main.dust[dust].color = Main.hslToRgb(Main.rand.NextFloat(), 1f, 0.5f);
		Main.dust[dust].noGravity = true;
		Main.dust[dust].velocity /= 2f;
	}
}