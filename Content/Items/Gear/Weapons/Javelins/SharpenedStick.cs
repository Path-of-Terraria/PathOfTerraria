
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Javelins;

internal class SharpenedStick : Javelin
{
	public override Vector2 ItemSize => new(54);
	public override int DeathDustType => DustID.WoodFurniture;

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.damage = 5;
	}
}
