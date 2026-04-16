using PathOfTerraria.Common.Mapping;
using PathOfTerraria.Common.Utilities;
using PathOfTerraria.Content.Dusts;
using PathOfTerraria.Core.Time;
using PathOfTerraria.Utilities;
using PathOfTerraria.Utilities.Terraria;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Content.Conflux;

/// <summary>
/// Represents a conflux resource.<br/>
/// These are not actually used as items, cannot be picked up, and are only used to play an animation before being sent to the <see cref="MapResources"/> storage.
/// </summary>
internal abstract class ConfluxResource : ModItem
{
	private static readonly SoundStyle acquireSound = new($"{nameof(PathOfTerraria)}/Assets/Sounds/Conflux/ResourceAcquire")
	{
		PitchVariance = 0.15f,
		MaxInstances = 3,
		Volume = 0.75f,
	};

	public bool Initialized;
	public (int Initial, int Remaining, int Persistent) Time;

	protected byte MergeProgress;
	protected SlotId Sound;

	public override string Texture => base.Texture + (Item.stack >= 4 ? "_Large" : (Item.stack >= 2 ? "_Medium" : "_Small"));

	public abstract MapResource GetMapResource();

	public override void SetStaticDefaults()
	{
		ItemID.Sets.IsAPickup[Type] = true;
		ItemID.Sets.ItemNoGravity[Type] = true;

		MapResources.Register(GetMapResource());
	}
	public override void SetDefaults()
	{
		Item.maxStack = 999;
	}

	// Never pickup, never go towards the player, never stack in world.
	public override bool GrabStyle(Player player) { return true; }
	public override bool CanPickup(Player player) { return false; }
	public override bool CanStackInWorld(Item source) { return false; }

	public override void Update(ref float gravity, ref float maxFallSpeed)
	{
		if (!Initialized)
		{
			// Treat this line as desync-proof random.
			const int MinDelay = 120;
			//const int MaxDelay = 210;
			Time.Initial = MinDelay; //MinDelay + ((Math.Max(0, Array.IndexOf(Main.item, Item)) * 374) % (MaxDelay - MinDelay));

			// Delay the despawn on servers, so that they do not interrupt clients' own animations.
			if (Main.netMode == NetmodeID.Server) { Time.Initial += 60; }

			Time.Remaining = Time.Initial;
			Time.Persistent = Time.Initial;
			Initialized = true;

			return;
		}

		const uint FlyUpTick = 15;
		Vector2 center = Item.Center;

		Time.Remaining--;
		Time.Persistent = Math.Max(0, Time.Persistent - 1);

		float currentProgress = 1f - (Time.Remaining / (float)Time.Initial);
		float persistentProgress = 1f - (Time.Persistent / (float)Time.Initial);

		if (Time.Remaining > FlyUpTick)
		{
			// Move towards nearby stacks.
			const uint MergeDelay = 30;
			const float MergeDistance = 32f;
			const float PullDistance = 1500f;
			const float PullPowFactor = 0.5f;
			const float PullSpeed = 10f;
			const float PullAccel = 0.9f;
			const float PushSpeed = 10f;
			const float PushAccel = 1.00f;

			bool progressedMerge = false;
			float basePower = MathUtils.Clamp01(persistentProgress * 2f);

			foreach (Item other in Main.ActiveItems)
			{
				if (other.type != Item.type || other.ModItem is not ConfluxResource otherRes || other == Item) { continue; }

				if (((ConfluxResource)other.ModItem).Time.Remaining <= FlyUpTick) { continue; }

				Vector2 diff = other.Center - center;
				float distance = diff.Length();
				float pullPower = basePower * MathF.Pow(MathUtils.DistancePower(distance, 8f, PullDistance), PullPowFactor);
				float pushPower = MathUtils.DistancePower(distance, 8f, 32f);
				Vector2 direction = distance > 0f ? diff / distance : Vector2.UnitX;
				Vector2 sideDir = direction.RotatedBy(MathHelper.PiOver2);

				Item.velocity = MovementUtils.DirAccel(Item.velocity, direction, PullSpeed, pullPower * PullAccel * TimeSystem.LogicDeltaTime);
				Item.velocity = MovementUtils.DirAccel(Item.velocity, sideDir, PushSpeed, pushPower * PushAccel * TimeSystem.LogicDeltaTime);
				//Item.velocity += direction * movePower * PullSpeed * TimeSystem.LogicDeltaTime;

				if (pullPower > 0f)
				{
					otherRes.Time.Remaining = Math.Max(otherRes.Time.Remaining, (int)(otherRes.Time.Initial * 0.5f));
				}

				if (distance < MergeDistance)
				{
					if (!progressedMerge) { MergeProgress++; progressedMerge = true; }
				}
			}

			// Merge all similar nearby items after a period of them being close.
			if (MergeProgress >= MergeDelay)
			{
				MergeProgress = 0;

				int mergeCount = 1;
				Vector2 mergePos = center;

				foreach (Item other in Main.ActiveItems)
				{
					if (other.type != Item.type || other.ModItem is not ConfluxResource otherRes || other == Item) { continue; }

					if (other.Distance(center) > MergeDistance) { continue; }

					mergePos += other.Center;
					Item.stack += other.stack;
					other.active = false;
					mergeCount++;
					otherRes.MergeProgress = 0;
				}

				Item.Center = center = mergePos / mergeCount;

				SoundEngine.PlaySound(new($"{nameof(PathOfTerraria)}/Assets/Sounds/Conflux/RiftEnemySpawn") { Volume = 0.2f, Pitch = 0.2f, PitchVariance = 0.2f }, center);
			}
		}
		else if (Time.Remaining == FlyUpTick)
		{
			Item.Size = Vector2.Zero;

			if (!Main.dedServ)
			{
				SoundEngine.PlaySound(acquireSound, Item.Center);
				DespawnEffects();
			}
		}
		else
		{
			Item.velocity.Y -= 2f;

			if (!Main.dedServ)
			{
				DespawnEffects();
			}
		}

		if (Item.position.Y <= 0f || Time.Remaining <= -60)
		{
			// Convert to resource.
			MapResources.ModifyValue(Item.netID, Math.Max(1, Item.stack));
			Item.active = false;
		}

		// Play audio.
		if (!Main.dedServ)
		{
			int idx = Array.FindIndex(Main.item, item => item == Item);
			float speed = Item.velocity.Length();
			var style = new SoundStyle($"{nameof(PathOfTerraria)}/Assets/Sounds/Conflux/RiftApproached") { MaxInstances = 3, IsLooped = true, PauseBehavior = PauseBehavior.StopWhenGamePaused };
			float volume = MathF.Max(float.Epsilon * 2f, (0.1f + MathF.Min(0.2f, speed / 10f)) * MathUtils.Clamp01((persistentProgress - 0.1f) * 5f));
			float pitch = -1f + MathHelper.Clamp(speed / 10f, 0f, 0.5f);
			SoundUtils.UpdateLoopingSound(ref Sound, Item.Center, volume, pitch, style, _ => Item.active && Main.item[idx] == Item);
		}

		Item.oldPosition = Item.position;

		// Cause the next update frame to not do collision logic.
		Item.beingGrabbed = true;
	}

	public virtual void DespawnEffects()
	{
		/*
		Vector2 oldCenter = Item.oldPosition + (Item.Size * 0.5f);
		Vector2 newCenter = Item.position + (Item.Size * 0.5f);
		MapResource resource = GetMapResource();

		float distance = Item.oldPosition.Distance(Item.position);
		int numParticles = (int)(distance / 3f);

		for (int i = 0; i < numParticles; i++)
		{
			float step = (i + 0.5f) / (float)numParticles;
			Vector2 center = Vector2.Lerp(oldCenter, newCenter, step);
			Vector2 position = center + Main.rand.NextVector2Circular(8f, 8f);
			Vector2 velocity = (Item.velocity * Main.rand.NextFloat(0.65f, 1.0f)) + Main.rand.NextVector2Circular(1f, 1f);
			var dust = Dust.NewDustPerfect(Type: ModContent.DustType<ConfluxRiftSmoke>(), Position: position, Velocity: velocity);
			dust.scale *= Main.rand.NextFloat(0.4f, 0.75f);
			dust.color = Color.Lerp(resource.AccentColor, Color.White, Main.rand.NextFloat(0.25f, 0.75f));
			dust.alpha = Main.rand.Next(175, 200);
		}
		*/
	}

	public override bool PreDrawInInventory(SpriteBatch sb, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
	{
		if (ModContent.Request<Texture2D>(Texture) is not { IsLoaded: true, Value: { } texture }) { return true; }

		frame = Main.itemAnimations[Item.type]?.GetFrame(texture) ?? texture.Frame();
		scale = MathF.Min(1f, 50f / texture.Width);
		sb.Draw(texture, position, frame, drawColor, 0f, frame.Size() * 0.5f, scale, 0, 0f);

		return false;
	}

	public override bool PreDrawInWorld(SpriteBatch sb, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
	{
		if (ModContent.Request<Texture2D>(Texture) is not { IsLoaded: true, Value: { } texture }) { return true; }

		float x = MathF.Pow(MathUtils.Clamp01(1f - ((Time.Remaining - 10f) / (float)Time.Initial)), 35f);
		x = (MathF.Pow(2f, -15f * x) * MathF.Sin((x * 10f - 0.75f) * ((2f * MathHelper.Pi) / 3f)) + 1f);
		rotation = MathHelper.Lerp(rotation, MathHelper.Pi, x);

		var scale2D = new Vector2(1f, MathF.Max(1f, Item.velocity.Length() / 7f));

		Vector2 pos = Item.Center;
		Vector2 off = new Vector2((float)Math.Sin(Main.timeForVisualEffects * 0.5f), (float)Math.Sin(Main.timeForVisualEffects * 0.2f));
		Rectangle frame = Main.itemAnimations[Item.type]?.GetFrame(texture) ?? texture.Frame();
		alphaColor = Item.GetAlpha(Color.White);
		sb.Draw(texture, pos + off - Main.screenPosition, frame, alphaColor, rotation, frame.Size() * 0.5f, scale2D, 0, 0f);

		return false;
	}
}

internal sealed class InfernalConflux : ConfluxResource
{
	public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(GetMapResource().Cost);

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();
		ItemID.Sets.AnimatesAsSoul[Type] = true;
		Main.RegisterItemAnimation(Type, new DrawAnimationVertical(5, 4, false));
	}

	public override MapResource GetMapResource()
	{
		return new()
		{
			Cost = 100,
			AssociatedItem = Type,
			AccentColor = Color.OrangeRed,
			PortalDestination = $"{nameof(PathOfTerraria)}/{nameof(InfernalRealm)}",
			CanisterLiquidTexture = $"{nameof(PathOfTerraria)}/Assets/UI/MapDevice/Liquid_{GetType().Name}",
		};
	}

	public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
	{
		Lighting.AddLight(Item.Center, Color.IndianRed.ToVector3());
	}
}

internal sealed class GlacialConflux : ConfluxResource
{
	public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(GetMapResource().Cost);

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();
		//ItemID.Sets.AnimatesAsSoul[Type] = true;
		//Main.RegisterItemAnimation(Type, new DrawAnimationVertical(5, 4, false));
	}

	public override MapResource GetMapResource()
	{
		return new()
		{
			Cost = 100,
			AssociatedItem = Type,
			AccentColor = Color.Cyan,
			CanisterLiquidTexture = $"{nameof(PathOfTerraria)}/Assets/UI/MapDevice/Liquid_{GetType().Name}",
		};
	}

	public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
	{
		Lighting.AddLight(Item.Center, Color.LightSkyBlue.ToVector3());
	}
}

internal sealed class CelestialConflux : ConfluxResource
{
	public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(GetMapResource().Cost);

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();
		ItemID.Sets.AnimatesAsSoul[Type] = true;
		Main.RegisterItemAnimation(Type, new DrawAnimationVertical(5, 4, false));
	}

	public override MapResource GetMapResource()
	{
		return new()
		{
			Cost = 100,
			AssociatedItem = Type,
			AccentColor = Color.Magenta,
			CanisterLiquidTexture = $"{nameof(PathOfTerraria)}/Assets/UI/MapDevice/Liquid_{GetType().Name}",
		};
	}

	public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
	{
		Lighting.AddLight(Item.Center, Color.Violet.ToVector3());
	}
}
