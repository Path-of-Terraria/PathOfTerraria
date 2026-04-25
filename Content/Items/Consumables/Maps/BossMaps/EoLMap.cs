using PathOfTerraria.Common.Subworlds.BossDomains.Hardmode;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using PathOfTerraria.Content.Items.Currency;
using PathOfTerraria.Core.Items;
using SubworldLibrary;
using Terraria.Localization;
using Terraria.Utilities;

namespace PathOfTerraria.Content.Items.Consumables.Maps.BossMaps;

internal class EoLMap() : HardmodeBossMap(8, () => NPC.downedEmpressOfLight)
{
	public override void SetStaticDefaults()
	{
		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.Size = new Vector2(34, 38);
	}

	internal override Subworld GetDestination()
	{
		return ModContent.GetInstance<EmpressDomain>();
	}

	public override void ModifyCorruptionAffixes(WeightedRandom<ItemAffix> affixes)
	{
		affixes.Add(CorruptShard.GenerateMapAffix<EoLDaylightAffix>(1, 1, 30, 30));
	}

	public override string GenerateName(string defaultName)
	{
		return Language.GetTextValue($"Mods.{PoTMod.ModName}.Items.{Name}.DisplayName");
	}
}