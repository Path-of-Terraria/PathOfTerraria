using System.Collections.Generic;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Subworlds;

/// <summary>
/// Tracks the amount of times each tier of map has been defeated.
/// </summary>
public class MappingDomainSystem : ModSystem
{
	public const int RequiredCompletionsPerTier = 5;
	public class TiersDownedTracker
	{
		private readonly Dictionary<int, int> TierCompletions = [];

		/// <summary>
		/// Adds <paramref name="count"/> completion(s) to the given <paramref name="tier"/>.
		/// </summary>
		/// <param name="tier">Map tier that has been completed.</param>
		/// <param name="count">How many completions to add.</param>
		public void AddCompletion(int tier, int count = 1)
		{
			if (!TierCompletions.TryAdd(tier, count))
			{
				TierCompletions[tier] += count;
			}
		}

		/// <summary>
		/// Forcefully sets the amount of completions in <paramref name="tier"/> to <paramref name="count"/>. Used for syncing.
		/// </summary>
		/// <param name="tier">Map tier that has been completed.</param>
		/// <param name="count">How many times this tier has been completed.</param>
		internal void SetCompletion(int tier, int count)
		{
			if (!TierCompletions.TryAdd(tier, count))
			{
				TierCompletions[tier] = count;
			}
		}

		public int CompletionsAtOrAboveTier(int checkTier)
		{
			int total = 0;

			foreach ((int tier, int count) in TierCompletions)
			{
				if (tier >= checkTier)
				{
					total += count;
				}
			}

			return total;
		}

		public void ClearHigherCompletions(int currentTier)
		{
			foreach (int tier in TierCompletions.Keys)
			{
				if (tier > currentTier)
				{
					TierCompletions[tier] = 0;
				}
			}
		}
		
		public void Save(TagCompound tag)
		{
			tag.Add("count", (byte)TierCompletions.Count);
			int id = 0;

			foreach ((int tier, int count) in TierCompletions)
			{
				tag.Add("pair" + id + "Tier", (byte)tier);
				tag.Add("pair" + id + "Count", (short)count);
				id++;
			}
		}

		public static TiersDownedTracker Load(TagCompound tag)
		{
			TiersDownedTracker tracker = new();
			int count = tag.GetByte("count");

			for (int i = 0; i < count; ++i)
			{
				tracker.AddCompletion(tag.GetByte("pair" + i + "Tier"), tag.GetShort("pair" + i + "Count"));
			}

			return tracker;
		}
	}

	public TiersDownedTracker Tracker = new();

	public override void SaveWorldData(TagCompound tag)
	{
		TagCompound trackerTag = [];
		Tracker.Save(trackerTag);
		tag.Add("tracker", trackerTag);
	}

	public override void LoadWorldData(TagCompound tag)
	{
		if (tag.TryGet("tracker", out TagCompound trackerTag))
		{
			Tracker = TiersDownedTracker.Load(trackerTag);
		}
	}
}
