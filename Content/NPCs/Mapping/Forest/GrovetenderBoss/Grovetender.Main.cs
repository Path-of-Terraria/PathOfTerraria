using NPCUtils;
using PathOfTerraria.Common.NPCs;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using PathOfTerraria.Content.Items.Gear.Amulets.AddedLife;
using PathOfTerraria.Content.Items.Gear.Weapons.Bow;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.Mapping.Forest.GrovetenderBoss;

internal partial class Grovetender : ModNPC
{
	private static Asset<Texture2D> Glow = null;

	public enum AIState
	{
		Asleep,
		Idle,
		BoulderThrow,
		RainProjectiles,
	}

	public Player Target => Main.player[NPC.target];

	private AIState State
	{
		get => (AIState)NPC.ai[0];
		set => NPC.ai[0] = (float)value;
	}

	private ref float Timer => ref NPC.ai[1];

	private int ControlledWhoAmI
	{
		get => (int)NPC.ai[2];
		set => NPC.ai[2] = value;
	}

	private bool Initialized
	{
		get => NPC.ai[3] == 1;
		set => NPC.ai[3] = value ? 1 : 0;
	}

	private readonly Dictionary<Point16, int> poweredRunestonePositions = [];

	public override void SetStaticDefaults()
	{
		//Glow = ModContent.Request<Texture2D>(Texture + "_Glow");
		
		NPCID.Sets.MustAlwaysDraw[Type] = true;
	}

	public override void SetDefaults()
	{
		NPC.Size = new Vector2(233, 183);
		NPC.aiStyle = -1;
		NPC.lifeMax = 20000;
		NPC.defense = 50;
		NPC.damage = 0;
		NPC.knockBackResist = 0f;
		NPC.HitSound = SoundID.NPCHit30;
		NPC.noTileCollide = true;
		NPC.noGravity = true;
		NPC.boss = true;

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

	public override void AI()
	{
		Lighting.AddLight(NPC.Center - new Vector2(30, 10), new Vector3(0.5f, 0.5f, 0.25f));
		Lighting.AddLight(NPC.Center + new Vector2(30, -10), new Vector3(0.5f, 0.5f, 0.25f));

		if (State == AIState.Asleep)
		{
			if (!Initialized)
			{
				InitPoweredRunestones();

				Initialized = true;
			}

			State = AIState.Idle;
		}
		else if (State == AIState.Idle)
		{
			Timer++;

			if (Timer > 180)
			{
				State = AIState.RainProjectiles;
				Timer = 0;
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

		UpdatePoweredRunestones();
	}

	public override void ModifyNPCLoot(NPCLoot npcLoot)
	{
		npcLoot.AddOneFromOptions<RootedAmulet, WardensBow>(1);
	}

	public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		Texture2D tex = TextureAssets.Npc[Type].Value;
		SpriteEffects effect = NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
		Vector2 position = NPC.Center - screenPos - new Vector2(-20, -NPC.gfxOffY + 200);

		for (int i = 0; i < tex.Width / 16 + 1; ++i)
		{
			for (int j = 0; j < tex.Height / 16 + 1; ++j)
			{
				var frame = new Rectangle(NPC.frame.X + i * 16, NPC.frame.Y + j * 16, 16, 16);
				Vector2 drawPos = position + new Vector2(i * 16, j * 16);
				Color color = Lighting.GetColor((drawPos + screenPos - new Vector2(200, 340)).ToTileCoordinates());

				Main.EntitySpriteDraw(tex, drawPos, frame, color, NPC.rotation, NPC.frame.Size() / 2f, 1f, effect);
			}
		}
		//Main.EntitySpriteDraw(Glow.Value, position, NPC.frame, Color.White, NPC.rotation, NPC.frame.Size() / 2f, 1f, effect);
		return false;
	}
}