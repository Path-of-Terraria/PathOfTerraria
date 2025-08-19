using PathOfTerraria.Common.UI.Elements;
using PathOfTerraria.Content.Items.Gear.Amulets;
using PathOfTerraria.Content.Items.Gear.Armor.Chestplate;
using PathOfTerraria.Content.Items.Gear.Armor.Helmet;
using PathOfTerraria.Content.Items.Gear.Armor.Leggings;
using PathOfTerraria.Content.Items.Gear.Offhands;
using PathOfTerraria.Content.Items.Gear.Rings;
using ReLogic.Content;
using Terraria.Localization;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.Armor.Elements;

public sealed class UIDefaultArmor : UIArmorPage
{
	public static readonly Asset<Texture2D> DefaultFrameTexture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/Inventory/Frame_Default", AssetRequestMode.ImmediateLoad);

	//This is to update the slot once WoF is killed. 
	private UICustomHoverImageItemSlot accessorySlot4;
	private bool wasHardMode = false;

	public override void OnInitialize()
	{
		base.OnInitialize();

		Player.extraAccessorySlots = 3;

		Width = StyleDimension.FromPixels(UIArmorInventory.ArmorPageWidth);
		Height = StyleDimension.FromPixels(UIArmorInventory.ArmorPageHeight);

		var wings = new UIHoverImageItemSlot(DefaultFrameTexture, WingsIconTexture, ref Player.armor, 4, $"Mods.{PoTMod.ModName}.UI.Slots.10", ItemSlot.Context.EquipAccessory)
		{
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};

		wings.OnMouseOver += UpdateMouseOver;
		wings.OnMouseOut += UpdateMouseOut;
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
		Append(necklace);
		
		var weapon = new UIHoverImageItemSlot(DefaultFrameTexture, WeaponIconTexture, ref Player.inventory, 0, $"Mods.{PoTMod.ModName}.UI.Slots.4")
		{
			HAlign = 0f,
			VAlign = 0.25f,
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};

		weapon.OnMouseOver += UpdateMouseOver;
		weapon.OnMouseOut += UpdateMouseOut;
		Append(weapon);

		var chest = new UIHoverImageItemSlot(DefaultFrameTexture, ChestIconTexture, ref Player.armor, 1, $"Mods.{PoTMod.ModName}.UI.Slots.1", ItemSlot.Context.EquipArmor)
		{
			HAlign = 0.5f,
			VAlign = 0.25f,
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};

		chest.OnMouseOver += UpdateMouseOver;
		chest.OnMouseOut += UpdateMouseOut;
		Append(chest);

		var offhand = new UIHoverImageItemSlot(DefaultFrameTexture, OffhandIconTexture, ref Player.armor, 6, $"Mods.{PoTMod.ModName}.UI.Slots.6", ItemSlot.Context.EquipAccessory)
		{
			HAlign = 1f,
			VAlign = 0.25f,
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};

		offhand.OnMouseOver += UpdateMouseOver;
		offhand.OnMouseOut += UpdateMouseOut;
		Append(offhand);

		var leftRing = new UIHoverImageItemSlot(DefaultFrameTexture, RingIconTexture, ref Player.armor, 7, $"Mods.{PoTMod.ModName}.UI.Slots.7", ItemSlot.Context.EquipAccessory)
		{
			HAlign = 0f,
			VAlign = 0.5f,
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};
		
		leftRing.OnMouseOver += UpdateMouseOver;
		leftRing.OnMouseOut += UpdateMouseOut;
		Append(leftRing);

		var legs = new UIHoverImageItemSlot(DefaultFrameTexture, LegsIconTexture, ref Player.armor, 2, $"Mods.{PoTMod.ModName}.UI.Slots.2", ItemSlot.Context.EquipArmor)
		{
			HAlign = 0.5f,
			VAlign = 0.5f,
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};

		legs.OnMouseOver += UpdateMouseOver;
		legs.OnMouseOut += UpdateMouseOut;
		Append(legs);

		var rightRing = new UIHoverImageItemSlot(DefaultFrameTexture, RingIconTexture, ref Player.armor, 8, $"Mods.{PoTMod.ModName}.UI.Slots.8", ItemSlot.Context.EquipAccessory)
		{
			HAlign = 1f,
			VAlign = 0.5f,
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};

		rightRing.OnMouseOver += UpdateMouseOver;
		rightRing.OnMouseOut += UpdateMouseOut;
		Append(rightRing);

		var accessorySlot1 = new UIHoverImageItemSlot(DefaultFrameTexture, MiscellaneousIconTexture, ref Player.armor, 3, $"Mods.{PoTMod.ModName}.UI.Slots.3", ItemSlot.Context.EquipAccessory)
		{
			VAlign = .75f,
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};
		
		accessorySlot1.OnMouseOver += UpdateMouseOver;
		accessorySlot1.OnMouseOut += UpdateMouseOut;
		Append(accessorySlot1);
		
		var accessorySlot2 = new UIHoverImageItemSlot(DefaultFrameTexture, MiscellaneousIconTexture, ref Player.armor, 9, $"Mods.{PoTMod.ModName}.UI.Slots.9", ItemSlot.Context.EquipAccessory)
		{
			HAlign = 0.5f,
			VAlign = .75f,
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};
		
		accessorySlot2.OnMouseOver += UpdateMouseOver;
		accessorySlot2.OnMouseOut += UpdateMouseOut;
		Append(accessorySlot2);
		
		var accessorySlot3 = new UICustomHoverImageItemSlot(DefaultFrameTexture, MiscellaneousIconTexture, 20, $"Mods.{PoTMod.ModName}.UI.Slots.11", ItemSlot.Context.EquipAccessory)
		{
			HAlign = 1f,
			VAlign = .75f,
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};
		
		accessorySlot3.OnMouseOver += UpdateMouseOver;
		accessorySlot3.OnMouseOut += UpdateMouseOut;
		Append(accessorySlot3);
		
		accessorySlot4 = new UICustomHoverImageItemSlot(DefaultFrameTexture, MiscellaneousIconTexture,  21, $"Mods.{PoTMod.ModName}.UI.Slots.12", ItemSlot.Context.EquipAccessory)
		{
			HAlign = 0,
			VAlign = 1.0f,
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};
		
		accessorySlot4.OnMouseOver += UpdateMouseOver;
		accessorySlot4.OnMouseOut += UpdateMouseOut;
		
		wasHardMode = Main.hardMode;
		if (wasHardMode)
		{
			Append(accessorySlot4);	
		}

	}
	
	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		if (Main.hardMode != wasHardMode)
		{
			if (Main.hardMode)
			{
				// Make sure the slot is properly initialized before appending
				if (accessorySlot4.Icon == null)
				{
					accessorySlot4.OnInitialize();
				}
				Append(accessorySlot4);
			}
			else
			{
				RemoveChild(accessorySlot4);
			}
			wasHardMode = Main.hardMode;
		}
	}

}