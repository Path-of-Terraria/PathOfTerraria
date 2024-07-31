using System.Collections.Generic;
using Terraria.UI;

namespace PathOfTerraria.Content.GUI;

[Autoload(Side = ModSide.Client)]
public sealed class UISystem : ModSystem
{
	private readonly record struct UIStateData<T>(
		string Identifier,
		string Layer,
		int Offset,
		T? Value,
		InterfaceScaleType Type = InterfaceScaleType.UI
	) where T : UIState
	{
		public readonly UserInterface UserInterface = new();
	}

	private static class UITypeData<T> where T : UIState
	{
		public static readonly List<UIStateData<T?>> Data = new();

		static UITypeData()
		{
			OnUpdate += Update;
			OnRenderUpdate += Draw;
		}

		private static void Update(GameTime gameTime)
		{
			for (int i = 0; i < Data.Count; i++)
			{
				UIStateData<T> data = Data[i];

				data.UserInterface.Update(gameTime);
			}
		}

		private static void Draw(List<GameInterfaceLayer> layers, GameTime? gameTime)
		{
			for (int i = 0; i < Data.Count; i++)
			{
				UIStateData<T> data = Data[i];

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
	private static GameTime lastGameTime;

	private static event Action<GameTime?> OnUpdate;
	private static event Action<List<GameInterfaceLayer?>, GameTime?> OnRenderUpdate;

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
	
	public static T? Get<T>(string identifier) where T : UIState
	{
		int index = UITypeData<T>.Data.FindIndex(s => s.Identifier == identifier);

		if (index < 0)
		{
			return null;
		}

		return UITypeData<T>.Data[index].Value;
	}

	public static T? Enable<T>(string identifier, string layer, int offset, T? value, InterfaceScaleType type = InterfaceScaleType.UI) where T : UIState
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
		
		return value;
	}

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

	public static void Toggle<T>(string identifier, string layer, int offset, T? value, InterfaceScaleType type = InterfaceScaleType.UI) where T : UIState
	{
		int index = UITypeData<T>.Data.FindIndex(s => s.Identifier == identifier);

		if (index < 0)
		{
			Enable(identifier, layer, offset, value, type);
		}
		else
		{
			Disable<T>(identifier);
		}
	}
}