using PathOfTerraria.Common.Systems;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Common.NPCs;

public class BossDownedCondition(BossDownedCondition.Bosses boss) : IItemDropRuleCondition, IProvideItemConditionDescription
{
	public enum Bosses : int
	{
		/// <summary>
		/// Applies to either Brain of Cthulhu or Eater of Worlds being downed.
		/// </summary>
		EvilBoss,

		BrainOfCthulhu,
		EaterofWorlds,
		Any_Mech,
		KingSlime,
		Skeletron,
		Deerclops,
		WallOfFlesh,
		SingularCutoff, //Keep this after all non-"the" bosses (i.e. after Skeletron but before THE Eye of Cthulhu)
		EyeOfCthulhu,
		QueenBee,
	}

	readonly Bosses boss = boss;

	public bool CanDrop(DropAttemptInfo info)
	{
		if (!info.IsInSimulation)
		{
			return boss switch
			{
				Bosses.KingSlime => NPC.downedSlimeKing,
				Bosses.EyeOfCthulhu => NPC.downedBoss1,
				Bosses.EvilBoss => NPC.downedBoss2,
				Bosses.Skeletron => NPC.downedBoss3,
				Bosses.QueenBee => NPC.downedQueenBee,
				Bosses.Any_Mech => NPC.downedMechBossAny,
				Bosses.EaterofWorlds => BossTracker.DownedEaterOfWorlds,
				Bosses.BrainOfCthulhu => BossTracker.DownedBrainOfCthulhu,
				Bosses.Deerclops => NPC.downedDeerclops,
				Bosses.WallOfFlesh => Main.hardMode,
				_ => false,
			};
		}

		return false;
	}

	public bool CanShowItemDropInUI()
	{
		return true;
	}

	public string GetConditionDescription()
	{
		LocalizedText defText = Language.GetText("Mods.PathOfTerraria.Condition.BossDowned.Drop");

		int npcId = boss switch
		{
			Bosses.KingSlime => NPCID.KingSlime,
			Bosses.Skeletron => NPCID.Skeleton,
			Bosses.QueenBee => NPCID.QueenBee,
			Bosses.EvilBoss => WorldGen.crimson ? NPCID.BrainofCthulhu : NPCID.EaterofWorldsHead,
			Bosses.EyeOfCthulhu => NPCID.EyeofCthulhu,
			Bosses.EaterofWorlds => NPCID.EaterofWorldsHead,
			Bosses.BrainOfCthulhu => NPCID.BrainofCthulhu,
			Bosses.Deerclops => NPCID.Deerclops,
			Bosses.WallOfFlesh => NPCID.WallofFlesh,
			_ => NPCID.BlueSlime
		};

		string npcName;

		if (npcId == NPCID.BlueSlime)
		{
			if (boss == Bosses.Any_Mech)
			{
				npcName = Language.GetTextValue("Mods.PathOfTerraria.Conditions.BossDowned.AnyMech");
			}
			else
			{
				npcName = "Unknown";
			}
		}
		else
		{
			npcName = Lang.GetNPCNameValue(npcId);
		}

		string plural = (int)boss < (int)Bosses.SingularCutoff ? "" : Language.GetTextValue("Mods.PathOfTerraria.Condition.BossDowned.The");
		defText = defText.WithFormatArgs(plural + npcName);

		return defText.Value;
	}
}
