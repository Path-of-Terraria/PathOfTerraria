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
	public static readonly Asset<Texture2D> DefaultFrameTexture = ModContent.Request<Texture2D>(
		$"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Frame_Default",
		AssetRequestMode.ImmediateLoad
	);
	
	public static readonly Asset<Texture2D> VanityFrameTexture = ModContent.Request<Texture2D>(
		$"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Frame_Vanity",
		AssetRequestMode.ImmediateLoad
	);
	
	public static readonly Asset<Texture2D> DyeFrameTexture = ModContent.Request<Texture2D>(
		$"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Frame_Dye",
		AssetRequestMode.ImmediateLoad
	);
	
	public static readonly Asset<Texture2D> LeftButtonTexture = ModContent.Request<Texture2D>(
		$"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Button_Left",
		AssetRequestMode.ImmediateLoad
	);
	
	public static readonly Asset<Texture2D> RightButtonTexture = ModContent.Request<Texture2D>(
		$"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Button_Right",
		AssetRequestMode.ImmediateLoad
	);
	
	public static readonly Asset<Texture2D> HelmetIconTexture = ModContent.Request<Texture2D>(
		$"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Helmet",
		AssetRequestMode.ImmediateLoad
	);
	
	public static readonly Asset<Texture2D> ChestIconTexture = ModContent.Request<Texture2D>(
		$"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Chest",
		AssetRequestMode.ImmediateLoad
	);
	
	public static readonly Asset<Texture2D> LegsIconTexture = ModContent.Request<Texture2D>(
		$"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Legs",
		AssetRequestMode.ImmediateLoad
	);
	
	public static readonly Asset<Texture2D> WeaponIconTexture = ModContent.Request<Texture2D>(
		$"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Weapon",
		AssetRequestMode.ImmediateLoad
	);
	
	public static readonly Asset<Texture2D> OffhandIconTexture = ModContent.Request<Texture2D>(
		$"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Offhand",
		AssetRequestMode.ImmediateLoad
	);
	
	public static readonly Asset<Texture2D> RingIconTexture = ModContent.Request<Texture2D>(
		$"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Ring",
		AssetRequestMode.ImmediateLoad
	);
	
	public static readonly Asset<Texture2D> NecklaceIconTexture = ModContent.Request<Texture2D>(
		$"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Necklace",
		AssetRequestMode.ImmediateLoad
	);
	
	public static readonly Asset<Texture2D> WingsIconTexture = ModContent.Request<Texture2D>(
		$"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Wings",
		AssetRequestMode.ImmediateLoad
	);

	public const float InventoryWidth = 160f;
	public const float InventoryHeight = 190f;

	public const float ActiveRotation = MathHelper.Pi / 16f;
	public const float InactiveRotation = 0f;

	public const float ActiveScale = 1.15f;
	public const float InactiveScale = 1f;

	public const float Smoothness = 0.3f;
	
	private static Player Player => Main.LocalPlayer;
 
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
	
	private int currentPage;
	
	private UIElement root;
	private UIElement[] pages = { BuildDefaultInventory(), BuildVanityInventory(), BuildDyeInventory() };

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
		
		var rightButton = new UIImage(RightButtonTexture)
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

	    var wings = new UICustomItemSlot(DefaultFrameTexture, WingsIconTexture, ref Player.armor, 4, ItemSlot.Context.EquipAccessory);
	    
	    wings.OnMouseOver += UpdateMouseOver;
	    wings.OnMouseOut += UpdateMouseOut;
	    wings.OnUpdate += UpdateSlotHoverEffects;

	    wings.Predicate = (item, _) => item.wingSlot > 0;

	    inventory.Append(wings);

	    var helmet = new UICustomItemSlot(DefaultFrameTexture, HelmetIconTexture, ref Player.armor, 0, ItemSlot.Context.EquipArmor)
	    {
	        HAlign = 0.5f,
	        VAlign = 0f
	    };

	    helmet.OnMouseOver += UpdateMouseOver;
	    helmet.OnMouseOut += UpdateMouseOut;
	    helmet.OnUpdate += UpdateSlotHoverEffects;
	    
	    helmet.Predicate = (item, _) => item.headSlot > 0;

	    inventory.Append(helmet);
	    
	    var necklace = new UICustomItemSlot(DefaultFrameTexture, NecklaceIconTexture, ref Player.armor, 5, ItemSlot.Context.EquipAccessory)
	    {
	        HAlign = 1f,
	        VAlign = 0f,
	    };

	    necklace.OnMouseOver += UpdateMouseOver;
	    necklace.OnMouseOut += UpdateMouseOut;
	    necklace.OnUpdate += UpdateSlotHoverEffects;

	    necklace.Predicate = (item, _) => item.accessory && item.wingSlot <= 0;

	    inventory.Append(necklace);

	    var weapon = new UICustomItemSlot(DefaultFrameTexture, WeaponIconTexture, ref Player.inventory, 0)
	    {
	        HAlign = 0f,
	        VAlign = 0.5f
	    };

	    weapon.OnMouseOver += UpdateMouseOver;
	    weapon.OnMouseOut += UpdateMouseOut;
	    weapon.OnUpdate += UpdateSlotHoverEffects;

	    weapon.Predicate = (item, _) => item.damage > 0;

	    inventory.Append(weapon);

	    var chest = new UICustomItemSlot(DefaultFrameTexture, ChestIconTexture, ref Player.armor, 1, ItemSlot.Context.EquipArmor)
	    {
	        HAlign = 0.5f,
	        VAlign = 0.5f
	    };

	    chest.OnMouseOver += UpdateMouseOver;
	    chest.OnMouseOut += UpdateMouseOut;
	    chest.OnUpdate += UpdateSlotHoverEffects;
	    
	    chest.Predicate = (item, _) => item.bodySlot > 0;

	    inventory.Append(chest);

	    var offhand = new UICustomItemSlot(DefaultFrameTexture, OffhandIconTexture, ref Player.armor, 6, ItemSlot.Context.EquipAccessory)
	    {
	        HAlign = 1f,
	        VAlign = 0.5f
	    };

	    offhand.OnMouseOver += UpdateMouseOver;
	    offhand.OnMouseOut += UpdateMouseOut;
	    offhand.OnUpdate += UpdateSlotHoverEffects;
	    
	    offhand.Predicate = (item, _) => item.pick > 0;

	    inventory.Append(offhand);

	    var leftRing = new UICustomItemSlot(DefaultFrameTexture, RingIconTexture, ref Player.armor, 7, ItemSlot.Context.EquipAccessory)
	    {
	        HAlign = 0f,
	        VAlign = 1f,
	    };

	    leftRing.OnMouseOver += UpdateMouseOver;
	    leftRing.OnMouseOut += UpdateMouseOut;
	    leftRing.OnUpdate += UpdateSlotHoverEffects;

	    leftRing.Predicate = (item, _) => item.accessory && item.wingSlot <= 0;

	    inventory.Append(leftRing);
	    
	    var legs = new UICustomItemSlot(DefaultFrameTexture, LegsIconTexture, ref Player.armor, 2, ItemSlot.Context.EquipArmor)
	    {
	        HAlign = 0.5f,
	        VAlign = 1f
	    };

	    legs.OnMouseOver += UpdateMouseOver;
	    legs.OnMouseOut += UpdateMouseOut;
	    legs.OnUpdate += UpdateSlotHoverEffects;
	    
	    legs.Predicate = (item, _) => item.legSlot > 0;

	    inventory.Append(legs);
	    
	    var rightRing = new UICustomItemSlot(DefaultFrameTexture, RingIconTexture, ref Player.armor, 8, ItemSlot.Context.EquipAccessory)
	    {
	        HAlign = 1f,
	        VAlign = 1f
	    };

	    rightRing.OnMouseOver += UpdateMouseOver;
	    rightRing.OnMouseOut += UpdateMouseOut;
	    rightRing.OnUpdate += UpdateSlotHoverEffects;

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

	    var wings = new UICustomItemSlot(VanityFrameTexture, WingsIconTexture, ref Player.armor, 14, ItemSlot.Context.EquipAccessoryVanity);
	    
	    wings.OnMouseOver += UpdateMouseOver;
	    wings.OnMouseOut += UpdateMouseOut;
	    wings.OnUpdate += UpdateSlotHoverEffects;

	    wings.Predicate = (item, _) => item.wingSlot > 0;

	    inventory.Append(wings);

	    var helmet = new UICustomItemSlot(VanityFrameTexture, HelmetIconTexture, ref Player.armor, 10, ItemSlot.Context.EquipArmorVanity)
	    {
	        HAlign = 0.5f,
	        VAlign = 0f
	    };

	    helmet.OnMouseOver += UpdateMouseOver;
	    helmet.OnMouseOut += UpdateMouseOut;
	    helmet.OnUpdate += UpdateSlotHoverEffects;

	    helmet.Predicate = (item, _) => item.headSlot > 0;

	    inventory.Append(helmet);

	    var necklace = new UICustomItemSlot(VanityFrameTexture, NecklaceIconTexture, ref Player.armor, 15, ItemSlot.Context.EquipAccessoryVanity)
	    {
	        HAlign = 1f,
	        VAlign = 0f,
	    };

	    necklace.OnMouseOver += UpdateMouseOver;
	    necklace.OnMouseOut += UpdateMouseOut;
	    necklace.OnUpdate += UpdateSlotHoverEffects;

	    necklace.Predicate = (item, _) => item.accessory && item.wingSlot <= 0;

	    inventory.Append(necklace);

	    var chest = new UICustomItemSlot(VanityFrameTexture, ChestIconTexture, ref Player.armor, 11, ItemSlot.Context.EquipArmorVanity)
	    {
	        HAlign = 0.5f,
	        VAlign = 0.5f
	    };

	    chest.OnMouseOver += UpdateMouseOver;
	    chest.OnMouseOut += UpdateMouseOut;
	    chest.OnUpdate += UpdateSlotHoverEffects;

	    chest.Predicate = (item, _) => item.bodySlot > 0;

	    inventory.Append(chest);
	    
	    var offhand = new UICustomItemSlot(VanityFrameTexture, OffhandIconTexture, ref Player.armor, 16, ItemSlot.Context.EquipAccessoryVanity)
	    {
		    HAlign = 1f,
		    VAlign = 0.5f
	    };

	    offhand.OnMouseOver += UpdateMouseOver;
	    offhand.OnMouseOut += UpdateMouseOut;
	    offhand.OnUpdate += UpdateSlotHoverEffects;
	    
	    offhand.Predicate = (item, _) => item.pick > 0;
	    
	    inventory.Append(offhand);

	    var leftRing = new UICustomItemSlot(VanityFrameTexture, RingIconTexture, ref Player.armor, 16, ItemSlot.Context.EquipAccessoryVanity)
	    {
	        HAlign = 0f,
	        VAlign = 1f
	    };

	    leftRing.OnMouseOver += UpdateMouseOver;
	    leftRing.OnMouseOut += UpdateMouseOut;
	    leftRing.OnUpdate += UpdateSlotHoverEffects;

	    leftRing.Predicate = (item, _) => item.accessory && item.wingSlot <= 0;

	    inventory.Append(leftRing);

	    var legs = new UICustomItemSlot(VanityFrameTexture, LegsIconTexture, ref Player.armor, 12, ItemSlot.Context.EquipArmorVanity)
	    {
	        HAlign = 0.5f,
	        VAlign = 1f
	    };

	    legs.OnMouseOver += UpdateMouseOver;
	    legs.OnMouseOut += UpdateMouseOut;
	    legs.OnUpdate += UpdateSlotHoverEffects;

	    legs.Predicate = (item, _) => item.legSlot > 0;

	    inventory.Append(legs);

	    var rightRing = new UICustomItemSlot(VanityFrameTexture, RingIconTexture, ref Player.armor, 18, ItemSlot.Context.EquipAccessoryVanity)
	    {
	        HAlign = 1f,
	        VAlign = 1f
	    };

	    rightRing.OnMouseOver += UpdateMouseOver;
	    rightRing.OnMouseOut += UpdateMouseOut;
	    rightRing.OnUpdate += UpdateSlotHoverEffects;

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

	    var wings = new UICustomItemSlot(DyeFrameTexture, WingsIconTexture, ref Player.dye, 4, ItemSlot.Context.EquipDye);
	    
	    wings.OnMouseOver += UpdateMouseOver;
	    wings.OnMouseOut += UpdateMouseOut;
	    wings.OnUpdate += UpdateSlotHoverEffects;

	    wings.Predicate = (item, _) => item.dye > 0;

	    inventory.Append(wings);
	    
	    var helmet = new UICustomItemSlot(DyeFrameTexture, HelmetIconTexture, ref Player.dye, 0, ItemSlot.Context.EquipDye)
	    {
	        HAlign = 0.5f,
	        VAlign = 0f
	    };

	    helmet.OnMouseOver += UpdateMouseOver;
	    helmet.OnMouseOut += UpdateMouseOut;
	    helmet.OnUpdate += UpdateSlotHoverEffects;
	    
	    helmet.Predicate = (item, _) => item.dye > 0;

	    inventory.Append(helmet);

	    var necklace = new UICustomItemSlot(DyeFrameTexture, NecklaceIconTexture, ref Player.dye, 5, ItemSlot.Context.EquipDye)
	    {
	        HAlign = 1f,
	        VAlign = 0f
	    };

	    necklace.OnMouseOver += UpdateMouseOver;
	    necklace.OnMouseOut += UpdateMouseOut;
	    necklace.OnUpdate += UpdateSlotHoverEffects;

	    necklace.Predicate = (item, _) => item.dye > 0;

	    inventory.Append(necklace);

	    var chest = new UICustomItemSlot(DyeFrameTexture, ChestIconTexture, ref Player.dye, 1, ItemSlot.Context.EquipDye)
	    {
	        HAlign = 0.5f,
	        VAlign = 0.5f
	    };

	    chest.OnMouseOver += UpdateMouseOver;
	    chest.OnMouseOut += UpdateMouseOut;
	    chest.OnUpdate += UpdateSlotHoverEffects;

	    chest.Predicate = (item, _) => item.dye > 0;
	    
	    inventory.Append(chest);

	    var offhand = new UICustomItemSlot(DyeFrameTexture, OffhandIconTexture, ref Player.dye, 6, ItemSlot.Context.EquipAccessory)
	    {
		    HAlign = 1f,
		    VAlign = 0.5f
	    };

	    offhand.OnMouseOver += UpdateMouseOver;
	    offhand.OnMouseOut += UpdateMouseOut;
	    offhand.OnUpdate += UpdateSlotHoverEffects;
	    
	    offhand.Predicate = (item, _) => item.pick > 0;

	    inventory.Append(offhand);

	    var leftRing = new UICustomItemSlot(DyeFrameTexture, RingIconTexture, ref Player.dye, 7, ItemSlot.Context.EquipDye)
	    {
	        HAlign = 0f,
	        VAlign = 1f
	    };

	    leftRing.OnMouseOver += UpdateMouseOver;
	    leftRing.OnMouseOut += UpdateMouseOut;
	    leftRing.OnUpdate += UpdateSlotHoverEffects;

	    leftRing.Predicate = (item, _) => item.dye > 0;

	    inventory.Append(leftRing);

	    var legs = new UICustomItemSlot(DyeFrameTexture, LegsIconTexture, ref Player.dye, 2, ItemSlot.Context.EquipDye)
	    {
	        HAlign = 0.5f,
	        VAlign = 1f
	    };

	    legs.OnMouseOver += UpdateMouseOver;
	    legs.OnMouseOut += UpdateMouseOut;
	    legs.OnUpdate += UpdateSlotHoverEffects;

	    legs.Predicate = (item, _) => item.dye > 0;

	    inventory.Append(legs);

	    var rightRing = new UICustomItemSlot(DyeFrameTexture, RingIconTexture, ref Player.dye, 8, ItemSlot.Context.EquipDye)
	    {
	        HAlign = 1f,
	        VAlign = 1f
	    };

	    rightRing.OnMouseOver += UpdateMouseOver;
	    rightRing.OnMouseOut += UpdateMouseOut;
	    rightRing.OnUpdate += UpdateSlotHoverEffects;
	    
	    rightRing.Predicate = (item, _) => item.dye > 0;

	    inventory.Append(rightRing);

	    return inventory;
	}
}