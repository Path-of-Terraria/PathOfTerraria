using PathOfTerraria.Common.Subworlds.BossDomains.Hardmode;
using PathOfTerraria.Core.Items;
using SubworldLibrary;
using Terraria.Localization;

namespace PathOfTerraria.Content.Items.Consumables.Maps.BossMaps;

internal class PlanteraMap() : HardmodeBossMap(5, () => NPC.downedPlantBoss)
{
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

	public override float GetDropWeight(NPC npc)
	{
		Player player = GetClosestPlayer(npc);
		return player.ZoneJungle && player.ZoneRockLayerHeight ? 3f : 1f;
	}

	internal override Subworld GetDestination()
	{
		return ModContent.GetInstance<PlanteraDomain>();
	}

	public override string GenerateName(string defaultName)
	{
		return Language.GetTextValue($"Mods.{PoTMod.ModName}.Items.{Name}.DisplayName");
	}
}