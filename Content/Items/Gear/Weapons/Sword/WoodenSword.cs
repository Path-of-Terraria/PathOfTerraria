using PathOfTerraria.Core;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Sword;

internal class WoodenSword : Sword
{
	public override string Texture => $"{PathOfTerraria.ModName}/Assets/Items/Gear/Weapons/Sword/WoodenSword";

	public override float DropChance => 1f;
	public override int ItemLevel => 1;

	public override void Defaults()
	{
		base.Defaults();
		Item.height = 52;
		Item.damage = 4;
		Item.UseSound = SoundID.Item1;
		ItemType = ItemType.Sword;
	}
}