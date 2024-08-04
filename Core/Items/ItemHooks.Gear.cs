using Terraria.ModLoader.Core;

namespace PathOfTerraria.Core.Items;

public sealed class GearLocalizationCategory : ILoadable
{
	public interface IItem
	{
		string GetGearLocalizationCategory(string defaultCategory);
	}

	public interface IGlobal
	{
		void ModifyGearLocalizationCategory(Item item, ref string defaultCategory);
	}

	private static GlobalHookList<GlobalItem> _hook = ItemLoader.AddModHook(GlobalHookList<GlobalItem>.Create(x => ((IGlobal)x).ModifyGearLocalizationCategory));

	public static string Invoke(Item item)
	{
		string gearLocalizationCategory = item.ModItem?.GetType().Name ?? "";

		if (item.ModItem is IItem i)
		{
			gearLocalizationCategory = i.GetGearLocalizationCategory(gearLocalizationCategory);
		}

		foreach (GlobalItem g in _hook.Enumerate(item))
		{
			if (g is not IGlobal gl)
			{
				continue;
			}

			gl.ModifyGearLocalizationCategory(item, ref gearLocalizationCategory);
		}

		return gearLocalizationCategory;
	}

	void ILoadable.Load(Mod mod) { }

	void ILoadable.Unload()
	{
		_hook = null;
	}
}