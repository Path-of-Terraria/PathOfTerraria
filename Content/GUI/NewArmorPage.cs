using System.Collections.Generic;
using System.Reflection;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using PathOfTerraria.Content.GUI.UIItemSlots;
using PathOfTerraria.Core.Loaders.UILoading;
using PathOfTerraria.Helpers.Extensions;
using ReLogic.Content;
using Terraria.ID;
using Terraria.UI;

namespace PathOfTerraria.Content.GUI;

public sealed class NewArmorPage : SmartUIState
{
	private sealed class NewArmorPagePatches : ILoadable
	{
		private static readonly MethodInfo DrawAccSlotsInfo = typeof(AccessorySlotLoader).GetMethod(
			nameof(AccessorySlotLoader.DrawAccSlots),
			BindingFlags.Public | BindingFlags.Instance
		);

		private static ILHook? drawAccHook;

		void ILoadable.Load(Mod mod)
		{
			IL_Main.DrawInventory += DrawInventoryEdit;

			drawAccHook = new ILHook(DrawAccSlotsInfo, DrawAccSlotsEdit);
			drawAccHook.Apply();
		}

		void ILoadable.Unload()
		{
			drawAccHook?.Dispose();
			drawAccHook = null;
		}

		private static void DrawInventoryEdit(ILContext il)
		{
			var cursor = new ILCursor(il);

			ILLabel label = cursor.MarkLabel();

			cursor.GotoNext(
				MoveType.After,
				i => i.MatchLdsfld<Main>(nameof(Main.EquipPage)),
				i => i.MatchBrtrue(out label)
			);

			cursor.EmitBr(label);
		}

		private static void DrawAccSlotsEdit(ILContext il)
		{
			var cursor = new ILCursor(il);

			cursor.Emit(OpCodes.Ret);
		}
	}

	private const float Margin = 4f;

	private const int HorizontalSlots = 3;
	private const int VerticalSlots = 3;

	public override bool Visible => Main.playerInventory && Main.EquipPage == 0;

	public override int InsertionIndex(List<GameInterfaceLayer> layers)
	{
		return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
	}

	public override void OnInitialize()
	{
		Player player = Main.CurrentPlayer;

		Asset<Texture2D> frame = ModContent.Request<Texture2D>(
			$"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Frame",
			AssetRequestMode.ImmediateLoad
		);

		HAlign = 1f;
		VAlign = 0.5f;

		Width = StyleDimension.FromPixels(frame.Width() * HorizontalSlots + Margin * HorizontalSlots);
		Height = StyleDimension.FromPixels(frame.Height() * VerticalSlots + Margin * VerticalSlots);

		Asset<Texture2D> icon = ModContent.Request<Texture2D>(
			$"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Wings",
			AssetRequestMode.ImmediateLoad
		);
		
		var wings = new UICustomItemSlot(frame, icon);

		wings.SetMargin(Margin);

		wings.HAlign = 0f;
		wings.VAlign = 0f;

		Append(wings);
		
		icon = ModContent.Request<Texture2D>(
			$"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Helmet",
			AssetRequestMode.ImmediateLoad
		);
		
		var helmet = new UICustomItemSlot(frame, icon);

		helmet.SetMargin(Margin);

		helmet.HAlign = 0.5f;
		helmet.VAlign = 0f;

		Append(helmet);
		
		icon = ModContent.Request<Texture2D>(
			$"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Necklace",
			AssetRequestMode.ImmediateLoad
		);

		var necklace = new UICustomItemSlot(frame, icon);

		necklace.SetMargin(Margin);

		necklace.HAlign = 1f;
		necklace.VAlign = 0f;

		Append(necklace);
		
		icon = ModContent.Request<Texture2D>(
			$"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Left_Hand", 
			AssetRequestMode.ImmediateLoad
		);

		var left = new UICustomItemSlot(frame, icon);

		left.SetMargin(Margin);

		left.HAlign = 0f;
		left.VAlign = 0.5f;

		Append(left);

		icon = ModContent.Request<Texture2D>(
			$"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Chest", 
			AssetRequestMode.ImmediateLoad
		);

		var chest = new UICustomItemSlot(frame, icon);

		chest.SetMargin(Margin);

		chest.HAlign = 0.5f;
		chest.VAlign = 0.5f;

		Append(chest);

		icon = ModContent.Request<Texture2D>(
			$"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Right_Hand", 
			AssetRequestMode.ImmediateLoad
		);
		
		var right = new UICustomItemSlot(frame, icon);

		right.SetMargin(Margin);

		right.HAlign = 1f;
		right.VAlign = 0.5f;

		Append(right);
		
		icon = ModContent.Request<Texture2D>(
			$"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Ring", 
			AssetRequestMode.ImmediateLoad
		);

		var leftRing = new UICustomItemSlot(frame, icon);

		leftRing.SetMargin(Margin);

		leftRing.HAlign = 0f;
		leftRing.VAlign = 1f;

		Append(leftRing);
		
		icon = ModContent.Request<Texture2D>(
			$"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Legs", 
			AssetRequestMode.ImmediateLoad
		);

		var legs = new UICustomItemSlot(frame, icon);

		legs.SetMargin(Margin);

		legs.HAlign = 0.5f;
		legs.VAlign = 1f;

		Append(legs);
		
		icon = ModContent.Request<Texture2D>(
			$"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Ring", 
			AssetRequestMode.ImmediateLoad
		);

		var rightRing = new UICustomItemSlot(frame, icon);

		rightRing.SetMargin(Margin);

		rightRing.HAlign = 1f;
		rightRing.VAlign = 1f;

		Append(rightRing);
	}
}