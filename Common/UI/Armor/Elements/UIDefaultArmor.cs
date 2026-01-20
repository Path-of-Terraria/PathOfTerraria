using PathOfTerraria.Common.AccessorySlots;
using PathOfTerraria.Common.UI.Elements;
using ReLogic.Content;
using Terraria.UI;

#nullable enable

namespace PathOfTerraria.Common.UI.Armor.Elements;

public sealed class UIDefaultArmor : UIArmorPage
{
	const string LocPrefix = $"Mods.{PoTMod.ModName}.UI.Slots.";

	protected override Asset<Texture2D> DefaultFrameTexture { get; } = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/Inventory/Frame_Default", AssetRequestMode.ImmediateLoad);

	protected override UIHoverImageItemSlot?[] GetDefaultSlots(ref int numAccessorySlots)
	{
		numAccessorySlots += 2;

		return [
			new(DefaultFrameTexture, WingsIconTexture, new(() => (Player.armor, (int)RemappedEquipSlots.Wings)), (LocPrefix + "Wings", null)!, ItemSlot.Context.EquipAccessory, armorHideSlot: ((int)RemappedEquipSlots.Wings, true)),
			new(DefaultFrameTexture, HelmetIconTexture, new(() => (Player.armor, (int)RemappedEquipSlots.Head)), (LocPrefix + "Head", null)!, ItemSlot.Context.EquipArmor),
			new(DefaultFrameTexture, NecklaceIconTexture, new(() => (Player.armor, (int)RemappedEquipSlots.Necklace)), (LocPrefix + "Necklace", null)!, ItemSlot.Context.EquipAccessory, armorHideSlot: ((int)RemappedEquipSlots.Necklace, true)),
			//
			new(DefaultFrameTexture, WeaponIconTexture, new(() => (Player.inventory, 0)), (LocPrefix + "Weapon", null)!, 0),
			new(DefaultFrameTexture, ChestIconTexture, new(() => (Player.armor, (int)RemappedEquipSlots.Body)), (LocPrefix + "Body", null)!, ItemSlot.Context.EquipArmor),
			new(DefaultFrameTexture, OffhandIconTexture, new(() => (Player.armor, (int)RemappedEquipSlots.Offhand)), (LocPrefix + "Offhand", null)!, ItemSlot.Context.EquipAccessory, armorHideSlot: ((int)RemappedEquipSlots.Offhand, true)),
			//
			new(DefaultFrameTexture, RingIconTexture, new(() => (Player.armor, (int)RemappedEquipSlots.RingOn)), (LocPrefix + "RingLeft", null)!, ItemSlot.Context.EquipAccessory, armorHideSlot: ((int)RemappedEquipSlots.RingOn, true)),
			new(DefaultFrameTexture, LegsIconTexture, new(() => (Player.armor, (int)RemappedEquipSlots.Legs)), (LocPrefix + "Legs", null)!, ItemSlot.Context.EquipArmor),
			new(DefaultFrameTexture, RingIconTexture, new(() => (Player.armor, (int)RemappedEquipSlots.RingOff)), (LocPrefix + "RingRight", null)!, ItemSlot.Context.EquipAccessory, armorHideSlot: ((int)RemappedEquipSlots.RingOff, true)),
			//
			new(DefaultFrameTexture, MiscellaneousIconTexture, new(() => (Player.armor, (int)RemappedEquipSlots.Accessory1)), (LocPrefix + "NumberedAccessory", 1), ItemSlot.Context.EquipAccessory, armorHideSlot: ((int)RemappedEquipSlots.Accessory1, true)),
			new(DefaultFrameTexture, MiscellaneousIconTexture, new(() => (Player.armor, (int)RemappedEquipSlots.Accessory2)), (LocPrefix + "NumberedAccessory", 2), ItemSlot.Context.EquipAccessory, armorHideSlot: ((int)RemappedEquipSlots.Accessory2, true)),
		];
	}

	protected override UIHoverImageItemSlot CreateCustomAccessorySlot(ModAccessorySlot modSlot, ref int numAccessorySlots, int customModSlot)
	{
		int accessoryNumber = ++numAccessorySlots;
		(int, bool)? armorHideSlot = (customModSlot, false);
		var handler = new UIImageItemSlot.SlotWrapper(() => modSlot.FunctionalItem, value => modSlot.FunctionalItem = value);
		var uiSlot = new UIHoverImageItemSlot(DefaultFrameTexture, MiscellaneousIconTexture, handler, (LocPrefix + "NumberedAccessory", accessoryNumber), ItemSlot.Context.ModdedAccessorySlot, armorHideSlot: armorHideSlot);

		return uiSlot;
	}
}