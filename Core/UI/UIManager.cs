using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Terraria.UI;

// TODO: TryX safe alternatives to the current methods (TryGet, TryEnable, TryDisable, TryToggle).
namespace PathOfTerraria.Core.UI;

/// <summary>
///		Manages the registration, updating, and rendering of <see cref="UIState"/> instances.
/// </summary>
/// <remarks>
///		Registered <see cref="UIState"/> instances are identified by a unique identifier, which determines
///		their behavior and associated data.
///		This system allows multiple entries of the same <see cref="UIState"/> type, provided each entry has
///		a distinct identifier.
/// </remarks>
[Autoload(Side = ModSide.Client)]
public sealed class UISystem : ModSystem
{
	// TODO: Consider making this public, and implement alternatives to retrieve the data of a state.
	private record class UIStateData<T>(
		string Identifier,
		string Layer,
		int Offset,
		T? Value,
		InterfaceScaleType Type = InterfaceScaleType.UI
	) where T : UIState
	{
		public UserInterface UserInterface = new();

		public bool Enabled = true;
	}

	private static class UITypeData<T> where T : UIState
	{
		public static readonly List<UIStateData<T?>> Data = new();

		static UITypeData()
		{
			OnUnload += Unload;
			OnUpdate += Update;
			OnRenderUpdate += Draw;
		}

		private static void Unload()
		{
			for (int i = 0; i < Data.Count; i++)
			{
				UIStateData<T> data = Data[i];

				data.UserInterface?.SetState(null);
				data.UserInterface = null;
			}
		}

		private static void Update(GameTime gameTime)
		{
			for (int i = 0; i < Data.Count; i++)
			{
				UIStateData<T> data = Data[i];

				if (!data.Enabled)
				{
					continue;
				}

				data.UserInterface.Update(gameTime);
			}
		}

		private static void Draw(List<GameInterfaceLayer> layers, GameTime? gameTime)
		{
			for (int i = 0; i < Data.Count; i++)
			{
				UIStateData<T> data = Data[i];
				
				if (!data.Enabled)
				{
					continue;
				}

				int index = layers.FindIndex(l => l.Name == data.Layer);

				if (index < 0)
				{
					continue;
				}

				LegacyGameInterfaceLayer layer = new(
					data.Identifier,
					() =>
					{
						data.UserInterface.Draw(Main.spriteBatch, gameTime);
						return true;
					}
				);

				layers.Insert(index + data.Offset, layer);
			}
		}
	}

	// Terraria doesn't provide any game time instance during rendering, so we keep track of it ourselves.
	private static GameTime? lastGameTime;

	private static event Action? OnUnload;
	private static event Action<GameTime?>? OnUpdate;
	private static event Action<List<GameInterfaceLayer?>, GameTime?>? OnRenderUpdate;

	public override void Unload()
	{
		base.Unload();
		
		OnUnload?.Invoke();
	}

	public override void UpdateUI(GameTime gameTime)
	{
		base.UpdateUI(gameTime);

		OnUpdate?.Invoke(gameTime);

		lastGameTime = gameTime;
	}

	public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
	{
		base.ModifyInterfaceLayers(layers);

		OnRenderUpdate?.Invoke(layers, lastGameTime);
	}

	/// <summary>
	///		Retrieves a registered <see cref="UIState"/> instance by its identifier.
	/// </summary>
	/// <param name="identifier">The identifier of the <see cref="UIState"/> to retrieve.</param>
	/// <typeparam name="T">The type of the <see cref="UIState"/>.</typeparam>
	/// <returns>The <see cref="UIState"/> instance associated with the specified identifier, or <c>null</c> if no such instance exists.</returns>
	public static T? Get<T>(string identifier) where T : UIState
	{
		int index = UITypeData<T>.Data.FindIndex(s => s.Identifier == identifier);

		if (index < 0)
		{
			return null;
		}

		return UITypeData<T>.Data[index].Value;
	}

	/// <summary>
	///		Enables a <see cref="UIState"/> instance.
	/// </summary>
	/// <remarks>
	///		If another instance with the same identifier already exists, this will refresh its properties.
	/// </remarks>
	/// <param name="identifier">The unique identifier of the <see cref="UIState"/> to enable.</param>
	/// <param name="layer">The layer at which to insert the <see cref="UIState"/>.</param>
	/// <param name="offset">The index offset within the specified insertion layer.</param>
	/// <param name="value">The value of the <see cref="UIState"/>.</param>
	/// <param name="type">The interface scale type of the <see cref="UIState"/>. Defaults to <see cref="InterfaceScaleType.UI"/>.</param>
	/// <typeparam name="T">The type of the <see cref="UIState"/> to enable.</typeparam>
	public static void Enable<T>(string identifier, string layer, int offset, T? value, InterfaceScaleType type = InterfaceScaleType.UI) where T : UIState
	{
		int index = UITypeData<T>.Data.FindIndex(s => s.Identifier == identifier);

		var data = new UIStateData<T>(identifier, layer, offset, value, type);

		data.UserInterface.SetState(value);
		
		if (index < 0)
		{
			UITypeData<T>.Data.Add(data);
		}
		else
		{
			UITypeData<T>.Data[index] = data;
		}
	}

	/// <summary>
	///		Disables a <see cref="UIState"/> instance.
	/// </summary>
	/// <remarks>
	///		This method does nothing if the instance is already disabled or cannot be found.
	/// </remarks>
	/// <param name="identifier">The unique identifier of the <see cref="UIState"/> to disable.</param>
	/// <typeparam name="T">The type of the <see cref="UIState"/> to disable.</typeparam>
	public static void Disable<T>(string identifier) where T : UIState
	{
		int index = UITypeData<T>.Data.FindIndex(s => s.Identifier == identifier);

		if (index < 0)
		{
			return;
		}
		
		UITypeData<T>.Data[index].UserInterface.SetState(null);

		UITypeData<T>.Data.RemoveAt(index);
	}

	/// <summary>
	///		Toggles the enabled state of a <see cref="UIState"/> instance.
	/// </summary>
	/// <param name="identifier">The unique identifier of the <see cref="UIState"/> to enable.</param>
	/// <typeparam name="T">The type of the <see cref="UIState"/> to enable.</typeparam>
	public static void Toggle<T>(string identifier) where T : UIState
	{
		int index = UITypeData<T>.Data.FindIndex(s => s.Identifier == identifier);

		if (index < 0)
		{
			return;
		}

		UITypeData<T>.Data[index].Enabled = !UITypeData<T>.Data[index].Enabled;
	}
}