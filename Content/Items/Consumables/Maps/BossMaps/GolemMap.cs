using PathOfTerraria.Common.Subworlds.BossDomains.Hardmode;
using PathOfTerraria.Core.Items;
using SubworldLibrary;
using Terraria.Localization;

namespace PathOfTerraria.Content.Items.Consumables.Maps.BossMaps;

internal class GolemMap : Map
{
	public override int MaxUses => GetBossUseCount();
	public override int WorldTier => 63;
	public override bool CanDrop => PoTItemHelper.PickItemLevel() >= 62 && NPC.downedGolemBoss;

	public override void SetStaticDefaults()
	{
		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.Size = new Vector2(40, 26);
	}

	protected override void OpenMapInternal()
	{
		SubworldSystem.Enter<GolemDomain>();
	}

	public override string GenerateName(string defaultName)
	{
		return Language.GetTextValue($"Mods.{PoTMod.ModName}.Items.{Name}.DisplayName");
	}
}