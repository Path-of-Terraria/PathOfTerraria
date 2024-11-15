using System.Reflection.Metadata;
using PathOfTerraria.Core.UI;
using Terraria.UI;

[assembly: MetadataUpdateHandler(typeof(UIHotReloadUpdateHandler))]

namespace PathOfTerraria.Core.UI;

internal static class UIHotReloadUpdateHandler
{
	// This method is required for the handler to work, despite not doing anything.
#pragma warning disable IDE0060 // Remove unused parameter
	internal static void ClearCache(Type[]? updatedTypes) { }
#pragma warning restore IDE0060 // Remove unused parameter

	internal static void UpdateApplication(Type[]? updatedTypes)
	{
		Main.QueueMainThreadAction(
			() =>
			{
				foreach (Type type in updatedTypes)
				{
					if (typeof(UIState).IsAssignableFrom(type) || typeof(UIElement).IsAssignableFrom(type))
					{
						UIManager.RefreshAllStates();
					}
				}
			}
		);
	}
}