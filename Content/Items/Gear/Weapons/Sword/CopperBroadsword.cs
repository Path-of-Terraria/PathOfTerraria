using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Sword;

internal class CopperBroadsword : Sword
{
	public override string Texture => $"{PathOfTerraria.ModName}/Assets/Items/Gear/Weapons/Sword/CopperBroadsword";

	public override float DropChance => 1f;
	public override int ItemLevel => 5;

	public override void Defaults()
	{
		base.Defaults();
		Item.damage = 8;
		Item.width = 46;
		Item.height = 46;
		Item.UseSound = SoundID.Item1;
		GearType = GearType.Sword;
	}
}