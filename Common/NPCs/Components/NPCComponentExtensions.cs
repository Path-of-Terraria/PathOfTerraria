namespace PathOfTerraria.Common.NPCs.Components;

// This file is placed outside of the common 'Common/Utilities/Extensions' scope for the sake
// of convenience when using NPC components.
public static class NPCComponentExtensions
{
	public static bool TryEnableComponent<T>(this NPC npc, Action<T>? initializer = null) where T : NPCComponent
	{
		if (!npc.TryGetGlobalNPC(out T? component))
		{
			return false;
		}

		component.Enabled = true;
		
		initializer?.Invoke(component);

		return true;
	}
}