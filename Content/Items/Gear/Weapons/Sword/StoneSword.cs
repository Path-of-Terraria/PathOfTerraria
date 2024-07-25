using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Sword;

internal class StoneSword : Sword
{
	public override float DropChance => 1f;
	public override int ItemLevel => 1;

	public override void Defaults()
	{
		base.Defaults();
		Item.damage = 6;
		Item.UseSound = SoundID.Item1;
		ItemType = ItemType.Sword;
	}
}