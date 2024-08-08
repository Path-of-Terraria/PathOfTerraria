using Terraria.UI;

namespace PathOfTerraria.Core.UI;

[Autoload(Side = ModSide.Client)]
public sealed partial class UIManager : ModSystem
{
	/// <summary>
	///		Reloads all registered <see cref="UIState"/> instances by its type.
	/// </summary>
	/// <typeparam name="T">The type of the <see cref="UIState"/> instances.</typeparam>
	internal static void RefreshStates<T>() where T : UIState
	{
		for (int i = 0; i < UITypeData<T>.Data.Count; i++)
		{
			UIStateData<T> data = UITypeData<T>.Data[i];

			if (data.Value == null)
			{
				continue;
			}
			
			data.Value.RemoveAllChildren();
			
			data.Value.OnActivate();
			data.Value.OnInitialize();
			
			data.UserInterface?.SetState(null);
			data.UserInterface?.SetState(data.Value);
		}	
	}
}