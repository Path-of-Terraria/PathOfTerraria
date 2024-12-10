using PathOfTerraria.Common.UI.Elements;
using ReLogic.Content;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.Armor.Elements;

public sealed class UIDyeArmor : UIArmorPage
{
	public static readonly Asset<Texture2D> DyeFrameTexture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/Inventory/Frame_Dye", AssetRequestMode.ImmediateLoad);

	public static readonly Asset<Texture2D> DyeIconTexture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/Inventory/Dye", AssetRequestMode.ImmediateLoad);

	public override void OnInitialize()
	{
		base.OnInitialize();

		Width = StyleDimension.FromPixels(UIArmorInventory.ArmorPageWidth);
		Height = StyleDimension.FromPixels(UIArmorInventory.ArmorPageHeight);

		var wings = new UIHoverImageItemSlot(DyeFrameTexture, DyeIconTexture, ref Player.dye, 4, ItemSlot.Context.EquipDye)
		{
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};

		wings.OnMouseOver += UpdateMouseOver;
		wings.OnMouseOut += UpdateMouseOut;

		wings.Predicate = (item, _) => item.dye > 0;

		Append(wings);

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

		Append(helmet);

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

		Append(necklace);

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

		Append(chest);

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

		Append(offhand);

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

		Append(leftRing);

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

		Append(legs);

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

		Append(rightRing);
	}
}