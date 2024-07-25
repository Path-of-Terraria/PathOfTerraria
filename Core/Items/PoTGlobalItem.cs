using PathOfTerraria.Core.Items.Hooks;
using PathOfTerraria.Core.Systems.VanillaInterfaceSystem;

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

		RollAffixes();

		if (item.TryGetInterface(out IPostRollItem postRollItem))
		{
			postRollItem.PostRoll();
		}

		data.SpecialName = GenerateName();
	}






}
