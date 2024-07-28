using System.Collections.Generic;
using Terraria.UI;
using PathOfTerraria.Common.Loaders.UILoading;
using Terraria.DataStructures;
using PathOfTerraria.Common.UI.Hotbar.Components;
using PathOfTerraria.Common.UI.Hotbar.Components.Sets;

namespace PathOfTerraria.Common.UI.Hotbar;

internal sealed class NewHotbar : SmartUIState
{
	/// <summary>
	///		Offsets the rendering of buffs to render below our custom hotbar.
	/// </summary>
	private sealed class OffsetBuffRendering : GlobalBuff
	{
		public override bool PreDraw(SpriteBatch spriteBatch, int type, int buffIndex, ref BuffDrawParams drawParams)
		{
			// TODO: Make constant when a good value is found.
			int buffPositionOffsetY = 20;
			drawParams.Position = new Vector2(drawParams.Position.X, drawParams.Position.Y + buffPositionOffsetY);
			drawParams.MouseRectangle.Y += buffPositionOffsetY;
			return true;
		}
	}

	/// <summary>
	///		Hijacks and handles clicking on the hotbar ourselves.
	/// </summary>
	public class HijackHotbarClick : ModSystem
	{
		public override void Load()
		{
			On_Main.GUIHotbarDrawInner += StopClickOnHotbar;
		}

		private void StopClickOnHotbar(On_Main.orig_GUIHotbarDrawInner orig, Main self)
		{
			bool hbLocked = Main.LocalPlayer.hbLocked; // Lock hotbar for the original method so we don't fight against vanilla
			Main.LocalPlayer.hbLocked = true;
			orig(self);
			Main.LocalPlayer.hbLocked = hbLocked;
		}
	}

	public override bool Visible => !Main.playerInventory;

	public override int InsertionIndex(List<GameInterfaceLayer> layers)
	{
		return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
	}

	public override void OnInitialize()
	{
		base.OnInitialize();

		var hotbar = new HotbarUI();
		hotbar.AddSet(new CombatSet());
		hotbar.AddSet(new BuildingSet());
		Append(hotbar);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{

	}
}
