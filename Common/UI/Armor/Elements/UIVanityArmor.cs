using PathOfTerraria.Common.UI.Elements;
using PathOfTerraria.Content.Items.Gear.Amulets;
using PathOfTerraria.Content.Items.Gear.Offhands;
using PathOfTerraria.Content.Items.Gear.Rings;
using ReLogic.Content;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.Armor.Elements;

public sealed class UIVanityArmor : UIArmorPage
{
	public static readonly Asset<Texture2D> VanityFrameTexture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/Inventory/Frame_Vanity", AssetRequestMode.ImmediateLoad);

	private UICustomHoverImageItemSlot[] customAccessorySlots = [];

	public override void OnInitialize()
	{
		base.OnInitialize();

		Width = StyleDimension.FromPixels(UIArmorInventory.ArmorPageWidth);
		Height = StyleDimension.FromPixels(UIArmorInventory.ArmorPageHeight);

		var wings = new UIHoverImageItemSlot(VanityFrameTexture, WingsIconTexture, ref Player.armor, 14, $"Mods.{PoTMod.ModName}.UI.Slots.10", ItemSlot.Context.EquipAccessoryVanity)
		{
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};

		wings.OnMouseOver += UpdateMouseOver;
		wings.OnMouseOut += UpdateMouseOut;
		wings.Predicate = (item, _) => item.wingSlot > 0;
		Append(wings);

		var helmet = new UIHoverImageItemSlot(VanityFrameTexture, HelmetIconTexture, ref Player.armor, 10, $"Mods.{PoTMod.ModName}.UI.Slots.0", ItemSlot.Context.EquipArmorVanity)
		{
			HAlign = 0.5f,
			VAlign = 0f,
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};

		helmet.OnMouseOver += UpdateMouseOver;
		helmet.OnMouseOut += UpdateMouseOut;
		helmet.Predicate = (item, _) => item.headSlot > 0;
		Append(helmet);

		var necklace = new UIHoverImageItemSlot(VanityFrameTexture, NecklaceIconTexture, ref Player.armor, 15, $"Mods.{PoTMod.ModName}.UI.Slots.5", ItemSlot.Context.EquipAccessoryVanity)
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

		// Chest vanity slot (corresponds to functional slot 1)
		var chest = new UIHoverImageItemSlot(VanityFrameTexture, ChestIconTexture, ref Player.armor, 11, $"Mods.{PoTMod.ModName}.UI.Slots.1", ItemSlot.Context.EquipArmorVanity)
		{
			HAlign = 0.5f,
			VAlign = 0.25f,
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};

		chest.OnMouseOver += UpdateMouseOver;
		chest.OnMouseOut += UpdateMouseOut;
		chest.Predicate = (item, _) => item.bodySlot > 0;
		Append(chest);

		var offhand = new UIHoverImageItemSlot(VanityFrameTexture, OffhandIconTexture, ref Player.armor, 16, $"Mods.{PoTMod.ModName}.UI.Slots.6", ItemSlot.Context.EquipAccessoryVanity)
		{
			HAlign = 1f,
			VAlign = 0.25f,
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};

		offhand.OnMouseOver += UpdateMouseOver;
		offhand.OnMouseOut += UpdateMouseOut;
		offhand.Predicate = (item, _) => item.ModItem is Offhand;
		Append(offhand);

		var leftRing = new UIHoverImageItemSlot(VanityFrameTexture, RingIconTexture, ref Player.armor, 17, $"Mods.{PoTMod.ModName}.UI.Slots.7", ItemSlot.Context.EquipAccessoryVanity)
		{
			HAlign = 0f,
			VAlign = 0.50f,
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};

		leftRing.OnMouseOver += UpdateMouseOver;
		leftRing.OnMouseOut += UpdateMouseOut;
		leftRing.Predicate = (item, _) => item.ModItem is Ring;
		Append(leftRing);

		var legs = new UIHoverImageItemSlot(VanityFrameTexture, LegsIconTexture, ref Player.armor, 12, $"Mods.{PoTMod.ModName}.UI.Slots.2", ItemSlot.Context.EquipArmorVanity)
		{
			HAlign = 0.5f,
			VAlign = 0.50f,
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};

		legs.OnMouseOver += UpdateMouseOver;
		legs.OnMouseOut += UpdateMouseOut;
		legs.Predicate = (item, _) => item.legSlot > 0;
		Append(legs);

		var rightRing = new UIHoverImageItemSlot(VanityFrameTexture, RingIconTexture, ref Player.armor, 18, $"Mods.{PoTMod.ModName}.UI.Slots.8", ItemSlot.Context.EquipAccessoryVanity)
		{
			HAlign = 1f,
			VAlign = 0.5f,
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};

		rightRing.OnMouseOver += UpdateMouseOver;
		rightRing.OnMouseOut += UpdateMouseOut;
		rightRing.Predicate = (item, _) => item.ModItem is Ring;
		Append(rightRing);
		
		var accessorySlot1 = new UIHoverImageItemSlot(VanityFrameTexture, MiscellaneousIconTexture, ref Player.armor, 13, $"Mods.{PoTMod.ModName}.UI.Slots.3", ItemSlot.Context.EquipAccessoryVanity)
		{
			VAlign = .75f,
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};
		
		accessorySlot1.OnMouseOver += UpdateMouseOver;
		accessorySlot1.OnMouseOut += UpdateMouseOut;
		accessorySlot1.Predicate = (item, _) => item.accessory && item.wingSlot <= 0;
		Append(accessorySlot1);
		
		var accessorySlot2 = new UIHoverImageItemSlot(VanityFrameTexture, MiscellaneousIconTexture, ref Player.armor, 19, $"Mods.{PoTMod.ModName}.UI.Slots.9", ItemSlot.Context.EquipAccessoryVanity)
		{
			HAlign = 0.5f,
			VAlign = .75f,
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};
		
		accessorySlot2.OnMouseOver += UpdateMouseOver;
		accessorySlot2.OnMouseOut += UpdateMouseOut;
		accessorySlot2.Predicate = (item, _) => item.accessory && item.wingSlot <= 0;
		Append(accessorySlot2);
		
		var accessorySlot3 = new UICustomHoverImageItemSlot(VanityFrameTexture, MiscellaneousIconTexture, 20, $"Mods.{PoTMod.ModName}.UI.Slots.11", ItemSlot.Context.EquipAccessoryVanity)
		{
			HAlign = 1f,
			VAlign = .75f,
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};
		
		accessorySlot3.OnMouseOver += UpdateMouseOver;
		accessorySlot3.OnMouseOut += UpdateMouseOut;
		accessorySlot3.Predicate = (item, _) => item.accessory && item.wingSlot <= 0;	
		Append(accessorySlot3);

		var accessorySlot4 = new UICustomHoverImageItemSlot(VanityFrameTexture, MiscellaneousIconTexture, 21, $"Mods.{PoTMod.ModName}.UI.Slots.12", ItemSlot.Context.EquipAccessoryVanity)
		{
			HAlign = 0f,
			VAlign = 1f,
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};
		
		accessorySlot4.OnMouseOver += UpdateMouseOver;
		accessorySlot4.OnMouseOut += UpdateMouseOut;
		accessorySlot4.Predicate = (item, _) => item.accessory && item.wingSlot <= 0;	
		Append(accessorySlot4);

		customAccessorySlots = [ accessorySlot3, accessorySlot4 ];
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		MaintainCustomAccessorySlots(customAccessorySlots);
	}
}