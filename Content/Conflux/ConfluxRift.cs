using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using PathOfTerraria.Common.Encounters;
using PathOfTerraria.Common.Projectiles;
using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.Systems.Synchronization;
using PathOfTerraria.Common.UI;
using PathOfTerraria.Content.Dusts;
using PathOfTerraria.Core.Time;
using PathOfTerraria.Utilities.Xna;
using ReLogic.Content;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;

#nullable enable

namespace PathOfTerraria.Content.Conflux;

internal enum ConfluxRiftKind : byte
{
	Glacial,
	Infernal,
	Celestial,
	Count,
}

internal sealed class ConfluxRift : ModProjectile, IRightClickableProjectile
{
	[Flags]
	public enum Flags : int
	{
		Activated = 1,
		Closing = 2,
	}

	private static Asset<Texture2D> Highlight = null!;

	public uint EndTime { get; set; }
	public Encounter Encounter { get; private set; }
	public float OpeningAnimation { get; private set; }
	public float ClosingAnimation { get; private set; }

	/// <summary> The rift's type. </summary>
	public ConfluxRiftKind Kind
	{
		get => (ConfluxRiftKind)(byte)Projectile.ai[0];
		set => Projectile.ai[0] = (byte)value;
	}
	/// <summary> The rift's state flags. </summary>
	public Flags BitFlags
	{
		get => Unsafe.BitCast<float, Flags>(Projectile.ai[1]);
		set => Projectile.ai[1] = Unsafe.BitCast<Flags, float>(value);
	}
	/// <summary> The synchronized progress of the rift's encounter. </summary>
	public ref float Progress => ref Projectile.ai[2];
	/// <summary> Whether the rift has been opened. </summary>
	public bool Activated => (BitFlags & Flags.Activated) != 0;
	/// <summary> Whether the rift is about to disappear. </summary>
	public bool Closing => (BitFlags & Flags.Closing) != 0;

	private static Asset<Effect>? shader;
	public override void SetStaticDefaults()
	{
		ProjectileID.Sets.IsInteractable[Type] = true;

		if (!Main.dedServ)
		{
			Highlight = ModContent.Request<Texture2D>(Texture + "_Highlight");
		}
	}

	public override void SetDefaults()
	{
		Projectile.friendly = false;
		Projectile.hostile = false;
		Projectile.timeLeft = 2;
		Projectile.tileCollide = false;
		Projectile.Size = new Vector2(120, 120);
		Projectile.Opacity = 0f;
		Projectile.hide = true;
	}

	public override bool? CanDamage()
	{
		return false;
	}

	public override bool PreAI()
	{
		// Despawn only after the closing animation finishes.
		if (!Closing || ClosingAnimation < 1f)
		{
			Projectile.timeLeft++;
		}

		return base.PreAI();
	}

	public override void AI()
	{
		Projectile.rotation += 0.05f;
		Projectile.Opacity = MathHelper.Lerp(Projectile.Opacity, 1f, 0.25f);

		if (Closing)
		{
			ClosingAnimation = MathF.Min(1f, ClosingAnimation + (TimeSystem.LogicDeltaTime / 3f));
		}
		else if (Activated)
		{
			OpeningAnimation = MathF.Min(1f, OpeningAnimation + (TimeSystem.LogicDeltaTime / 3f));

			if (OpeningAnimation > 0.34f && false)
			{
				for (int i = 0; i < 3; i++)
				{
					Vector2 offset = Vector2.One.RotatedBy(Main.rand.NextDouble() * 6.28);
					Vector2 dustVel = offset.RotatedBy(1.57f) * Main.rand.NextFloat() * 0.85f;
					offset *= new Vector2(Main.rand.NextFloat(0.6f, 1) * 60, Main.rand.NextFloat(0.5f, 1f) * 70);
					Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, ModContent.DustType<ConfluxRiftSmoke>(), dustVel);
					dust.scale = Main.rand.NextFloat(0.5f, 0.8f);
					dust.alpha = Main.rand.Next(175, 200);
					dust.color = Color.Lerp(Color.Purple, Color.White, Main.rand.NextFloat(0.25f, 0.75f));
				}
			}
		}

		// Encounter logic.
		if (Main.netMode != NetmodeID.MultiplayerClient)
		{
			// Synchronize the progress to clients.
			if (Activated && Encounter.IsValid)
			{
				UpdateProgress();
			}

			// Start closing when the encounter has been completed, or if the players have ran out of time.
			if (Activated && !Closing && (!Encounter.IsValid || Encounter.Instance.State == EncounterState.Completed || Main.GameUpdateCount >= EndTime))
			{
				Close();
			}
		}

		// Effects.
		if (!Main.dedServ)
		{
			(int torchId, int dustId, _) = GetVisualParameters();

			if (Main.rand.NextBool(10))
			{
				Dust.NewDust(Projectile.position + new Vector2(8), Projectile.width - 16, Projectile.height - 16, dustId);
			}

			Lighting.AddLight(Projectile.Center, torchId);
		}
	}

	public override void OnKill(int timeLeft)
	{
		Close();
	}

	public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
	{
		behindNPCsAndTiles.Add(index);
	}

	public override bool PreDraw(ref Color lightColor)
	{
		Texture2D tex = TextureAssets.Projectile[Type].Value;
		Vector2 position = Projectile.Center - Main.screenPosition;

		if (shader?.Value is not Effect effect)
		{
			return false;
		}

		effect.Parameters["timeManual"].SetValue((float)Main.timeForVisualEffects * 0.027f);
		effect.Parameters["progress"].SetValue(OpeningAnimation);
		effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>(Texture + "_PerlinNoiseMap").Value);
		effect.Parameters["_PaletteTex"].SetValue(ModContent.Request<Texture2D>(Texture + "_Palette").Value);
		effect.Parameters["_PNoiseTex"].SetValue(ModContent.Request<Texture2D>(Texture + "_PerlinNoiseMap").Value);
		effect.Parameters["_DNoiseTex"].SetValue(ModContent.Request<Texture2D>(Texture + "_DisplacementNoiseMap").Value);

		Main.spriteBatch.End();
		Main.spriteBatch.Begin(SpriteSortMode.Immediate, default, SamplerState.PointClamp, default, default, effect, Main.GameViewMatrix.TransformationMatrix);
		Main.spriteBatch.Draw(tex, position, null, Color.White, 0, tex.Size() / 2f, Projectile.scale * 4.0f, SpriteEffects.None, 0);

		Main.spriteBatch.End();
		Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
		this.DrawHighlightAndCheckRightClickInteraction(Highlight.Value, position, lightColor);

		/*
		(_, _, Color colorBase) = GetVisualParameters();
        var sizeTargets = Vector2.Lerp
        (
            Vector2.Lerp(new(0.5f, 0.75f), new(3.50f, 3.75f), OpeningAnimation),
            new(0.0f, 0.0f),
            ClosingAnimation
        );
        (float minSize, float maxSize) = (sizeTargets.X, sizeTargets.Y);

        const int numLayers = 3;

        for (int i = 0; i < numLayers; ++i)
        {
            float rotation = (Projectile.rotation * (i % 2 == 0 ? -1 : 1)) + (i * 1.5f);
            Color color = colorBase * ((3 - i) * 0.33f) * Projectile.Opacity;
            float size = MathHelper.Lerp(minSize, maxSize, i / (float)(numLayers - 1));

            Main.EntitySpriteDraw(tex, position, null, color, rotation, tex.Size() / 2f, size, SpriteEffects.None, 0);
        }

        this.DrawHighlightAndCheckRightClickInteraction(Highlight.Value, position, lightColor);
		*/

		return false;
	}

	public void Activate()
	{
		if ((BitFlags & Flags.Activated) != 0) { return; }

		if (Main.netMode == NetmodeID.MultiplayerClient)
		{
			Vector2 compareSpot = Main.LocalPlayer.Center;
			if (!Main.LocalPlayer.IsProjectileInteractibleAndInInteractionRange(Projectile, ref compareSpot)) { return; }

			RiftInteractionHandler.Send(Projectile.identity);
			return;
		}

		const uint lengthInSeconds = 30;

		BitFlags |= Flags.Activated;
		EndTime = Main.GameUpdateCount + (lengthInSeconds * (uint)TimeSystem.LogicFramerate);
		Encounter = CreateEncounter(lengthInSeconds);

		if (Main.netMode == NetmodeID.Server)
		{
			NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, Projectile.whoAmI);
		}
	}

	public void Close()
	{
		if (Closing || Main.netMode == NetmodeID.MultiplayerClient) { return; }

		BitFlags |= Flags.Closing;

		if (Main.netMode == NetmodeID.Server)
		{
			NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, Projectile.whoAmI);
		}

		UpdateProgress();
		RemoveEncounter();
		DropRewards();
	}

	public void UpdateProgress()
	{
		if (Main.netMode == NetmodeID.MultiplayerClient) { return; }

		float progress = Encounter.Instance.EncounterScore / MathF.Max(0.001f, Encounter.Instance.TotalBaseScore);
		if (progress != Progress)
		{
			Progress = progress;

			if (Main.netMode == NetmodeID.Server)
			{
				NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, Projectile.whoAmI);
			}
		}
	}

	private Encounter CreateEncounter(uint lengthInSeconds)
	{
		const int extentsW = 40;
		const int extentsH = 20;
		Vector2 worldOrigin = Projectile.Center;
		Point16 tileOrigin = worldOrigin.ToTileCoordinates16();

		var area = new Vector4Int
		{
			X = Math.Max(tileOrigin.X - extentsW, 0),
			Y = Math.Max(tileOrigin.Y - extentsH, 0),
			Z = Math.Min(tileOrigin.X + extentsW, Main.maxTilesX - 1),
			W = Math.Min(tileOrigin.Y + extentsH, Main.maxTilesY - 1),
		};
		var rectangle = new Rectangle(area.X, area.Y, area.Z - area.X, area.W - area.Y);

		string[] encounterPaths =
		[
			"Content/Encounters/Squads/BloodSquadSmall",
			"Content/Encounters/Squads/CaveSquadMedium",
			"Content/Encounters/Squads/CorruptionSquadSmall",
			"Content/Encounters/Squads/UndeadSquadLarge",
			"Content/Encounters/Squads/ZombieSquadLarge",
			"Content/Encounters/Squads/ZombieSquadSmall",
			"Content/Encounters/Squads/BruiserBrothers",
		];
		EncounterDescription[] baseEncounters = encounterPaths.Select(p => EncounterIO.GetEncounterFromModPath(Mod, p)).ToArray();

		const uint timeInSecondsToCeaseSpawningFor = 15;
		uint encounterSpawnDelays = 0;
		uint encounterTargetSpawnDelays = Math.Max(0, (uint)((lengthInSeconds - timeInSecondsToCeaseSpawningFor) * TimeSystem.LogicFramerate));
		var waves = new List<EncounterWave>();

		// The amount of enemies spawned every second is scaled by map tier.
		uint cooldownPerEnemy = (uint)(180 / MathHelper.Lerp(1f, 5f, MathHelper.Clamp(MappingWorld.MapTier, 1f, 10f)));
		uint accumulatedCooldown = 0;
		uint cooldownPerBatch = 60;

		// Add base encounters as waves until we hit the time quota.
		while (encounterSpawnDelays < encounterTargetSpawnDelays)
		{
			ref readonly EncounterDescription baseEncounter = ref baseEncounters[Main.rand.Next(baseEncounters.Length)];

			EnemySpawn[] spawns = baseEncounter.Waves.SelectMany(w => w.Spawns).ToArray();

			foreach (ref EnemySpawn spawn in spawns.AsSpan())
			{
				spawn.Effect = EnemySpawnEffect.Teleport;
				if (spawn.SpawnPlacement.HasValue)
				{
					spawn.SpawnPlacement = spawn.SpawnPlacement.Value with { MinDistanceFromEnemies = 8f };
				}

				accumulatedCooldown += cooldownPerEnemy;
				uint cooldown = 0;

				if (accumulatedCooldown >= cooldownPerBatch)
				{
					cooldown = cooldownPerBatch;
					accumulatedCooldown -= cooldownPerBatch;
				}

				spawn.CooldownInTicks = cooldown;
				encounterSpawnDelays += cooldown;
			}

			waves.Add(new EncounterWave
			{
				Spawns = spawns.ToArray(),
				// Spawning does not wait for the player to kill anything. Waves can be thought of as cosmetic.
				TargetEncounterScore = 0.0f,
				TargetWaveScore = 0.0f,
				TargetSpawnScore = 0.0f,
			});
		}

		Encounter encounter = EnemyEncounters.CreateEncounter(new EncounterDescription
		{
			Identifier = "Rift",
			SpawnOrigin = tileOrigin,
			ActivationOrigin = worldOrigin,
			ActivationRange = 2048f,
			SpawnArea = rectangle,
			Waves = waves.ToArray(),
			Music = new(MusicID.LunarBoss),
			SceneEffectPriority = SceneEffectPriority.BossLow,
		});

		return encounter;
	}

	private void RemoveEncounter()
	{
		if (!Encounter.IsValid) { return; }

		// Despawn all remaining NPCs.
		foreach (NPC npc in Encounter.IterateRemainingEnemies())
		{
			EnemySpawning.SpawnEffects(npc, EnemySpawnEffect.Teleport, npc.Center);
			npc.active = false;

			if (Main.netMode == NetmodeID.Server)
			{
				NetMessage.SendData(MessageID.SyncNPC, number: npc.whoAmI);
			}
		}

		// Remove encounter.
		Encounter.Remove();
	}

	private void DropRewards()
	{
		int rewardType = Kind switch
		{
			ConfluxRiftKind.Glacial => ModContent.ItemType<GlacialConflux>(),
			ConfluxRiftKind.Celestial => ModContent.ItemType<CelestialConflux>(),
			_ => ModContent.ItemType<InfernalConflux>(),
		};
		int rewardAmount = Progress switch
		{
			<= 0.20f => 0,
			<= 0.50f => 1,
			<= 0.80f => 2,
			< 1.000f => 3,
			_ => 4,
		};

		float multiplier = 1f;
		// 5% increase per map tier.
		multiplier += (MappingWorld.MapTier * 0.05f);
		// 2% increase per map modifier weight.
		multiplier += (MappingWorld.TotalWeight() * 0.02f);

		rewardAmount = (int)MathF.Floor(rewardAmount * multiplier);

		for (int i = 0; i < rewardAmount; i++)
		{
			int itemIdx = Item.NewItem(null, Projectile.Center + Main.rand.NextVector2Circular(72f, 72f), rewardType, 1, noBroadcast: true);
			Item item = Main.item[itemIdx];
			item.velocity = Main.rand.NextVector2Circular(10f, 10f);

			if (Main.netMode == NetmodeID.Server)
			{
				NetMessage.SendData(MessageID.SyncItem, number: itemIdx);
			}
		}
	}

	private (int torchId, int dustId, Color colorBase) GetVisualParameters()
	{
		return Kind switch
		{
			ConfluxRiftKind.Glacial => (TorchID.Ice, DustID.Firework_Blue, Color.AliceBlue),
			ConfluxRiftKind.Infernal => (TorchID.Red, DustID.Firework_Red, Color.OrangeRed),
			ConfluxRiftKind.Celestial => (TorchID.Purple, DustID.WitherLightning, Color.MediumVioletRed),
			_ => throw new NotImplementedException(),
		};
	}

	bool IRightClickableProjectile.RightClick(Player player, bool mouseDirectlyOver)
	{
		if (Main.mouseRight && Main.mouseRightRelease)
		{
			Activate();

			return true;
		}

		if (mouseDirectlyOver)
		{
			Tooltip.Create(new TooltipDescription
			{
				Identifier = "ConfluxRift",
				SimpleTitle = Language.GetTextValue($"Mods.{PoTMod.ModName}.Misc.Enter"),
			});
		}

		return false;
	}

	public override void Load()
	{
		base.Load();
		shader = Mod.Assets.Request<Effect>($"Assets/Effects/ConfluxRift");
	}

	public override void Unload()
	{
		shader = null;
	}
}

/// <summary>
/// Synchronizes right click interactions with rifts.
/// </summary>
internal class RiftInteractionHandler : Handler
{
	public static void Send(int riftIdentity)
	{
		ModPacket packet = Networking.GetPacket<RiftInteractionHandler>();
		packet.Write(riftIdentity);
		packet.Send();
	}

	internal override void ServerReceive(BinaryReader reader, byte sender)
	{
		ModPacket packet = Networking.GetPacket(Id);
		int riftIdentity = reader.ReadInt32();

		if (Main.player[sender] is not { active: true } player) { return; }

		if (Main.projectile.FirstOrDefault(p => p.identity == riftIdentity) is not { ModProjectile: ConfluxRift rift }) { return; }

		// Increased reach distance for synchronization grace.
		Point tileTarget = rift.Projectile.Hitbox.ClosestPointInRect(player.Center).ToTileCoordinates();
		if (!player.IsInTileInteractionRange(tileTarget.X, tileTarget.Y, TileReachCheckSettings.Simple with { TileRangeMultiplier = 2 })) { return; }

		rift.Activate();
	}
}
