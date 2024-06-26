using PathOfTerraria.Core;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Sword;

internal class SteelBroadsword : Sword
{
	public override string Texture => $"{PathOfTerraria.ModName}/Assets/Items/Gear/Weapons/Sword/SteelBroadsword";

	public override float DropChance => 1f;
	public override int ItemLevel => 20;

	public override void Defaults()
	{
		base.Defaults();
		Item.damage = 12;
		Item.width = 46;
		Item.height = 46;
		Item.UseSound = SoundID.Item1;
		ItemType = ItemType.Sword;
	}
}