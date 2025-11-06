using PathOfTerraria.Common.Mapping;
using PathOfTerraria.Content.Dusts;
using Terraria.Audio;
using Terraria.ID;

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

	}

	// Never pickup, never go towards the player.
	public override bool GrabStyle(Player player) { return true; }
	public override bool CanPickup(Player player) { return false; }

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

		if (!Main.dedServ && Time.Remaining == 30)
		{
			SoundEngine.PlaySound(acquireSound, Item.Center);
		}

		if (Time.Remaining <= 0)
		{
			// Convert to resource.
			MapResources.AddOrRemove(Item.netID, 1);
			Item.active = false;

			if (!Main.dedServ)
			{
				DespawnEffects();
			}
		}
	}

	public virtual void DespawnEffects()
	{
		Vector2 center = Item.Center;

		for (int i = 0; i < 10; i++)
		{
			Vector2 position = center + Main.rand.NextVector2Circular(12f, 12f);
			Vector2 velocity = Main.rand.NextVector2Circular(5f, 5f);
			Dust.NewDustPerfect(position, ModContent.DustType<ConfluxRiftSmoke>(), velocity);
		}
	}
}

internal sealed class InfernalConflux : ConfluxResource
{
	public override MapResource GetMapResource()
	{
		return new()
		{
			AssociatedItem = Type,
			Color = Color.OrangeRed,
		};
	}

	public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
	{
		Lighting.AddLight(Item.Center, Color.IndianRed.ToVector3());
	}
}

internal sealed class GlacialConflux : ConfluxResource
{
	public override MapResource GetMapResource()
	{
		return new()
		{
			AssociatedItem = Type,
			Color = Color.AliceBlue,
		};
	}

	public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
	{
		Lighting.AddLight(Item.Center, Color.LightSkyBlue.ToVector3());
	}
}

internal sealed class CelestialConflux : ConfluxResource
{
	public override MapResource GetMapResource()
	{
		return new()
		{
			AssociatedItem = Type,
			Color = Color.MediumPurple,
		};
	}

	public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
	{
		Lighting.AddLight(Item.Center, Color.Violet.ToVector3());
	}
}
