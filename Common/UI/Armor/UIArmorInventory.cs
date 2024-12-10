using PathOfTerraria.Common.UI.Armor.Elements;
using PathOfTerraria.Common.UI.Elements;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.Armor;

public sealed class UIArmorInventory : UIState
{
	/// <summary>
	///     The width of each armor page, in pixels.
	/// </summary>
	public const float ArmorPageWidth = 160f;

	/// <summary>
	///     The height of each armor page, in pixels.
	/// </summary>
	public const float ArmorPageHeight = 190f;

	public const float Smoothness = 0.3f;

	public const float Margin = 40f;

	public const float RootPadding = 16f;

	public const float DefensePadding = 4f;

	public const float LoadoutPadding = 8f;

	public static readonly Asset<Texture2D> LeftButtonTexture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/Inventory/Button_Left", AssetRequestMode.ImmediateLoad);

	public static readonly Asset<Texture2D> RightButtonTexture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/Inventory/Button_Right", AssetRequestMode.ImmediateLoad);

	/// <summary>
	///     The texture used to represent the player's defense.
	/// </summary>
	public static readonly Asset<Texture2D> DefenseCounterTexture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/Inventory/Defense", AssetRequestMode.ImmediateLoad);

	/// <summary>
	///     The texture used to represent the player's first loadout.
	/// </summary>
	public static readonly Asset<Texture2D> FirstLoadoutIconTexture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/Inventory/FirstLoadout", AssetRequestMode.ImmediateLoad);

	/// <summary>
	///     The texture used to represent the player's second loadout.
	/// </summary>
	public static readonly Asset<Texture2D> SecondLoadoutIconTexture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/Inventory/SecondLoadout", AssetRequestMode.ImmediateLoad);

	/// <summary>
	///     The texture used to represent the player's third loadout.
	/// </summary>
	public static readonly Asset<Texture2D> ThirdLoadoutIconTexture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/Inventory/ThirdLoadout", AssetRequestMode.ImmediateLoad);

	public readonly UIElement[] Pages = [new UIDefaultArmor(), new UIVanityArmor(), new UIDyeArmor()];

	private int currentPage;

	private bool fadingButton;

	private string previousDefense;

	internal UIElement Root;

	/// <summary>
	///     <para>
	///         The index of the current armor page.
	///     </para>
	///     <para>
	///         <c>0</c> - Default <br></br>
	///         <c>1</c> - Vanity <br></br>
	///         <c>2</c> - Dyes
	///     </para>
	///     <remarks>
	///         Automatically wraps itself around the length of armor pages.
	///     </remarks>
	/// </summary>
	public int CurrentPage
	{
		get => currentPage;
		set
		{
			int min = 0;
			int max = Pages.Length - 1;

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

	public override void OnInitialize()
	{
		Pages[0] = new UIDefaultArmor();
		Pages[1] = new UIVanityArmor();
		Pages[2] = new UIDyeArmor();

		Root = new UIElement
		{
			Width = StyleDimension.FromPixels(ArmorPageWidth + LoadoutPadding + FirstLoadoutIconTexture.Width() + RootPadding),
			Height = StyleDimension.FromPixels(ArmorPageHeight + LeftButtonTexture.Height() + DefenseCounterTexture.Height() + RootPadding * 2f),
			Left = StyleDimension.FromPixels(Main.screenWidth + ArmorPageWidth + Margin),
			Top = StyleDimension.FromPixels(Main.mapStyle == 1 ? 436f : 180f)
		};

		Append(Root);

		Root.Append(BuildGearPages());
		Root.Append(BuildPageButtons());
		Root.Append(BuildDefenseCounter());
		Root.Append(BuildLoadouts());
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		for (int i = 0; i < Pages.Length; i++)
		{
			UIElement page = Pages[i];

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

		Root.Left.Set(MathHelper.SmoothStep(Root.Left.Pixels, left, Smoothness), 0f);
		Root.Top.Set(MathHelper.SmoothStep(Root.Top.Pixels, top, Smoothness), 0f);
	}

	private void HandleLeftPageClick(UIMouseEvent @event, UIElement element)
	{
		SoundEngine.PlaySound
		(
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
		SoundEngine.PlaySound
		(
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
		if (image.ContainsPoint(Main.MouseScreen))
		{
			Main.LocalPlayer.mouseInterface = true;
		}

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
		SoundEngine.PlaySound
		(
			SoundID.MenuTick with
			{
				Pitch = 0.15f,
				MaxInstances = 1
			}
		);
	}

	private static void UpdateMouseOut(UIMouseEvent @event, UIElement element)
	{
		SoundEngine.PlaySound
		(
			SoundID.MenuTick with
			{
				Pitch = -0.25f,
				MaxInstances = 1
			}
		);
	}

	private UIElement BuildGearPages()
	{
		var gearRoot = new UIElement
		{
			Width = StyleDimension.FromPixels(ArmorPageWidth),
			Height = StyleDimension.FromPixels(ArmorPageHeight),
			Left = StyleDimension.FromPixels(FirstLoadoutIconTexture.Width() + RootPadding)
		};

		Root.Append(gearRoot);

		for (int i = 0; i < Pages.Length; i++)
		{
			UIElement page = Pages[i];

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

		var leftButton = new UIHoverTooltipImage(LeftButtonTexture, $"Mods.{PoTMod.ModName}.UI.Gear.Buttons.Previous")
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

		var rightButton = new UIHoverTooltipImage(RightButtonTexture, $"Mods.{PoTMod.ModName}.UI.Gear.Buttons.Next")
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

		pageText.OnUpdate += _ =>
		{
			string key = CurrentPage switch
			{
				0 => $"Mods.{PoTMod.ModName}.UI.Gear.Pages.Gear",
				1 => $"Mods.{PoTMod.ModName}.UI.Gear.Pages.Vanity",
				2 => $"Mods.{PoTMod.ModName}.UI.Gear.Pages.Dyes",
				_ => $"Mods.{PoTMod.ModName}.UI.Gear.Pages.Gear"
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
		defenseText.OnUpdate += _ =>
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

		firstLoadout.OnUpdate += _ => UpdateLoadoutIcon(firstLoadout, 0);

		firstLoadout.OnLeftClick += (_, _) => Player.TrySwitchingLoadout(0);

		loadoutRoot.Append(firstLoadout);

		var secondLoadout = new UIHoverTooltipImage(SecondLoadoutIconTexture, "UI.Loadout2")
		{
			VAlign = 0.5f,
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};

		secondLoadout.OnUpdate += _ => UpdateLoadoutIcon(secondLoadout, 1);

		secondLoadout.OnLeftClick += (_, _) => Player.TrySwitchingLoadout(1);

		loadoutRoot.Append(secondLoadout);

		var thirdLoadout = new UIHoverTooltipImage(ThirdLoadoutIconTexture, "UI.Loadout3")
		{
			VAlign = 1f,
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};

		thirdLoadout.OnUpdate += _ => UpdateLoadoutIcon(thirdLoadout, 2);

		thirdLoadout.OnLeftClick += (_, _) => Player.TrySwitchingLoadout(2);

		loadoutRoot.Append(thirdLoadout);

		return loadoutRoot;
	}
}