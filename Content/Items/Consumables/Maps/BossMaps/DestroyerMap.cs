using PathOfTerraria.Common.Subworlds.BossDomains.Hardmode;
using PathOfTerraria.Core.Items;
using SubworldLibrary;
using Terraria.Localization;

namespace PathOfTerraria.Content.Items.Consumables.Maps.BossMaps;

internal class DestroyerMap : Map
{
	public override int MaxUses => GetBossUseCount();
	public override int WorldTier => WorldLevelBasedOnTier(3) + 1;
	public override bool CanDrop => PoTItemHelper.PickItemLevel() >= WorldLevelBasedOnTier(3) && NPC.downedMechBoss1;

	public override void SetStaticDefaults()
	{
		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.Size = new Vector2(34, 32);
	}

	protected override void OpenMapInternal()
	{
		SubworldSystem.Enter<DestroyerDomain>();
	}

	public override string GenerateName(string defaultName)
	{
		return Language.GetTextValue($"Mods.{PoTMod.ModName}.Items.{Name}.DisplayName");
	}
}