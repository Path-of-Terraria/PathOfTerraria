using System.Collections.Generic;
using MonoMod.Cil;
using PathOfTerraria.Content.GUI.UIItemSlots;
using PathOfTerraria.Core.Loaders.UILoading;
using PathOfTerraria.Helpers;
using ReLogic.Content;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace PathOfTerraria.Content.GUI;

internal class NewArmorPage : SmartUIState
{
	private UIItemSlot _headSlot;
	private UIItemSlot _bodySlot;
	private UIItemSlot _legSlot;
	private UIWeaponSlot _weaponSlot;
	
	public override bool Visible => Main.playerInventory && Main.EquipPage == 0;
	
	public override void Load()
	{
		IL_Main.DrawInventory += RemoveVanillaArmorPage;
		MonoModHooks.Modify(typeof(AccessorySlotLoader).GetMethod(nameof(AccessorySlotLoader.DrawAccSlots)), il => new ILCursor(il).EmitRet());
	}

	private static void RemoveVanillaArmorPage(ILContext il)
	{
		var cursor = new ILCursor(il);

		ILLabel label = null;
		cursor.GotoNext(MoveType.After, i => i.MatchLdsfld<Main>(nameof(Main.EquipPage)), i => i.MatchBrtrue(out label));
		cursor.EmitBr(label);
	}

	public override int InsertionIndex(List<GameInterfaceLayer> layers)
	{
		return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
	}

	public override void OnInitialize()
	{
		_headSlot = new UIItemSlot(Main.CurrentPlayer.armor, 0, ItemSlot.Context.EquipArmor) { HAlign = 0.5f, VAlign = 0.5f };
		Append(_headSlot);
		
		_bodySlot = new UIItemSlot(Main.CurrentPlayer.armor, 1, ItemSlot.Context.EquipArmor) { HAlign = 0.5f, VAlign = 0.6f };
		Append(_bodySlot);
		
		_legSlot = new UIItemSlot(Main.CurrentPlayer.armor, 2, ItemSlot.Context.EquipArmor) { HAlign = 0.5f, VAlign = 0.7f };
		Append(_legSlot);

		_weaponSlot = new UIWeaponSlot(Color.Red * 0.7f) { HAlign = 0.4f, VAlign = 0.6f };
		Append(_weaponSlot);
	}
}
