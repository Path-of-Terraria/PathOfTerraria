using Terraria.ModLoader.Core;
using Terraria.UI;

namespace PathOfTerraria.Core.UI.SmartUI;

internal sealed partial class SmartUiLoader
{
	public override void Load()
	{
		foreach (Type type in AssemblyManager.GetLoadableTypes(Mod.Code))
		{
			if (type.IsAbstract || !type.IsSubclassOf(typeof(SmartUiState)))
			{
				continue;
			}

			if (Activator.CreateInstance(type, null) is not SmartUiState state)
			{
				// Unreachable.
				throw new InvalidOperationException($"Attempted to initialize non-{nameof(SmartUiState)}!?");
			}

			uiStates.Add(state);
		}

		var comp = new Comparison<SmartUiState>((s1, s2) => s2.DepthPriority - s1.DepthPriority);
		uiStates.Sort(comp);

		foreach (SmartUiState state in uiStates)
		{
			var userInterface = new UserInterface();
			userInterface.SetState(state);
			state.UserInterface = userInterface;
			userInterfaces.Add(userInterface);
		}

		Main.OnResolutionChanged += UpdateUiStateForResolutionChange;
	}

	public override void Unload()
	{
		foreach (SmartUiState n in uiStates)
		{
			n.Unload();
		}

		userInterfaces.Clear();
		uiStates.Clear();

		Main.OnResolutionChanged -= UpdateUiStateForResolutionChange;
	}

	private void UpdateUiStateForResolutionChange(Vector2 obj)
	{
		foreach (SmartUiState item in uiStates)
		{
			item.Recalculate();
		}
	}
}