using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Content.Items.Gear.Armor.Helmet;

[AutoloadEquip(EquipType.Face)]
internal class CrystalVisors : Gear
{
	public class VisorPlayer : ModPlayer
	{
		public bool Active = false;

		public override void ResetEffects()
		{
			Active = false;
		}

		public override void RefreshInfoAccessoriesFromTeamPlayers(Player otherPlayer)
		{
			if (otherPlayer.GetModPlayer<VisorPlayer>().Active)
			{
				Player.GetModPlayer<VisorPlayer>().Active = true;
			}
		}
	}

	protected override string GearLocalizationCategory => "Visors";

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;
		staticData.IsUnique = true;
		staticData.DropChance = null;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		PoTInstanceItemData data = this.GetInstanceData();
		data.ItemType = ItemType.Accessories;

		Item.defense = 2;
		Item.accessory = true;
	}

	public override void UpdateAccessory(Player player, bool hideVisual)
	{
		player.GetModPlayer<VisorPlayer>().Active = true;
	}
}