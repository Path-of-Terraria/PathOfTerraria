using PathOfTerraria.Content.GUI.Elements;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;

namespace PathOfTerraria.Content.GUI.Armor;

public sealed class UIGearInventory : UIState
{
	/// <summary>
	///     The width of each individual armor page.
	/// </summary>
	public const float ArmorPageWidth = 160f;

	/// <summary>
	///     The height of each individual armor page.
	/// </summary>
	public const float ArmorPageHeight = 190f;

	/// <summary>
	///     The smoothness used to perform position interpolations.
	/// </summary>
	public const float Smoothness = 0.3f;

	/// <summary>
	///     The margin around the inventory.
	/// </summary>
	public const float Margin = 40f;

	/// <summary>
	///     The padding between each root element in the inventory.
	/// </summary>
	public const float RootPadding = 16f;

	/// <summary>
	///     The padding between each defense element in the inventory.
	/// </summary>
	public const float DefensePadding = 4f;

	/// <summary>
	///     The padding between each loadout element in the inventory.
	/// </summary>
	public const float LoadoutPadding = 8f;

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

	public static readonly Asset<Texture2D> DyeIconTexture = ModContent.Request<Texture2D>(
		$"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Dye",
		AssetRequestMode.ImmediateLoad
	);

	public static readonly Asset<Texture2D> DefenseCounterTexture = ModContent.Request<Texture2D>(
		$"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Defense",
		AssetRequestMode.ImmediateLoad
	);

	public static readonly Asset<Texture2D> FirstLoadoutIconTexture = ModContent.Request<Texture2D>(
		$"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/FirstLoadout",
		AssetRequestMode.ImmediateLoad
	);

	public static readonly Asset<Texture2D> SecondLoadoutIconTexture = ModContent.Request<Texture2D>(
		$"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/SecondLoadout",
		AssetRequestMode.ImmediateLoad
	);

	public static readonly Asset<Texture2D> ThirdLoadoutIconTexture = ModContent.Request<Texture2D>(
		$"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/ThirdLoadout",
		AssetRequestMode.ImmediateLoad
	);

	/// <summary>
	///     The index of the current gear page.
	///     <br></br>
	///     <c>0</c> - Default <br></br>
	///     <c>1</c> - Vanity <br></br>
	///     <c>2</c> - Dyes
	/// </summary>
	/// <remarks>
	///     This will be reset to 2 if you go out of negative bounds, and 0 out of
	///     positive bounds.
	/// </remarks>
	public int CurrentPage
	{
		get => currentPage;
		set
		{
			int min = 0;
			int max = pages.Length - 1;

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

	private static Player Player => Main.LocalPlayer;
	
	private string previousDefense;
	
	private bool fadingButton;

	private int currentPage;
	private readonly UIElement[] pages = { BuildDefaultInventory(), BuildVanityInventory(), BuildDyeInventory() };

	private UIElement root;

	public override void OnInitialize()
	{
		root = new UIElement
		{
			Width = StyleDimension.FromPixels(ArmorPageWidth + LoadoutPadding + FirstLoadoutIconTexture.Width() + RootPadding),
			Height = StyleDimension.FromPixels(ArmorPageHeight + LeftButtonTexture.Height() + DefenseCounterTexture.Height() + RootPadding * 2f),
			Left = StyleDimension.FromPixels(Main.screenWidth + ArmorPageWidth + Margin),
			Top = StyleDimension.FromPixels(Main.mapStyle == 1 ? 436f : 180f)
		};

		Append(root);

		root.Append(BuildGearPages());
		root.Append(BuildPageButtons());
		root.Append(BuildDefenseCounter());
		root.Append(BuildLoadouts());
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		for (int i = 0; i < pages.Length; i++)
		{
			UIElement page = pages[i];

			UpdatePagePosition(page, i);
		}

		UpdateRootPosition();
	}

	private void UpdatePagePosition(UIElement page, int index)
	{
		bool enabled = CurrentPage == index;

		float left = enabled ? 0f : ArmorPageWidth + Margin;
		float top = 0f;

		page.Left.Set(MathHelper.SmoothStep(page.Left.Pixels, left, Smoothness), 0f);
		page.Top.Set(MathHelper.SmoothStep(page.Top.Pixels, top, Smoothness), 0f);
	}

	private void UpdateRootPosition()
	{
		bool enabled = Main.playerInventory && Main.EquipPage == 0;

		float left = enabled
			? Main.screenWidth - ArmorPageWidth - RootPadding - FirstLoadoutIconTexture.Width() - Margin
			: LoadoutPadding + Main.screenWidth;

		float top = Main.mapStyle == 1 ? 446f : 190f;

		root.Left.Set(MathHelper.SmoothStep(root.Left.Pixels, left, Smoothness), 0f);
		root.Top.Set(MathHelper.SmoothStep(root.Top.Pixels, top, Smoothness), 0f);
	}

	private void HandleLeftPageClick(UIMouseEvent @event, UIElement element)
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

	private void HandleRightPageClick(UIMouseEvent @event, UIElement element)
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

	private static void UpdateLoadoutIcon(UIImage image, int index)
	{
		if (Player.CurrentLoadoutIndex == index)
		{
			image.Color = Color.White;
			image.Left.Pixels = MathHelper.SmoothStep(image.Left.Pixels, -LoadoutPadding, Smoothness);
		}
		else
		{
			image.Left.Pixels = MathHelper.SmoothStep(image.Left.Pixels, 0f, Smoothness);
			image.Color = Color.DarkGray;
		}
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

	private static UIElement BuildDefaultInventory()
	{
		var inventoryRoot = new UIElement
		{
			Width = StyleDimension.FromPixels(ArmorPageWidth),
			Height = StyleDimension.FromPixels(ArmorPageHeight)
		};

		var wings = new UIHoverImageItemSlot(DefaultFrameTexture, WingsIconTexture, ref Player.armor, 4, ItemSlot.Context.EquipAccessory)
		{
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};

		wings.OnMouseOver += UpdateMouseOver;
		wings.OnMouseOut += UpdateMouseOut;

		wings.Predicate = (item, _) => item.wingSlot > 0;

		inventoryRoot.Append(wings);

		var helmet = new UIHoverImageItemSlot(DefaultFrameTexture, HelmetIconTexture, ref Player.armor, 0, ItemSlot.Context.EquipArmor)
		{
			HAlign = 0.5f,
			VAlign = 0f,
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};

		helmet.OnMouseOver += UpdateMouseOver;
		helmet.OnMouseOut += UpdateMouseOut;

		helmet.Predicate = (item, _) => item.headSlot > 0;

		inventoryRoot.Append(helmet);

		var necklace = new UIHoverImageItemSlot(DefaultFrameTexture, NecklaceIconTexture, ref Player.armor, 5, ItemSlot.Context.EquipAccessory)
		{
			HAlign = 1f,
			VAlign = 0f,
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};

		necklace.OnMouseOver += UpdateMouseOver;
		necklace.OnMouseOut += UpdateMouseOut;

		necklace.Predicate = (item, _) => item.accessory && item.wingSlot <= 0;

		inventoryRoot.Append(necklace);

		var weapon = new UIHoverImageItemSlot(DefaultFrameTexture, WeaponIconTexture, ref Player.inventory, 0)
		{
			HAlign = 0f,
			VAlign = 0.5f,
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};

		weapon.OnMouseOver += UpdateMouseOver;
		weapon.OnMouseOut += UpdateMouseOut;

		weapon.Predicate = (item, _) => item.damage > 0;

		inventoryRoot.Append(weapon);

		var chest = new UIHoverImageItemSlot(DefaultFrameTexture, ChestIconTexture, ref Player.armor, 1, ItemSlot.Context.EquipArmor)
		{
			HAlign = 0.5f,
			VAlign = 0.5f,
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};

		chest.OnMouseOver += UpdateMouseOver;
		chest.OnMouseOut += UpdateMouseOut;

		chest.Predicate = (item, _) => item.bodySlot > 0;

		inventoryRoot.Append(chest);

		var offhand = new UIHoverImageItemSlot(DefaultFrameTexture, OffhandIconTexture, ref Player.armor, 6, ItemSlot.Context.EquipAccessory)
		{
			HAlign = 1f,
			VAlign = 0.5f,
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};

		offhand.OnMouseOver += UpdateMouseOver;
		offhand.OnMouseOut += UpdateMouseOut;

		offhand.Predicate = (item, _) => item.accessory && item.wingSlot <= 0;

		inventoryRoot.Append(offhand);

		var leftRing = new UIHoverImageItemSlot(DefaultFrameTexture, RingIconTexture, ref Player.armor, 7, ItemSlot.Context.EquipAccessory)
		{
			HAlign = 0f,
			VAlign = 1f,
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};

		leftRing.OnMouseOver += UpdateMouseOver;
		leftRing.OnMouseOut += UpdateMouseOut;

		leftRing.Predicate = (item, _) => item.accessory && item.wingSlot <= 0;

		inventoryRoot.Append(leftRing);

		var legs = new UIHoverImageItemSlot(DefaultFrameTexture, LegsIconTexture, ref Player.armor, 2, ItemSlot.Context.EquipArmor)
		{
			HAlign = 0.5f,
			VAlign = 1f,
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};

		legs.OnMouseOver += UpdateMouseOver;
		legs.OnMouseOut += UpdateMouseOut;

		legs.Predicate = (item, _) => item.legSlot > 0;

		inventoryRoot.Append(legs);

		var rightRing = new UIHoverImageItemSlot(DefaultFrameTexture, RingIconTexture, ref Player.armor, 8, ItemSlot.Context.EquipAccessory)
		{
			HAlign = 1f,
			VAlign = 1f,
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};

		rightRing.OnMouseOver += UpdateMouseOver;
		rightRing.OnMouseOut += UpdateMouseOut;

		rightRing.Predicate = (item, _) => item.accessory && item.wingSlot <= 0;

		inventoryRoot.Append(rightRing);

		return inventoryRoot;
	}

	private static UIElement BuildVanityInventory()
	{
		var inventoryRoot = new UIElement
		{
			Width = StyleDimension.FromPixels(ArmorPageWidth),
			Height = StyleDimension.FromPixels(ArmorPageHeight)
		};

		var wings = new UIHoverImageItemSlot(VanityFrameTexture, WingsIconTexture, ref Player.armor, 14, ItemSlot.Context.EquipAccessoryVanity)
		{
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};

		wings.OnMouseOver += UpdateMouseOver;
		wings.OnMouseOut += UpdateMouseOut;

		wings.Predicate = (item, _) => item.wingSlot > 0;

		inventoryRoot.Append(wings);

		var helmet = new UIHoverImageItemSlot(VanityFrameTexture, HelmetIconTexture, ref Player.armor, 10, ItemSlot.Context.EquipArmorVanity)
		{
			HAlign = 0.5f,
			VAlign = 0f,
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};

		helmet.OnMouseOver += UpdateMouseOver;
		helmet.OnMouseOut += UpdateMouseOut;

		helmet.Predicate = (item, _) => item.headSlot > 0;

		inventoryRoot.Append(helmet);

		var necklace = new UIHoverImageItemSlot(VanityFrameTexture, NecklaceIconTexture, ref Player.armor, 15, ItemSlot.Context.EquipAccessoryVanity)
		{
			HAlign = 1f,
			VAlign = 0f,
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};

		necklace.OnMouseOver += UpdateMouseOver;
		necklace.OnMouseOut += UpdateMouseOut;

		necklace.Predicate = (item, _) => item.accessory && item.wingSlot <= 0;

		inventoryRoot.Append(necklace);

		var chest = new UIHoverImageItemSlot(VanityFrameTexture, ChestIconTexture, ref Player.armor, 11, ItemSlot.Context.EquipArmorVanity)
		{
			HAlign = 0.5f,
			VAlign = 0.5f,
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};

		chest.OnMouseOver += UpdateMouseOver;
		chest.OnMouseOut += UpdateMouseOut;

		chest.Predicate = (item, _) => item.bodySlot > 0;

		inventoryRoot.Append(chest);

		var offhand = new UIHoverImageItemSlot(VanityFrameTexture, OffhandIconTexture, ref Player.armor, 16, ItemSlot.Context.EquipAccessoryVanity)
		{
			HAlign = 1f,
			VAlign = 0.5f,
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};

		offhand.OnMouseOver += UpdateMouseOver;
		offhand.OnMouseOut += UpdateMouseOut;

		offhand.Predicate = (item, _) => item.accessory && item.wingSlot <= 0;

		inventoryRoot.Append(offhand);

		var leftRing = new UIHoverImageItemSlot(VanityFrameTexture, RingIconTexture, ref Player.armor, 16, ItemSlot.Context.EquipAccessoryVanity)
		{
			HAlign = 0f,
			VAlign = 1f,
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};

		leftRing.OnMouseOver += UpdateMouseOver;
		leftRing.OnMouseOut += UpdateMouseOut;

		leftRing.Predicate = (item, _) => item.accessory && item.wingSlot <= 0;

		inventoryRoot.Append(leftRing);

		var legs = new UIHoverImageItemSlot(VanityFrameTexture, LegsIconTexture, ref Player.armor, 12, ItemSlot.Context.EquipArmorVanity)
		{
			HAlign = 0.5f,
			VAlign = 1f,
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};

		legs.OnMouseOver += UpdateMouseOver;
		legs.OnMouseOut += UpdateMouseOut;

		legs.Predicate = (item, _) => item.legSlot > 0;

		inventoryRoot.Append(legs);

		var rightRing = new UIHoverImageItemSlot(VanityFrameTexture, RingIconTexture, ref Player.armor, 18, ItemSlot.Context.EquipAccessoryVanity)
		{
			HAlign = 1f,
			VAlign = 1f,
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};

		rightRing.OnMouseOver += UpdateMouseOver;
		rightRing.OnMouseOut += UpdateMouseOut;

		rightRing.Predicate = (item, _) => item.accessory && item.wingSlot <= 0;

		inventoryRoot.Append(rightRing);

		return inventoryRoot;
	}

	private static UIElement BuildDyeInventory()
	{
		var inventoryRoot = new UIElement
		{
			Width = StyleDimension.FromPixels(ArmorPageWidth),
			Height = StyleDimension.FromPixels(ArmorPageHeight)
		};

		var wings = new UIHoverImageItemSlot(DyeFrameTexture, DyeIconTexture, ref Player.dye, 4, ItemSlot.Context.EquipDye)
		{
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};

		wings.OnMouseOver += UpdateMouseOver;
		wings.OnMouseOut += UpdateMouseOut;

		wings.Predicate = (item, _) => item.dye > 0;

		inventoryRoot.Append(wings);

		var helmet = new UIHoverImageItemSlot(DyeFrameTexture, DyeIconTexture, ref Player.dye, 0, ItemSlot.Context.EquipDye)
		{
			HAlign = 0.5f,
			VAlign = 0f,
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};

		helmet.OnMouseOver += UpdateMouseOver;
		helmet.OnMouseOut += UpdateMouseOut;

		helmet.Predicate = (item, _) => item.dye > 0;

		inventoryRoot.Append(helmet);

		var necklace = new UIHoverImageItemSlot(DyeFrameTexture, DyeIconTexture, ref Player.dye, 5, ItemSlot.Context.EquipDye)
		{
			HAlign = 1f,
			VAlign = 0f,
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};

		necklace.OnMouseOver += UpdateMouseOver;
		necklace.OnMouseOut += UpdateMouseOut;

		necklace.Predicate = (item, _) => item.dye > 0;

		inventoryRoot.Append(necklace);

		var chest = new UIHoverImageItemSlot(DyeFrameTexture, DyeIconTexture, ref Player.dye, 1, ItemSlot.Context.EquipDye)
		{
			HAlign = 0.5f,
			VAlign = 0.5f,
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};

		chest.OnMouseOver += UpdateMouseOver;
		chest.OnMouseOut += UpdateMouseOut;

		chest.Predicate = (item, _) => item.dye > 0;

		inventoryRoot.Append(chest);

		var offhand = new UIHoverImageItemSlot(DyeFrameTexture, DyeIconTexture, ref Player.dye, 6, ItemSlot.Context.EquipAccessory)
		{
			HAlign = 1f,
			VAlign = 0.5f,
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};

		offhand.OnMouseOver += UpdateMouseOver;
		offhand.OnMouseOut += UpdateMouseOut;

		offhand.Predicate = (item, _) => item.dye > 0;

		inventoryRoot.Append(offhand);

		var leftRing = new UIHoverImageItemSlot(DyeFrameTexture, DyeIconTexture, ref Player.dye, 7, ItemSlot.Context.EquipDye)
		{
			HAlign = 0f,
			VAlign = 1f,
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};

		leftRing.OnMouseOver += UpdateMouseOver;
		leftRing.OnMouseOut += UpdateMouseOut;

		leftRing.Predicate = (item, _) => item.dye > 0;

		inventoryRoot.Append(leftRing);

		var legs = new UIHoverImageItemSlot(DyeFrameTexture, DyeIconTexture, ref Player.dye, 2, ItemSlot.Context.EquipDye)
		{
			HAlign = 0.5f,
			VAlign = 1f,
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};

		legs.OnMouseOver += UpdateMouseOver;
		legs.OnMouseOut += UpdateMouseOut;

		legs.Predicate = (item, _) => item.dye > 0;

		inventoryRoot.Append(legs);

		var rightRing = new UIHoverImageItemSlot(DyeFrameTexture, DyeIconTexture, ref Player.dye, 8, ItemSlot.Context.EquipDye)
		{
			HAlign = 1f,
			VAlign = 1f,
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};

		rightRing.OnMouseOver += UpdateMouseOver;
		rightRing.OnMouseOut += UpdateMouseOut;

		rightRing.Predicate = (item, _) => item.dye > 0;

		inventoryRoot.Append(rightRing);

		return inventoryRoot;
	}

	private UIElement BuildGearPages()
	{
		var gearRoot = new UIElement
		{
			Width = StyleDimension.FromPixels(ArmorPageWidth),
			Height = StyleDimension.FromPixels(ArmorPageHeight),
			Left = StyleDimension.FromPixels(FirstLoadoutIconTexture.Width() + RootPadding)
		};

		root.Append(gearRoot);

		for (int i = 0; i < pages.Length; i++)
		{
			UIElement page = pages[i];

			gearRoot.Append(page);
		}

		return gearRoot;
	}

	private UIElement BuildPageButtons()
	{
		var buttonRoot = new UIElement
		{
			Width = StyleDimension.FromPixels(ArmorPageWidth),
			Height = StyleDimension.FromPixels(LeftButtonTexture.Height()),
			Left = StyleDimension.FromPixels(FirstLoadoutIconTexture.Width() + RootPadding),
			Top = StyleDimension.FromPixels(ArmorPageHeight + RootPadding)
		};

		var leftButton = new UIHoverTooltipImage(LeftButtonTexture, $"Mods.{nameof(PathOfTerraria)}.UI.Gear.Buttons.Previous")
		{
			OverrideSamplerState = SamplerState.PointClamp,
			HAlign = 0f,
			VAlign = 0.5f,
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};

		leftButton.OnLeftClick += HandleLeftPageClick;
		leftButton.OnMouseOver += UpdateMouseOver;
		leftButton.OnMouseOut += UpdateMouseOut;

		buttonRoot.Append(leftButton);

		var rightButton = new UIHoverTooltipImage(RightButtonTexture, $"Mods.{nameof(PathOfTerraria)}.UI.Gear.Buttons.Next")
		{
			OverrideSamplerState = SamplerState.PointClamp,
			HAlign = 1f,
			VAlign = 0.5f,
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};

		rightButton.OnLeftClick += HandleRightPageClick;
		rightButton.OnMouseOver += UpdateMouseOver;
		rightButton.OnMouseOut += UpdateMouseOut;

		buttonRoot.Append(rightButton);

		var pageText = new UIText("Gear")
		{
			HAlign = 0.5f,
			VAlign = 0.5f
		};

		pageText.OnUpdate += (_) =>
		{
			string key = CurrentPage switch
			{
				0 => $"Mods.{nameof(PathOfTerraria)}.UI.Gear.Pages.Gear",
				1 => $"Mods.{nameof(PathOfTerraria)}.UI.Gear.Pages.Vanity",
				2 => $"Mods.{nameof(PathOfTerraria)}.UI.Gear.Pages.Dyes",
				_ => $"Mods.{nameof(PathOfTerraria)}.UI.Gear.Pages.Gear"
			};

			pageText.SetText(Language.GetTextValue(key));
		};

		buttonRoot.Append(pageText);

		return buttonRoot;
	}

	private UIElement BuildDefenseCounter()
	{
		var defenseRoot = new UIElement
		{
			Width = StyleDimension.FromPixels(ArmorPageWidth),
			Height = StyleDimension.FromPixels(DefenseCounterTexture.Height()),
			Left = StyleDimension.FromPixels(FirstLoadoutIconTexture.Width() + RootPadding),
			Top = StyleDimension.FromPixels(ArmorPageHeight + RootPadding + DefenseCounterTexture.Height() + RootPadding)
		};

		var defenseText = new UIText(Player.statDefense.ToString())
		{
			Left = StyleDimension.FromPixels(DefenseCounterTexture.Width() + DefensePadding),
			VAlign = 0.5f
		};

		// TODO: This is nasty, find a better way to handle UI movement.
		defenseText.OnUpdate += (_) =>
		{
			string defense = Player.statDefense.ToString();
			
			if (defense != previousDefense)
			{
				fadingButton = true;
			}
			
			if (fadingButton) 
			{				
				if (MathF.Floor(defenseText.Left.Pixels) == DefensePadding)
				{
					defenseText.SetText(Player.statDefense.ToString());
					
					fadingButton = false;
				}
				
				defenseText.Left.Set(MathHelper.SmoothStep(defenseText.Left.Pixels, DefensePadding, Smoothness), 0f);
			}
			else
			{
				defenseText.Left.Set(MathHelper.SmoothStep(defenseText.Left.Pixels, DefenseCounterTexture.Width() + 8f, Smoothness), 0f);
			}
			
			previousDefense = defense;
		};

		defenseRoot.Append(defenseText);

		var defenseCounter = new UIHoverTooltipImage(DefenseCounterTexture, "BestiaryInfo.Defense")
		{
			OverrideSamplerState = SamplerState.PointClamp,
			VAlign = 0.5f,
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};

		defenseRoot.Append(defenseCounter);

		return defenseRoot;
	}

	private static UIElement BuildLoadouts()
	{
		var loadoutRoot = new UIElement
		{
			Width = StyleDimension.FromPixels(FirstLoadoutIconTexture.Width()),
			Height = StyleDimension.FromPixels(FirstLoadoutIconTexture.Height() * 3f + LoadoutPadding * 3f)
		};

		var firstLoadout = new UIHoverTooltipImage(FirstLoadoutIconTexture, "UI.Loadout1")
		{
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};

		firstLoadout.OnUpdate += (_) => UpdateLoadoutIcon(firstLoadout, 0);

		firstLoadout.OnLeftClick += (_, _) => Player.TrySwitchingLoadout(0);

		loadoutRoot.Append(firstLoadout);

		var secondLoadout = new UIHoverTooltipImage(SecondLoadoutIconTexture, "UI.Loadout2")
		{
			VAlign = 0.5f,
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};

		secondLoadout.OnUpdate += (_) => UpdateLoadoutIcon(secondLoadout, 1);

		secondLoadout.OnLeftClick += (_, _) => Player.TrySwitchingLoadout(1);

		loadoutRoot.Append(secondLoadout);

		var thirdLoadout = new UIHoverTooltipImage(ThirdLoadoutIconTexture, "UI.Loadout3")
		{
			VAlign = 1f,
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};

		thirdLoadout.OnUpdate += (_) => UpdateLoadoutIcon(thirdLoadout, 2);

		thirdLoadout.OnLeftClick += (_, _) => Player.TrySwitchingLoadout(2);

		loadoutRoot.Append(thirdLoadout);

		return loadoutRoot;
	}
}