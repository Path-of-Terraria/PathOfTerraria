using PathOfTerraria.Core;
using PathOfTerraria.Core.Items;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Sword;

internal class StoneSword : Sword
{
	public override float DropChance => 1f;
	public override int ItemLevel => 1;

	public override void SetDefaults()
	{
		Item.damage = 6;
		Item.UseSound = SoundID.Item1;

		PoTInstanceItemData data = this.GetInstanceData();
		data.ItemType = ItemType.Sword;
	}
}