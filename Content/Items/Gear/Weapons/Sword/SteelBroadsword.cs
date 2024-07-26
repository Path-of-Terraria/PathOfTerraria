using PathOfTerraria.Core;
using PathOfTerraria.Core.Items;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Sword;

internal class SteelBroadsword : Sword
{
	public override float DropChance => 1f;
	public override int MinDropItemLevel => 20;

	public override void SetDefaults()
	{
		Item.damage = 12;
		Item.width = 46;
		Item.height = 46;
		Item.UseSound = SoundID.Item1;

		PoTInstanceItemData data = this.GetInstanceData();
		data.ItemType = ItemType.Sword;
	}
}