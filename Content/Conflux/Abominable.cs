using System.Runtime.CompilerServices;
using PathOfTerraria.Core.Time;
using PathOfTerraria.Utilities;
using PathOfTerraria.Utilities.Xna;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

#nullable enable

namespace PathOfTerraria.Content.Conflux;

internal sealed class Abominable : ModNPC
{
	private record struct Animation()
	{
		public required string Id;
		public required int[] Frames;
		public float Speed = 1f;
		public bool Loop = true;

		public readonly bool Is(in Animation other)
		{
			return Id == other.Id;
		}
	}

	private enum AnimationAction
	{
		Advance,
	}

	private static readonly SpriteFrame baseFrame = new(5, 5);
	private static readonly Animation animWalk = new()
	{
		Id = "walk",
		Frames = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11],
		Speed = 5f,
	};
	private static readonly Animation animJump = new()
	{
		Id = "jump",
		Frames = [14, 15],
		Speed = 5f,
		Loop = true,
	};
	private static readonly Animation animAttack = new()
	{
		Id = "attack",
		Frames = [12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22],
		Speed = 5f,
		Loop = false,
	};

	private Animation animation = animWalk;
	private int animationFrame;
	private int? animationFramePrev;
	private bool animationOver;
	private float frameCounter;

	public ref BitMask<uint> Flags => ref Unsafe.As<float, BitMask<uint>>(ref NPC.localAI[0]);
	public ref float AttackCooldown => ref NPC.localAI[2];
	public int AttackDirection
	{
		get => Flags.Get(0) ? 1 : -1;
		set => Flags.SetTo(0, value >= 0);
	}

	public override void SetStaticDefaults()
	{
		NPCID.Sets.TeleportationImmune[Type] = true;
		NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Shimmer] = true;

		NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, new()
		{
			CustomTexturePath = $"{Mod.Name}/Assets/Conflux/{Name}_Bestiary",
		});

		Main.npcFrameCount[Type] = baseFrame.RowCount;
	}

	public override void SetDefaults()
	{
		NPC.aiStyle = NPCAIStyleID.Fighter;
		NPC.damage = 20;
		NPC.width = 88;
		NPC.height = 100;
		NPC.lifeMax = 200;
		NPC.HitSound = SoundID.NPCHit56 with { Pitch = -0.06f, PitchVariance = 0.15f };
		NPC.DeathSound = SoundID.NPCDeath23 with { Pitch = -0.35f, PitchVariance = 0.15f };
	}

	public override bool PreAI()
	{
		return true;
	}

	public override void AI()
	{
		if (AttackCooldown > 0f)
		{
			AttackCooldown = Math.Max(0f, AttackCooldown - TimeSystem.LogicDeltaTime);
		}

		AdvanceFrames();
		UpdateAnimations();
	}

	public override void PostAI()
	{
		if (!animation.Is(animAttack))
		{
			NPC.velocity.X = MathF.Min(2f, MathF.Abs(NPC.velocity.X)) * MathF.Sign(NPC.velocity.X);
		}
	}

	private void UpdateAnimations()
	{
		const float AttackDistance = 215f;

		Entity? target = NPC.HasPlayerTarget ? Main.player[NPC.target] : (NPC.HasNPCTarget ? Main.npc[NPC.target] : null);
		float targetDistance = (target is { active: true } and not Player { dead: true }) ? target.Distance(NPC.Center) : float.PositiveInfinity;
		Animation? nextAnimation = animation switch
		{
			_ when animation.Is(animAttack) && animationOver => animWalk,
			_ when animation.Is(animWalk) && AttackCooldown <= 0f && targetDistance <= AttackDistance => animAttack with { Speed = 9f },
			_ when animation.Is(animWalk) && MathF.Abs(NPC.velocity.Y) > 5f => animJump with { Speed = 4f },
			_ when animation.Is(animJump) && NPC.velocity.Y == 0f => animWalk,
			_ => null,
		};

		if (nextAnimation != null)
		{
			SetAnimation(nextAnimation.Value);
		}

		NPC.damage = -1;
		NPC.knockBackResist = 0.5f;

		if (animation.Is(animWalk))
		{
			NPC.spriteDirection = NPC.direction;
		}

		if (animation.Is(animAttack))
		{
			const int HitFrame = 5;
			const int LastFrame = 8;

			float speed = animationFrame >= 5 ? 4f : 0.4f;
			//NPC.velocity.Y = Math.Max(0f, NPC.velocity.Y);
			NPC.aiStyle = -1;
			NPC.knockBackResist = 1.0f;

			if (animationFrame == 0 && animationFramePrev != 0)
			{
				AttackDirection = (target!.Center.X - NPC.Center.X) > 0 ? 1 : -1;
				AttackCooldown = 2.25f;

				SoundEngine.PlaySound(position: NPC.Center, style: SoundID.NPCHit56 with { Pitch = -0.25f, PitchVariance = 0.15f });
			}
			else if (animationFrame == 4 && animationFramePrev != 4)
			{
				SoundEngine.PlaySound(position: NPC.Center, style: SoundID.Item71 with { Pitch = -1.00f, PitchVariance = 0.15f });
			}

			NPC.spriteDirection = AttackDirection;

			if (animationFrame < HitFrame)
			{
				NPC.velocity.X = 0.4f * AttackDirection;
			}
			else if (animationFrame == HitFrame && animationFramePrev != HitFrame)
			{
				//NPC.velocity.X = 25f * AttackDirection;
				NPC.velocity = (target!.Center - NPC.Center).SafeNormalize(new Vector2(AttackDirection, 0f)) * new Vector2(30f, 12f);
			}
			else
			{
				NPC.velocity.X *= 0.86f;
				//NPC.velocity.Y += NPC.gravity;
			}

			bool isDamaging = animationFrame >= HitFrame && animationFrame <= LastFrame;
			NPC.damage = isDamaging ? NPC.defDamage : -1;
			NPC.color = isDamaging ? Color.Red : Color.Transparent;
		}
		else
		{
			NPC.aiStyle = NPCAIStyleID.Fighter;
		}
	}

	private void AdvanceFrames()
	{
		frameCounter += animation.Speed * TimeSystem.LogicDeltaTime;
		animationFramePrev = animationFrame;

		while (frameCounter >= 1f)
		{
			(int numFrames, int nextFrame) = (animation.Frames.Length, animationFrame + 1);
			animationFrame = animation.Loop ? (nextFrame % numFrames) : Math.Min(nextFrame, numFrames);
			animationOver |= !animation.Loop && nextFrame >= numFrames;
			frameCounter -= 1f;
		}
	}

	private void SetAnimation(in Animation animation)
	{
		if (animation.Is(this.animation))
		{
			this.animation.Speed = animation.Speed;
			return;
		}

		this.animation = animation;
		animationFrame = 0;
		animationFramePrev = null;
		frameCounter = 0;
		animationOver = false;
	}

	public override void FindFrame(int frameHeight)
	{
		if (Main.dedServ || TextureAssets.Npc[Type] is not { IsLoaded: true, Value: { } texture })
		{
			return;
		}

		int frameIndex = animation.Frames[animationFrame % animation.Frames.Length];
		SpriteFrame frameRect = baseFrame.With((byte)(frameIndex % baseFrame.ColumnCount), (byte)(frameIndex / baseFrame.RowCount));

		NPC.frame = frameRect.GetSourceRectangle(texture);
	}

	public override void HitEffect(NPC.HitInfo hit)
	{
		if (Main.dedServ) { return; }

		bool dead = NPC.life <= 0;
		int numBlood = dead ? 100 : Math.Min(30, hit.Damage);
		int numGore = dead ? 10 : 0;

		for (int i = 0; i < numBlood; i++)
		{
			Vector2 position = Main.rand.NextVector2FromRectangle(NPC.getRect());
			Vector2 velocity = new Vector2(hit.HitDirection, -2f) + (NPC.velocity * 0.5f) + Main.rand.NextVector2Circular(10f, 10f);

			Dust.NewDustPerfect(position, DustID.Blood, velocity, Scale: Main.rand.NextFloat(0.8f, 1.1f));
		}

		for (int i = 0; i < numGore; i++)
		{
			Vector2 position = Main.rand.NextVector2FromRectangle(NPC.getRect().Inflated(-10, -10));
			Vector2 velocity = new Vector2(hit.HitDirection, -2f) + (NPC.velocity * 0.5f) + Main.rand.NextVector2Circular(10f, 10f);

			Gore.NewGorePerfect(Entity.GetSource_Death(), position, velocity, 132);
		}
	}

	public override bool PreDraw(SpriteBatch sb, Vector2 screenPos, Color color)
	{
		Texture2D tex = TextureAssets.Npc[NPC.type].Value;
		Vector2 position = NPC.Center - Main.screenPosition;

		sb.Draw(tex, position, NPC.frame, color, NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, NPC.spriteDirection >= 0 ? (SpriteEffects)1 : 0, 0f);

		return false;
	}

	public override void PostDraw(SpriteBatch sb, Vector2 screenPos, Color color)
	{
		base.PostDraw(sb, screenPos, color);
	}
}
