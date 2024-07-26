using Terraria.ModLoader.Core;

namespace PathOfTerraria.Core.Items.Hooks;

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

	private static GlobalHookList<GlobalItem> _hook = ItemLoader.AddModHook(GlobalHookList<GlobalItem>.Create(x => ((IGlobal)x).ExtraRolls));

	void ILoadable.Load(Mod mod) { }

	void ILoadable.Unload()
	{
		_hook = null;
	}
}
