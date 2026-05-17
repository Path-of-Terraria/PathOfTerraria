using NPCUtils;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.Subworlds.RavencrestContent;
using PathOfTerraria.Common.Systems.Questing;
using PathOfTerraria.Common.Systems.Questing.Quests.MainPath;
using SubworldLibrary;
using Terraria.GameContent.Bestiary;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.Town;

public sealed class TownScoutNPC : ModNPC
{
	private const string SurveyorQuestStep = "KillSurveyor";

	private bool HasPlayerBeenNear
	{
		get => NPC.ai[0] == 1;
		set => NPC.ai[0] = value ? 1 : 0;
	}

	public override void SetStaticDefaults()
	{
		Main.npcFrameCount[Type] = 16;
	}

	public override void SetDefaults()
	{
		NPC.CloneDefaults(NPCID.GoblinScout);
		NPC.rarity = 1;

		AnimationType = NPCID.GoblinScout;
		AIType = NPCID.GoblinScout;

		NPC.TryEnableComponent<NPCHitEffects>(c =>
		{
			c.AddDust(new(DustID.Blood, 4));
			c.AddDust(new(DustID.Blood, 15, NPCHitEffects.OnDeath));
		});
	}

	public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
	{
		bestiaryEntry.AddInfo(this, "Surface");
	}

	public override bool PreAI()
	{
		if (Main.netMode != NetmodeID.MultiplayerClient && !AnyPlayerCanEncounterSurveyor())
		{
			NPC.active = false;

			if (Main.netMode == NetmodeID.Server)
			{
				NetMessage.SendData(MessageID.SyncNPC, number: NPC.whoAmI);
			}

			return false;
		}

		if (!HasPlayerBeenNear)
		{
			foreach (Player plr in Main.ActivePlayers)
			{
				if (plr.DistanceSQ(NPC.Center) < 400 * 400)
				{
					HasPlayerBeenNear = true;
					break;
				}
			}

			if (NPC.life < NPC.lifeMax)
			{
				HasPlayerBeenNear = true;
			}

			if (Math.Abs(NPC.Center.X / 16 - Main.maxTilesX / 2) > 220)
			{
				NPC.velocity.X = NPC.Center.X / 16 >= Main.maxTilesX / 2 ? -2 : 2;
			}
			else
			{
				NPC.velocity.X = 0;
			}
		}
		else
		{
			float target = NPC.Center.X < Main.maxTilesX * 8 ? -4 : 4;
			NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, target, 0.03f);
		}

		NPC.direction = NPC.spriteDirection = Math.Sign(NPC.velocity.X);
		ModContent.GetInstance<RavencrestSystem>().SpawnedScout = true;

		Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);

		if (NPC.velocity.Y != 0)
		{
			return false;
		}

		if (NPC.velocity.X < 0)
		{
			if (Collision.SolidCollision(NPC.position - new Vector2(6, 0), 6, 16))
			{
				NPC.velocity.Y = -6;
			}
		}
		else
		{
			if (Collision.SolidCollision(NPC.TopRight, 6, 16))
			{
				NPC.velocity.Y = -6;
			}
		}

		return false;
	}

	public override void OnKill()
	{
		Main.invasionSize = 20;
		Main.StartInvasion(InvasionID.GoblinArmy);
	}

	public override float SpawnChance(NPCSpawnInfo spawnInfo)
	{
		float questChance = GetQuestSpawnChance();

		if (questChance <= 0)
		{
			return 0;
		}
		
		bool spawnedScout = ModContent.GetInstance<RavencrestSystem>().SpawnedScout;

		return SubworldSystem.Current is RavencrestSubworld && spawnInfo.SpawnTileX < 180 && !spawnedScout ? questChance : 0;
	}

	public override bool CheckActive()
	{
		return false;
	}

	private static bool AnyPlayerCanEncounterSurveyor()
	{
		return GetQuestSpawnChance() > 0;
	}

	private static float GetQuestSpawnChance()
	{
		string questName = ModContent.GetInstance<WizardStartQuest>().FullName;
		bool questCompleted = false;

		foreach (Player plr in Main.ActivePlayers)
		{
			QuestModPlayer questPlayer = plr.GetModPlayer<QuestModPlayer>();

			if (!questPlayer.QuestsByName.TryGetValue(questName, out Quest quest))
			{
				continue;
			}

			if (quest.Active && quest.ActiveStep.Id == SurveyorQuestStep)
			{
				return 100f;
			}

			questCompleted |= quest.Completed;
		}

		return questCompleted ? (NPC.downedGoblins ? 0.1f : 5) : 0;
	}
}
