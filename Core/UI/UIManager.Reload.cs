using Terraria.UI;

namespace PathOfTerraria.Core.UI;

[Autoload(Side = ModSide.Client)]
public sealed partial class UIManager : ModSystem
{
	/// <summary>
	///		Attempts to refresh a <see cref="UIState"/> instance.
	/// </summary>
	/// <param name="identifier">The identifier of the <see cref="UIState"/>.</param>
	/// <returns><c>true</c> if the state was successfully refreshed; otherwise, <c>false</c>.</returns>
	internal static bool TryRefresh(string identifier)
	{
		int index = Data.FindIndex(s => s.Identifier == identifier);

		if (index < 0)
		{
			return false;
		}

		UIStateData data = Data[index];
		
		data.Value.RemoveAllChildren();
			
		data.Value.OnActivate();
		data.Value.OnInitialize();
			
		data.UserInterface.SetState(null);
		data.UserInterface.SetState(data.Value);

		return true;
	}
	
	/// <summary>
	///		Refreshes all registered <see cref="UIState"/> instances by their type.
	/// </summary>
	internal static void RefreshAllStates()
	{
		for (int i = 0; i < Data.Count; i++)
		{
			UIManager.TryRefresh(Data[i].Identifier);
		}	
	}
}