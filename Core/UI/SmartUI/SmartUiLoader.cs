using System.Collections.Generic;
using PathOfTerraria.Common.UI.Utilities;
using PathOfTerraria.Utilities;
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

	private UIState stateToBlockAt;
	private SmartUiElement elementToBlockAt;

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

			SmartUiState state = (SmartUiState)userInterface.CurrentState;
			UIElement hoveredElement = state.GetElementAt(Main.MouseScreen);
			bool shouldBlockClickThrough = state.ShouldBlockClickThrough(Main.MouseScreen);

			if (hoveredElement is null && !shouldBlockClickThrough)
			{
				continue;
			}

			SmartUiElement parent = DeriveSmartUiElement(hoveredElement);
			if (stateToBlockAt != state && hoveredElement is UIState && !shouldBlockClickThrough)
			{
				continue;
			}

			Main.blockMouse = true;
			Main.isMouseLeftConsumedByUI = true;
			Main.LocalPlayer.mouseInterface = true;
			BlockClickItem.Block = true;
			stateToBlockAt = state;

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
							// Temporarily disable the click blocking, so that it does not interfere with Smart UIs themselves.
							using var _ = ValueOverride.Create(ref BlockClickItem.Block, false);
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

		foreach (UIManager.UIStateData data in UIManager.Data)
		{
			if (data.Enabled && data.Value is IMutuallyExclusiveUI toggle)
			{
				toggle.Toggle();
			}
		}
	}
}
