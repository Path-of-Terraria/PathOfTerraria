using PathOfTerraria.Common.AccessorySlots;
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

	protected override UIHoverImageItemSlot?[] GetDefaultSlots(ref int numAccessorySlots)
	{
		numAccessorySlots += 2;

		UIHoverImageItemSlot?[] slots = [
			new(DyeFrameTexture, DyeIconTexture, new(() => (Player.dye, (int)RemappedEquipSlots.Wings)), ($"Mods.{PoTMod.ModName}.UI.Slots.Wings", null), ItemSlot.Context.EquipDye),
			new(DyeFrameTexture, DyeIconTexture, new(() => (Player.dye, (int)RemappedEquipSlots.Head)), ($"Mods.{PoTMod.ModName}.UI.Slots.Head", null), ItemSlot.Context.EquipDye),
			new(DyeFrameTexture, DyeIconTexture, new(() => (Player.dye, (int)RemappedEquipSlots.Necklace)), ($"Mods.{PoTMod.ModName}.UI.Slots.Necklace", null), ItemSlot.Context.EquipDye),
			//
			null,
			new(DyeFrameTexture, DyeIconTexture, new(() => (Player.dye, (int)RemappedEquipSlots.Body)), ($"Mods.{PoTMod.ModName}.UI.Slots.Body", null), ItemSlot.Context.EquipDye),
			new(DyeFrameTexture, DyeIconTexture, new(() => (Player.dye, (int)RemappedEquipSlots.Offhand)), ($"Mods.{PoTMod.ModName}.UI.Slots.Offhand", null), ItemSlot.Context.EquipDye),
			//
			new(DyeFrameTexture, DyeIconTexture, new(() => (Player.dye, (int)RemappedEquipSlots.RingOn)), ($"Mods.{PoTMod.ModName}.UI.Slots.RingLeft", null), ItemSlot.Context.EquipDye),
			new(DyeFrameTexture, DyeIconTexture, new(() => (Player.dye, (int)RemappedEquipSlots.Legs)), ($"Mods.{PoTMod.ModName}.UI.Slots.Legs", null), ItemSlot.Context.EquipDye),
			new(DyeFrameTexture, DyeIconTexture, new(() => (Player.dye, (int)RemappedEquipSlots.RingOff)), ($"Mods.{PoTMod.ModName}.UI.Slots.RingRight", null), ItemSlot.Context.EquipDye),
			//
			new(DyeFrameTexture, DyeIconTexture, new(() => (Player.dye, (int)RemappedEquipSlots.Accessory1)), ($"Mods.{PoTMod.ModName}.UI.Slots.NumberedAccessory", 1), ItemSlot.Context.EquipDye),
			new(DyeFrameTexture, DyeIconTexture, new(() => (Player.dye, (int)RemappedEquipSlots.Accessory2)), ($"Mods.{PoTMod.ModName}.UI.Slots.NumberedAccessory", 2), ItemSlot.Context.EquipDye),
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

	protected override UIHoverImageItemSlot CreateCustomAccessorySlot(ModAccessorySlot modSlot, ref int numAccessorySlots)
	{
		int accessoryNumber = ++numAccessorySlots;
		var handler = new UIImageItemSlot.SlotWrapper(() => modSlot.DyeItem, value => modSlot.DyeItem = value);
		var uiSlot = new UIHoverImageItemSlot(DyeFrameTexture, DyeIconTexture, handler, ($"Mods.{PoTMod.ModName}.UI.Slots.NumberedAccessory", accessoryNumber), ItemSlot.Context.ModdedDyeSlot);

		return uiSlot;
	}
}
