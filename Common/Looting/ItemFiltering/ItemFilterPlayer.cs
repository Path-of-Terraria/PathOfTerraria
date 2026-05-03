using PathOfTerraria.Core.UI;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace PathOfTerraria.Common.Looting.ItemFiltering;

/// <summary>
///		Per-player storage of saved item filters and which one is currently active for routing
///		pickups in the Sack of Holding.
/// </summary>
internal sealed class ItemFilterPlayer : ModPlayer
{
	public static ModKeybind FilterEditorKeybind;

	public List<ItemFilter> Filters = [];

	/// <summary>
	///		Index into <see cref="Filters"/> of the active filter, or -1 if none.
	///		With no active filter, all picked-up items go to the matched bucket.
	/// </summary>
	public int ActiveFilterIndex = -1;

	public ItemFilter ActiveFilter =>
		ActiveFilterIndex >= 0 && ActiveFilterIndex < Filters.Count ? Filters[ActiveFilterIndex] : null;

	public override void Load()
	{
		if (!Main.dedServ)
		{
			FilterEditorKeybind = KeybindLoader.RegisterKeybind(Mod, "ItemFilterHotkey", Microsoft.Xna.Framework.Input.Keys.F8);
		}
	}

	public override void ProcessTriggers(TriggersSet triggersSet)
	{
		if (FilterEditorKeybind.JustPressed)
		{
			UIManager.TryToggleOrRegister(ItemFilterUIState.Identifier, "Vanilla: Mouse Text", new ItemFilterUIState(), 0, InterfaceScaleType.UI);

			SoundEngine.PlaySound(UIManager.TryGet(ItemFilterUIState.Identifier, out UIManager.UIStateData data) && data.Enabled
				? SoundID.MenuOpen
				: SoundID.MenuClose);
		}
	}

	public override void SaveData(TagCompound tag)
	{
		tag["activeIndex"] = ActiveFilterIndex;
		tag["filters"] = Filters.Select(f => f.Save()).ToList();
	}

	public override void LoadData(TagCompound tag)
	{
		Filters.Clear();
		ActiveFilterIndex = tag.TryGet("activeIndex", out int active) ? active : -1;

		if (tag.GetList<TagCompound>("filters") is { } list)
		{
			foreach (TagCompound entry in list)
			{
				Filters.Add(ItemFilter.Load(entry));
			}
		}

		if (ActiveFilterIndex >= Filters.Count)
		{
			ActiveFilterIndex = -1;
		}
	}

	public override void Unload()
	{
		UIManager.TryDisable(ItemFilterUIState.Identifier);
	}
}
