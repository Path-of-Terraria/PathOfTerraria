﻿using PathOfTerraria.Common.Subworlds.BossDomains;
using PathOfTerraria.Core.Items;
using SubworldLibrary;
using Terraria.Localization;

namespace PathOfTerraria.Content.Items.Consumables.Maps.BossMaps;

internal class SkeletronMap : Map
{
	public override int MaxUses => GetBossUseCount();

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.Size = new Vector2(36, 28);
	}

	public override void OpenMap()
	{
		SubworldSystem.Enter<SkeletronDomain>();
	}

	public override string GenerateName(string defaultName)
	{
		return Language.GetTextValue($"Mods.{PoTMod.ModName}.Items.{Name}.DisplayName");
	}
}