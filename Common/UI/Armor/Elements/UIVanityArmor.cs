using PathOfTerraria.Common.UI.Elements;
using ReLogic.Content;
using Terraria.UI;

#nullable enable

namespace PathOfTerraria.Common.UI.Armor.Elements;

public sealed class UIVanityArmor : UIArmorPage
{
	protected override Asset<Texture2D> DefaultFrameTexture { get; } = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/Inventory/Frame_Vanity", AssetRequestMode.ImmediateLoad);

	protected override UIHoverImageItemSlot?[] GetDefaultSlots()
	{
		return [
			new(DefaultFrameTexture, WingsIconTexture, new(() => ref Player.armor[4]), $"Mods.{PoTMod.ModName}.UI.Slots.10", ItemSlot.Context.EquipAccessoryVanity),
			new(DefaultFrameTexture, HelmetIconTexture, new(() => ref Player.armor[0]), $"Mods.{PoTMod.ModName}.UI.Slots.0", ItemSlot.Context.EquipArmorVanity),
			new(DefaultFrameTexture, NecklaceIconTexture, new(() => ref Player.armor[5]), $"Mods.{PoTMod.ModName}.UI.Slots.5", ItemSlot.Context.EquipAccessoryVanity),
			//
			new(DefaultFrameTexture, WeaponIconTexture, new(() => ref Player.inventory[0]), $"Mods.{PoTMod.ModName}.UI.Slots.4", 0),
			new(DefaultFrameTexture, ChestIconTexture, new(() => ref Player.armor[1]), $"Mods.{PoTMod.ModName}.UI.Slots.1", ItemSlot.Context.EquipArmorVanity),
			new(DefaultFrameTexture, OffhandIconTexture, new(() => ref Player.armor[6]), $"Mods.{PoTMod.ModName}.UI.Slots.6", ItemSlot.Context.EquipAccessoryVanity),
			//
			new(DefaultFrameTexture, RingIconTexture, new(() => ref Player.armor[7]), $"Mods.{PoTMod.ModName}.UI.Slots.7", ItemSlot.Context.EquipAccessoryVanity),
			new(DefaultFrameTexture, LegsIconTexture, new(() => ref Player.armor[2]), $"Mods.{PoTMod.ModName}.UI.Slots.2", ItemSlot.Context.EquipArmorVanity),
			new(DefaultFrameTexture, RingIconTexture, new(() => ref Player.armor[8]), $"Mods.{PoTMod.ModName}.UI.Slots.8", ItemSlot.Context.EquipAccessoryVanity),
			//
			new(DefaultFrameTexture, MiscellaneousIconTexture, new(() => ref Player.armor[3]), $"Mods.{PoTMod.ModName}.UI.Slots.3", ItemSlot.Context.EquipAccessoryVanity),
			new(DefaultFrameTexture, MiscellaneousIconTexture, new(() => ref Player.armor[9]), $"Mods.{PoTMod.ModName}.UI.Slots.9", ItemSlot.Context.EquipAccessoryVanity),
		];
	}

	protected override UIHoverImageItemSlot CreateCustomAccessorySlot(ModAccessorySlot modSlot)
	{
		var handler = new UIImageItemSlot.ItemHandler(() => modSlot.VanityItem, value => modSlot.VanityItem = value);
		var uiSlot = new UIHoverImageItemSlot(DefaultFrameTexture, MiscellaneousIconTexture, handler, $"Mods.{PoTMod.ModName}.UI.Slots.10", ItemSlot.Context.EquipAccessoryVanity);

		return uiSlot;
	}
}