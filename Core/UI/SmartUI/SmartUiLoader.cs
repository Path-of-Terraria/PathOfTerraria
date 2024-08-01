using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using PathOfTerraria.Common.UI.Utilities;
using Terraria.ModLoader.Core;
using Terraria.UI;

namespace PathOfTerraria.Core.UI.SmartUI;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
internal sealed class UiLoader : ModSystem
{
	/// <summary>
	/// The collection of automatically craetaed UserInterfaces for SmartUIStates.
	/// </summary>
	private static List<UserInterface> userInterfaces = [];

	/// <summary>
	/// The collection of all automatically loaded SmartUIStates.
	/// </summary>
	private static List<SmartUiState> uiStates = [];

	/// <summary>
	/// Uses reflection to scan through and find all types extending SmartUIState that arent abstract, and loads an instance of them.
	/// </summary>
	public override void Load()
	{
		if (Main.dedServ)
		{
			return;
		}

		userInterfaces = [];
		uiStates = [];

		foreach (Type t in AssemblyManager.GetLoadableTypes(Mod.Code))
		{
			if (!t.IsAbstract && t.IsSubclassOf(typeof(SmartUiState)))
			{
				var state = (SmartUiState)Activator.CreateInstance(t, null);
				uiStates?.Add(state);
			}
		}

		var comp = new Comparison<SmartUiState>((s1, s2) => s2.DepthPriority - s1.DepthPriority);
		uiStates.Sort(comp);

		for (int k = 0; k < uiStates.Count; k++)
		{
			SmartUiState state = uiStates[k];
			var userInterface = new UserInterface();
			userInterface.SetState(state);
			state.UserInterface = userInterface;
			userInterfaces?.Add(userInterface);
		}

		Main.OnResolutionChanged += UpdateUIStateForResolutionChange;
	}

	private void UpdateUIStateForResolutionChange(Vector2 obj)
	{
		foreach (SmartUiState item in uiStates)
		{
			item.Recalculate();
		}
	}

	public override void Unload()
	{
		uiStates.ForEach(n => n.Unload());
		userInterfaces = null;
		uiStates = null;
	}

	/// <summary>
	/// Helper method for creating and inserting a LegacyGameInterfaceLayer automatically
	/// </summary>
	/// <param name="layers">The vanilla layers</param>
	/// <param name="state">the UIState to bind to the layer</param>
	/// <param name="index">Where this layer should be inserted</param>
	/// <param name="visible">The logic dictating the visibility of this layer</param>
	/// <param name="scale">The scale settings this layer should scale with</param>
	public static void AddLayer(List<GameInterfaceLayer> layers, UIState state, int index, bool visible, InterfaceScaleType scale)
	{
		string name = state == null ? "Unknown" : state.ToString();
		layers.Insert(index + 1, new LegacyGameInterfaceLayer("BrickAndMortar: " + name,
			delegate
			{
				if (visible)
				{
					state.Draw(Main.spriteBatch);
				}

				return true;
			}, scale));
	}

	private SmartUiElement GetSmartUiParent(UIElement e)
	{
		if (e is null)
		{
			return null;
		}

		if (e is SmartUiElement es && e.Parent is not SmartUiElement)
		{
			return es;
		}

		return GetSmartUiParent(e.Parent);
	}

	private bool _mouseState = false;
	private UIState _blockAt = null;
	private SmartUiElement _blockAtE = null;

	/// <summary>
	/// Handles updating the UI states correctly
	/// </summary>
	/// <param name="gameTime"></param>
	public override void UpdateUI(GameTime gameTime)
	{
		foreach (UserInterface eachState in userInterfaces)
		{
			if (eachState?.CurrentState != null && ((SmartUiState)eachState.CurrentState).Visible)
			{
				eachState.Update(gameTime);

				UIElement e = eachState.CurrentState.GetElementAt(Main.MouseScreen);
				SmartUiElement se = GetSmartUiParent(e);

				if (_blockAt == eachState.CurrentState || e is not null && e is not UIState)
				{
					Main.blockMouse = true;
					BlockClickItem.Block = true;
					_blockAt = eachState.CurrentState;

					if (se is not null)
					{
						_blockAtE ??= se;

						_blockAtE.MouseContained.Right = Main.mouseRight;
						_blockAtE.MouseContained.Left = Main.mouseLeft;
					}
				}
			}
		}

		_mouseState = Main.mouseLeft || Main.mouseRight;

		if (!_mouseState)
		{
			_blockAt = null;

			if (_blockAtE is not null)
			{
				_blockAtE.MouseContained = new();
			}

			_blockAtE = null;
		}
	}

	/// <summary>
	/// Gets the autoloaded SmartUIState instance for a given SmartUIState subclass
	/// </summary>
	/// <typeparam name="T">The SmartUIState subclass to get the instance of</typeparam>
	/// <returns>The autoloaded instance of the desired SmartUIState</returns>
	public static T GetUIState<T>() where T : SmartUiState
	{
		return uiStates.FirstOrDefault(n => n is T) as T;
	}

	/// <summary>
	/// Forcibly reloads a SmartUIState and it's associated UserInterface
	/// </summary>
	/// <typeparam name="T">The SmartUIState subclass to reload</typeparam>
	public static void ReloadState<T>() where T : SmartUiState
	{
		int index = uiStates.IndexOf(GetUIState<T>());
		uiStates[index] = (T)Activator.CreateInstance(typeof(T), null);
		userInterfaces[index] = new UserInterface();
		userInterfaces[index].SetState(uiStates[index]);
	}

	/// <summary>
	/// Handles the insertion of the automatically generated UIs
	/// </summary>
	/// <param name="layers"></param>
	public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
	{
		for (int k = 0; k < uiStates.Count; k++)
		{
			SmartUiState state = uiStates[k];
			AddLayer(layers, state, state.InsertionIndex(layers), state.Visible, state.Scale);
		}
	}
}