using PathOfTerraria.Common;
using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.World.Generation;
using System.IO;
using Terraria.ID;

namespace PathOfTerraria.Content.Swamp.NPCs.SwampBoss;

[AutoloadBossHead]
internal partial class Mossmother : ModNPC
{
	public enum BehaviorState : byte
	{
		Huddled,
		SpawnAnimation,
		MoveToWall,
		IdleInWall,
		GasCrawl,
		Desperation,
	}

	public BehaviorState State
	{
		get => (BehaviorState)NPC.ai[0];
		set => NPC.ai[0] = (float)value;
	}

	public const int SplineCountMax = 30;
	public const int TrailingIndex = 20;
	public const int PosionAuraRadiusSize = 340;

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

	public override void SetStaticDefaults()
	{
		NPCID.Sets.TrailCacheLength[Type] = TrailingIndex;
		NPCID.Sets.TrailingMode[Type] = 1;
	}

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

		//if (NPC.CountNPCS(Type) == 1)
		//{
		//	State = BehaviorState.Desperation;
		//}
	}
}
