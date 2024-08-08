using Terraria.UI;

namespace PathOfTerraria.Core.UI;

[Autoload(Side = ModSide.Client)]
public sealed partial class UIManager : ModSystem
{
	/// <summary>
	///		Refreshes all registered <see cref="UIState"/> instances by their type.
	/// </summary>
	/// <typeparam name="T">The type of the <see cref="UIState"/> instances.</typeparam>
	internal static void RefreshAllStates<T>() where T : UIState
	{
		for (int i = 0; i < UITypeData<T>.Data.Count; i++)
		{
			UIManager.TryRefresh<T>(UITypeData<T>.Data[i].Identifier);
		}	
	}
}