using PathOfTerraria.Core.Items;
using Terraria.Localization;

namespace PathOfTerraria.Content.Items.Gear.Offhands.Talismans;

internal abstract class Talisman : Offhand
{
	protected abstract float MinionDamage { get; }
	protected override string GearLocalizationCategory => "Talisman";

	public override void SetStaticDefaults()
	{
		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;
		staticData.MinDropItemLevel = 1;
	}

	public override void SetDefaults()
	{
		Item.accessory = true;

		PoTInstanceItemData data = this.GetInstanceData();
		data.ItemType = Common.Enums.ItemType.Talisman;

		InternalDefaults();
	}

	protected virtual void InternalDefaults()
	{
	}
	
	public override void UpdateAccessory(Player player, bool hideVisual)
	{
		player.GetDamage(DamageClass.Summon) += MinionDamage;
	}

	public override void ModifyTooltips(System.Collections.Generic.List<Terraria.ModLoader.TooltipLine> tooltips)
	{
		base.ModifyTooltips(tooltips);

		if (MinionDamage > 0)
		{
			tooltips.Add(new TooltipLine(Mod, "StatMinionDamage", Language.GetText("Mods.PathOfTerraria.Gear.Talisman.MinionDamage").Format((MinionDamage * 100).ToString("#0.##"))));
		}
	}
}