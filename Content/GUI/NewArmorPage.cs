using System.Collections.Generic;
using System.Reflection;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using PathOfTerraria.Content.GUI.UIItemSlots;
using PathOfTerraria.Core.Loaders.UILoading;
using PathOfTerraria.Helpers;
using PathOfTerraria.Helpers.Extensions;
using ReLogic.Content;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace PathOfTerraria.Content.GUI;

internal class NewArmorPage : SmartUIState
{
	private static readonly MethodInfo DrawAccSlotsInfo = typeof(AccessorySlotLoader).GetMethod(nameof(AccessorySlotLoader.DrawAccSlots), BindingFlags.Public | BindingFlags.Instance);

	private ILHook? drawAccHook;
	
	public override bool Visible => Main.playerInventory && Main.EquipPage == 0;

	public override void Load()
	{
		IL_Main.DrawInventory += DrawInventoryEdit;
		
		drawAccHook = new ILHook(DrawAccSlotsInfo, DrawAccSlotsEdit);
		drawAccHook.Apply();
	}
	
	public override void Unload()
	{
		drawAccHook?.Dispose();
		drawAccHook = null;
	}

	public override int InsertionIndex(List<GameInterfaceLayer> layers)
	{
		return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
	}

	public override void OnInitialize()
	{
		const float Margin = 4f;
		
		const int HorizontalSlots = 3;
		const int VerticalSlots = 3;
		
		var player = Main.CurrentPlayer;

		var frame = ModContent.Request<Texture2D>($"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Frame", AssetRequestMode.ImmediateLoad);
		var icon = ModContent.Request<Texture2D>($"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Helmet", AssetRequestMode.ImmediateLoad);

		var area = new UIElement
		{
			HAlign = 0.5f,
			VAlign = 0.5f,
			Width = StyleDimension.FromPixels(frame.Width() * HorizontalSlots + Margin * HorizontalSlots),
			Height = StyleDimension.FromPixels(frame.Height() * VerticalSlots + Margin * VerticalSlots),
		};
		
		Append(area);
		
		var helmet = new UICustomItemSlot(
			frame,
			icon,
			null,
			ItemSlot.Context.EquipArmor
		);
		
		helmet.SetMargin(Margin);
		
		helmet.HAlign = 0.5f;
		helmet.VAlign = 0f;
		
		area.Append(helmet);
		
		icon = ModContent.Request<Texture2D>($"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Chest", AssetRequestMode.ImmediateLoad);
		
		var chest = new UICustomItemSlot(
			frame,
			icon,
			null,
			ItemSlot.Context.EquipArmor
		);
		
		chest.SetMargin(Margin);
		
		chest.HAlign = 0.5f;
		chest.VAlign = 0.5f;
		
		area.Append(chest);
		
		icon = ModContent.Request<Texture2D>($"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Left_Hand", AssetRequestMode.ImmediateLoad);
		
		var left = new UICustomItemSlot(
			frame,
			icon,
			null,
			ItemSlot.Context.EquipArmor
		);
		
		left.SetMargin(Margin);
		
		left.HAlign = 0f;
		left.VAlign = 0.5f;
		
		area.Append(left);
		
		icon = ModContent.Request<Texture2D>($"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Right_Hand", AssetRequestMode.ImmediateLoad);
		
		var right = new UICustomItemSlot(
			frame,
			icon,
			null,
			ItemSlot.Context.EquipArmor
		);
		
		right.SetMargin(Margin);
		
		right.HAlign = 1f;
		right.VAlign = 0.5f;
		
		area.Append(right);
	}

	// TODO: Redo the patch.
	private static void DrawInventoryEdit(ILContext il)
	{
		var cursor = new ILCursor(il);

		var label = cursor.MarkLabel();
		
		cursor.GotoNext(MoveType.After, i => i.MatchLdsfld<Main>(nameof(Main.EquipPage)), i => i.MatchBrtrue(out label));
		cursor.EmitBr(label);
	}
	
	private static void DrawAccSlotsEdit(ILContext il)
	{
		var cursor = new ILCursor(il);

		cursor.Emit(OpCodes.Ret);
	}
}
