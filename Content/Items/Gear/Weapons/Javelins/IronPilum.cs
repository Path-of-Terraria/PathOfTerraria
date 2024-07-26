using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Javelins;

internal class IronPilum : Javelin
{
	public override Vector2 ItemSize => new(78);
	public override int DeathDustType => DustID.Iron;
	public override int MinDropItemLevel => 5;

	public override void SetDefaults()
	{
		Item.damage = 8;
	}
}
