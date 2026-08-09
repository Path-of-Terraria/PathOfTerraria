using PathOfTerraria.Common.Subworlds.MappingAreas;
using PathOfTerraria.Core.Items;
using SubworldLibrary;
using Terraria.Localization;

namespace PathOfTerraria.Content.Items.Consumables.Maps.ExplorableMaps;

internal class ForestMap : ExplorableMap
{
	public override int MaxUses => GetBossUseCount();
	public override bool CanDrop => NPC.downedMoonlord;

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;
	}

	internal override Subworld GetDestination()
	{
		return ModContent.GetInstance<ForestArea>();
	}

	public override string GenerateName(string defaultName)
	{
		return Language.GetTextValue($"Mods.{PoTMod.ModName}.Items.{Name}.DisplayName");
	}
}