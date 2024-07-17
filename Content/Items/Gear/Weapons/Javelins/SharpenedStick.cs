
namespace PathOfTerraria.Content.Items.Gear.Weapons.Javelins;

internal class SharpenedStick : Javelin
{
	public override Vector2 ItemSize => new(54);

	public override void Defaults()
	{
		base.Defaults();

		Item.damage = 5;
	}
}
