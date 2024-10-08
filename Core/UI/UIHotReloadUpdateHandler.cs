using System.Reflection;
using System.Reflection.Metadata;
using PathOfTerraria.Core.UI;
using Terraria.UI;

[assembly: MetadataUpdateHandler(typeof(UIHotReloadUpdateHandler))]

namespace PathOfTerraria.Core.UI;

internal static class UIHotReloadUpdateHandler
{        
	internal static void ClearCache(Type[]? types) { }
	
	internal static void UpdateApplication(Type[]? updatedTypes)
	{	
		Main.QueueMainThreadAction(
			() =>
			{
				foreach (Type type in updatedTypes)
				{
					if (!typeof(UIState).IsAssignableFrom(type) || !typeof(UIElement).IsAssignableFrom(type))
					{
						continue;
					}
					
					UIManager.RefreshAllStates();
				}
			}
		);
	}
}