using PathOfTerraria.Core.Items;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Common.Systems.VanillaModifications.BossItemRemovals;

internal class BossGlobalItemDisabler : GlobalItem, InsertAdditionalTooltipLines.IGlobal
{
	public static readonly HashSet<int> BossSpawners = [ItemID.SlimeCrown,
		ItemID.SuspiciousLookingEye,
		ItemID.WormFood,
		ItemID.BloodySpine,
		ItemID.Abeemination,
		ItemID.ClothierVoodooDoll,
		ItemID.GuideVoodooDoll,
		ItemID.DeerThing,
		ItemID.MechanicalEye,
		ItemID.MechanicalSkull,
		ItemID.MechanicalWorm,
		ItemID.QueenSlimeCrystal,
		ItemID.CelestialSigil,
		ItemID.MechdusaSummon,
		ItemID.GoblinBattleStandard]; 
	
	// Important to note that the Lihzahrd Power Cell is disabled because NPC.downedPlantBoss isn't kept from the Plantera domain
	// This stops the power cells from working even if the player somehow obtains a power cell in the overworld

	public override bool AppliesToEntity(Item entity, bool lateInstantiation)
	{
		return BossSpawners.Contains(entity.type);
	}

	public override bool CanUseItem(Item item, Player player)
	{
		return false;
	}

	void InsertAdditionalTooltipLines.IGlobal.InsertAdditionalTooltipLines(Item item, List<TooltipLine> tooltips)
	{
		if (AppliesToEntity(item, true))
		{
			tooltips.Add(new TooltipLine(Mod, "Disabled", Language.GetTextValue("Mods.PathOfTerraria.TooltipNotices.Disabled")) { OverrideColor = new Color(255, 160, 160) });
		}
	}
}
