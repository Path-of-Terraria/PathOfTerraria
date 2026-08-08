using PathOfTerraria.Common.UI.GrimoireSelection;
using PathOfTerraria.Content.Projectiles.Summoner;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems.ModPlayers;

internal class GrimoirePlayer : ModPlayer
{
	public struct GrimoireStats()
	{
		public StatModifier DamageModifier = new(1, 1, 0, 0);
		public AddableFloat SpeedModifier = default;
		public AddableFloat CriticalStrikeChanceModifier = default;
	}

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
	public GrimoireStats Stats = default;

	public static GrimoirePlayer Get(Player p = null)
	{
		p ??= Main.LocalPlayer;
		return p.GetModPlayer<GrimoirePlayer>();
	}

	public override void ResetEffects()
	{
		Stats = new();
	}

	public override void SaveData(TagCompound tag)
	{
		Storage.RemoveAll(x => x.IsAir || x.type == ItemID.None || x.stack == 0);

		if (CurrentSummonId != -1 && ContentSamples.ProjectilesByType.TryGetValue(CurrentSummonId, out Projectile summonProjectile) &&
			summonProjectile.ModProjectile is GrimoireSummon grimoireSummon)
		{
			tag.Add("currentSummon", grimoireSummon.FullName);
		}

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
		CurrentSummonId = LoadCurrentSummonId(tag);

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

	private static int LoadCurrentSummonId(TagCompound tag)
	{
		if (!tag.ContainsKey("currentSummon"))
		{
			return -1;
		}

		object currentSummon = tag["currentSummon"];

		if (currentSummon is string summonName && ModContent.TryFind(summonName, out ModProjectile modProjectile) && modProjectile is GrimoireSummon)
		{
			return modProjectile.Type;
		}

		return currentSummon is int summon &&
			ContentSamples.ProjectilesByType.TryGetValue(summon, out Projectile projectile) &&
			projectile.ModProjectile is GrimoireSummon
				? summon
				: -1;
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
