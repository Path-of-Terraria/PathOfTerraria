using PathOfTerraria.Common.UI.GrimoireSelection;
using PathOfTerraria.Content.Projectiles.Summoner;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems.ModPlayers;

internal class GrimoirePlayer : ModPlayer
{
	/// <summary> The items corresponding to those in the summon ritual slots. </summary>
	public readonly Item[] StoredParts = 
		[
		GrimoireSelectionUIState.EmptyItem, 
		GrimoireSelectionUIState.EmptyItem, 
		GrimoireSelectionUIState.EmptyItem,
		GrimoireSelectionUIState.EmptyItem, 
		GrimoireSelectionUIState.EmptyItem
		];

	public readonly List<Item> Storage = [];

	/// <summary>
	/// The currently selected grimoire summon for the player. This will be -1 if no summon is selected.
	/// </summary>
	public int CurrentSummonId = -1;

	public bool FirstOpenMenagerie = true;
	public bool HasObtainedGrimoire = false;

	public static GrimoirePlayer Get(Player p = null)
	{
		p ??= Main.LocalPlayer;
		return p.GetModPlayer<GrimoirePlayer>();
	}

	public override void SaveData(TagCompound tag)
	{
		Storage.RemoveAll(x => x.IsAir || x.type == ItemID.None || x.stack == 0);

		tag.Add("hasGrimoire", HasObtainedGrimoire);
		tag.Add("count", Storage.Count);

		for (int i = 0; i < Storage.Count; i++)
		{
			Item item = Storage[i];
			tag.Add("item" + i, ItemIO.Save(item));
		}

		for (int i = 0; i < StoredParts.Length; i++)
		{
			tag.Add("part" + i, StoredParts[i]);
		}

		if (FirstOpenMenagerie)
		{
			tag.Add("firstOpen", FirstOpenMenagerie);
		}
	}

	public override void LoadData(TagCompound tag)
	{
		HasObtainedGrimoire = tag.TryGet("hasGrimoire", out bool hasGrimoire) && hasGrimoire;
		int count = tag.GetInt("count");

		for (int i = 0; i < count; i++)
		{
			Storage.Add(ItemIO.Load(tag.GetCompound("item" + i)));
		}

		for (int i = 0; i < StoredParts.Length; i++)
		{
			StoredParts[i] = ItemIO.Load(tag.GetCompound("part" + i));
		}

		FirstOpenMenagerie = tag.ContainsKey("firstOpen");
	}

	/// <returns> The types and stacks of all items in <see cref="StoredParts"/>. </returns>
	internal Dictionary<int, int> GetStoredCount()
	{
		List<Item> storage = Player.GetModPlayer<GrimoirePlayer>().Storage;
		Dictionary<int, int> stacksById = [];

		foreach (Item item in storage)
		{
			if (!stacksById.TryAdd(item.type, item.stack))
			{
				stacksById[item.type] += item.stack;
			}
		}

		return stacksById;
	}

	public bool CanUseSummon(GrimoireSummon summon, out GrimoireSummonLoader.Requirement requirements)
	{
		requirements = ModContent.GetInstance<GrimoireSummonLoader>().RequiredPartsByProjectileId[summon.Type];
		List<int> types = requirements.Types;

		for (int i = 0; i < StoredParts.Length; i++)
		{
			int type = StoredParts[i].type;
			types.Remove(type);
		}

		return types.Count == 0;
	}
}