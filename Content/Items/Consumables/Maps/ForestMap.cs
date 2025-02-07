﻿using PathOfTerraria.Common.Subworlds.MappingAreas;
using PathOfTerraria.Core.Items;
using SubworldLibrary;
using Terraria.Localization;

namespace PathOfTerraria.Content.Items.Consumables.Maps;

internal class ForestMap : Map
{
	public override int MaxUses => GetBossUseCount();

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;
	}

	protected override void OpenMapInternal()
	{
		SubworldSystem.Enter<ForestArea>();
	}

	public override string GenerateName(string defaultName)
	{
		return Language.GetTextValue($"Mods.{PoTMod.ModName}.Items.{Name}.DisplayName");
	}
}