using Terraria.UI;

namespace PathOfTerraria.Core.UI;

[Autoload(Side = ModSide.Client)]
public sealed partial class UIManager : ModSystem
{
	/// <summary>
	///		Attempts to refresh a <see cref="UIState"/> instance.
	/// </summary>
	/// <param name="identifier">The identifier of the <see cref="UIState"/>.</param>
	/// <typeparam name="T">The type of the <see cref="UIState"/>.</typeparam>
	/// <returns><c>true</c> if the state was successfully refreshed; otherwise, <c>false</c>.</returns>
	internal static bool TryRefresh<T>(string identifier) where T : UIState
	{
		int index = UITypeData<T>.Data.FindIndex(s => s.Identifier == identifier);

		if (index < 0)
		{
			return false;
		}

		UIStateData<T> data = UITypeData<T>.Data[index];
		
		data.Value?.RemoveAllChildren();
			
		data.Value?.OnActivate();
		data.Value?.OnInitialize();
			
		data.UserInterface?.SetState(null);
		data.UserInterface?.SetState(data.Value);

		return true;
	}
	
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