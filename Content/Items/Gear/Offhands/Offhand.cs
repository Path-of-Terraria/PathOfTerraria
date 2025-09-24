using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Content.Items.Gear.Offhands;

public abstract class Offhand : Gear, GenerateNameAffixes.IItem
{
	protected override string GearLocalizationCategory => "Offhand";

	public virtual (sbyte, sbyte) GenerateAffixIds()
	{
		return (-1, -1);
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		PoTInstanceItemData data = this.GetInstanceData();
		data.ItemType = Common.Enums.ItemType.Offhand;
	}
}