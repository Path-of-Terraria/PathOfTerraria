using PathOfTerraria.Common.Mapping;
using PathOfTerraria.Content.Dusts;
using Terraria.Audio;
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
	public (int Initial, int Remaining) Time;

	public abstract MapResource GetMapResource();

	public override void SetStaticDefaults()
	{
		ItemID.Sets.IsAPickup[Type] = true;
		ItemID.Sets.ItemNoGravity[Type] = true;

		MapResources.Register(GetMapResource());
	}
	public override void SetDefaults()
	{
		Item.maxStack = 100;
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
			const int MinDelay = 180;
			const int MaxDelay = 210;
			Time.Initial = MinDelay + ((Math.Max(0, Array.IndexOf(Main.item, Item)) * 374) % (MaxDelay - MinDelay));

			// Delay the despawn on servers, so that they do not interrupt clients' own animations.
			if (Main.netMode == NetmodeID.Server) { Time.Initial += 60; }

			Time.Remaining = Time.Initial;
			Initialized = true;

			return;
		}

		Time.Remaining--;

		if (Time.Remaining == 15)
		{
			Item.Size = Vector2.Zero;

			if (!Main.dedServ)
			{
				SoundEngine.PlaySound(acquireSound, Item.Center);
				DespawnEffects();
			}
		}
		else if (Time.Remaining < 15)
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
			MapResources.AddOrRemove(Item.netID, Math.Max(1, Item.stack));
			Item.active = false;
		}

		Item.oldPosition = Item.position;
	}

	public virtual void DespawnEffects()
	{
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
	}
}

internal sealed class InfernalConflux : ConfluxResource
{
	public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(GetMapResource().Cost);

	public override MapResource GetMapResource()
	{
		return new()
		{
			Cost = 100,
			AssociatedItem = Type,
			AccentColor = Color.OrangeRed,
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
