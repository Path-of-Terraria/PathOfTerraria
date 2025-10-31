using System.Collections.Generic;
using PathOfTerraria.Utilities;
using ReLogic.Utilities;
using Terraria.Audio;
using Terraria.DataStructures;

#nullable enable

namespace PathOfTerraria.Core.Audio;

/// <summary> Tracks sounds attached to tiles and/or tile entities, to stop them once the tile is removed. </summary>
internal sealed class TileSoundTracker : ModSystem
{
	public record struct Entry
	{
		public required SlotId Handle;
		public int? TileType;
		public Type EntityType;
	}

	private static readonly Dictionary<Point16, Entry> lookup = [];

	public override void PostUpdatePlayers()
	{
		CleanupSounds();
	}

	public override void UpdateUI(GameTime gameTime)
	{
		if (Main.gameMenu)
		{
			CleanupSounds();
		}
	}

	public static void Track(Point16 point, Entry entry)
	{
		if (entry.Handle == SlotId.Invalid) { return; }

		if (entry.TileType == null && entry.EntityType == null)
		{
			throw new ArgumentException($"Incorrect usage, at least one entry parameter must not be null.");
		}

		lookup[point] = entry;
	}

	private static void CleanupSounds()
	{
		// Cleanup handles of missing tile entities.
		Dictionary<Point16, Entry>.Enumerator it = lookup.GetEnumerator();
		while (it.MoveNext())
		{
			(Point16 point, Entry entry) = it.Current;

			if (Main.gameMenu
			|| !SoundEngine.TryGetActiveSound(entry.Handle, out _)
			|| (entry.TileType != null && Main.tile[point] is { } tile && (!tile.HasTile || tile.TileType != entry.TileType.Value))
			|| (entry.EntityType != null && (!TileEntity.ByPosition.TryGetValue(it.Current.Key, out TileEntity? entity) || entity.GetType() != entry.EntityType)))
			{
				// Stop the sound.
				SlotId handleCopy = entry.Handle;
				SoundUtils.StopAndInvalidateSoundSlot(ref handleCopy);

				// Remove key and restart iterator.
				lookup.Remove(it.Current.Key);
				it = lookup.GetEnumerator();
			}
		}
	}
}
