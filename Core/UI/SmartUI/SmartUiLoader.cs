using System.Collections.Generic;
using PathOfTerraria.Common.UI.Utilities;
using Terraria.UI;

namespace PathOfTerraria.Core.UI.SmartUI;

/// <summary>
///		Automatically loads <see cref="SmartUiState"/> singletons.
/// </summary>
[Autoload(Side = ModSide.Client)]
internal sealed partial class SmartUiLoader : ModSystem
{
	private readonly List<UserInterface> userInterfaces = [];
	private readonly List<SmartUiState> uiStates = [];

	private UIState? stateToBlockAt;
	private SmartUiElement? elementToBlockAt;

	public override void ClearWorld()
	{
		foreach (SmartUiState state in uiStates)
		{
			state.NeedsRecalculation = true;
		}
	}

	public override void UpdateUI(GameTime gameTime)
	{
		// Update the user interfaces.
		foreach (UserInterface userInterface in userInterfaces)
		{
			if (userInterface.CurrentState is not SmartUiState { Visible: true })
			{
				continue;
			}

			userInterface.Update(gameTime);
		}

		foreach (UserInterface userInterface in userInterfaces)
		{
			if (userInterface.CurrentState is not SmartUiState { Visible: true })
			{
				continue;
			}

			UIElement? hoveredElement = userInterface.CurrentState.GetElementAt(Main.MouseScreen);
			if (hoveredElement is null)
			{
				continue;
			}

			SmartUiElement? parent = DeriveSmartUiElement(hoveredElement);
			if (stateToBlockAt != userInterface.CurrentState && hoveredElement is UIState)
			{
				continue;
			}

			Main.blockMouse = true;
			BlockClickItem.Block = true;
			stateToBlockAt = userInterface.CurrentState;

			if (parent is null)
			{
				continue;
			}

			elementToBlockAt ??= parent;
			elementToBlockAt.MouseContained.Right = Main.mouseRight;
			elementToBlockAt.MouseContained.Left = Main.mouseLeft;
		}

		if (Main.mouseLeft || Main.mouseRight)
		{
			return;
		}

		if (elementToBlockAt is not null)
		{
			elementToBlockAt.MouseContained = new MouseContainedState();
		}

		stateToBlockAt = null;
		elementToBlockAt = null;
	}

	public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
	{
		foreach (SmartUiState state in uiStates)
		{
			layers.Insert(
				state.InsertionIndex(layers) + 1,
				new LegacyGameInterfaceLayer(
					$"{Mod.Name}: {state.ToString() ?? "<unknown name; null>"}",
					() =>
					{
						if (state.Visible)
						{
							state.Draw(Main.spriteBatch);
						}

						return true;
					},
					state.Scale
				)
			);
		}
	}

	/// <summary>
	/// Clears all <see cref="IMutuallyExclusiveUI"/> that aren't of type <typeparamref name="T"/>.
	/// </summary>
	/// <typeparam name="T">The type to NOT call Toggle on.</typeparam>
	public void ClearMutuallyExclusive<T>() where T : IMutuallyExclusiveUI
	{
		foreach (SmartUiState ui in uiStates)
		{
			if (ui is not T && ui.Visible && ui is IMutuallyExclusiveUI toggle)
			{
				toggle.Toggle();
			}
		}
	}
}