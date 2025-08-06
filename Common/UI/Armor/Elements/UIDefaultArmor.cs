using PathOfTerraria.Common.UI.Elements;
using PathOfTerraria.Content.Items.Gear.Amulets;
using PathOfTerraria.Content.Items.Gear.Armor.Chestplate;
using PathOfTerraria.Content.Items.Gear.Armor.Helmet;
using PathOfTerraria.Content.Items.Gear.Armor.Leggings;
using PathOfTerraria.Content.Items.Gear.Offhands;
using PathOfTerraria.Content.Items.Gear.Rings;
using ReLogic.Content;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.Armor.Elements;

public sealed class UIDefaultArmor : UIArmorPage
{
	public static readonly Asset<Texture2D> DefaultFrameTexture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/Inventory/Frame_Default", AssetRequestMode.ImmediateLoad);

	public override void OnInitialize()
	{
		base.OnInitialize();

		Width = StyleDimension.FromPixels(UIArmorInventory.ArmorPageWidth);
		Height = StyleDimension.FromPixels(UIArmorInventory.ArmorPageHeight);

		var wings = new UIHoverImageItemSlot(DefaultFrameTexture, WingsIconTexture, ref Player.armor, 4, $"Mods.{PoTMod.ModName}.UI.Slots.10", ItemSlot.Context.EquipAccessory)
		{
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};

		wings.OnMouseOver += UpdateMouseOver;
		wings.OnMouseOut += UpdateMouseOut;

		wings.Predicate = (item, _) => item.wingSlot > 0;

		Append(wings);

		var helmet = new UIHoverImageItemSlot(DefaultFrameTexture, HelmetIconTexture, ref Player.armor, 0, $"Mods.{PoTMod.ModName}.UI.Slots.0", ItemSlot.Context.EquipArmor)
		{
			HAlign = 0.5f,
			VAlign = 0f,
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};

		helmet.OnMouseOver += UpdateMouseOver;
		helmet.OnMouseOut += UpdateMouseOut;

		helmet.Predicate = (item, _) => item.headSlot > 0 || item.ModItem is Helmet;

		Append(helmet);

		var necklace = new UIHoverImageItemSlot(DefaultFrameTexture, NecklaceIconTexture, ref Player.armor, 5, $"Mods.{PoTMod.ModName}.UI.Slots.5", ItemSlot.Context.EquipAccessory)
		{
			HAlign = 1f,
			VAlign = 0f,
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};

		necklace.OnMouseOver += UpdateMouseOver;
		necklace.OnMouseOut += UpdateMouseOut;

		necklace.Predicate = (item, _) => item.ModItem is Amulet;

		Append(necklace);
		
		var weapon = new UIHoverImageItemSlot(DefaultFrameTexture, WeaponIconTexture, ref Player.inventory, 0, $"Mods.{PoTMod.ModName}.UI.Slots.4")
		{
			HAlign = 0f,
			VAlign = 0.33f,
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};

		weapon.OnMouseOver += UpdateMouseOver;
		weapon.OnMouseOut += UpdateMouseOut;

		weapon.Predicate = (item, _) => item.damage > 0;

		Append(weapon);

		var chest = new UIHoverImageItemSlot(DefaultFrameTexture, ChestIconTexture, ref Player.armor, 1, $"Mods.{PoTMod.ModName}.UI.Slots.1", ItemSlot.Context.EquipArmor)
		{
			HAlign = 0.5f,
			VAlign = 0.33f,
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};

		chest.OnMouseOver += UpdateMouseOver;
		chest.OnMouseOut += UpdateMouseOut;

		chest.Predicate = (item, _) => item.bodySlot > 0 || item.ModItem is Chestplate;

		Append(chest);

		var offhand = new UIHoverImageItemSlot(DefaultFrameTexture, OffhandIconTexture, ref Player.armor, 6, $"Mods.{PoTMod.ModName}.UI.Slots.6", ItemSlot.Context.EquipAccessory)
		{
			HAlign = 1f,
			VAlign = 0.33f,
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};

		offhand.OnMouseOver += UpdateMouseOver;
		offhand.OnMouseOut += UpdateMouseOut;

		offhand.Predicate = (item, _) => item.ModItem is Offhand;

		Append(offhand);

		var leftRing = new UIHoverImageItemSlot(DefaultFrameTexture, RingIconTexture, ref Player.armor, 7, $"Mods.{PoTMod.ModName}.UI.Slots.7", ItemSlot.Context.EquipAccessory)
		{
			HAlign = 0f,
			VAlign = 0.66f,
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};
		
		leftRing.OnMouseOver += UpdateMouseOver;
		leftRing.OnMouseOut += UpdateMouseOut;

		leftRing.Predicate = (item, _) => item.ModItem is Ring;

		Append(leftRing);

		var legs = new UIHoverImageItemSlot(DefaultFrameTexture, LegsIconTexture, ref Player.armor, 2, $"Mods.{PoTMod.ModName}.UI.Slots.2", ItemSlot.Context.EquipArmor)
		{
			HAlign = 0.5f,
			VAlign = 0.66f,
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};

		legs.OnMouseOver += UpdateMouseOver;
		legs.OnMouseOut += UpdateMouseOut;

		legs.Predicate = (item, _) => item.legSlot > 0 || item.ModItem is Leggings;

		Append(legs);

		var rightRing = new UIHoverImageItemSlot(DefaultFrameTexture, RingIconTexture, ref Player.armor, 8, $"Mods.{PoTMod.ModName}.UI.Slots.8", ItemSlot.Context.EquipAccessory)
		{
			HAlign = 1f,
			VAlign = 0.66f,
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};

		rightRing.OnMouseOver += UpdateMouseOver;
		rightRing.OnMouseOut += UpdateMouseOut;

		rightRing.Predicate = (item, _) => item.ModItem is Ring;

		Append(rightRing);

		var leftMiscellaneous = new UIHoverImageItemSlot(DefaultFrameTexture, MiscellaneousIconTexture, ref Player.armor, 9, $"Mods.{PoTMod.ModName}.UI.Slots.9", ItemSlot.Context.EquipAccessory)
		{
			VAlign = 1f,
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};
		
		leftMiscellaneous.OnMouseOver += UpdateMouseOver;
		leftMiscellaneous.OnMouseOut += UpdateMouseOut;
		
		Append(leftMiscellaneous);
		
		var middleMiscellaneous = new UIHoverImageItemSlot(DefaultFrameTexture, MiscellaneousIconTexture, ref Player.armor, 3, $"Mods.{PoTMod.ModName}.UI.Slots.3", ItemSlot.Context.EquipAccessory)
		{
			HAlign = 0.5f,
			VAlign = 1f,
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};
		
		middleMiscellaneous.OnMouseOver += UpdateMouseOver;
		middleMiscellaneous.OnMouseOut += UpdateMouseOut;
		
		Append(middleMiscellaneous);
	}
}