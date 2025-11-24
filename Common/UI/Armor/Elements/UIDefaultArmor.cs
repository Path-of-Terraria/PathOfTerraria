using PathOfTerraria.Common.AccessorySlots;
using PathOfTerraria.Common.UI.Elements;
using ReLogic.Content;
using Terraria.Localization;
using Terraria.UI;

#nullable enable

namespace PathOfTerraria.Common.UI.Armor.Elements;

public sealed class UIDefaultArmor : UIArmorPage
{
	protected override Asset<Texture2D> DefaultFrameTexture { get; } = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/Inventory/Frame_Default", AssetRequestMode.ImmediateLoad);

	protected override UIHoverImageItemSlot?[] GetDefaultSlots(ref int numAccessorySlots)
	{
		numAccessorySlots += 2;

		return [
			new(DefaultFrameTexture, WingsIconTexture, new(() => (Player.armor, (int)RemappedEquipSlots.Wings)), ($"Mods.{PoTMod.ModName}.UI.Slots.Wings", null)!, ItemSlot.Context.EquipAccessory),
			new(DefaultFrameTexture, HelmetIconTexture, new(() => (Player.armor, (int)RemappedEquipSlots.Head)), ($"Mods.{PoTMod.ModName}.UI.Slots.Head", null)!, ItemSlot.Context.EquipArmor),
			new(DefaultFrameTexture, NecklaceIconTexture, new(() => (Player.armor, (int)RemappedEquipSlots.Necklace)), ($"Mods.{PoTMod.ModName}.UI.Slots.Necklace", null)!, ItemSlot.Context.EquipAccessory),
			//
			new(DefaultFrameTexture, WeaponIconTexture, new(() => (Player.inventory, 0)), ($"Mods.{PoTMod.ModName}.UI.Slots.Weapon", null)!, 0),
			new(DefaultFrameTexture, ChestIconTexture, new(() => (Player.armor, (int)RemappedEquipSlots.Body)), ($"Mods.{PoTMod.ModName}.UI.Slots.Body", null)!, ItemSlot.Context.EquipArmor),
			new(DefaultFrameTexture, OffhandIconTexture, new(() => (Player.armor, (int)RemappedEquipSlots.Offhand)), ($"Mods.{PoTMod.ModName}.UI.Slots.Offhand", null)!, ItemSlot.Context.EquipAccessory),
			//
			new(DefaultFrameTexture, RingIconTexture, new(() => (Player.armor, (int)RemappedEquipSlots.RingOn)), ($"Mods.{PoTMod.ModName}.UI.Slots.RingLeft", null)!, ItemSlot.Context.EquipAccessory),
			new(DefaultFrameTexture, LegsIconTexture, new(() => (Player.armor, (int)RemappedEquipSlots.Legs)), ($"Mods.{PoTMod.ModName}.UI.Slots.Legs", null)!, ItemSlot.Context.EquipArmor),
			new(DefaultFrameTexture, RingIconTexture, new(() => (Player.armor, (int)RemappedEquipSlots.RingOff)), ($"Mods.{PoTMod.ModName}.UI.Slots.RingRight", null)!, ItemSlot.Context.EquipAccessory),
			//
			new(DefaultFrameTexture, MiscellaneousIconTexture, new(() => (Player.armor, (int)RemappedEquipSlots.Accessory1)), ($"Mods.{PoTMod.ModName}.UI.Slots.NumberedAccessory", 1), ItemSlot.Context.EquipAccessory),
			new(DefaultFrameTexture, MiscellaneousIconTexture, new(() => (Player.armor, (int)RemappedEquipSlots.Accessory2)), ($"Mods.{PoTMod.ModName}.UI.Slots.NumberedAccessory", 2), ItemSlot.Context.EquipAccessory),
		];
	}

	protected override UIHoverImageItemSlot CreateCustomAccessorySlot(ModAccessorySlot modSlot, ref int numAccessorySlots)
	{
		int accessoryNumber = ++numAccessorySlots;
		var handler = new UIImageItemSlot.SlotWrapper(() => modSlot.FunctionalItem, value => modSlot.FunctionalItem = value);
		var uiSlot = new UIHoverImageItemSlot(DefaultFrameTexture, MiscellaneousIconTexture, handler, ($"Mods.{PoTMod.ModName}.UI.Slots.NumberedAccessory", accessoryNumber), ItemSlot.Context.ModdedAccessorySlot);

		return uiSlot;
	}
}