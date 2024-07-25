using System.Collections.Generic;
using System.Linq;
using PathOfTerraria.Common.UI.Utilities;
using Terraria.UI;

namespace PathOfTerraria.Common.Loaders.UILoading;

/// <summary>
/// Automatically loads SmartUIStates ala IoC.
/// </summary>
class UILoader : ModSystem
{
	/// <summary>
	/// The collection of automatically craetaed UserInterfaces for SmartUIStates.
	/// </summary>
	public static List<UserInterface> UserInterfaces = [];

	/// <summary>
	/// The collection of all automatically loaded SmartUIStates.
	/// </summary>
	public static List<SmartUIState> UIStates = [];

	/// <summary>
	/// Uses reflection to scan through and find all types extending SmartUIState that arent abstract, and loads an instance of them.
	/// </summary>
	public override void Load()
	{
		if (Main.dedServ)
		{
			return;
		}

		UserInterfaces = [];
		UIStates = [];

		foreach (Type t in Mod.Code.GetTypes())
		{
			if (!t.IsAbstract && t.IsSubclassOf(typeof(SmartUIState)))
			{
				var state = (SmartUIState)Activator.CreateInstance(t, null);
				UIStates?.Add(state);
			}
		}

		Comparison<SmartUIState> comp = new Comparison<SmartUIState>((s1, s2) => s2.DepthPriority - s1.DepthPriority);
		UIStates.Sort(comp);

		for (int k = 0; k < UIStates.Count; k++)
		{
			SmartUIState state = UIStates[k];
			var userInterface = new UserInterface();
			userInterface.SetState(state);
			state.UserInterface = userInterface;
			UserInterfaces?.Add(userInterface);
		}

		Main.OnResolutionChanged += UpdateUIStateForResolutionChange;
	}

	private void UpdateUIStateForResolutionChange(Vector2 obj)
	{
		foreach (SmartUIState item in UIStates)
		{
			item.Recalculate();
		}
	}

	public override void Unload()
	{
		UIStates.ForEach(n => n.Unload());
		UserInterfaces = null;
		UIStates = null;
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

	private SmartUIElement GetSmartUiParent(UIElement e)
	{
		if (e is null)
		{
			return null;
		}

		if (e is SmartUIElement es && e.Parent is not SmartUIElement)
		{
			return es;
		}

		return GetSmartUiParent(e.Parent);
	}

	private bool _mouseState = false;
	private UIState _blockAt = null;
	private SmartUIElement _blockAtE = null;

	/// <summary>
	/// Handles updating the UI states correctly
	/// </summary>
	/// <param name="gameTime"></param>
	public override void UpdateUI(GameTime gameTime)
	{
		foreach (UserInterface eachState in UserInterfaces)
		{
			if (eachState?.CurrentState != null && ((SmartUIState)eachState.CurrentState).Visible)
			{
				eachState.Update(gameTime);

				UIElement e = eachState.CurrentState.GetElementAt(Main.MouseScreen);
				SmartUIElement se = GetSmartUiParent(e);

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
	public static T GetUIState<T>() where T : SmartUIState
	{
		return UIStates.FirstOrDefault(n => n is T) as T;
	}

	/// <summary>
	/// Forcibly reloads a SmartUIState and it's associated UserInterface
	/// </summary>
	/// <typeparam name="T">The SmartUIState subclass to reload</typeparam>
	public static void ReloadState<T>() where T : SmartUIState
	{
		int index = UIStates.IndexOf(GetUIState<T>());
		UIStates[index] = (T)Activator.CreateInstance(typeof(T), null);
		UserInterfaces[index] = new UserInterface();
		UserInterfaces[index].SetState(UIStates[index]);
	}

	/// <summary>
	/// Handles the insertion of the automatically generated UIs
	/// </summary>
	/// <param name="layers"></param>
	public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
	{
		for (int k = 0; k < UIStates.Count; k++)
		{
			SmartUIState state = UIStates[k];
			AddLayer(layers, state, state.InsertionIndex(layers), state.Visible, state.Scale);
		}
	}
}