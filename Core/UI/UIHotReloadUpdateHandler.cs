using System.Reflection;
using System.Reflection.Metadata;
using PathOfTerraria.Core.UI;
using Terraria.UI;

[assembly: MetadataUpdateHandler(typeof(UIHotReloadUpdateHandler))]

namespace PathOfTerraria.Core.UI;

internal static class UIHotReloadUpdateHandler
{        
	// This method is required for the handler to work, despite not doing anything.
	internal static void ClearCache(Type[]? updatedTypes) { }
	
	internal static void UpdateApplication(Type[]? updatedTypes)
	{	
		Main.QueueMainThreadAction(
			() =>
			{
				foreach (Type type in updatedTypes)
				{
					if (!typeof(UIState).IsAssignableFrom(type))
					{
						continue;
					}
					
					MethodInfo? methodInfo = typeof(UIManager).GetMethod("RefreshStates", BindingFlags.NonPublic | BindingFlags.Static);
					MethodInfo? generatedMethodInfo = methodInfo?.MakeGenericMethod(type);
					
					generatedMethodInfo?.Invoke(null, null);
				}
			}
		);
	}
}