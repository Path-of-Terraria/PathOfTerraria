using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using PathOfTerraria.Common.AccessorySlots;
using PathOfTerraria.Common.UI.Elements;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader.Default;
using Terraria.UI;

#nullable enable

namespace PathOfTerraria.Common.UI.Armor.Elements;

public abstract class UIArmorPage : UIElement
{
	public static readonly Asset<Texture2D> HelmetIconTexture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/Inventory/Helmet", AssetRequestMode.ImmediateLoad);

	public static readonly Asset<Texture2D> ChestIconTexture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/Inventory/Chest", AssetRequestMode.ImmediateLoad);

	public static readonly Asset<Texture2D> LegsIconTexture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/Inventory/Legs", AssetRequestMode.ImmediateLoad);

	public static readonly Asset<Texture2D> WeaponIconTexture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/Inventory/Weapon", AssetRequestMode.ImmediateLoad);

	public static readonly Asset<Texture2D> OffhandIconTexture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/Inventory/Offhand", AssetRequestMode.ImmediateLoad);

	public static readonly Asset<Texture2D> RingIconTexture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/Inventory/Ring", AssetRequestMode.ImmediateLoad);

	public static readonly Asset<Texture2D> NecklaceIconTexture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/Inventory/Necklace", AssetRequestMode.ImmediateLoad);

	public static readonly Asset<Texture2D> WingsIconTexture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/Inventory/Wings", AssetRequestMode.ImmediateLoad);
	
	public static readonly Asset<Texture2D> MiscellaneousIconTexture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/Inventory/Miscellaneous", AssetRequestMode.ImmediateLoad);

	protected static Player Player => Main.LocalPlayer;

	private readonly List<(ModAccessorySlot Slot, UIHoverImageItemSlot UI)> customSlots = [];

	protected abstract Asset<Texture2D> DefaultFrameTexture { get; }

	protected abstract UIHoverImageItemSlot?[] GetDefaultSlots();

	protected abstract UIHoverImageItemSlot CreateCustomAccessorySlot(ModAccessorySlot modSlot);

	protected virtual void UpdateMouseOver(UIMouseEvent @event, UIElement element)
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

	protected virtual void UpdateMouseOut(UIMouseEvent @event, UIElement element)
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

	public override void OnInitialize()
	{
		base.OnInitialize();

		Width = StyleDimension.FromPixels(UIArmorInventory.ArmorPageWidth);
		Height = StyleDimension.FromPixels(UIArmorInventory.ArmorPageHeight);

		AddChildren();
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		MaintainCustomAccessorySlots(CollectionsMarshal.AsSpan(customSlots));
	}

	public void AddChildren()
	{
		UIHoverImageItemSlot?[] defaultSlots = GetDefaultSlots();

		AccessorySlotLoader accessoryLoader = LoaderManager.Get<AccessorySlotLoader>();
		ModAccessorySlotPlayer accessoryPlayer = Player.GetModPlayer<ModAccessorySlotPlayer>();
		int numDefaultSlots = defaultSlots.Length;
		int numAdditionalSlots = accessoryPlayer.SlotCount;
		int numTotalSlots = numDefaultSlots + numAdditionalSlots;
		int numLocationsTaken = 0;

		for (int i = 0; i < numTotalSlots; i++)
		{
			UIHoverImageItemSlot uiSlot;
			bool append = true;

			if (i < numDefaultSlots)
			{
				if (defaultSlots[i] is not UIHoverImageItemSlot def)
				{
					numLocationsTaken++;
					continue;
				}

				uiSlot = def;
			}
			else
			{
				ModAccessorySlot modSlot = accessoryLoader.Get(i - numDefaultSlots, Player);
				bool isVisible = ExtraAccessorySlots.IsModAccessorySlotVisible(modSlot);

				uiSlot = CreateCustomAccessorySlot(modSlot);
				append = isVisible;

				customSlots.Add((modSlot, uiSlot));
			}

			uiSlot.ActiveScale = 1.15f;
			uiSlot.ActiveRotation = MathHelper.ToRadians(1f);
			uiSlot.OnMouseOver += UpdateMouseOver;
			uiSlot.OnMouseOut += UpdateMouseOut;

			if (append)
			{
				uiSlot.HAlign = (numLocationsTaken % 3) * 0.5f;
				uiSlot.VAlign = (numLocationsTaken / 3) * 0.25f;
				numLocationsTaken++;

				Append(uiSlot);
			}
		}
	}

	private void MaintainCustomAccessorySlots(ReadOnlySpan<(ModAccessorySlot, UIHoverImageItemSlot)> slots)
	{
		int numLocationsTaken = GetDefaultSlots().Length;

		foreach ((ModAccessorySlot modSlot, UIHoverImageItemSlot uiSlot) in slots)
		{
			bool isPresent = Children.Contains(uiSlot);
			bool shouldBePresent = ExtraAccessorySlots.IsModAccessorySlotVisible(modSlot);

			if (isPresent != shouldBePresent)
			{
				if (shouldBePresent)
				{
					// Make sure the slot is properly initialized before appending
					if (uiSlot.Icon == null) { uiSlot.OnInitialize(); }

					Append(uiSlot);
				}
				else
				{
					RemoveChild(uiSlot);
				}
			}

			if (shouldBePresent)
			{
				uiSlot.HAlign = (numLocationsTaken % 3) * 0.5f;
				uiSlot.VAlign = (numLocationsTaken / 3) * 0.25f;
				numLocationsTaken++;
			}
		}
	}
}