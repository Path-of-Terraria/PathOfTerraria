using PathOfTerraria.Common;
using PathOfTerraria.Common.World.Generation;
using System.IO;
using System.Linq;

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

	private Vector2[] movementSpline = [];

	public override void SetDefaults()
	{
		NPC.Size = new Vector2(132);
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
				LastWallNodeSelected = Main.rand.Next(SwampArenaGeneration.TraverseWalls.Length);
				SetState(BehaviorState.SpawnAnimation);
			}
		}
		else if (State == BehaviorState.SpawnAnimation)
		{
			NPC.dontTakeDamage = false;
			SetState(BehaviorState.MoveToWall);
		}
		else if (State == BehaviorState.MoveToWall)
		{
			ref float splineSlot = ref MiscNumber;

			if (Timer == 1)
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

			Vector2 nextSpline = movementSpline[(int)splineSlot];
			NPC.velocity = Vector2.SmoothStep(NPC.velocity, NPC.SafeDirectionTo(nextSpline) * 22, 0.16f);
			NPC.rotation = NPC.velocity.ToRotation() - MathHelper.PiOver2;

			if (NPC.DistanceSQ(nextSpline) < 50 * 50)
			{
				splineSlot++;

				if (splineSlot >= movementSpline.Length)
				{
					SetState(BehaviorState.IdleInWall);
				}
			}
		}
		else if (State == BehaviorState.IdleInWall)
		{
			NPC.velocity *= 0.8f;

			if (Timer == 180)
			{
				SetState(BehaviorState.MoveToWall);
			}
		}
	}

	public override void SendExtraAI(BinaryWriter writer)
	{
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
