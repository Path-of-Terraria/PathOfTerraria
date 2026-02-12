using PathOfTerraria.Common;
using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.World.Generation;
using System.IO;
using Terraria.ID;

namespace PathOfTerraria.Content.Swamp.NPCs.SwampBoss;

[AutoloadBossHead]
internal class Mossmother : ModNPC
{
	public enum BehaviorState : byte
	{
		Huddled,
		SpawnAnimation,
		MoveToWall,
		IdleInWall,
		GasCrawl,
	}

	public BehaviorState State
	{
		get => (BehaviorState)NPC.ai[0];
		set => NPC.ai[0] = (float)value;
	}

	public const int SplineCountMax = 30;

	private ref float Timer => ref NPC.ai[1];
	private ref float MiscNumber => ref NPC.ai[2];

	private ref float LastWallNodeSelected => ref NPC.localAI[0];
	
	private bool ReadyForGas
	{
		get => NPC.localAI[1] == 1;
		set => NPC.localAI[1] = value ? 1 : 0;
	}

	private ref float TimesWallCrawled => ref NPC.localAI[2];

	private Vector2[] movementSpline = [];

	public override void SetDefaults()
	{
		NPC.Size = new Vector2(180);
		NPC.lifeMax = 80_000;
		NPC.defense = 15;
		NPC.aiStyle = -1;
		NPC.noGravity = true;
		NPC.noTileCollide = true;
		NPC.boss = true;
		NPC.knockBackResist = 0f;
	}

	public override bool CheckActive()
	{
		return false;
	}

	public override bool? CanBeHitByProjectile(Projectile projectile)
	{
		return State is BehaviorState.Huddled or BehaviorState.SpawnAnimation ? false : null;
	}

	public override bool? CanBeHitByItem(Player player, Item item)
	{
		return State is BehaviorState.Huddled or BehaviorState.SpawnAnimation ? false : null;
	}

	public override bool CanBeHitByNPC(NPC attacker)
	{
		return State is not BehaviorState.Huddled and not BehaviorState.SpawnAnimation;
	}

	public override void BossHeadRotation(ref float rotation)
	{
		rotation = NPC.rotation;
	}

	public void SetState(BehaviorState state)
	{
		Timer = 0;
		MiscNumber = 0;
		State = state;
		NPC.netUpdate = true;

		if (State == BehaviorState.MoveToWall && Main.netMode != NetmodeID.MultiplayerClient)
		{
			BuildSplineForMovement();
		}
	}

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
		else if (State == BehaviorState.IdleInWall)
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
					if (npc.ModNPC is Mossmother mother && mother.State != BehaviorState.IdleInWall && mother.Timer >= 240)
					{
						allReady = false;
						break;
					}
				}

				if (allReady)
				{
					SetState(BehaviorState.GasCrawl);
				}
			}
		}
		else if (State == BehaviorState.GasCrawl)
		{
			ref float splineSlot = ref MiscNumber;
			Vector2 nextSpline = movementSpline[(int)splineSlot];
			NPC.velocity = Vector2.SmoothStep(NPC.velocity, NPC.SafeDirectionTo(nextSpline) * 5, 0.16f);
			NPC.rotation = Utils.AngleLerp(NPC.rotation, NPC.velocity.ToRotation() - MathHelper.PiOver2, 0.6f);

			if (NPC.DistanceSQ(nextSpline) < 20 * 20)
			{
				splineSlot++;

				if (splineSlot >= movementSpline.Length)
				{
					SetState(BehaviorState.IdleInWall);
					ReadyForGas = false;
					MiscNumber = movementSpline[^2].AngleTo(movementSpline[^1]);
					TimesWallCrawled = 0; 
				}
			}
		}

		NPC.velocity *= 0;
	}

	private void BuildSplineForMovement()
	{
		int endIndex;

		do
		{
			endIndex = Main.rand.Next(SwampArenaGeneration.TraverseWalls.Length);
		} while (endIndex == LastWallNodeSelected);

		Vector2 end = SwampArenaGeneration.TraverseWalls[endIndex].ToWorldCoordinates();
		Vector2 node = Main.rand.Next(SwampArenaGeneration.TraverseNodes).ToWorldCoordinates();
		LastWallNodeSelected = endIndex;

		movementSpline = Spline.InterpolateXY([NPC.Center, node, end], SplineCountMax);
	}

	public override void SendExtraAI(BinaryWriter writer)
	{
		writer.Write(ReadyForGas);
		writer.Write((Half)LastWallNodeSelected);
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
		LastWallNodeSelected = (float)reader.ReadHalf();
		byte length = reader.ReadByte();
		movementSpline = new Vector2[length];

		for (int i = 0; i < length; ++i)
		{
			Vector2 pos = new(reader.ReadSingle(), reader.ReadSingle());
			movementSpline[i] = pos;
		}
	}
}
