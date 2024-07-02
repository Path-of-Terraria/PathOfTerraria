using ReLogic.Content;
using Terraria.UI;

namespace PathOfTerraria.Content.GUI.UIItemSlots;

internal class UIWeaponSlot : UICustomItemSlot
{
	internal UIWeaponSlot(Color? slotBackgroundColor = null, float scale = 1f) : base(
		ModContent.Request<Texture2D>($"{nameof(PathOfTerraria)}/Assets/Slots/NormalBack",
			AssetRequestMode.ImmediateLoad),
		ModContent.Request<Texture2D>($"{nameof(PathOfTerraria)}/Assets/Slots/Icons/Weapon",
			AssetRequestMode.ImmediateLoad),
		slotBackgroundColor,
		ItemSlot.Context.InventoryItem,
		scale
	)
	{
		
	}
	
	internal override Item Item
	{
		get => Main.CurrentPlayer.inventory[0];
		set => Main.CurrentPlayer.inventory[0] = value;
	}
}