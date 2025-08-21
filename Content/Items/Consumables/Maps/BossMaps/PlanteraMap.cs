using PathOfTerraria.Common.Subworlds.BossDomains.Hardmode;
using PathOfTerraria.Core.Items;
using SubworldLibrary;
using Terraria.Localization;

namespace PathOfTerraria.Content.Items.Consumables.Maps.BossMaps;

internal class PlanteraMap : Map
{
	public override int MaxUses => GetBossUseCount();
	public override int WorldTier => WorldLevelBasedOnTier(5) + 1;
	public override bool CanDrop => PoTItemHelper.PickItemLevel() >= WorldLevelBasedOnTier(5) && NPC.downedPlantBoss;

	public override void SetStaticDefaults()
	{
		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.Size = new Vector2(38, 28);
	}

	protected override void OpenMapInternal()
	{
		SubworldSystem.Enter<PlanteraDomain>();
	}

	public override string GenerateName(string defaultName)
	{
		return Language.GetTextValue($"Mods.{PoTMod.ModName}.Items.{Name}.DisplayName");
	}
}