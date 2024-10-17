using Terraria.ID;

namespace PathOfTerraria.Common.NPCs.Components;

// This file is placed outside of the common 'Common/Utilities/Extensions' scope for the sake
// of convenience when using NPC components.
public static class NPCComponentExtensions
{
	/// <summary>
	/// Tries to enable the given <typeparamref name="T"/> component on the current entity. This will not run on servers.
	/// </summary>
	/// <typeparam name="T">The component to enable.</typeparam>
	/// <param name="npc">The NPC to enable the component on.</param>
	/// <param name="initializer">Optional initialization, such as setting parameters, for the component.</param>
	/// <returns>Whether the component was successfully enabled.</returns>
	public static bool TryEnableComponent<T>(this NPC npc, Action<T>? initializer = null) where T : NPCComponent
	{
		if (Main.netMode == NetmodeID.Server)
		{
			return false;
		}

		if (!npc.TryGetGlobalNPC(out T? component))
		{
			return false;
		}

		component.Enabled = true;
		
		initializer?.Invoke(component);

		return true;
	}
}