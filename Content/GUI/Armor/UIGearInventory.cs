using System.Collections.Generic;
using System.Reflection;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using PathOfTerraria.Content.GUI.Elements;
using PathOfTerraria.Core.Loaders.UILoading;
using PathOfTerraria.Helpers.Extensions;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;

namespace PathOfTerraria.Content.GUI;

public sealed class UIGearInventory : UIState
{
	public static readonly Asset<Texture2D> LeftButtonTexture = ModContent.Request<Texture2D>(
		$"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/LeftButton",
		AssetRequestMode.ImmediateLoad
	);
	
	public const float InventoryWidth = 160f;
	public const float InventoryHeight = 190f;

	public const float ActiveRotation = MathHelper.Pi / 16f;
	public const float InactiveRotation = 0f;

	public const float ActiveScale = 1.15f;
	public const float InactiveScale = 1f;

	public const float Smoothness = 0.3f;

	private UIElement root;
	private UIElement[] pages = { BuildDefaultInventory(), BuildVanityInventory(), BuildDyeInventory() };

	private int currentPage;
 
	/// <summary>
	///		The index of the current gear page. <br></br>
	///		<c>0</c> - Default <br></br>
	///		<c>1</c> - Vanity <br></br>
	///		<c>2</c> - Dye
	/// </summary>
	/// <remarks>
	///		This will be reset to 2 if you go out of negative bounds, and 0 out of
	///		positive bounds.
	/// </remarks>
	public int CurrentPage
	{
		get => currentPage;
		set
		{
			var min = 0;
			var max = pages.Length - 1;
			
			if (value < min)
			{
				currentPage = max;
			}
			else if (value > max)
			{
				currentPage = min;
			}
			else
			{
				currentPage = (int)MathHelper.Clamp(value, min, max);
			}
		}
	}

	public override void OnInitialize()
	{
		Player player = Main.CurrentPlayer;

		root = new UIElement
		{
			Width = StyleDimension.FromPixels(InventoryWidth),
			Height = StyleDimension.FromPixels(InventoryHeight + 40f),
			Left = StyleDimension.FromPixels(Main.screenWidth + InventoryWidth + 40f),
			Top = StyleDimension.FromPixels(Main.mapStyle == 1 ? 436f : 180f)
		};
		
		Append(root);

		var leftButton = new UIImage(LeftButtonTexture)
		{
			OverrideSamplerState = SamplerState.PointClamp,
			NormalizedOrigin = new Vector2(0.5f),
			HAlign = 0f,
			VAlign = 1f
		};

		leftButton.OnLeftClick += UpdatePageLeftButton;
		leftButton.OnMouseOver += UpdateMouseOver;
		leftButton.OnMouseOut += UpdateMouseOut;
		leftButton.OnUpdate += UpdateImageHoverEffects;
		
		root.Append(leftButton);
		
		var rightButton = new UIImage(LeftButtonTexture)
		{
			OverrideSamplerState = SamplerState.PointClamp,
			NormalizedOrigin = new Vector2(0.5f),
			HAlign = 1f,
			VAlign = 1f
		};

		rightButton.OnLeftClick += UpdatePageRightButton;
		rightButton.OnMouseOver += UpdateMouseOver;
		rightButton.OnMouseOut += UpdateMouseOut;
		rightButton.OnUpdate += UpdateImageHoverEffects;
		
		root.Append(rightButton);
		
		for (var i = 0; i < pages.Length; i++)
		{
			var page = pages[i];
			
			root.Append(page);
		}
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		for (var i = 0; i < pages.Length; i++)
		{
			var page = pages[i];
			
			UpdatePagePosition(page, i);
		}
		
		UpdateRootPosition();
	}

	private void UpdatePagePosition(UIElement page, int index)
	{
		var pageEnabled = CurrentPage == index;
		
		var left = pageEnabled ? 0f : InventoryWidth + 40f;
		var top = 0f;

		page.Left.Set(MathHelper.SmoothStep(page.Left.Pixels, left, Smoothness), 0f);
		page.Top.Set(MathHelper.SmoothStep(page.Top.Pixels, top, Smoothness), 0f);
	}

	private void UpdateRootPosition()
	{
		var inventoryEnabled = Main.playerInventory && Main.EquipPage == 0;
		
		var left = inventoryEnabled ? Main.screenWidth - InventoryWidth - 40f : Main.screenWidth;
		var top = Main.mapStyle == 1 ? 446f : 190f;

		root.Left.Set(MathHelper.SmoothStep(root.Left.Pixels, left, Smoothness), 0f);
		root.Top.Set(MathHelper.SmoothStep(root.Top.Pixels, top, Smoothness), 0f);
	}

	private void UpdatePageLeftButton(UIMouseEvent @event, UIElement element)
	{
		SoundEngine.PlaySound(
			SoundID.MenuTick with
			{
				Pitch = 0.25f,
				MaxInstances = 1
			}
		);
		
		CurrentPage--;
	}

	private void UpdatePageRightButton(UIMouseEvent @event, UIElement element)
	{
		SoundEngine.PlaySound(
			SoundID.MenuTick with
			{
				Pitch = 0.25f,
				MaxInstances = 1
			}
		);
		
		CurrentPage++;
	}
	
	private static void UpdateMouseOver(UIMouseEvent @event, UIElement element)
	{
		SoundEngine.PlaySound(
			SoundID.MenuTick with
			{
				Pitch = 0.15f,
				MaxInstances = 1
			}
		);
	}

	private static void UpdateMouseOut(UIMouseEvent @event, UIElement element)
	{
		SoundEngine.PlaySound(
			SoundID.MenuTick with
			{
				Pitch = -0.25f,
				MaxInstances = 1
			}
		);
	}
	
	private static void UpdateSlotHoverEffects(UIElement element)
	{
		if (element is not UICustomItemSlot slot)
		{
			return;
		}
		
		slot.Icon.Rotation = MathHelper.SmoothStep(slot.Icon.Rotation, slot.IsMouseHovering ? ActiveRotation : InactiveRotation, Smoothness);
		slot.Icon.ImageScale = MathHelper.SmoothStep(slot.Icon.ImageScale, slot.IsMouseHovering ? ActiveScale : InactiveScale, Smoothness);
		
		slot.Background.Rotation = MathHelper.SmoothStep(slot.Background.Rotation, slot.IsMouseHovering ? ActiveRotation : InactiveRotation, Smoothness);
		slot.Background.ImageScale = MathHelper.SmoothStep(slot.Background.ImageScale, slot.IsMouseHovering ? ActiveScale : InactiveScale, Smoothness);
	}

	private static void UpdateImageHoverEffects(UIElement element)
	{
		if (element is not UIImage image)
		{
			return;
		}
		
		image.Rotation = MathHelper.SmoothStep(image.Rotation, image.IsMouseHovering ? ActiveRotation : InactiveRotation, Smoothness);
		image.ImageScale = MathHelper.SmoothStep(image.ImageScale, image.IsMouseHovering ? ActiveScale : InactiveScale, Smoothness);
	}

	private static UIElement BuildDefaultInventory()
	{
		var inventory = new UIElement
		{
			Width = StyleDimension.FromPixels(InventoryWidth),
			Height = StyleDimension.FromPixels(InventoryHeight)
		};

		Asset<Texture2D> frame = ModContent.Request<Texture2D>(
	        $"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Frame",
	        AssetRequestMode.ImmediateLoad
	    );
	    
	    Asset<Texture2D> icon = ModContent.Request<Texture2D>(
	        $"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Wings",
	        AssetRequestMode.ImmediateLoad
	    );

	    var wings = new UICustomItemSlot(frame, icon, ItemSlot.Context.EquipAccessory) { Slot = 4 };
	    
	    wings.OnMouseOver += UpdateMouseOver;
	    wings.OnMouseOut += UpdateMouseOut;
	    wings.OnUpdate += UpdateSlotHoverEffects;

	    wings.InventoryGetter = (player) => ref player.armor;
	    wings.Predicate = (item, _) => item.wingSlot > 0;

	    inventory.Append(wings);
	    
	    icon = ModContent.Request<Texture2D>(
	        $"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Helmet",
	        AssetRequestMode.ImmediateLoad
	    );

	    var helmet = new UICustomItemSlot(frame, icon, ItemSlot.Context.EquipArmor)
	    {
	        HAlign = 0.5f,
	        VAlign = 0f
	    };

	    helmet.OnMouseOver += UpdateMouseOver;
	    helmet.OnMouseOut += UpdateMouseOut;
	    helmet.OnUpdate += UpdateSlotHoverEffects;
	    
	    helmet.InventoryGetter = (player) => ref player.armor;
	    helmet.Predicate = (item, _) => item.headSlot > 0;

	    inventory.Append(helmet);
	    
	    icon = ModContent.Request<Texture2D>(
	        $"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Necklace",
	        AssetRequestMode.ImmediateLoad
	    );

	    var necklace = new UICustomItemSlot(frame, icon, ItemSlot.Context.EquipAccessory)
	    {
	        HAlign = 1f,
	        VAlign = 0f,
	        Slot = 5
	    };

	    necklace.OnMouseOver += UpdateMouseOver;
	    necklace.OnMouseOut += UpdateMouseOut;
	    necklace.OnUpdate += UpdateSlotHoverEffects;

	    necklace.InventoryGetter = (player) => ref player.armor;
	    necklace.Predicate = (item, _) => item.accessory && item.wingSlot <= 0;

	    inventory.Append(necklace);
	    
	    icon = ModContent.Request<Texture2D>(
	        $"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Left_Hand", 
	        AssetRequestMode.ImmediateLoad
	    );

	    var weapon = new UICustomItemSlot(frame, icon)
	    {
	        HAlign = 0f,
	        VAlign = 0.5f
	    };

	    weapon.OnMouseOver += UpdateMouseOver;
	    weapon.OnMouseOut += UpdateMouseOut;
	    weapon.OnUpdate += UpdateSlotHoverEffects;

	    weapon.InventoryGetter = (player) => ref player.inventory;
	    weapon.Predicate = (item, _) => item.damage > 0;

	    inventory.Append(weapon);

	    icon = ModContent.Request<Texture2D>(
	        $"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Chest", 
	        AssetRequestMode.ImmediateLoad
	    );

	    var chest = new UICustomItemSlot(frame, icon, ItemSlot.Context.EquipArmor)
	    {
	        HAlign = 0.5f,
	        VAlign = 0.5f,
	        Slot = 1
	    };

	    chest.OnMouseOver += UpdateMouseOver;
	    chest.OnMouseOut += UpdateMouseOut;
	    chest.OnUpdate += UpdateSlotHoverEffects;

	    chest.InventoryGetter = (player) => ref player.armor;
	    chest.Predicate = (item, _) => item.bodySlot > 0;

	    inventory.Append(chest);

	    icon = ModContent.Request<Texture2D>(
	        $"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Right_Hand", 
	        AssetRequestMode.ImmediateLoad
	    );

	    var offhand = new UICustomItemSlot(frame, icon)
	    {
	        HAlign = 1f,
	        VAlign = 0.5f,
	        Slot = 2
	    };

	    offhand.OnMouseOver += UpdateMouseOver;
	    offhand.OnMouseOut += UpdateMouseOut;
	    offhand.OnUpdate += UpdateSlotHoverEffects;
	    
	    offhand.InventoryGetter = (player) => ref player.inventory;
	    offhand.Predicate = (item, _) => item.pick > 0;

	    inventory.Append(offhand);
	    
	    icon = ModContent.Request<Texture2D>(
	        $"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Ring", 
	        AssetRequestMode.ImmediateLoad
	    );

	    var leftRing = new UICustomItemSlot(frame, icon, ItemSlot.Context.EquipAccessory)
	    {
	        HAlign = 0f,
	        VAlign = 1f,
	        Slot = 6
	    };

	    leftRing.OnMouseOver += UpdateMouseOver;
	    leftRing.OnMouseOut += UpdateMouseOut;
	    leftRing.OnUpdate += UpdateSlotHoverEffects;

	    leftRing.InventoryGetter = (player) => ref player.armor;
	    leftRing.Predicate = (item, _) => item.accessory && item.wingSlot <= 0;

	    inventory.Append(leftRing);
	    
	    icon = ModContent.Request<Texture2D>(
	        $"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Legs", 
	        AssetRequestMode.ImmediateLoad
	    );

	    var legs = new UICustomItemSlot(frame, icon, ItemSlot.Context.EquipArmor)
	    {
	        HAlign = 0.5f,
	        VAlign = 1f,
	        Slot = 2
	    };

	    legs.OnMouseOver += UpdateMouseOver;
	    legs.OnMouseOut += UpdateMouseOut;
	    legs.OnUpdate += UpdateSlotHoverEffects;

	    legs.InventoryGetter = (player) => ref player.armor;
	    legs.Predicate = (item, _) => item.legSlot > 0;

	    inventory.Append(legs);
	    
	    icon = ModContent.Request<Texture2D>(
	        $"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Ring", 
	        AssetRequestMode.ImmediateLoad
	    );

	    var rightRing = new UICustomItemSlot(frame, icon, ItemSlot.Context.EquipAccessory)
	    {
	        HAlign = 1f,
	        VAlign = 1f,
	        Slot = 7
	    };

	    rightRing.OnMouseOver += UpdateMouseOver;
	    rightRing.OnMouseOut += UpdateMouseOut;
	    rightRing.OnUpdate += UpdateSlotHoverEffects;

	    rightRing.InventoryGetter = (player) => ref player.armor;
	    rightRing.Predicate = (item, _) => item.accessory && item.wingSlot <= 0;

	    inventory.Append(rightRing);

	    return inventory;
	}
	
	private static UIElement BuildVanityInventory()
	{
		var inventory = new UIElement()
		{
			Width = StyleDimension.FromPixels(InventoryWidth),
			Height = StyleDimension.FromPixels(InventoryHeight)
		};

		Asset<Texture2D> frame = ModContent.Request<Texture2D>(
	        $"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Frame_Vanity",
	        AssetRequestMode.ImmediateLoad
	    );
	    
	    Asset<Texture2D> icon = ModContent.Request<Texture2D>(
	        $"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Wings",
	        AssetRequestMode.ImmediateLoad
	    );

	    var wings = new UICustomItemSlot(frame, icon, ItemSlot.Context.EquipAccessoryVanity) { Slot = 14 };
	    
	    wings.OnMouseOver += UpdateMouseOver;
	    wings.OnMouseOut += UpdateMouseOut;
	    wings.OnUpdate += UpdateSlotHoverEffects;

	    wings.InventoryGetter = (player) => ref player.armor;
	    wings.Predicate = (item, _) => item.wingSlot > 0;

	    inventory.Append(wings);
	    
	    icon = ModContent.Request<Texture2D>(
	        $"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Helmet",
	        AssetRequestMode.ImmediateLoad
	    );

	    var helmet = new UICustomItemSlot(frame, icon, ItemSlot.Context.EquipArmorVanity)
	    {
	        HAlign = 0.5f,
	        VAlign = 0f,
	        Slot = 10
	    };

	    helmet.OnMouseOver += UpdateMouseOver;
	    helmet.OnMouseOut += UpdateMouseOut;
	    helmet.OnUpdate += UpdateSlotHoverEffects;

	    helmet.InventoryGetter = (player) => ref player.armor;
	    helmet.Predicate = (item, _) => item.headSlot > 0;

	    inventory.Append(helmet);
	    
	    icon = ModContent.Request<Texture2D>(
	        $"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Necklace",
	        AssetRequestMode.ImmediateLoad
	    );

	    var necklace = new UICustomItemSlot(frame, icon, ItemSlot.Context.EquipAccessoryVanity)
	    {
	        HAlign = 1f,
	        VAlign = 0f,
	        Slot = 15
	    };

	    necklace.OnMouseOver += UpdateMouseOver;
	    necklace.OnMouseOut += UpdateMouseOut;
	    necklace.OnUpdate += UpdateSlotHoverEffects;

	    necklace.InventoryGetter = (player) => ref player.armor;
	    necklace.Predicate = (item, _) => item.accessory && item.wingSlot <= 0;

	    inventory.Append(necklace);

	    icon = ModContent.Request<Texture2D>(
	        $"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Chest", 
	        AssetRequestMode.ImmediateLoad
	    );

	    var chest = new UICustomItemSlot(frame, icon, ItemSlot.Context.EquipArmorVanity)
	    {
	        HAlign = 0.5f,
	        VAlign = 0.5f,
	        Slot = 11
	    };

	    chest.OnMouseOver += UpdateMouseOver;
	    chest.OnMouseOut += UpdateMouseOut;
	    chest.OnUpdate += UpdateSlotHoverEffects;

	    chest.InventoryGetter = (player) => ref player.armor;
	    chest.Predicate = (item, _) => item.bodySlot > 0;

	    inventory.Append(chest);

	    icon = ModContent.Request<Texture2D>(
	        $"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Ring", 
	        AssetRequestMode.ImmediateLoad
	    );

	    var leftRing = new UICustomItemSlot(frame, icon, ItemSlot.Context.EquipAccessoryVanity)
	    {
	        HAlign = 0f,
	        VAlign = 1f,
	        Slot = 16
	    };

	    leftRing.OnMouseOver += UpdateMouseOver;
	    leftRing.OnMouseOut += UpdateMouseOut;
	    leftRing.OnUpdate += UpdateSlotHoverEffects;

	    leftRing.InventoryGetter = (player) => ref player.armor;
	    leftRing.Predicate = (item, _) => item.accessory && item.wingSlot <= 0;

	    inventory.Append(leftRing);
	    
	    icon = ModContent.Request<Texture2D>(
	        $"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Legs", 
	        AssetRequestMode.ImmediateLoad
	    );

	    var legs = new UICustomItemSlot(frame, icon, ItemSlot.Context.EquipArmorVanity)
	    {
	        HAlign = 0.5f,
	        VAlign = 1f,
	        Slot = 12
	    };

	    legs.OnMouseOver += UpdateMouseOver;
	    legs.OnMouseOut += UpdateMouseOut;
	    legs.OnUpdate += UpdateSlotHoverEffects;

	    legs.InventoryGetter = (player) => ref player.armor;
	    legs.Predicate = (item, _) => item.legSlot > 0;

	    inventory.Append(legs);
	    
	    icon = ModContent.Request<Texture2D>(
	        $"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Ring", 
	        AssetRequestMode.ImmediateLoad
	    );

	    var rightRing = new UICustomItemSlot(frame, icon, ItemSlot.Context.EquipAccessoryVanity)
	    {
	        HAlign = 1f,
	        VAlign = 1f,
	        Slot = 17
	    };

	    rightRing.OnMouseOver += UpdateMouseOver;
	    rightRing.OnMouseOut += UpdateMouseOut;
	    rightRing.OnUpdate += UpdateSlotHoverEffects;

	    rightRing.InventoryGetter = (player) => ref player.armor;
	    rightRing.Predicate = (item, _) => item.accessory && item.wingSlot <= 0;

	    inventory.Append(rightRing);

	    return inventory;
	}
	
	private static UIElement BuildDyeInventory()
	{
		var inventory = new UIElement()
		{
			Width = StyleDimension.FromPixels(InventoryWidth),
			Height = StyleDimension.FromPixels(InventoryHeight)
		};

		Asset<Texture2D> frame = ModContent.Request<Texture2D>(
	        $"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Frame_Dye",
	        AssetRequestMode.ImmediateLoad
	    );
	    
	    Asset<Texture2D> icon = ModContent.Request<Texture2D>(
	        $"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Wings",
	        AssetRequestMode.ImmediateLoad
	    );

	    var wings = new UICustomItemSlot(frame, icon, ItemSlot.Context.EquipDye) { Slot = 4 };
	    
	    wings.OnMouseOver += UpdateMouseOver;
	    wings.OnMouseOut += UpdateMouseOut;
	    wings.OnUpdate += UpdateSlotHoverEffects;

	    wings.InventoryGetter = (player) => ref player.dye;
	    wings.Predicate = (item, _) => item.dye > 0;

	    inventory.Append(wings);
	    
	    icon = ModContent.Request<Texture2D>(
	        $"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Helmet",
	        AssetRequestMode.ImmediateLoad
	    );

	    var helmet = new UICustomItemSlot(frame, icon, ItemSlot.Context.EquipDye)
	    {
	        HAlign = 0.5f,
	        VAlign = 0f
	    };

	    helmet.OnMouseOver += UpdateMouseOver;
	    helmet.OnMouseOut += UpdateMouseOut;
	    helmet.OnUpdate += UpdateSlotHoverEffects;
	    
	    helmet.InventoryGetter = (player) => ref player.dye;
	    helmet.Predicate = (item, _) => item.dye > 0;

	    inventory.Append(helmet);
	    
	    icon = ModContent.Request<Texture2D>(
	        $"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Necklace",
	        AssetRequestMode.ImmediateLoad
	    );

	    var necklace = new UICustomItemSlot(frame, icon, ItemSlot.Context.EquipDye)
	    {
	        HAlign = 1f,
	        VAlign = 0f,
	        Slot = 5
	    };

	    necklace.OnMouseOver += UpdateMouseOver;
	    necklace.OnMouseOut += UpdateMouseOut;
	    necklace.OnUpdate += UpdateSlotHoverEffects;

	    necklace.InventoryGetter = (player) => ref player.dye;
	    necklace.Predicate = (item, _) => item.dye > 0;

	    inventory.Append(necklace);

	    icon = ModContent.Request<Texture2D>(
	        $"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Chest", 
	        AssetRequestMode.ImmediateLoad
	    );

	    var chest = new UICustomItemSlot(frame, icon, ItemSlot.Context.EquipDye)
	    {
	        HAlign = 0.5f,
	        VAlign = 0.5f,
	        Slot = 1
	    };

	    chest.OnMouseOver += UpdateMouseOver;
	    chest.OnMouseOut += UpdateMouseOut;
	    chest.OnUpdate += UpdateSlotHoverEffects;

	    chest.InventoryGetter = (player) => ref player.dye;
	    chest.Predicate = (item, _) => item.dye > 0;

	    inventory.Append(chest);

	    icon = ModContent.Request<Texture2D>(
	        $"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Ring", 
	        AssetRequestMode.ImmediateLoad
	    );

	    var leftRing = new UICustomItemSlot(frame, icon, ItemSlot.Context.EquipDye)
	    {
	        HAlign = 0f,
	        VAlign = 1f,
	        Slot = 6
	    };

	    leftRing.OnMouseOver += UpdateMouseOver;
	    leftRing.OnMouseOut += UpdateMouseOut;
	    leftRing.OnUpdate += UpdateSlotHoverEffects;

	    leftRing.InventoryGetter = (player) => ref player.dye;
	    leftRing.Predicate = (item, _) => item.dye > 0;

	    inventory.Append(leftRing);
	    
	    icon = ModContent.Request<Texture2D>(
	        $"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Legs", 
	        AssetRequestMode.ImmediateLoad
	    );

	    var legs = new UICustomItemSlot(frame, icon, ItemSlot.Context.EquipDye)
	    {
	        HAlign = 0.5f,
	        VAlign = 1f,
	        Slot = 2
	    };

	    legs.OnMouseOver += UpdateMouseOver;
	    legs.OnMouseOut += UpdateMouseOut;
	    legs.OnUpdate += UpdateSlotHoverEffects;

	    legs.InventoryGetter = (player) => ref player.dye;
	    legs.Predicate = (item, _) => item.dye > 0;

	    inventory.Append(legs);
	    
	    icon = ModContent.Request<Texture2D>(
	        $"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Ring", 
	        AssetRequestMode.ImmediateLoad
	    );

	    var rightRing = new UICustomItemSlot(frame, icon, ItemSlot.Context.EquipDye)
	    {
	        HAlign = 1f,
	        VAlign = 1f,
	        Slot = 7
	    };

	    rightRing.OnMouseOver += UpdateMouseOver;
	    rightRing.OnMouseOut += UpdateMouseOut;
	    rightRing.OnUpdate += UpdateSlotHoverEffects;
	    
	    rightRing.InventoryGetter = (player) => ref player.dye;
	    rightRing.Predicate = (item, _) => item.dye > 0;

	    inventory.Append(rightRing);

	    return inventory;
	}
}