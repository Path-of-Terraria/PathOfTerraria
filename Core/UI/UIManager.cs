using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Metadata;
using PathOfTerraria.Core.UI;
using Terraria.UI;

namespace PathOfTerraria.Core.UI;

/// <summary>
///		Manages the registration, updating, and rendering of <see cref="UIState"/> instances.
/// </summary>
/// <remarks>
///		Registered <see cref="UIState"/> instances are identified by a unique identifier, which determines
///		their behavior and associated data. They will be automatically unloaded when no longer needed.
///		<br />
///		This system allows multiple entries of the same <see cref="UIState"/> type, provided each entry has
///		a distinct identifier.
/// </remarks>
[Autoload(Side = ModSide.Client)]
public sealed partial class UIManager : ModSystem
{
	private sealed class UIStateData<T>(string identifier, string layer, T? value, int offset = 0, InterfaceScaleType type = InterfaceScaleType.UI) where T : UIState
	{
		/// <summary>
		///		Whether the state is enabled or not.
		/// </summary>
		public bool Enabled;
		
		/// <summary>
		///		The <see cref="UserInterface"/> instance associated with the <see cref="UIState"/>.
		/// </summary>
		public UserInterface UserInterface;

		/// <summary>
		///		The identifier of the <see cref="UIState"/>.
		/// </summary>
		public readonly string Identifier = identifier;

		/// <summary>
		///		The layer at which to insert the <see cref="UIState"/>.
		/// </summary>
		public readonly string Layer = layer;

		/// <summary>
		///		The value of the <see cref="UIState"/>.
		/// </summary>
		public readonly T? Value = value;

		/// <summary>
		///		The index offset within the specified insertion layer. 
		/// </summary>
		/// <remarks>
		///		Defaults to <c>0</c>.
		/// </remarks>
		public readonly int Offset = offset;

		/// <summary>
		///		The interface scale type of the <see cref="UIState"/>. 
		/// </summary>
		/// <remarks>
		///		Defaults to <see cref="InterfaceScaleType.UI"/>.
		/// </remarks>
		public readonly InterfaceScaleType Type = type;
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
	///		Attempts to retrieve a <see cref="UIState"/> instance by its identifier.
	/// </summary>
	/// <param name="identifier">The identifier of the <see cref="UIState"/> to retrieve.</param>
	/// <param name="value">
	///		When this method returns, contains the <see cref="UIState"/> instance associated with the specified identifier,
	///		if the identifier is found; otherwise, <c>null</c>. This parameter is passed uninitialized.
	/// </param>
	/// <typeparam name="T">The type of the <see cref="UIState"/>.</typeparam>
	/// <returns><c>true</c> if a <see cref="UIState"/> with the specified identifier was found; otherwise, <c>false</c>.</returns>
	public static bool TryGet<T>(string identifier, [MaybeNullWhen(false)] out T? value) where T : UIState
	{
		value = Get<T>(identifier);

		return value == null;
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
	///		Registers a <see cref="UIState"/> instance.
	/// </summary>
	/// <remarks>
	///		If another instance with the same identifier already exists, this will refresh its properties.
	/// </remarks>
	/// <param name="identifier">The identifier of the <see cref="UIState"/>.</param>
	/// <param name="layer">The layer at which to insert the <see cref="UIState"/>.</param>
	/// <param name="value">The value of the <see cref="UIState"/>.</param>
	/// <param name="offset">The index offset within the specified insertion layer. Defaults to <c>0</c>.</param>
	/// <param name="type">The interface scale type of the <see cref="UIState"/>. Defaults to <see cref="InterfaceScaleType.UI"/>.</param>
	/// <typeparam name="T">The type of the <see cref="UIState"/>.</typeparam>
	public static void Register<T>(string identifier, string layer, T? value, int offset = 0, InterfaceScaleType type = InterfaceScaleType.UI) where T : UIState
	{
		int index = UITypeData<T>.Data.FindIndex(s => s.Identifier == identifier);

		var data = new UIStateData<T>(identifier, layer, value, offset, type)
		{
			UserInterface = new UserInterface(),
			Enabled = true
		};

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
	///		Attemps to enable a <see cref="UIState"/> instance by its identifier.
	/// </summary>
	/// <param name="identifier">The identifier of the <see cref="UIState"/>.</param>
	/// <typeparam name="T">The type of the <see cref="UIState"/>.</typeparam>
	/// <returns><c>true</c> if the state was successfully enabled; otherwise, <c>false</c>.</returns>
	public static bool TryEnable<T>(string identifier) where T : UIState
	{
		int index = UITypeData<T>.Data.FindIndex(s => s.Identifier == identifier);

		if (index < 0)
		{
			return false;
		}

		UITypeData<T>.Data[index].Enabled = true;

		return true;
	}
	
	/// <summary>
	///		Attempts to enable a <see cref="UIState"/> instance. If it doesn't exist, registers it and then enables it.
	/// </summary>
	/// <param name="identifier">The identifier of the <see cref="UIState"/>.</param>
	/// <param name="layer">The layer at which to insert the <see cref="UIState"/>.</param>
	/// <param name="value">The value of the <see cref="UIState"/>.</param>
	/// <param name="offset">The index offset within the specified insertion layer. Defaults to <c>0</c>.</param>
	/// <param name="type">The interface scale type of the <see cref="UIState"/>. Defaults to <see cref="InterfaceScaleType.UI"/>.</param>
	/// <typeparam name="T">The type of the <see cref="UIState"/>.</typeparam>
	public static bool TryEnableOrRegister<T>(string identifier, string layer, T? value, int offset = 0, InterfaceScaleType type = InterfaceScaleType.UI) where T : UIState
	{
		int index = UITypeData<T>.Data.FindIndex(s => s.Identifier == identifier);

		if (index < 0)
		{
			Register(identifier, layer, value, offset, type);
		}

		return TryEnable<T>(identifier);
	}
	
	/// <summary>
	///		Attemps to disable a <see cref="UIState"/> instance by its identifier.
	/// </summary>
	/// <remarks>
	///		This method does nothing if the instance is already disabled or cannot be found.
	/// </remarks>
	/// <param name="identifier">The identifier of the <see cref="UIState"/>.</param>
	/// <typeparam name="T">The type of the <see cref="UIState"/>.</typeparam>
	/// <returns><c>true</c> if the state was successfully disabled; otherwise, <c>false</c>.</returns>
	public static bool TryDisable<T>(string identifier) where T : UIState
	{
		int index = UITypeData<T>.Data.FindIndex(s => s.Identifier == identifier);

		if (index < 0)
		{
			return false;
		}

		UITypeData<T>.Data[index].Enabled = false;

		return true;
	}
	
	/// <summary>
	///		Attemps to toggle the enabled state of a <see cref="UIState"/> instance by its identifier.
	/// </summary>
	/// <param name="identifier">The identifier of the <see cref="UIState"/>.</param>
	/// <typeparam name="T">The type of the <see cref="UIState"/>.</typeparam>
	/// <returns><c>true</c> if the state was successfully toggled; otherwise, <c>false</c>.</returns>
	public static bool TryToggle<T>(string identifier, bool refresh = true) where T : UIState
	{
		int index = UITypeData<T>.Data.FindIndex(s => s.Identifier == identifier);

		if (index < 0)
		{
			return false;
		}
		
		UITypeData<T>.Data[index].Enabled = !UITypeData<T>.Data[index].Enabled;

		return true;
	}
	
	/// <summary>
	///		Attempts to toggle a <see cref="UIState"/> instance. If it doesn't exist, registers it and then toggles it.
	/// </summary>
	/// <param name="identifier">The identifier of the <see cref="UIState"/>.</param>
	/// <param name="layer">The layer at which to insert the <see cref="UIState"/>.</param>
	/// <param name="value">The value of the <see cref="UIState"/>.</param>
	/// <param name="offset">The index offset within the specified insertion layer. Defaults to <c>0</c>.</param>
	/// <param name="type">The interface scale type of the <see cref="UIState"/>. Defaults to <see cref="InterfaceScaleType.UI"/>.</param>
	/// <typeparam name="T">The type of the <see cref="UIState"/>.</typeparam>
	public static bool TryToggleOrRegister<T>(string identifier, string layer, T? value, int offset = 0, InterfaceScaleType type = InterfaceScaleType.UI) where T : UIState
	{
		int index = UITypeData<T>.Data.FindIndex(s => s.Identifier == identifier);

		if (index < 0)
		{
			Register(identifier, layer, value, offset, type);
		}
		
		return TryToggle<T>(identifier);
	}

	/// <summary>
	///		Attempts to refresh a <see cref="UIState"/> instance.
	/// </summary>
	/// <param name="identifier">The identifier of the <see cref="UIState"/>.</param>
	/// <typeparam name="T">The type of the <see cref="UIState"/>.</typeparam>
	/// <returns><c>true</c> if the state was successfully refreshed; otherwise, <c>false</c>.</returns>
	public static bool TryRefresh<T>(string identifier) where T : UIState
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
}