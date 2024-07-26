using PathOfTerraria.Core.Items.Hooks;
using PathOfTerraria.Core.Systems.Affixes;
using PathOfTerraria.Data.Models;
using PathOfTerraria.Data;
using System.Collections.Generic;

namespace PathOfTerraria.Core.Items;

internal sealed partial class PoTGlobalItem : GlobalItem
{
	// IMPORTANT: Called *after* ModItem::SetDefaults.
	// https://github.com/tModLoader/tModLoader/blob/1.4.4/patches/tModLoader/Terraria/ModLoader/Core/GlobalLoaderUtils.cs#L20
	public override void SetDefaults(Item entity)
	{
		base.SetDefaults(entity);

		Roll(entity, PickItemLevel());
	}

	public void Roll(Item item, int itemLevel)
	{
		PoTInstanceItemData data = item.GetInstanceData();
		PoTStaticItemData staticData = item.GetStaticData();

		data.ItemLevel = itemLevel;

		// Only level 50+ gear can get influence.
		if (data.ItemLevel > 50 && !staticData.IsUnique && (data.ItemType & ItemType.AllGear) == ItemType.AllGear)
		{
			// Quality does not affect influence right now.
			// Might not need to, seems to generaet plenty often late game.
			int inf = Main.rand.Next(400) - data.ItemLevel;

			if (inf < 30)
			{
				data.Influence = Main.rand.NextBool() ? Influence.Solar : Influence.Lunar;
			}
		}

		RollAffixes(item);
		IPostRollItem.Invoke(item);
		data.SpecialName = IGenerateNameItem.Invoke(item);
	}

	public void Reroll(Item item)
	{
		// TODO: Don't call ANY variant of SetDefaults here... please?
		item.GetInstanceData().Affixes.Clear();
		item.ModItem?.SetDefaults();
		Roll(item, PickItemLevel());
	}

	private static void RollAffixes(Item item)
	{
		PoTInstanceItemData data = item.GetInstanceData();
		PoTStaticItemData staticData = item.GetStaticData();

		data.Affixes.Clear();
		data.Affixes.AddRange(IGenerateAffixesItem.Invoke(item));

		data.ImplicitCount = data.Affixes.Count;
		for (int i = 0; i < GetAffixCount(item); i++)
		{
			ItemAffixData chosenAffix = AffixRegistry.GetRandomAffixDataByItemType(data.ItemType);
			if (chosenAffix is null)
			{
				continue;
			}

			ItemAffix affix = AffixRegistry.ConvertToItemAffix(chosenAffix);
			if (affix is null)
			{
				continue;
			}

			affix.Value = AffixRegistry.GetRandomAffixValue(affix, data.ItemLevel);
			data.Affixes.Add(affix);
		}

		if (staticData.IsUnique)
		{
			List<ItemAffix> uniqueItemAffixes = IGenerateAffixesItem.Invoke(item);

			foreach (ItemAffix affix in uniqueItemAffixes)
			{
				affix.Roll();
			}

			data.Affixes.AddRange(uniqueItemAffixes);
		}
	}

	// TODO: Un-hardcode? Or at least move it elsewhere!
	internal static int PickItemLevel()
	{
		if (NPC.downedMoonlord)
		{
			return Main.rand.Next(150, 201);
		}

		if (NPC.downedAncientCultist)
		{
			return Main.rand.Next(110, 151);
		}

		if (NPC.downedGolemBoss)
		{
			return Main.rand.Next(95, 131);
		}

		if (NPC.downedPlantBoss)
		{
			return Main.rand.Next(80, 121);
		}

		if (NPC.downedMechBossAny)
		{
			return Main.rand.Next(75, 111);
		}

		if (Main.hardMode)
		{
			return Main.rand.Next(50, 91);
		}

		if (NPC.downedBoss3)
		{
			return Main.rand.Next(30, 50);
		}

		if (NPC.downedBoss2)
		{
			return Main.rand.Next(20, 41);
		}

		if (NPC.downedBoss1)
		{
			return Main.rand.Next(10, 26);
		}

		return Main.rand.Next(5, 21);
	}
}
