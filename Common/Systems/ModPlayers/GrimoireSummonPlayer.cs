using System.Collections.Generic;
using System.Linq;
using PathOfTerraria.Common.UI.GrimoireSelection;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems.ModPlayers;

internal class GrimoireSummonPlayer : ModPlayer
{
	public readonly Item[] StoredParts = [GrimoireSelectionUIState.EmptyItem, GrimoireSelectionUIState.EmptyItem, GrimoireSelectionUIState.EmptyItem, 
		GrimoireSelectionUIState.EmptyItem, GrimoireSelectionUIState.EmptyItem];

	internal readonly HashSet<string> UnlockedSummons = [];

	public int CurrentSummonId = -1;
	public bool HasObtainedGrimoire = false;

	public bool HasSummon<T>() where T : ModProjectile
	{
		return HasSummon(ModContent.ProjectileType<T>());
	}

	public bool HasSummon(int id)
	{
		return UnlockedSummons.Contains(ContentSamples.ProjectilesByType[id].ModProjectile.FullName);
	}

	public void UnlockSummon<T>() where T : ModProjectile
	{
		UnlockSummon(ModContent.ProjectileType<T>());
	}

	public void UnlockSummon(int id)
	{
		UnlockedSummons.Add(ContentSamples.ProjectilesByType[id].ModProjectile.FullName);
	}

	public override void SaveData(TagCompound tag)
	{
		tag.Add("hasGrimoire", HasObtainedGrimoire);
		tag.Add("count", UnlockedSummons.Count);

		for (int i = 0; i < UnlockedSummons.Count; i++)
		{
			tag.Add("summon" + i, UnlockedSummons.ElementAt(i));
		}

		for (int i = 0; i < StoredParts.Length; i++)
		{
			tag.Add("part" + i, StoredParts[i]);
		}
	}

	public override void LoadData(TagCompound tag)
	{
		HasObtainedGrimoire = tag.TryGet("hasGrimoire", out bool hasGrimoire) && hasGrimoire;
		int count = tag.GetInt("count");

		for (int i = 0; i < count; i++)
		{
			UnlockedSummons.Add(tag.GetString("summon" + i));
		}

		for (int i = 0; i < StoredParts.Length; i++)
		{
			StoredParts[i] = ItemIO.Load(tag.GetCompound("part" + i));
		}
	}
}
