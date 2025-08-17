using PathOfTerraria.Common.UI.Elements;
using ReLogic.Content;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.Armor.Elements;

public sealed class UIDyeArmor : UIArmorPage
{
	public static readonly Asset<Texture2D> DyeFrameTexture =
		ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/Inventory/Frame_Dye",
			AssetRequestMode.ImmediateLoad);

	public static readonly Asset<Texture2D> DyeIconTexture =
		ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/Inventory/Dye", AssetRequestMode.ImmediateLoad);

	private UICustomHoverImageItemSlot accessorySlot4;
	private bool wasHardMode = false;

	public override void OnInitialize()
	{
		base.OnInitialize();

		Width = StyleDimension.FromPixels(UIArmorInventory.ArmorPageWidth);
		Height = StyleDimension.FromPixels(UIArmorInventory.ArmorPageHeight);

		var wings = new UIHoverImageItemSlot(DyeFrameTexture, DyeIconTexture, ref Player.dye, 4,
			$"Mods.{PoTMod.ModName}.UI.Slots.10", ItemSlot.Context.EquipDye)
		{
			ActiveScale = 1.15f, ActiveRotation = MathHelper.ToRadians(1f)
		};

		wings.OnMouseOver += UpdateMouseOver;
		wings.OnMouseOut += UpdateMouseOut;

		wings.Predicate = (item, _) => item.dye > 0;

		Append(wings);

		var helmet =
			new UIHoverImageItemSlot(DyeFrameTexture, DyeIconTexture, ref Player.dye, 0,
				$"Mods.{PoTMod.ModName}.UI.Slots.0", ItemSlot.Context.EquipDye)
			{
				HAlign = 0.5f, VAlign = 0f, ActiveScale = 1.15f, ActiveRotation = MathHelper.ToRadians(1f)
			};

		helmet.OnMouseOver += UpdateMouseOver;
		helmet.OnMouseOut += UpdateMouseOut;

		helmet.Predicate = (item, _) => item.dye > 0;

		Append(helmet);

		var necklace =
			new UIHoverImageItemSlot(DyeFrameTexture, DyeIconTexture, ref Player.dye, 5,
				$"Mods.{PoTMod.ModName}.UI.Slots.5", ItemSlot.Context.EquipDye)
			{
				HAlign = 1f, VAlign = 0f, ActiveScale = 1.15f, ActiveRotation = MathHelper.ToRadians(1f)
			};

		necklace.OnMouseOver += UpdateMouseOver;
		necklace.OnMouseOut += UpdateMouseOut;

		necklace.Predicate = (item, _) => item.dye > 0;

		Append(necklace);

		var chest = new UIHoverImageItemSlot(DyeFrameTexture, DyeIconTexture, ref Player.dye, 1,
			$"Mods.{PoTMod.ModName}.UI.Slots.1", ItemSlot.Context.EquipDye)
		{
			HAlign = 0.5f, VAlign = 0.25f, ActiveScale = 1.15f, ActiveRotation = MathHelper.ToRadians(1f)
		};

		chest.OnMouseOver += UpdateMouseOver;
		chest.OnMouseOut += UpdateMouseOut;

		chest.Predicate = (item, _) => item.dye > 0;

		Append(chest);

		var offhand =
			new UIHoverImageItemSlot(DyeFrameTexture, DyeIconTexture, ref Player.dye, 6,
				$"Mods.{PoTMod.ModName}.UI.Slots.6", ItemSlot.Context.EquipAccessory)
			{
				HAlign = 1f, VAlign = 0.25f, ActiveScale = 1.15f, ActiveRotation = MathHelper.ToRadians(1f)
			};

		offhand.OnMouseOver += UpdateMouseOver;
		offhand.OnMouseOut += UpdateMouseOut;

		offhand.Predicate = (item, _) => item.dye > 0;

		Append(offhand);

		var leftRing =
			new UIHoverImageItemSlot(DyeFrameTexture, DyeIconTexture, ref Player.dye, 7,
				$"Mods.{PoTMod.ModName}.UI.Slots.7", ItemSlot.Context.EquipDye)
			{
				HAlign = 0f, VAlign = 0.5f, ActiveScale = 1.15f, ActiveRotation = MathHelper.ToRadians(1f)
			};

		leftRing.OnMouseOver += UpdateMouseOver;
		leftRing.OnMouseOut += UpdateMouseOut;

		leftRing.Predicate = (item, _) => item.dye > 0;

		Append(leftRing);

		var legs = new UIHoverImageItemSlot(DyeFrameTexture, DyeIconTexture, ref Player.dye, 2,
			$"Mods.{PoTMod.ModName}.UI.Slots.2", ItemSlot.Context.EquipDye)
		{
			HAlign = 0.5f, VAlign = 0.5f, ActiveScale = 1.15f, ActiveRotation = MathHelper.ToRadians(1f)
		};

		legs.OnMouseOver += UpdateMouseOver;
		legs.OnMouseOut += UpdateMouseOut;

		legs.Predicate = (item, _) => item.dye > 0;

		Append(legs);

		var rightRing =
			new UIHoverImageItemSlot(DyeFrameTexture, DyeIconTexture, ref Player.dye, 8,
				$"Mods.{PoTMod.ModName}.UI.Slots.8", ItemSlot.Context.EquipDye)
			{
				HAlign = 1f, VAlign = 0.5f, ActiveScale = 1.15f, ActiveRotation = MathHelper.ToRadians(1f)
			};

		rightRing.OnMouseOver += UpdateMouseOver;
		rightRing.OnMouseOut += UpdateMouseOut;

		rightRing.Predicate = (item, _) => item.dye > 0;

		Append(rightRing);

		var accessorySlot1 =
			new UIHoverImageItemSlot(DyeFrameTexture, DyeIconTexture, ref Player.dye, 3,
				$"Mods.{PoTMod.ModName}.UI.Slots.3", ItemSlot.Context.EquipDye)
			{
				VAlign = 0.75f, ActiveScale = 1.15f, ActiveRotation = MathHelper.ToRadians(1f)
			};

		accessorySlot1.OnMouseOver += UpdateMouseOver;
		accessorySlot1.OnMouseOut += UpdateMouseOut;

		Append(accessorySlot1);

		var accessorySlot2 =
			new UIHoverImageItemSlot(DyeFrameTexture, DyeIconTexture, ref Player.dye, 9,
				$"Mods.{PoTMod.ModName}.UI.Slots.9", ItemSlot.Context.EquipDye)
			{
				HAlign = 0.5f, VAlign = .75f, ActiveScale = 1.15f, ActiveRotation = MathHelper.ToRadians(1f)
			};

		accessorySlot2.OnMouseOver += UpdateMouseOver;
		accessorySlot2.OnMouseOut += UpdateMouseOut;

		Append(accessorySlot2);

		var accessorySlot3 =
			new UICustomHoverImageItemSlot(DyeFrameTexture, DyeIconTexture, 24, $"Mods.{PoTMod.ModName}.UI.Slots.11",
				ItemSlot.Context.EquipDye)
			{
				HAlign = 1.0f, VAlign = .75f, ActiveScale = 1.15f, ActiveRotation = MathHelper.ToRadians(1f)
			};

		accessorySlot3.OnMouseOver += UpdateMouseOver;
		accessorySlot3.OnMouseOut += UpdateMouseOut;

		Append(accessorySlot3);

		accessorySlot4 =
			new UICustomHoverImageItemSlot(DyeFrameTexture, DyeIconTexture, 25, $"Mods.{PoTMod.ModName}.UI.Slots.12",
				ItemSlot.Context.EquipDye)
			{
				HAlign = 0.0f, VAlign = 1f, ActiveScale = 1.15f, ActiveRotation = MathHelper.ToRadians(1f)
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
