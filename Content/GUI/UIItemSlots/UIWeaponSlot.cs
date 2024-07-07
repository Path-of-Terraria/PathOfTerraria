using ReLogic.Content;
using Terraria.UI;

namespace PathOfTerraria.Content.GUI.UIItemSlots;

internal class UIWeaponSlot : UICustomItemSlot
{
	internal UIWeaponSlot(Color? backgroundColor = null, float scale = 1f) : base(
		ModContent.Request<Texture2D>($"{nameof(PathOfTerraria)}/Assets/Slots/NormalBack",
			AssetRequestMode.ImmediateLoad),
		ModContent.Request<Texture2D>($"{nameof(PathOfTerraria)}/Assets/Slots/Icons/Weapon",
			AssetRequestMode.ImmediateLoad),
		backgroundColor,
		ItemSlot.Context.InventoryItem,
		scale
	)
	{
		
	}
	
	public override Item Item
	{
		get => Main.CurrentPlayer.inventory[0];
		set => Main.CurrentPlayer.inventory[0] = value;
	}
}