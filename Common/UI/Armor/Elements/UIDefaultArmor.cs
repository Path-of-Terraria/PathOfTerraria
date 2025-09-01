using PathOfTerraria.Common.AccessorySlots;
using PathOfTerraria.Common.UI.Elements;
using ReLogic.Content;
using Terraria.UI;

#nullable enable

namespace PathOfTerraria.Common.UI.Armor.Elements;

public sealed class UIDefaultArmor : UIArmorPage
{
	protected override Asset<Texture2D> DefaultFrameTexture { get; } = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/Inventory/Frame_Default", AssetRequestMode.ImmediateLoad);

	protected override UIHoverImageItemSlot?[] GetDefaultSlots()
	{
		return [
			new(DefaultFrameTexture, WingsIconTexture, new(() => (Player.armor, (int)RemappedEquipSlots.Wings)), $"Mods.{PoTMod.ModName}.UI.Slots.10", ItemSlot.Context.EquipAccessory),
			new(DefaultFrameTexture, HelmetIconTexture, new(() => (Player.armor, (int)RemappedEquipSlots.Head)), $"Mods.{PoTMod.ModName}.UI.Slots.0", ItemSlot.Context.EquipArmor),
			new(DefaultFrameTexture, NecklaceIconTexture, new(() => (Player.armor, (int)RemappedEquipSlots.Necklace)), $"Mods.{PoTMod.ModName}.UI.Slots.5", ItemSlot.Context.EquipAccessory),
			//
			new(DefaultFrameTexture, WeaponIconTexture, new(() => (Player.inventory, 0)), $"Mods.{PoTMod.ModName}.UI.Slots.4", 0),
			new(DefaultFrameTexture, ChestIconTexture, new(() => (Player.armor, (int)RemappedEquipSlots.Body)), $"Mods.{PoTMod.ModName}.UI.Slots.1", ItemSlot.Context.EquipArmor),
			new(DefaultFrameTexture, OffhandIconTexture, new(() => (Player.armor, (int)RemappedEquipSlots.Offhand)), $"Mods.{PoTMod.ModName}.UI.Slots.6", ItemSlot.Context.EquipAccessory),
			//
			new(DefaultFrameTexture, RingIconTexture, new(() => (Player.armor, (int)RemappedEquipSlots.RingOn)), $"Mods.{PoTMod.ModName}.UI.Slots.7", ItemSlot.Context.EquipAccessory),
			new(DefaultFrameTexture, LegsIconTexture, new(() => (Player.armor, (int)RemappedEquipSlots.Legs)), $"Mods.{PoTMod.ModName}.UI.Slots.2", ItemSlot.Context.EquipArmor),
			new(DefaultFrameTexture, RingIconTexture, new(() => (Player.armor, (int)RemappedEquipSlots.RingOff)), $"Mods.{PoTMod.ModName}.UI.Slots.8", ItemSlot.Context.EquipAccessory),
			//
			new(DefaultFrameTexture, MiscellaneousIconTexture, new(() => (Player.armor, (int)RemappedEquipSlots.Accessory1)), $"Mods.{PoTMod.ModName}.UI.Slots.3", ItemSlot.Context.EquipAccessory),
			new(DefaultFrameTexture, MiscellaneousIconTexture, new(() => (Player.armor, (int)RemappedEquipSlots.Accessory2)), $"Mods.{PoTMod.ModName}.UI.Slots.9", ItemSlot.Context.EquipAccessory),
		];
	}

	protected override UIHoverImageItemSlot CreateCustomAccessorySlot(ModAccessorySlot modSlot)
	{
		var handler = new UIImageItemSlot.SlotWrapper(() => modSlot.FunctionalItem, value => modSlot.FunctionalItem = value);
		var uiSlot = new UIHoverImageItemSlot(DefaultFrameTexture, MiscellaneousIconTexture, handler, $"Mods.{PoTMod.ModName}.UI.Slots.10", ItemSlot.Context.ModdedAccessorySlot);

		return uiSlot;
	}
}