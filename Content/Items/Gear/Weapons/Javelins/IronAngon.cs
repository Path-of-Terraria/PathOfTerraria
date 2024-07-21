using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Javelins;

internal class IronAngon : Javelin
{
	public override Vector2 ItemSize => new(76);
	public override int DeathDustType => DustID.Iron;
	public override int MinDropItemLevel => 12;

	public override void Defaults()
	{
		base.Defaults();

		Item.damage = 12;
	}
}
