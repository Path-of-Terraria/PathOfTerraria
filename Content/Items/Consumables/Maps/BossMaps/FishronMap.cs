using PathOfTerraria.Common.Subworlds.BossDomains.Hardmode;
using PathOfTerraria.Core.Items;
using SubworldLibrary;
using Terraria.Localization;

namespace PathOfTerraria.Content.Items.Consumables.Maps.BossMaps;

internal class FishronMap : Map
{
	public override int MaxUses => GetBossUseCount();
	public override int WorldTier => WorldLevelBasedOnTier(7) + 1;
	public override bool CanDrop => PoTItemHelper.PickItemLevel() >= WorldLevelBasedOnTier(7) && NPC.downedFishron;

	public override void SetStaticDefaults()
	{
		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.Size = new Vector2(26, 30);
	}

	protected override void OpenMapInternal()
	{
		SubworldSystem.Enter<FishronDomain>();
	}

	public override string GenerateName(string defaultName)
	{
		return Language.GetTextValue($"Mods.{PoTMod.ModName}.Items.{Name}.DisplayName");
	}
}