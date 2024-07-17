
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Javelins;

internal class SharpenedStick : Javelin
{
	public override Vector2 ItemSize => new(54);
	public override int DeathDustType => DustID.WoodFurniture;

	public override void Defaults()
	{
		base.Defaults();

		Item.damage = 5;
	}
}
