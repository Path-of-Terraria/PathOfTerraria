using System.Collections.Generic;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.Armor;

[Autoload(Side = ModSide.Client)]
public sealed class UIGearInventoryManager : ModSystem
{
	// Terraria doesn't provide any game time instance during rendering, so we keep track of it ourselves.
	private static GameTime lastGameTime;

	public static UserInterface UserInterface { get; private set; }
	
	public override void OnWorldLoad()
	{
		UserInterface.SetState(new UIGearInventory());
	}

	public override void UpdateUI(GameTime gameTime)
	{
		UserInterface.Update(gameTime);

		lastGameTime = gameTime ?? new GameTime();
	}

	public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
	{
		int index = layers.FindIndex(layer => layer.Name == "Vanilla: Inventory");

		if (index == -1)
		{
			return;
		}

		layers.Insert(
			index,
			new LegacyGameInterfaceLayer(
				$"{PoTMod.ModName}:{nameof(UIGearInventory)}",
				() =>
				{
					UserInterface.Draw(Main.spriteBatch, lastGameTime);

					return true;
				}
			)
		);
	}
}