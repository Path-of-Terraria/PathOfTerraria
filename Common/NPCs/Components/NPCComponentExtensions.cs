using Terraria.ID;

namespace PathOfTerraria.Common.NPCs.Components;

#nullable enable

// This file is placed outside of the common 'Common/Utilities/Extensions' scope for the sake
// of convenience when using NPC components.
public static class NPCComponentExtensions
{
	/// <summary>
	/// Tries to enable the given <typeparamref name="T"/> component on the current entity.
	/// </summary>
	public static bool TryEnableComponent<T>(this NPC npc, Action<T>? initializer = null) where T : NPCComponent
	{
		// Ensure that the component exists in this game instance (is not clientside while we are a server).
		if (ModContent.GetInstance<T>() == null || !npc.TryGetGlobalNPC(out T component))
		{
			return false;
		}

		component.Enabled = true;
		
		initializer?.Invoke(component);

		return true;
	}
}