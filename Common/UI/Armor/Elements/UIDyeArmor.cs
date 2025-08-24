using PathOfTerraria.Common.UI.Elements;
using ReLogic.Content;
using Terraria.UI;

#nullable enable

namespace PathOfTerraria.Common.UI.Armor.Elements;

public sealed class UIDyeArmor : UIArmorPage
{
	private Asset<Texture2D> DyeFrameTexture { get; } = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/Inventory/Frame_Dye", AssetRequestMode.ImmediateLoad);
	private Asset<Texture2D> DyeIconTexture { get; } = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/Inventory/Dye", AssetRequestMode.ImmediateLoad);

	protected override Asset<Texture2D> DefaultFrameTexture => DyeFrameTexture;

	protected override UIHoverImageItemSlot?[] GetDefaultSlots()
	{
		UIHoverImageItemSlot?[] slots = [
			new(DyeFrameTexture, DyeIconTexture, new(() => (Player.dye, 4)), $"Mods.{PoTMod.ModName}.UI.Slots.10", ItemSlot.Context.EquipDye),
			new(DyeFrameTexture, DyeIconTexture, new(() => (Player.dye, 0)), $"Mods.{PoTMod.ModName}.UI.Slots.0", ItemSlot.Context.EquipDye),
			new(DyeFrameTexture, DyeIconTexture, new(() => (Player.dye, 5)), $"Mods.{PoTMod.ModName}.UI.Slots.5", ItemSlot.Context.EquipDye),
			//
			null,
			new(DyeFrameTexture, DyeIconTexture, new(() => (Player.dye, 1)), $"Mods.{PoTMod.ModName}.UI.Slots.1", ItemSlot.Context.EquipDye),
			new(DyeFrameTexture, DyeIconTexture, new(() => (Player.dye, 6)), $"Mods.{PoTMod.ModName}.UI.Slots.6", ItemSlot.Context.EquipDye),
			//
			new(DyeFrameTexture, DyeIconTexture, new(() => (Player.dye, 7)), $"Mods.{PoTMod.ModName}.UI.Slots.7", ItemSlot.Context.EquipDye),
			new(DyeFrameTexture, DyeIconTexture, new(() => (Player.dye, 2)), $"Mods.{PoTMod.ModName}.UI.Slots.2", ItemSlot.Context.EquipDye),
			new(DyeFrameTexture, DyeIconTexture, new(() => (Player.dye, 8)), $"Mods.{PoTMod.ModName}.UI.Slots.8", ItemSlot.Context.EquipDye),
			//
			new(DyeFrameTexture, DyeIconTexture, new(() => (Player.dye, 3)), $"Mods.{PoTMod.ModName}.UI.Slots.3", ItemSlot.Context.EquipDye),
			new(DyeFrameTexture, DyeIconTexture, new(() => (Player.dye, 9)), $"Mods.{PoTMod.ModName}.UI.Slots.9", ItemSlot.Context.EquipDye),
		];

		foreach (UIHoverImageItemSlot? slot in slots)
		{
			if (slot != null)
			{
				slot.Predicate = (item, _) => item.dye > 0;
			}
		}

		return slots;
	}

	protected override UIHoverImageItemSlot CreateCustomAccessorySlot(ModAccessorySlot modSlot)
	{
		var handler = new UIImageItemSlot.SlotWrapper(() => modSlot.DyeItem, value => modSlot.DyeItem = value);
		var uiSlot = new UIHoverImageItemSlot(DyeFrameTexture, DyeIconTexture, handler, $"Mods.{PoTMod.ModName}.UI.Slots.10", ItemSlot.Context.ModdedDyeSlot);

		return uiSlot;
	}
}
