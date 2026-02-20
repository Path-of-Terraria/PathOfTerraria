using Microsoft.Xna.Framework.Graphics;
using NPCUtils;
using PathOfTerraria.Common.AI;
using PathOfTerraria.Common.NPCs;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using PathOfTerraria.Common.NPCs.Worms;
using PathOfTerraria.Common.Systems.MobSystem;
using PathOfTerraria.Common.Systems.Synchronization;
using PathOfTerraria.Common.Systems.Synchronization.Handlers;
using PathOfTerraria.Common.World.Generation;
using PathOfTerraria.Content.Buffs;
using PathOfTerraria.Content.Dusts;
using PathOfTerraria.Content.Gores;
using SubworldLibrary;
using System.IO;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;

namespace PathOfTerraria.Content.Swamp.NPCs;

#nullable enable

[AutoloadBossHead]
internal class GiantEel : ModNPC
{
	internal class GiantEelBody : WormSegment
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();

			Main.npcFrameCount[Type] = 6;
		}

		public override void Defaults()
		{
			NPC.Size = new Vector2(50);
			NPC.damage = 60;
			NPC.HitSound = SoundID.NPCHit34;
			NPC.DeathSound = SoundID.NPCHit53;
			NPC.defense = 100;
			NPC.netAlways = true;
		}

		public override bool CheckActive()
		{
			return false;
		}

		public override void FindFrame(int frameHeight)
		{
			if (NPC.ai[2] == 0)
			{
				NPC.ai[2] = Main.rand.Next(2) + 1;
			}

			NPC.frameCounter++;
			NPC.frame.Y = frameHeight * (int)((NPC.frameCounter * 0.15f + NPC.whoAmI * MathHelper.PiOver2) % 3) + frameHeight * (int)((NPC.ai[2] - 1) * 3);
		}

		public override void AI()
		{
			base.AI();

			if (!Parent.active)
			{
				NPC.active = false;
			}

			SpamDust(NPC, NPC.position - NPC.oldPosition);
		}

		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
		{
			if (target.statLife - hurtInfo.Damage <= 0)
			{
				if (Main.netMode == NetmodeID.SinglePlayer)
				{
					NPC parent = GetHead();

					if (parent.ModNPC is GiantEel eel)
					{
						eel.State = States.Flee;
						eel.MiscTimer = 0;
					}
				}
				else
				{
					SyncEelRetreat.Send(GetHead().whoAmI);
				}
			}
		}

		public override void HitEffect(NPC.HitInfo hit)
		{
			if (Main.dedServ)
			{
				return;
			}

			int reps = NPC.life < 0 ? 8 : 2;
			Vector2 velocity = NPC.position - NPC.oldPosition;

			for (int i = 0; i < reps; ++i)
			{
				Dust.NewDust(NPC.position, NPC.width, NPC.height, ModContent.DustType<DarkMossDust>(), velocity.X, velocity.X);
			}
		}
	}

	internal class GiantEelTail : GiantEelBody
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();

			Main.npcFrameCount[Type] = 3;
		}

		public override void FindFrame(int frameHeight)
		{
			NPC.frameCounter++;
			NPC.frame.Y = frameHeight * (int)((NPC.frameCounter * 0.15f + NPC.whoAmI * MathHelper.PiOver2) % 3);
		}
	}

	internal class SyncEelRetreat : Handler
	{
		public static void Send(int npcWho)
		{
			if (Main.netMode == NetmodeID.Server)
			{
				return;
			}

			ModPacket packet = Networking.GetPacket<SyncEelRetreat>();
			packet.Write((byte)npcWho);
			packet.Send();
		}

		internal override void Receive(BinaryReader reader, byte sender)
		{
			if (Main.npc[reader.ReadByte()].ModNPC is GiantEel eel)
			{
				eel.State = States.Flee;
				eel.MiscTimer = 0;
				eel.NPC.netUpdate = true;
			}
		}
	}

	private enum States
	{
		Roaming = 0,
		Chase,
		Fury,
		Flee,
	}

	private States State
	{
		get => (States)NPC.ai[0];
		set => NPC.ai[0] = (float)value;
	}

	private ref float Timer => ref NPC.ai[1];
	private ref float TargetPlayer => ref NPC.ai[2];

	private ref float SplineIndex => ref NPC.localAI[0];
	private ref float AvoidWaterTimer => ref NPC.localAI[1];
	private ref float MiscTimer => ref NPC.localAI[2];

	private bool _spawnedSegments = false;
	private Vector2[] _spline = [];

	public override void Load()
	{
		//GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, $"{Texture}_GoreBlade");
		//GoreLoader.AddGoreFromTexture<AdvancedGore>(Mod, $"{Texture}_GoreHead");
		//GoreLoader.AddGoreFromTexture<AdvancedGore>(Mod, $"{Texture}_GoreChest");
		//GoreLoader.AddGoreFromTexture<AdvancedGore>(Mod, $"{Texture}_GoreArm");
		//GoreLoader.AddGoreFromTexture<AdvancedGore>(Mod, $"{Texture}_GoreLeg");
	}

	public override void SetStaticDefaults()
	{
		ArpgNPC.NoAffixesSet.Add(Type);
		SkipSectionCheckNPC.SkipSectionCheck.Add(Type);
	}

	public override void SetDefaults()
	{
		NPC.aiStyle = -1;
		NPC.lifeMax = 150_000;
		NPC.defense = 100;
		NPC.damage = 50;
		NPC.width = 50;
		NPC.height = 50;
		NPC.knockBackResist = 0f;
		NPC.noGravity = true;
		NPC.noTileCollide = true;
		NPC.netAlways = true;

		NPC.HitSound = new($"{nameof(PathOfTerraria)}/Assets/Sounds/HitEffects/FleshHit", 3) { MaxInstances = 5, Volume = 0.4f };
		NPC.DeathSound = SoundID.NPCDeath23 with { Pitch = +0.1f, PitchVariance = 0.15f, Identifier = $"{Name}Death" };

		NPC.TryEnableComponent<NPCVoice>(e =>
		{
			e.Data.PainSound = (3, SoundID.NPCHit56 with { Pitch = +0.3f, PitchVariance = 0.4f, Identifier = $"{Name}Hit" });
		});
		NPC.TryEnableComponent<NPCHitEffects>(c =>
		{
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatMedium)}", 1));

			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatSmall)}", 2, NPCHitEffects.OnDeath));
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatMedium)}", 1, NPCHitEffects.OnDeath));
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatLarge)}", 1, NPCHitEffects.OnDeath));
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatSmall)}", 2, NPCHitEffects.OnDeath));
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatMedium)}", 1, NPCHitEffects.OnDeath));
			c.AddGore(new($"{PoTMod.ModName}/{nameof(BloodSplatLarge)}", 1, NPCHitEffects.OnDeath));
		});
	}

	public override bool CheckActive()
	{
		return false;
	}

	public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
	{
		bestiaryEntry.AddInfo(this, "");
	}

	public override void AI()
	{
		if (!_spawnedSegments && Main.netMode != NetmodeID.MultiplayerClient)
		{
			WormSegment.SpawnWhole<GiantEelBody, GiantEelTail>(NPC.GetSource_FromAI(), NPC, 42, 16);
			_spawnedSegments = true;

			NPC.netUpdate = true;
		}

		if (Math.Abs(NPC.velocity.X) > 0.01f)
		{
			NPC.direction = -Math.Sign(NPC.velocity.X);
		}

		Timer++;
		NPC.rotation = NPC.velocity.ToRotation();
		Lighting.AddLight(NPC.Center, new Vector3(0.6f, 0.6f, 0.1f));

		Player targetPlayer = Main.player[(int)TargetPlayer];
		SpamDust(NPC, NPC.velocity);

		if (State == States.Roaming)
		{
			float aggro = Utils.Remap(targetPlayer.Center.Y / 16f, 40, SwampArea.FloorY, 3000, 1200, true) * (targetPlayer.HasBuff<SwampAlgaeBuff>() ? 1.3333f : 1);
			Vector2 targetDirection = NPC.DirectionTo(new Vector2(targetPlayer.Center.X, MathF.Sin(Timer * 0.008f) * (30 * 16) + 180 * 16)) * new Vector2(2.5f, 1);
			NPC.velocity = Vector2.SmoothStep(NPC.velocity, targetDirection * 8, 0.05f) + new Vector2(MathF.Sin(Timer * 0.04f) * 0.25f, 0);

			bool inArenaLocation = IsInArena();

			if (targetPlayer.DistanceSQ(NPC.Center) < aggro * aggro && !inArenaLocation)
			{
				State = States.Chase;
				Timer = 0;
			}
		}
		else if (State == States.Chase)
		{
			if (MiscTimer == 0)
			{
				Vector2 destination = targetPlayer.Center;
				NPC.velocity = Vector2.SmoothStep(NPC.velocity, NPC.DirectionTo(destination) * 20, 0.2f);

				if (NPC.DistanceSQ(destination) < 120 * 120)
				{
					MiscTimer = 1;

					Vector2 reverseDirection = destination.DirectionFrom(NPC.Center);
					_spline = Spline.InterpolateXY([NPC.Center, destination + reverseDirection * 650, destination - reverseDirection * 300 - new Vector2(0, 800)], 6);
					NPC.netUpdate = true;
				}
			}
			else
			{
				if (SplineIndex >= _spline.Length)
				{
					return;
				}
				 
				Vector2 destination = _spline[(int)SplineIndex];
				NPC.velocity = Vector2.SmoothStep(NPC.velocity, NPC.DirectionTo(destination) * 20, 0.2f);

				if (NPC.DistanceSQ(destination) < 40 * 40 && SplineIndex < _spline.Length - 1)
				{
					SplineIndex++;
				}

				if (NPC.DistanceSQ(destination) < 60 * 60 && SplineIndex >= _spline.Length - 1)
				{
					MiscTimer = 0;
					SplineIndex = 0;
					NPC.netUpdate = true;
				}
			}

			if (Collision.WetCollision(NPC.position + new Vector2(0, 200), NPC.width, NPC.height) || Collision.WetCollision(NPC.position, NPC.width, NPC.height))
			{
				AvoidWaterTimer++;

				if (AvoidWaterTimer > 60)
				{
					State = States.Flee;
					MiscTimer = 0;
					AvoidWaterTimer = 0;
					NPC.netUpdate = true;
				}
			}

			if (targetPlayer.dead || !targetPlayer.active || IsInArena())
			{
				State = States.Flee;
				MiscTimer = 0;
				AvoidWaterTimer = 0;
				NPC.netUpdate = true;
			}
		}
		else if (State == States.Flee)
		{
			NPC.velocity = Vector2.SmoothStep(NPC.velocity, new Vector2(MathF.Sin(MiscTimer * 0.03f) * 2, -20), 0.2f);

			if (NPC.Center.Y / 16f < Main.offLimitBorderTiles * 4)
			{
				State = States.Roaming;
			}
		}
	}

	private bool IsInArena()
	{
		bool left = SwampArea.LeftSpawn;
		return SubworldSystem.Current is SwampArea
			&& (!left && NPC.Center.X / 16f < SwampArenaGeneration.ArenaWidth) || (left && NPC.Center.X / 16f > Main.maxTilesX - SwampArenaGeneration.ArenaWidth);
	}

	public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
	{
		if (target.statLife - hurtInfo.Damage <= 0)
		{
			if (Main.netMode == NetmodeID.SinglePlayer)
			{
				State = States.Flee;
				MiscTimer = 0;
			}
			else
			{
				SyncEelRetreat.Send(NPC.whoAmI);
			}
		}
	}

	private static void SpamDust(NPC npc, Vector2 velocity)
	{
		if (Main.rand.NextBool(30))
		{
			Dust dust = Main.dust[Dust.NewDust(npc.position, npc.width, npc.height, DustID.JungleGrass)];
			dust.velocity = velocity * 0.2f + new Vector2(0, Main.rand.NextFloat(1, 2));
			dust.noGravity = true;
			dust.alpha = 0;
			dust.scale = Main.rand.NextFloat(1, 2);
		}
	}

	public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		Texture2D tex = TextureAssets.Npc[Type].Value;
		SpriteEffects flip = NPC.velocity.X >= 0 ? SpriteEffects.None : SpriteEffects.FlipVertically;

		spriteBatch.Draw(tex, NPC.Center - screenPos, NPC.frame, drawColor, NPC.rotation, NPC.frame.Size() / 2f, 1f, flip, 0);
		return false;
	}

	public override void BossHeadRotation(ref float rotation)
	{
		rotation = NPC.rotation;
	}

	public override void BossHeadSpriteEffects(ref SpriteEffects spriteEffects)
	{
		spriteEffects = NPC.velocity.X >= 0 ? SpriteEffects.None : SpriteEffects.FlipVertically;
	}

	public override void SendExtraAI(BinaryWriter writer)
	{
		writer.Write((byte)_spline.Length);

		for (int i = 0; i < _spline.Length; i++)
		{
			writer.WriteVector2(_spline[i]);
		}
	}

	public override void ReceiveExtraAI(BinaryReader reader)
	{
		byte count = reader.ReadByte();

		_spline = new Vector2[count];
		
		for (int i = 0; i < count; ++i)
		{
			_spline[i] = reader.ReadVector2();
		}
	}
}
