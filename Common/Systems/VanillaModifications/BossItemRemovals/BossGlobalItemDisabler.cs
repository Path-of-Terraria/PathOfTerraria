using System.Collections.Generic;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.VanillaModifications.BossItemRemovals;

internal class BossGlobalItemDisabler : GlobalItem
{
	public static readonly HashSet<int> BossSpawners = [ItemID.SlimeCrown,
		ItemID.SuspiciousLookingEye,
		ItemID.WormFood,
		ItemID.BloodySpine,
		ItemID.Abeemination,
		ItemID.ClothierVoodooDoll,
		ItemID.GuideVoodooDoll,
		ItemID.DeerThing];

	public override bool AppliesToEntity(Item entity, bool lateInstantiation)
	{
		return BossSpawners.Contains(entity.type);
	}

	public override bool CanUseItem(Item item, Player player)
	{
		return false;
	}
}
