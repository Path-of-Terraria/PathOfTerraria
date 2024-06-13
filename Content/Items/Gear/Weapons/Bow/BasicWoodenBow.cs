namespace PathOfTerraria.Content.Items.Gear.Weapons.Bow;

internal class BasicWoodenBow : Bow
{
	public override string Texture => $"{PathOfTerraria.ModName}/Assets/Items/Gear/Weapons/Bow/BasicWoodenBow";
	public override float DropChance => 1f;

	public override void Defaults()
	{
		base.Defaults();
		Item.damage = 8;
	}
}