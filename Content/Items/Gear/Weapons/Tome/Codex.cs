using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Tome;

internal class Codex : Spellbook
{
	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.MinDropItemLevel = 60;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.damage = 50;
	}
}