using PathOfTerraria.Common;
using PathOfTerraria.Common.Encounters;
using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.World.Generation;
using PathOfTerraria.Content.Dusts;
using System.IO;
using Terraria.ID;
using Terraria.ModLoader.Config;

namespace PathOfTerraria.Content.Swamp.NPCs.SwampBoss;

internal partial class Mossmother
{
	public override void AI()
	{
		Timer++;

		if (Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
		{
			NPC.position -= NPC.velocity * 0.55f;
		}

		if (State == BehaviorState.Huddled) // Do nothing until all players are nearby, at which point, start the fight
		{
			if (Timer == 1 && Main.netMode != NetmodeID.MultiplayerClient)
			{
				VisualVariant = Main.rand.Next(3);
			}

			NPC.dontTakeDamage = true;
			bool allNearby = true;

			foreach (Player player in Main.ActivePlayers)
			{
				if (player.DistanceSQ(NPC.Center) > 700 * 700)
				{
					allNearby = false;
					break;
				}
			}

			if (allNearby)
			{
				if (Main.netMode != NetmodeID.MultiplayerClient) // Make sure the last wall node is set so the logic is consistent
				{
					LastWallNodeSelected = Main.rand.Next(SwampArenaGeneration.TraverseWalls.Length);
					NPC.netUpdate = true;
				}

				SetState(BehaviorState.SpawnAnimation);
			}
		}
		else if (State == BehaviorState.SpawnAnimation) // Does nothing but skip directly to the fight, as there is no spawn animation atm
		{
			NPC.GetGlobalNPC<ArenaEnemyNPC>().Arena = true;
			NPC.dontTakeDamage = false;

			SetState(BehaviorState.MoveToWall);
		}
		else if (State == BehaviorState.MoveToWall) 
		{
			// Using a spline curve, traverse the arena's walls to go from a moss patch -> different moss patch, spawning small adds in the way
			// When the boss collides with the adds, the adds explode violently into a triad of projectiles
			// This is synchronized between all living Caracarcosses, so ideally none will move to the same single moss patch
			// It also checks for if the boss should "gas crawl" randomly for variety

			// Wait timer for spawning adds, so the adds appear relatively slowly without the boss moving
			ref float spawnAddsTimer = ref MiscTwo;

			if (spawnAddsTimer > 0)
			{
				spawnAddsTimer--;

				if (Main.netMode != NetmodeID.MultiplayerClient && spawnAddsTimer > 50 && spawnAddsTimer % 30 == 29)
				{
					Vector2 pos;

					do 
					{
						int index = Main.rand.Next(2, SplineCountMax - 4);
						pos = Vector2.Lerp(movementSpline[index], movementSpline[index + 1], Main.rand.NextFloat(0.2f, 0.8f));
					} while (Collision.SolidCollision(pos - new Vector2(50), 100, 100));

					NPC.NewNPC(NPC.GetSource_FromThis(), (int)pos.X, (int)pos.Y, ModContent.NPCType<Mossling>(), NPC.whoAmI + 3, 0, 0, NPC.whoAmI);
				}

				return;
			}

			// Spline index to traverse to
			ref float splineSlot = ref MiscNumber;

			if (splineSlot >= movementSpline.Length && movementSpline.Length > 0)
			{
				SetState(BehaviorState.IdleInWall);
				MiscNumber = movementSpline[^2].AngleTo(movementSpline[^1]);
				TimesWallCrawled++;

				if (!Main.rand.NextBool((int)TimesWallCrawled))
				{
					ReadyForGas = true;
				}

				return;
			}

			Vector2 nextSpline = movementSpline[(int)splineSlot];
			NPC.velocity = Vector2.SmoothStep(NPC.velocity, NPC.SafeDirectionTo(nextSpline) * 18, 0.16f);
			NPC.rotation = Utils.AngleLerp(NPC.rotation, NPC.velocity.ToRotation() - MathHelper.PiOver2, 0.6f);

			if (NPC.DistanceSQ(nextSpline) < 60 * 60)
			{
				splineSlot++;
			}
		}
		else if (State == BehaviorState.IdleInWall) // Coordinate next movement, if going to MoveToWall, or telegraph poison smog w/ dust & do that if every other boss is prepared
		{
			IdleInWall();
		}
		else if (State == BehaviorState.GasCrawl) // Slowly move with the poison screen effect enabled, only safe areas being near the player, otherwise behave like a slow MoveToWall
		{
			if (!GasCrawlBehavior())
			{
				return;
			}
		}
		else if (State == BehaviorState.Desperation) // Sped up MoveToWall but no pauses, poison effect enabled (only above water) until dead, spawn adds in occasionally
		{
			ref float splineSlot = ref MiscNumber;

			Vector2 nextSpline = movementSpline[(int)splineSlot];
			NPC.velocity = Vector2.SmoothStep(NPC.velocity, NPC.SafeDirectionTo(nextSpline) * 22, 0.16f);
			NPC.rotation = Utils.AngleLerp(NPC.rotation, NPC.velocity.ToRotation() - MathHelper.PiOver2, 0.6f);

			if (!Main.rand.NextBool(3))
			{
				SpawnVenomDust();
			}

			if (NPC.DistanceSQ(nextSpline) < 60 * 60)
			{
				splineSlot++;

				if ((int)splineSlot == (int)(movementSpline.Length * 0.75f) && !Collision.SolidCollision(NPC.Center - new Vector2(60), 120, 120))
				{
					EnemySpawning.TrySpawningEnemy(NPC.GetSource_FromAI(), new EnemySpawn()
					{
						Effect = EnemySpawnEffect.Teleport,
						NpcType = new NPCDefinition(ModContent.NPCType<Mudsquit>()),
						SpawnPlacement = null,
						SpawnPosition = NPC.Center,
					}, out _);
				}

				if (splineSlot >= movementSpline.Length)
				{
					SetState(BehaviorState.Desperation);
					MiscNumber = movementSpline[^2].AngleTo(movementSpline[^1]);
					TimesWallCrawled++;

					if (!Main.rand.NextBool((int)TimesWallCrawled))
					{
						ReadyForGas = true;
					}
				}
			}
		}
		else if (State == BehaviorState.PreDesperation) // Slow down, don't take damage, spam dust, until desperation phase starts - allows players to move before the boss starts going
		{
			NPC.dontTakeDamage = true;
			NPC.velocity *= 0.95f;

			for (int i = 0; i < 3; ++i)
			{
				SpawnVenomDust();
			}
			
			if (Timer > 360)
			{
				NPC.dontTakeDamage = false;
				SetState(BehaviorState.Desperation);
			}
		}
	}

	private bool GasCrawlBehavior()
	{
		SpawnVenomDust();

		ref float splineSlot = ref MiscNumber;

		if (splineSlot >= movementSpline.Length)
		{
			NPC.velocity *= 0.90f;

			bool canMoveAlongNow = true;

			foreach (NPC npc in Main.ActiveNPCs)
			{
				if (npc.ModNPC is Mossmother { MiscNumber: < SplineCountMax, State: BehaviorState.GasCrawl })
				{
					canMoveAlongNow = false;
					break;
				}
			}

			if (canMoveAlongNow)
			{
				SetState(BehaviorState.IdleInWall);
				ReadyForGas = false;
				MiscNumber = movementSpline[^2].AngleTo(movementSpline[^1]);
				TimesWallCrawled = 0;
			}

			return false;
		}

		Vector2 nextSpline = movementSpline[(int)splineSlot];
		NPC.velocity = Vector2.SmoothStep(NPC.velocity, NPC.SafeDirectionTo(nextSpline) * 8, 0.16f);
		NPC.rotation = Utils.AngleLerp(NPC.rotation, NPC.velocity.ToRotation() - MathHelper.PiOver2, 0.6f);

		if (NPC.DistanceSQ(nextSpline) < 20 * 20)
		{
			splineSlot++;
		}

		return true;
	}

	private void IdleInWall()
	{
		NPC.velocity *= 0.90f;
		NPC.rotation = Utils.AngleLerp(NPC.rotation, MiscNumber + MathHelper.PiOver2, 0.02f);

		if (ReadyForGas)
		{
			SpawnVenomDust();
		}

		if (Timer == 240)
		{
			if (ReadyForGas)
			{
				bool allReady = true;

				foreach (NPC npc in Main.ActiveNPCs)
				{
					if (npc.ModNPC is Mossmother mother && !mother.ReadyForGas)
					{
						allReady = false;
						break;
					}
				}

				if (!allReady)
				{
					SetState(BehaviorState.MoveToWall);
				}
			}
			else
			{
				SetState(BehaviorState.MoveToWall);
			}
		}
		else if (Timer > 240)
		{
			bool allReady = true;

			foreach (NPC npc in Main.ActiveNPCs)
			{
				if (npc.whoAmI != NPC.whoAmI && npc.ModNPC is Mossmother mo && ((mo.State == BehaviorState.IdleInWall && mo.Timer < 240) && mo.State != BehaviorState.GasCrawl))
				{
					allReady = false;
					break;
				}
			}

			if (allReady)
			{
				PoisonShaderFunctionality.Intensity = 0.02f;
				SetState(BehaviorState.GasCrawl);
			}
		}
	}

	private void SpawnVenomDust()
	{
		Vector2 angle = (Main.rand.NextBool() ? new Vector2(42, 60) : new Vector2(-48, 60)).RotatedBy(NPC.rotation);
		byte opacity = (byte)(byte.MaxValue * PoisonShaderFunctionality.Intensity);
		Vector2 velocity = angle.SafeNormalize(Vector2.Zero).RotatedByRandom(0.4f) * Main.rand.NextFloat(5, 12);
		int type = Main.rand.NextBool(3) ? ModContent.DustType<BrightVenomDust>() : DustID.Venom;
		var dust = Dust.NewDustPerfect(NPC.Center + angle, type, velocity, opacity, default, Main.rand.NextFloat(2, 3));
		dust.noGravity = true;
	}

	/// <summary>
	/// Used to reset states between attacks.
	/// </summary>
	public void SetState(BehaviorState state)
	{
		Timer = 0;
		MiscNumber = 0;
		State = state;
		NPC.netUpdate = true;

		if (NPC.CountNPCS(Type) == 1 && state != BehaviorState.Desperation)
		{
			State = BehaviorState.PreDesperation;
		}

		if (Main.netMode != NetmodeID.MultiplayerClient)
		{
			if (State is BehaviorState.MoveToWall or BehaviorState.PreDesperation or BehaviorState.Desperation)
			{
				BuildSplineForMovement(State != BehaviorState.MoveToWall);
			}

			if (State == BehaviorState.MoveToWall)
			{
				MiscTwo = Main.rand.NextBool(1) ? 130 : 0;
			}
		}
	}

	/// <summary>
	/// Constructs the spline for the boss to traverse during various behavior states.
	/// </summary>
	private void BuildSplineForMovement(bool desperation)
	{
		int endIndex;

		do
		{
			endIndex = Main.rand.Next(SwampArenaGeneration.TraverseWalls.Length);
		} while (endIndex == LastWallNodeSelected || AnyOtherBossHasTargetNode(endIndex) || (desperation && SwampArenaGeneration.TraverseWalls[endIndex].Y <= SwampArea.WaterY));

		Vector2 end = SwampArenaGeneration.TraverseWalls[endIndex].ToWorldCoordinates();
		Vector2 node = Main.rand.Next(SwampArenaGeneration.TraverseNodes).ToWorldCoordinates();
		
		if (desperation)
		{
			node = new Vector2(SwampArea.ArenaMiddleX + Main.rand.Next(-60, 60), Main.rand.Next(SwampArea.WaterY, SwampArea.WaterY + 40)) * 16;
		}

		LastWallNodeSelected = endIndex;

		movementSpline = Spline.InterpolateXY([NPC.Center, node, end], SplineCountMax);
	}

	/// <summary>
	/// Used to stop the mossmothers from having the exact same path and overlapping, causing visual confusion.
	/// </summary>
	/// <param name="endIndex"></param>
	/// <returns></returns>
	private bool AnyOtherBossHasTargetNode(int endIndex)
	{
		for (int i = 0; i < NPC.whoAmI; ++i)
		{
			NPC npc = Main.npc[i];

			if (npc.active && npc.ModNPC is Mossmother { State: BehaviorState.MoveToWall } mother && mother.LastWallNodeSelected == endIndex)
			{
				return true;
			}
		}

		return false;
	}

	public override void SendExtraAI(BinaryWriter writer)
	{
		writer.Write(ReadyForGas);
		writer.Write((Half)MiscTwo);
		writer.Write((byte)movementSpline.Length);
		writer.Write((byte)VisualVariant);

		foreach (Vector2 position in movementSpline)
		{
			writer.Write(position.X);
			writer.Write(position.Y);
		}
	}

	public override void ReceiveExtraAI(BinaryReader reader)
	{
		ReadyForGas = reader.ReadBoolean();
		MiscTwo = (float)reader.ReadHalf();
		byte length = reader.ReadByte();
		VisualVariant = reader.ReadByte();
		movementSpline = new Vector2[length];

		for (int i = 0; i < length; ++i)
		{
			Vector2 pos = new(reader.ReadSingle(), reader.ReadSingle());
			movementSpline[i] = pos;
		}
	}
}
