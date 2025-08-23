using PathOfTerraria.Common.Subworlds.BossDomains.Prehardmode;
using PathOfTerraria.Core.Items;
using SubworldLibrary;
using Terraria.Localization;

namespace PathOfTerraria.Content.Items.Consumables.Maps.BossMaps;

internal class BeeMap() : BossMap(1, 30, () => NPC.downedQueenBee, false)
{
	public override void SetStaticDefaults()
	{
		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.Size = new Vector2(36, 36);
	}

	protected override void OpenMapInternal()
	{
		SubworldSystem.Enter<QueenBeeDomain>();
	}

	public override string GenerateName(string defaultName)
	{
		return Language.GetTextValue($"Mods.{PoTMod.ModName}.Items.{Name}.DisplayName");
	}
}