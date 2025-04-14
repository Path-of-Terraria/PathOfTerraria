using Terraria.ID;

namespace PathOfTerraria.Content.Items.Pickups;

/// <summary> Base class for pickup items that decay over time. </summary>
internal abstract class PickupItem : ModItem
{
	/// <summary> The time it takes, in ticks, for this pickup to despawn. </summary>
	public virtual int DecayTime => 60 * 60;
	public float DecayProgress => (float)(Item.timeSinceItemSpawned - ItemID.Sets.OverflowProtectionTimeOffset[Type]) / (DecayTime / ItemID.Sets.ItemSpawnDecaySpeed[Type]);

	public override void SetStaticDefaults()
	{
		ItemID.Sets.IsAPickup[Type] = true;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();
		Item.Size = new Vector2(16);
	}

	public override bool ItemSpace(Player player)
	{
		return true;
	}

	public override Color? GetAlpha(Color lightColor)
	{
		Color result = lightColor * Math.Min((1f - DecayProgress) * 10, 1); //Control fadeout alpha
		return result;
	}

	/// <summary> <inheritdoc cref="ModItem.Update"/><para/>
	/// Also handles natural decay for pickups.
	/// </summary>
	public override void Update(ref float gravity, ref float maxFallSpeed)
	{
		if (DecayProgress >= 1)
		{
			Item.active = false;
		}
	}
}