using PathOfTerraria.Common.AccessorySlots;
using PathOfTerraria.Common.UI.Elements;
using ReLogic.Content;
using Terraria.UI;

#nullable enable

namespace PathOfTerraria.Common.UI.Armor.Elements;

public sealed class UIVanityArmor : UIArmorPage
{
	protected override Asset<Texture2D> DefaultFrameTexture { get; } = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/Inventory/Frame_Vanity", AssetRequestMode.ImmediateLoad);

	protected override UIHoverImageItemSlot?[] GetDefaultSlots(ref int numAccessorySlots)
	{
		numAccessorySlots += 2;

		return [
			new(DefaultFrameTexture, WingsIconTexture, new(() => (Player.armor, (int)RemappedEquipSlots.VanityWings)), ($"Mods.{PoTMod.ModName}.UI.Slots.Wings", null)!, ItemSlot.Context.EquipAccessoryVanity),
			new(DefaultFrameTexture, HelmetIconTexture, new(() => (Player.armor, (int)RemappedEquipSlots.VanityHead)), ($"Mods.{PoTMod.ModName}.UI.Slots.Head", null)!, ItemSlot.Context.EquipArmorVanity),
			new(DefaultFrameTexture, NecklaceIconTexture, new(() => (Player.armor, (int)RemappedEquipSlots.VanityNecklace)), ($"Mods.{PoTMod.ModName}.UI.Slots.Necklace", null)!, ItemSlot.Context.EquipAccessoryVanity),
			//
			null,
			new(DefaultFrameTexture, ChestIconTexture, new(() => (Player.armor, (int)RemappedEquipSlots.VanityBody)), ($"Mods.{PoTMod.ModName}.UI.Slots.Body", null)!, ItemSlot.Context.EquipArmorVanity),
			new(DefaultFrameTexture, OffhandIconTexture, new(() => (Player.armor, (int)RemappedEquipSlots.VanityOffhand)), ($"Mods.{PoTMod.ModName}.UI.Slots.Offhand", null)!, ItemSlot.Context.EquipAccessoryVanity),
			//
			new(DefaultFrameTexture, RingIconTexture, new(() => (Player.armor, (int)RemappedEquipSlots.VanityRingOn)), ($"Mods.{PoTMod.ModName}.UI.Slots.RingLeft", null)!, ItemSlot.Context.EquipAccessoryVanity),
			new(DefaultFrameTexture, LegsIconTexture, new(() => (Player.armor, (int)RemappedEquipSlots.VanityLegs)), ($"Mods.{PoTMod.ModName}.UI.Slots.Legs", null)!, ItemSlot.Context.EquipArmorVanity),
			new(DefaultFrameTexture, RingIconTexture, new(() => (Player.armor, (int)RemappedEquipSlots.VanityRingOff)), ($"Mods.{PoTMod.ModName}.UI.Slots.RingRight", null)!, ItemSlot.Context.EquipAccessoryVanity),
			//
			new(DefaultFrameTexture, MiscellaneousIconTexture, new(() => (Player.armor, (int)RemappedEquipSlots.VanityAccessory1)), ($"Mods.{PoTMod.ModName}.UI.Slots.NumberedAccessory", 1), ItemSlot.Context.EquipAccessoryVanity),
			new(DefaultFrameTexture, MiscellaneousIconTexture, new(() => (Player.armor, (int)RemappedEquipSlots.VanityAccessory2)), ($"Mods.{PoTMod.ModName}.UI.Slots.NumberedAccessory", 2), ItemSlot.Context.EquipAccessoryVanity),
		];
	}

	protected override UIHoverImageItemSlot CreateCustomAccessorySlot(ModAccessorySlot modSlot, ref int numAccessorySlots)
	{
		int accessoryNumber = ++numAccessorySlots;
		var handler = new UIImageItemSlot.SlotWrapper(() => modSlot.VanityItem, value => modSlot.VanityItem = value);
		var uiSlot = new UIHoverImageItemSlot(DefaultFrameTexture, MiscellaneousIconTexture, handler, ($"Mods.{PoTMod.ModName}.UI.Slots.NumberedAccessory", accessoryNumber), ItemSlot.Context.ModdedVanityAccessorySlot);

		return uiSlot;
	}
}