using NPCUtils;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using PathOfTerraria.Common.Systems.HellEvent;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.HellEvent;

public sealed class AshWraith : ModNPC
{
	public enum States : byte
	{
		Follow,
		Dash,
		Pause
	}

	private States State
	{ 
		get => (States)NPC.ai[0];
		set => NPC.ai[0] = (float)value;
	}

	private ref float Timer => ref NPC.ai[1];

	Vector2 eyePos = Vector2.Zero;

	private Player Target => Main.player[NPC.target];

	public override void SetStaticDefaults()
	{
		Main.npcFrameCount[Type] = 3;
	}

	public override void SetDefaults()
	{
		NPC.CloneDefaults(NPCID.Crimera);
		NPC.aiStyle = -1;
		NPC.Size = new Vector2(22, 28);
		NPC.damage = 60;
		NPC.noTileCollide = true;
		NPC.defense = 20;
		NPC.value = Item.buyPrice(0, 0, 5, 0);
		NPC.lavaImmune = true;
		NPC.HitSound = SoundID.DD2_GhastlyGlaiveImpactGhost;
		NPC.DeathSound = SoundID.NPCDeath6;

		NPC.TryEnableComponent<NPCHitEffects>(c =>
		{
			c.AddDust(new(DustID.Ash, 4));
			c.AddDust(new(DustID.Ash, 15, NPCHitEffects.OnDeath));
		});
	}

	public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
	{
		bestiaryEntry.AddInfo(this, "TheUnderworld");
	}

	public override float SpawnChance(NPCSpawnInfo spawnInfo)
	{
		if (!HellEventSystem.EventOccuring)
		{
			return 0f;
		}

		return 0.6f;
	}

	public override void AI()
	{
		NPC.rotation += 0.16f;

		if (Main.rand.NextBool(20))
		{
			int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Ash, 0, 0, Main.rand.Next(50, 150), Scale: 2);

			Main.dust[dust].noGravity = true;
		}

		eyePos = NPC.DirectionTo(Target.Center) * 8;

		switch (State)
		{
			case States.Follow:
				FollowState();
				break;

			case States.Dash:
				DashState();
				break;

			case States.Pause:
				Timer++;

				if (Timer > 30)
				{
					State = States.Follow;
					Timer = 0;
				}

				break;
		}
	}

	private void DashState()
	{
		NPC.Opacity = MathHelper.Lerp(NPC.Opacity, 1f, 0.05f);
		Timer++;

		if (Timer < 30)
		{
			NPC.velocity -= NPC.DirectionTo(Target.Center) * 0.1f;
		}
		else if (Timer <= 35)
		{
			NPC.velocity += NPC.DirectionTo(Target.Center) * 1.6f;
		}
		else
		{
			if (NPC.velocity.Length() < 12f)
			{
				NPC.velocity *= 1.05f;
			}

			bool colliding = Collision.SolidCollision(NPC.position - new Vector2(2), NPC.width + 4, NPC.height + 4);

			if (colliding || Timer > 120)
			{
				Timer = 0;
				State = States.Pause;

				NPC.noTileCollide = true;
				NPC.knockBackResist = 1f;

				Collision.HitTiles(NPC.position - new Vector2(2), NPC.velocity, NPC.width + 4, NPC.height + 4);

				for (int i = 0; i < 25; ++i)
				{
					Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Ash, NPC.velocity.X, NPC.velocity.Y);
				}

				NPC.velocity = Vector2.Zero;
			}
		}
	}

	private void FollowState()
	{
		NPC.TargetClosest(true);
		NPC.Opacity = MathHelper.Lerp(NPC.Opacity, 0.25f, 0.05f);
		float dist = NPC.Distance(Target.Center);

		if (dist < 250f && !Collision.SolidCollision(NPC.position, NPC.width, NPC.height) && Collision.CanHit(NPC, Target))
		{
			Timer++;
			NPC.velocity *= 0.98f;

			if (Timer > 120f)
			{
				State = States.Dash;
				Timer = 0;

				NPC.noTileCollide = false;
				NPC.knockBackResist = 0;
			}
		}
		else
		{
			NPC.velocity = Vector2.Lerp(NPC.velocity, (Target.Center - NPC.Center) / (dist + 50) * 6, 0.1f);
		}
	}

	public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		if (NPC.IsABestiaryIconDummy)
		{
			NPC.rotation += 0.16f;
		}

		Texture2D tex = TextureAssets.Npc[Type].Value;
		Vector2 pos = NPC.Center - screenPos;

		for (int i = 0; i < 12; ++i)
		{
			Color color = drawColor * NPC.Opacity;
			Rectangle frame = new(14, 30 * i / 4, 22, 28);
			float rot = (NPC.rotation + i) * (i % 2 == 0 ? 1 : -1);

			Main.EntitySpriteDraw(tex, pos, frame, color * (2 - i / 6f), rot, frame.Size() / 2f, 1f + i / 15f, SpriteEffects.None);
		}

		var eyeFrame = new Rectangle(0, 0, 10, 12);
		Main.EntitySpriteDraw(tex, pos + eyePos, eyeFrame, Color.White * 0.8f, 0f, eyeFrame.Size() / 2f, 1f, SpriteEffects.None);

		return false;
	}
}