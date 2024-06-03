using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Sword;

internal class IronBroadsword : Sword
{
	public override string Texture => $"{PathOfTerraria.ModName}/Assets/Items/Gear/Weapons/Sword/IronBroadsword";

	public override float DropChance => 1f;
	public override int ItemLevel => 11;

	public override void SetDefaults()
	{
		base.SetDefaults();
		Item.damage = 10;
		Item.width = 46;
		Item.height = 46;
		Item.UseSound = SoundID.Item1;
		GearType = GearType.Sword;
	}
}