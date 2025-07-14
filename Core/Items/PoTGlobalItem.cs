using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Systems.ModPlayers;
using Terraria.ID;

namespace PathOfTerraria.Core.Items;

public sealed partial class PoTGlobalItem : GlobalItem
{
	public override void Load()
	{
		LoadBackImages();
	}

	// IMPORTANT: Called *after* ModItem::SetDefaults.
	// https://github.com/tModLoader/tModLoader/blob/1.4.4/patches/tModLoader/Terraria/ModLoader/Core/GlobalLoaderUtils.cs#L20
	public override void SetDefaults(Item item)
	{
		base.SetDefaults(item);

		PoTInstanceItemData data = item.GetInstanceData();
		PoTStaticItemData staticData = item.GetStaticData();

		// Makes Affixes use a new reference so that rerolling or updating Affixes in another instance doesn't share the reference
		data.Affixes = [];

		if (staticData.IsUnique)
		{
			data.Rarity = ItemRarity.Unique;
		}

		PoTItemHelper.Roll(item, PoTItemHelper.PickItemLevel());
	}

	public override void UpdateEquip(Item item, Player player)
	{
		base.UpdateEquip(item, player);

		PoTItemHelper.ApplyAffixes(item, player.GetModPlayer<UniversalBuffingPlayer>().UniversalModifier, player);
	}

	public override bool AppliesToEntity(Item entity, bool lateInstantiation)
	{
		bool anyValidTrait = entity.damage > 0 || entity.defense > 0 || entity.accessory || entity.headSlot > 0 || entity.bodySlot > 0 || 
			entity.legSlot > 0 || entity.ModItem is IPoTGlobalItem;

		return anyValidTrait && !entity.vanity;
	}
}
