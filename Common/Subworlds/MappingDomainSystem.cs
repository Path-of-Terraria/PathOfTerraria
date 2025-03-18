using System.Collections.Generic;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Subworlds;

/// <summary>
/// Tracks the amount of times each tier of map has been defeated.
/// </summary>
public class MappingDomainSystem : ModSystem
{
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

		/// <summary>
		/// Gets the amount of completions at or above every tier currently available. 
		/// If a tier does not exist, it has not been completed even once.
		/// </summary>
		/// <returns>Dictionary mapping tiers to <see cref="CompletionsAtOrAboveTier(int)"/> results.</returns>
		public Dictionary<int, int> CompletionsPerTier()
		{
			Dictionary<int, int> completionsByTier = [];

			foreach (int tier in TierCompletions.Keys)
			{
				completionsByTier[tier] = CompletionsAtOrAboveTier(tier);
			}

			return completionsByTier;
		}

		public void Save(TagCompound tag)
		{
			tag.Add("count", (byte)TierCompletions.Count);
			int id = 0;

			foreach ((int tier, int count) in TierCompletions)
			{
				tag.Add("pair" + id + "Tier", (byte)tier);
				tag.Add("pair" + id + "Count", (short)count);
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
