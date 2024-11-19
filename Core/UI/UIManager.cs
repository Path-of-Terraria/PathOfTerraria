using System.Collections.Generic;
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
	public sealed class UIStateData(string identifier, string layer, UIState? value, int offset = 0, InterfaceScaleType type = InterfaceScaleType.UI)
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
		public readonly UIState? Value = value;

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

	// Terraria doesn't provide any game time instance during rendering, so we keep track of it ourselves.
	private static GameTime? lastGameTime;

	/// <summary>
	///		The list of data from registered <see cref="UIState"/> instances.
	/// </summary>
	public static List<UIStateData> Data { get; set; } = [];

	public override void Unload()
	{
		base.Unload();

		for (int i = 0; i < Data.Count; i++)
		{
			UIStateData data = Data[i];

			data.UserInterface?.SetState(null);
			data.UserInterface = null;
		}

		Data?.Clear();
		Data = null;
	}

	public override void UpdateUI(GameTime gameTime)
	{
		base.UpdateUI(gameTime);

		for (int i = 0; i < Data.Count; i++)
		{
			UIStateData data = Data[i];

			data.UserInterface.Update(gameTime);
		}

		lastGameTime = gameTime;
	}

	public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
	{
		base.ModifyInterfaceLayers(layers);

		for (int i = 0; i < Data.Count; i++)
		{
			UIStateData data = Data[i];

			int index = layers.FindIndex(l => l.Name == data.Layer);

			if (index < 0)
			{
				continue;
			}

			LegacyGameInterfaceLayer layer = new(
				data.Identifier,
				() =>
				{
					data.UserInterface.Draw(Main.spriteBatch, lastGameTime);
					return true;
				}
			);

			layers.Insert(index + data.Offset, layer);
		}
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
	public static void Register(string identifier, string layer, UIState? value, int offset = 0, InterfaceScaleType type = InterfaceScaleType.UI)
	{
		int index = Data.FindIndex(s => s.Identifier == identifier);

		var data = new UIStateData(identifier, layer, value, offset, type)
		{
			UserInterface = new UserInterface()
		};

		data.UserInterface.SetState(value);

		if (index < 0)
		{
			Data.Add(data);
		}
		else
		{
			Data[index] = data;
		}
	}

	/// <summary>
	///		Attempts to enable a <see cref="UIState"/> instance by its identifier.
	/// </summary>
	/// <param name="identifier">The identifier of the <see cref="UIState"/>.</param>
	/// <returns><c>true</c> if the state was successfully enabled; otherwise, <c>false</c>.</returns>
	public static bool TryEnable(string identifier)
	{
		int index = Data.FindIndex(s => s.Identifier == identifier);

		if (index < 0)
		{
			return false;
		}

		UIStateData data = Data[index];

		data.UserInterface.CurrentState.Activate();
		data.Enabled = true;

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
	public static bool TryEnableOrRegister(string identifier, string layer, UIState? value, int offset = 0, InterfaceScaleType type = InterfaceScaleType.UI)
	{
		int index = Data.FindIndex(s => s.Identifier == identifier);

		if (index < 0)
		{
			Register(identifier, layer, value, offset, type);
		}

		return TryEnable(identifier);
	}

	/// <summary>
	///		Attempts to disable a <see cref="UIState"/> instance by its identifier.
	/// </summary>
	/// <remarks>
	///		This method does nothing if the instance is already disabled or cannot be found.
	/// </remarks>
	/// <param name="identifier">The identifier of the <see cref="UIState"/>.</param>
	/// <returns><c>true</c> if the state was successfully disabled; otherwise, <c>false</c>.</returns>
	public static bool TryDisable(string identifier)
	{
		int index = Data.FindIndex(s => s.Identifier == identifier);

		if (index < 0)
		{
			return false;
		}

		UIStateData data = Data[index];

		data.UserInterface.CurrentState.Deactivate();
		data.Enabled = false;

		return true;
	}

	/// <summary>
	///		Attempts to toggle the enabled state of a <see cref="UIState"/> instance by its identifier.
	/// </summary>
	/// <param name="identifier">The identifier of the <see cref="UIState"/>.</param>
	/// <returns><c>true</c> if the state was successfully toggled; otherwise, <c>false</c>.</returns>
	public static bool TryToggle(string identifier)
	{
		int index = Data.FindIndex(s => s.Identifier == identifier);

		if (index < 0)
		{
			return false;
		}

		return Data[index].Enabled ? TryDisable(identifier) : TryEnable(identifier);
	}

	/// <summary>
	///		Attempts to toggle a <see cref="UIState"/> instance. If it doesn't exist, registers it and then toggles it.
	/// </summary>
	/// <param name="identifier">The identifier of the <see cref="UIState"/>.</param>
	/// <param name="layer">The layer at which to insert the <see cref="UIState"/>.</param>
	/// <param name="value">The value of the <see cref="UIState"/>.</param>
	/// <param name="offset">The index offset within the specified insertion layer. Defaults to <c>0</c>.</param>
	/// <param name="type">The interface scale type of the <see cref="UIState"/>. Defaults to <see cref="InterfaceScaleType.UI"/>.</param>
	public static bool TryToggleOrRegister(string identifier, string layer, UIState? value, int offset = 0, InterfaceScaleType type = InterfaceScaleType.UI)
	{
		int index = Data.FindIndex(s => s.Identifier == identifier);

		if (index < 0)
		{
			Register(identifier, layer, value, offset, type);
		}

		return TryToggle(identifier);
	}
}