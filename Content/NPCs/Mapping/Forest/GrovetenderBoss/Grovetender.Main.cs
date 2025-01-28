using NPCUtils;
using PathOfTerraria.Common.NPCs;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using PathOfTerraria.Common.Subworlds.BossDomains.WoFDomain;
using PathOfTerraria.Content.Items.Gear.Amulets.AddedLife;
using PathOfTerraria.Content.Items.Gear.Weapons.Bow;
using PathOfTerraria.Content.Projectiles.Utility;
using PathOfTerraria.Content.Tiles.Maps.Forest;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.Mapping.Forest.GrovetenderBoss;

[AutoloadBossHead]
internal partial class Grovetender : ModNPC
{
	private static HashSet<int> ValidRainTiles => [TileID.LivingWood, TileID.LeafBlock, ModContent.TileType<LivingWoodBlocker>()];

	private static Asset<Texture2D> Sockets = null;
	private static Asset<Texture2D> EyeLarge = null;
	private static Asset<Texture2D> EyeSmall = null;
	private static Asset<Texture2D> Gradient = null;

	public enum AIState
	{
		Asleep,
		Idle,
		BoulderThrow,
		RainProjectiles,
		RootDig,
	}

	public enum EyeID
	{
		Left,
		Right,
		SmallLeft,
		SmallRight
	}

	public Player Target => Main.player[NPC.target];

	private AIState State
	{
		get => (AIState)NPC.ai[0];
		set => NPC.ai[0] = (float)value;
	}

	private ref float Timer => ref NPC.ai[1];

	private bool SecondPhase
	{
		get => NPC.ai[2] == 1;
		set => NPC.ai[2] = value ? 1 : 0;
	}

	private bool Initialized
	{
		get => NPC.ai[3] == 1;
		set => NPC.ai[3] = value ? 1 : 0;
	}

	internal readonly Dictionary<Point16, float> PoweredRunestonePositions = [];

	private readonly HashSet<int> _controlledWhoAmI = [];
	private readonly Dictionary<Point16, float> _rootPositionsAndTimers = [];

	public override void SetStaticDefaults()
	{
		Sockets = ModContent.Request<Texture2D>(Texture + "_Sockets");
		EyeLarge = ModContent.Request<Texture2D>(Texture + "_EyeLarge");
		EyeSmall = ModContent.Request<Texture2D>(Texture + "_EyeSmall");
		Gradient = ModContent.Request<Texture2D>(Texture + "_Gradient");

		NPCID.Sets.MustAlwaysDraw[Type] = true;
	}

	public override void SetDefaults()
	{
		NPC.Size = new Vector2(233, 183);
		NPC.aiStyle = -1;
		NPC.lifeMax = 12000;
		NPC.defense = 45;
		NPC.damage = 0;
		NPC.knockBackResist = 0f;
		NPC.HitSound = SoundID.NPCHit30;
		NPC.noTileCollide = true;
		NPC.noGravity = true;
		NPC.boss = true;
		NPC.hide = true;

		NPC.TryEnableComponent<NPCHitEffects>(
			c =>
			{
				//for (int i = 0; i < 6; ++i)
				//{
				//	c.AddGore(new NPCHitEffects.GoreSpawnParameters($"{PoTMod.ModName}/{Name}_" + i, 1, NPCHitEffects.OnDeath));
				//}

				c.AddDust(new NPCHitEffects.DustSpawnParameters(DustID.CorruptionThorns, 5));
				c.AddDust(new NPCHitEffects.DustSpawnParameters(ModContent.DustType<EntDust>(), 1));
				c.AddDust(new NPCHitEffects.DustSpawnParameters(DustID.CorruptionThorns, 20, NPCHitEffects.OnDeath));
				c.AddDust(new NPCHitEffects.DustSpawnParameters(ModContent.DustType<EntDust>(), 10, NPCHitEffects.OnDeath));
			}
		);
	}

	public override bool CheckActive()
	{
		return false;
	}

	public override void DrawBehind(int index)
	{
		Main.instance.DrawCacheNPCsBehindNonSolidTiles.Add(index);
	}

	public override bool? CanBeHitByItem(Player player, Item item)
	{
		return State != AIState.Asleep ? null : false;
	}

	public override bool? CanBeHitByProjectile(Projectile projectile)
	{
		return State != AIState.Asleep ? null : false;
	}

	public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
	{
		NPC.lifeMax = ModeUtils.ByMode(450, 600, 900);
		NPC.damage = ModeUtils.ByMode(65, 130, 195);
		NPC.knockBackResist = ModeUtils.ByMode(0.3f, 0.27f, 0.23f);
	}

	public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
	{
		bestiaryEntry.AddInfo(this, "Surface");
	}

	public override void BossLoot(ref string name, ref int potionType)
	{
		potionType = ItemID.HealingPotion;
	}

	public override void AI()
	{
		Lighting.AddLight(NPC.Center - GetEyeOffset(EyeID.Left, false), new Vector3(0.5f, 0.5f, 0.25f));
		Lighting.AddLight(NPC.Center - GetEyeOffset(EyeID.Right, false), new Vector3(0.5f, 0.5f, 0.25f));

		if (State == AIState.Asleep)
		{
			NPC.TargetClosest();

			if (!Initialized)
			{
				InitPoweredRunestones();

				NPC.position = NPC.position.ToTileCoordinates().ToWorldCoordinates(0, 0);

				Initialized = true;
			}

			bool anyPlayerFar = false;

			foreach (Player player in Main.ActivePlayers)
			{
				if (player.DistanceSQ(NPC.Center) > 1000 * 1000)
				{
					anyPlayerFar = true;
					break;
				}
			}

			if (!anyPlayerFar)
			{
				State = AIState.Idle;

				NPC.GetGlobalNPC<ArenaEnemyNPC>().Arena = true; // Captures everyone in the arena with the blocker trees
				NPC.GetGlobalNPC<ArenaEnemyNPC>().StillDropStuff = true; // But still allow drops
			}
		}
		else if (State == AIState.Idle)
		{
			Timer++;

			if (Timer > 150)
			{
				NPC.TargetClosest();

				State = NPC.DistanceSQ(Target.Center) < 600 * 600 ? AIState.BoulderThrow : AIState.RainProjectiles;
				Timer = 0;

				if (!SecondPhase && NPC.life < NPC.lifeMax / 2)
				{
					State = AIState.RootDig;

					SecondPhase = true;
				}
			}
		}
		else if (State == AIState.BoulderThrow)
		{
			BoulderThrowBehaviour();
		}
		else if (State == AIState.RainProjectiles)
		{
			RainProjectileBehaviour();
		}
		else if (State == AIState.RootDig)
		{
			RootDigBehaviour();
		}

		UpdatePoweredRunestones();

		if (SecondPhase)
		{
			UpdateRootOpenings();
		}
	}

	public override void ModifyNPCLoot(NPCLoot npcLoot)
	{
		npcLoot.AddOneFromOptions<RootedAmulet, WardensBow>(1);
	}

	public override void OnKill()
	{
		Vector2 pos = NPC.Center - new Vector2(0, 100);
		Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), pos, Vector2.Zero, ModContent.ProjectileType<ExitPortal>(), 0, 0, Main.myPlayer);
	}
}