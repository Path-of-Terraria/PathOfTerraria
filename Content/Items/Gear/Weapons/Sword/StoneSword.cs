using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Sword;

internal class StoneSword : Sword
{
	public override string Texture => $"{PathOfTerraria.ModName}/Assets/Items/Gear/Weapons/Sword/StoneSword";

	public override float DropChance => 1f;
	public override int ItemLevel => 1;

	public override void Defaults()
	{
		base.Defaults();
		Item.damage = 6;
		Item.UseSound = SoundID.Item1;
		GearType = GearType.Sword;
	}
}