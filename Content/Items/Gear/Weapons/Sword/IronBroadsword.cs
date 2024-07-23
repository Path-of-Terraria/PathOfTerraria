using PathOfTerraria.Core;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Sword;

internal class IronBroadsword : Sword
{
	public override string Texture => $"{PathOfTerraria.ModName}/Assets/Items/Gear/Weapons/Sword/IronBroadsword";

	public override float DropChance => 1f;
	public override int MinDropItemLevel => 11;

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		GearAlternatives.Register(Type, ItemID.IronBroadsword);
	}

	public override void Defaults()
	{
		base.Defaults();
		Item.damage = 10;
		Item.width = 46;
		Item.height = 46;
		Item.UseSound = SoundID.Item1;
		ItemType = ItemType.Sword;
	}
}