using log4net.Core;
using PathOfTerraria.Core.Items.Hooks;
using PathOfTerraria.Core.Systems;
using PathOfTerraria.Core.Systems.Affixes;
using System.Collections.Generic;
using Terraria.ModLoader.Core;
using Terraria.UI;

namespace PathOfTerraria.Core.Items;

public sealed class ExtraRolls : ILoadable
{
	public interface IItem
	{
		void ExtraRolls();
	}

	public interface IGlobal
	{
		void ExtraRolls(Item item);
	}

	private static GlobalHookList<GlobalItem> _hook = ItemLoader.AddModHook(GlobalHookList<GlobalItem>.Create(x => ((IGlobal)x).ExtraRolls));

	public static void Invoke(Item item)
	{
		if (item.ModItem is IItem i)
		{
			i.ExtraRolls();
		}

		foreach (GlobalItem g in _hook.Enumerate(item))
		{
			if (g is not IGlobal gl)
			{
				continue;
			}

			gl.ExtraRolls(item);
		}
	}


	void ILoadable.Load(Mod mod) { }

	void ILoadable.Unload()
	{
		_hook = null;
	}
}

public sealed class CopyToClipboard : ILoadable
{
	private sealed class Impl : ILoadable
	{
		private ModKeybind _copyItemKeybind;

		void ILoadable.Load(Mod mod)
		{
			_copyItemKeybind = KeybindLoader.RegisterKeybind(mod, "CopyItemInfo", "I");
			On_ItemSlot.LeftClick_ItemArray_int_int += AddKeybindPressEvent;
		}

		void ILoadable.Unload()
		{
			_copyItemKeybind = null;
		}

		private void AddKeybindPressEvent(On_ItemSlot.orig_LeftClick_ItemArray_int_int orig, Item[] inv, int context, int slot)
		{
			Item item = inv[slot];

			if (_copyItemKeybind.JustPressed && item.active)
			{
				Invoke(item);
			}

			orig(inv, context, slot);
		}
	}

	public interface IItem
	{
		void CopyToClipboard();
	}

	public interface IGlobal
	{
		void CopyToClipboard(Item item);
	}

	private static GlobalHookList<GlobalItem> _hook = ItemLoader.AddModHook(GlobalHookList<GlobalItem>.Create(x => ((IGlobal)x).CopyToClipboard));

	public static void Invoke(Item item)
	{
		/*
	void CopyToClipboard(Item item)
	{
		if (item.ModItem is null)
		{
			return;
		}

		if (!Keyboard.GetState().PressingShift())
		{
			TagCompound tag = [];

			item.ModItem.SaveData(tag);

			ClipboardService.SetText(StringTagRelation.FromTag(tag));
		}
#if DEBUG
		else
		{
			TagCompound tag = [];

			item.ModItem.SaveData(tag);

			item.ModItem.LoadData(StringTagRelation.FromString(ClipboardService.GetText(), tag));
		}
	}
#endif
		 */
		if (item.ModItem is IItem i)
		{
			i.CopyToClipboard();
		}

		foreach (GlobalItem g in _hook.Enumerate(item))
		{
			if (g is not IGlobal gl)
			{
				continue;
			}

			gl.CopyToClipboard(item);
		}
	}

	void ILoadable.Load(Mod mod) { }

	void ILoadable.Unload()
	{
		_hook = null;
	}
}

public sealed class GenerateAffixes : ILoadable
{
	public interface IItem
	{
		List<ItemAffix> GenerateAffixes();
	}

	public interface IGlobal
	{
		bool ModifyAffixes(Item item, List<ItemAffix> affixes);
	}

	private static GlobalHookList<GlobalItem> _hook = ItemLoader.AddModHook(GlobalHookList<GlobalItem>.Create(x => ((IGlobal)x).ModifyAffixes));

	public static List<ItemAffix> Invoke(Item item)
	{
		List<ItemAffix> affixes = [];

		if (item.ModItem is IItem i)
		{
			affixes = i.GenerateAffixes();
		}

		foreach (GlobalItem g in _hook.Enumerate(item))
		{
			if (g is not IGlobal gl)
			{
				continue;
			}

			gl.ModifyAffixes(item, affixes);
		}

		return affixes;
	}

	void ILoadable.Load(Mod mod) { }

	void ILoadable.Unload()
	{
		_hook = null;
	}
}

public sealed class GenerateImplicits : ILoadable
{
	public interface IItem
	{
		List<ItemAffix> GenerateImplicits();
	}

	public interface IGlobal
	{
		void ModifyImplicits(Item item, List<ItemAffix> implicits);
	}

	private static GlobalHookList<GlobalItem> _hook = ItemLoader.AddModHook(GlobalHookList<GlobalItem>.Create(x => ((IGlobal)x).ModifyImplicits));

	public static List<ItemAffix> Invoke(Item item)
	{
		List<ItemAffix> implicits = [];

		if (item.ModItem is IItem i)
		{
			implicits = i.GenerateImplicits();
		}

		foreach (GlobalItem g in _hook.Enumerate(item))
		{
			if (g is not IGlobal gl)
			{
				continue;
			}

			gl.ModifyImplicits(item, implicits);
		}

		return implicits;
	}

	void ILoadable.Load(Mod mod) { }

	void ILoadable.Unload()
	{
		_hook = null;
	}
}

public sealed class GenerateName : ILoadable
{
	public interface IItem
	{
		string GenerateName();
	}

	public interface IGlobal
	{
		void ModifyName(Item item, ref string name);
	}

	private static GlobalHookList<GlobalItem> _hook = ItemLoader.AddModHook(GlobalHookList<GlobalItem>.Create(x => ((IGlobal)x).ModifyName));

	public static string Invoke(Item item)
	{
		string name = GetDefaultName(item);

		if (item.ModItem is IItem i)
		{
			name = i.GenerateName();
		}

		foreach (GlobalItem g in _hook.Enumerate(item))
		{
			if (g is not IGlobal gl)
			{
				continue;
			}

			gl.ModifyName(item, ref name);
		}

		return name;
	}

	public static string GetDefaultName(Item item)
	{
		return item.GetInstanceData().Rarity switch
		{
			Rarity.Normal => item.Name,
			Rarity.Magic => $"{GeneratePrefix.Invoke(item)} {item.Name}",
			Rarity.Rare => $"{GeneratePrefix.Invoke(item)} {item.Name} {GenerateSuffix.Invoke(item)}",
			Rarity.Unique => item.Name, // uniques might just want to override the GenerateName function
			_ => "Unknown Item"
		};
	}

	void ILoadable.Load(Mod mod) { }

	void ILoadable.Unload()
	{
		_hook = null;
	}
}

public sealed class GeneratePrefix : ILoadable
{
	public interface IItem
	{
		string GeneratePrefix();
	}

	public interface IGlobal
	{
		void ModifyPrefix(Item item, ref string prefix);
	}

	private static GlobalHookList<GlobalItem> _hook = ItemLoader.AddModHook(GlobalHookList<GlobalItem>.Create(x => ((IGlobal)x).ModifyPrefix));

	public static string Invoke(Item item)
	{
		string prefix = GetDefaultPrefix();

		if (item.ModItem is IItem i)
		{
			prefix = i.GeneratePrefix();
		}

		foreach (GlobalItem g in _hook.Enumerate(item))
		{
			if (g is not IGlobal gl)
			{
				continue;
			}

			gl.ModifyPrefix(item, ref prefix);
		}

		return prefix;
	}

	public static string GetDefaultPrefix()
	{
		return "";
	}

	void ILoadable.Load(Mod mod) { }

	void ILoadable.Unload()
	{
		_hook = null;
	}
}

public sealed class GenerateSuffix : ILoadable
{
	public interface IItem
	{
		string GenerateSuffix();
	}

	public interface IGlobal
	{
		void ModifySuffix(Item item, ref string suffix);
	}

	private static GlobalHookList<GlobalItem> _hook = ItemLoader.AddModHook(GlobalHookList<GlobalItem>.Create(x => ((IGlobal)x).ModifySuffix));

	public static string Invoke(Item item)
	{
		string suffix = GetDefaultSuffix();

		if (item.ModItem is IItem i)
		{
			suffix = i.GenerateSuffix();
		}

		foreach (GlobalItem g in _hook.Enumerate(item))
		{
			if (g is not IGlobal gl)
			{
				continue;
			}

			gl.ModifySuffix(item, ref suffix);
		}

		return suffix;
	}

	public static string GetDefaultSuffix()
	{
		return "";
	}

	void ILoadable.Load(Mod mod) { }

	void ILoadable.Unload()
	{
		_hook = null;
	}
}

public sealed class InsertAdditionalTooltipLines : ILoadable
{
	public interface IItem
	{
		void InsertAdditionalTooltipLines(List<TooltipLine> tooltips, EntityModifier thisItemModifier);
	}

	public interface IGlobal
	{
		void InsertAdditionalTooltipLines(Item item, List<TooltipLine> tooltips, EntityModifier thisItemModifier);
	}

	private static GlobalHookList<GlobalItem> _hook = ItemLoader.AddModHook(GlobalHookList<GlobalItem>.Create(x => ((IGlobal)x).InsertAdditionalTooltipLines));

	public static void Invoke(Item item, List<TooltipLine> tooltips, EntityModifier thisItemModifier)
	{
		if (item.ModItem is IItem i)
		{
			i.InsertAdditionalTooltipLines(tooltips, thisItemModifier);
		}

		foreach (GlobalItem g in _hook.Enumerate(item))
		{
			if (g is not IGlobal gl)
			{
				continue;
			}

			gl.InsertAdditionalTooltipLines(item, tooltips, thisItemModifier);
		}
	}

	void ILoadable.Load(Mod mod) { }

	void ILoadable.Unload()
	{
		_hook = null;
	}
}

public sealed class GetItemLevel : ILoadable
{
	public interface IItem
	{
		int GetItemLevel(int realLevel);
	}

	public interface IGlobal
	{
		void GetItemLevel(Item item, int realLevel, ref int level);
	}

	private static GlobalHookList<GlobalItem> _hook = ItemLoader.AddModHook(GlobalHookList<GlobalItem>.Create(x => ((IGlobal)x).HookName));

	public static int Invoke(Item item)
	{
		int realLevel = item.GetInstanceData().RealLevel;
		int level = realLevel;

		if (item.ModItem is IItem i)
		{
			level = i.GetItemLevel(realLevel);
		}

		foreach (GlobalItem g in _hook.Enumerate(item))
		{
			if (g is not IGlobal gl)
			{
				continue;
			}

			gl.GetItemLevel(item, realLevel, ref level);
		}

		return level;
	}

	void ILoadable.Load(Mod mod) { }

	void ILoadable.Unload()
	{
		_hook = null;
	}
}

public sealed class SetItemLevel : ILoadable
{
	public interface IItem
	{
		void SetItemLevel(int level, ref int realLevel);
	}

	public interface IGlobal
	{
		void SetItemLevel(Item item, int level, ref int realLevel);
	}

	private static GlobalHookList<GlobalItem> _hook = ItemLoader.AddModHook(GlobalHookList<GlobalItem>.Create(x => ((IGlobal)x).HookName));

	public static void Invoke(Item item, int level)
	{
		int realLevel = item.GetInstanceData().RealLevel;

		if (item.ModItem is IItem i)
		{
			i.SetItemLevel(level, ref realLevel);
		}

		foreach (GlobalItem g in _hook.Enumerate(item))
		{
			if (g is not IGlobal gl)
			{
				continue;
			}

			gl.SetItemLevel(item, level, ref realLevel);
		}

		item.GetInstanceData().RealLevel = realLevel;
	}

	void ILoadable.Load(Mod mod) { }

	void ILoadable.Unload()
	{
		_hook = null;
	}
}

public sealed class PostRoll : ILoadable
{
	public interface IItem
	{
		void PostRoll();
	}

	public interface IGlobal
	{
		void PostRoll(Item item);
	}

	private static GlobalHookList<GlobalItem> _hook = ItemLoader.AddModHook(GlobalHookList<GlobalItem>.Create(x => ((IGlobal)x).PostRoll));

	public static void Invoke(Item item)
	{
		if (item.ModItem is IItem i)
		{
			i.PostRoll();
		}

		foreach (GlobalItem g in _hook.Enumerate(item))
		{
			if (g is not IGlobal gl)
			{
				continue;
			}

			gl.PostRoll(item);
		}
	}

	void ILoadable.Load(Mod mod) { }

	void ILoadable.Unload()
	{
		_hook = null;
	}
}

public sealed class SwapItemModifiers : ILoadable
{
	public interface IItem
	{
		void SwapItemModifiers(EntityModifier swapItemModifier);
	}

	public interface IGlobal
	{
		void SwapItemModifiers(Item item, EntityModifier swapItemModifier);
	}

	private static GlobalHookList<GlobalItem> _hook = ItemLoader.AddModHook(GlobalHookList<GlobalItem>.Create(x => ((IGlobal)x).SwapItemModifiers));

	public static void Invoke(Item item, EntityModifier swapItemModifier)
	{
		if (item.ModItem is IItem i)
		{
			i.SwapItemModifiers(swapItemModifier);
		}

		foreach (GlobalItem g in _hook.Enumerate(item))
		{
			if (g is not IGlobal gl)
			{
				continue;
			}

			gl.SwapItemModifiers(item, swapItemModifier);
		}
	}

	void ILoadable.Load(Mod mod) { }

	void ILoadable.Unload()
	{
		_hook = null;
	}
}
