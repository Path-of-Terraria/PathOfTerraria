using PathOfTerraria.Common;
using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.World.Generation;
using System.IO;
using Terraria.ID;

namespace PathOfTerraria.Content.Swamp.NPCs.SwampBoss;

internal partial class Mossmother
{
	public override void AI()
	{
		Timer++;

		if (Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
		{
			NPC.position -= NPC.velocity * 0.5f;
		}

		if (State == BehaviorState.Huddled)
		{
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
				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					LastWallNodeSelected = Main.rand.Next(SwampArenaGeneration.TraverseWalls.Length);
					NPC.netUpdate = true;
				}

				SetState(BehaviorState.SpawnAnimation);
			}
		}
		else if (State == BehaviorState.SpawnAnimation)
		{
			NPC.GetGlobalNPC<ArenaEnemyNPC>().Arena = true;
			NPC.dontTakeDamage = false;
			SetState(BehaviorState.MoveToWall);
		}
		else if (State == BehaviorState.MoveToWall)
		{
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

					NPC.NewNPC(NPC.GetSource_FromThis(), (int)pos.X, (int)pos.Y, ModContent.NPCType<Mossling>(), NPC.whoAmI + 3);
				}

				return;
			}

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
		else if (State == BehaviorState.IdleInWall)
		{
			IdleInWall();
		}
		else if (State == BehaviorState.GasCrawl)
		{
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

				return;
			}

			Vector2 nextSpline = movementSpline[(int)splineSlot];
			NPC.velocity = Vector2.SmoothStep(NPC.velocity, NPC.SafeDirectionTo(nextSpline) * 5, 0.16f);
			NPC.rotation = Utils.AngleLerp(NPC.rotation, NPC.velocity.ToRotation() - MathHelper.PiOver2, 0.6f);

			if (NPC.DistanceSQ(nextSpline) < 20 * 20)
			{
				splineSlot++;
			}
		}
		else if (State == BehaviorState.Desperation)
		{
			ref float splineSlot = ref MiscNumber;
			Vector2 nextSpline = movementSpline[(int)splineSlot];
			NPC.velocity = Vector2.SmoothStep(NPC.velocity, NPC.SafeDirectionTo(nextSpline) * 22, 0.16f);
			NPC.rotation = Utils.AngleLerp(NPC.rotation, NPC.velocity.ToRotation() - MathHelper.PiOver2, 0.6f);

			if (NPC.DistanceSQ(nextSpline) < 60 * 60)
			{
				splineSlot++;

				if (splineSlot >= movementSpline.Length)
				{
					SetState(BehaviorState.IdleInWall);
					MiscNumber = movementSpline[^2].AngleTo(movementSpline[^1]);
					TimesWallCrawled++;

					if (!Main.rand.NextBool((int)TimesWallCrawled))
					{
						ReadyForGas = true;
					}
				}
			}
		}
	}

	private void IdleInWall()
	{
		NPC.velocity *= 0.90f;
		NPC.rotation = Utils.AngleLerp(NPC.rotation, MiscNumber + MathHelper.PiOver2, 0.02f);

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
				if (npc.whoAmI != NPC.whoAmI && npc.ModNPC is Mossmother mother && ((mother.State == BehaviorState.IdleInWall && mother.Timer < 240) && mother.State != BehaviorState.GasCrawl))
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

	public void SetState(BehaviorState state)
	{
		Timer = 0;
		MiscNumber = 0;
		State = state;
		NPC.netUpdate = true;

		if (NPC.CountNPCS(Type) == 1)
		{
			State = BehaviorState.Desperation;
		}

		if (Main.netMode != NetmodeID.MultiplayerClient)
		{
			if (State is BehaviorState.MoveToWall or BehaviorState.Desperation)
			{
				BuildSplineForMovement(State == BehaviorState.Desperation);
			}

			if (State == BehaviorState.MoveToWall)
			{
				MiscTwo = Main.rand.NextBool(1) ? 130 : 0;
			}
		}
	}

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
	/// Used to stop the mossmothers from having the exact same path and overlapping, causing confusion.
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
		movementSpline = new Vector2[length];

		for (int i = 0; i < length; ++i)
		{
			Vector2 pos = new(reader.ReadSingle(), reader.ReadSingle());
			movementSpline[i] = pos;
		}
	}
}
