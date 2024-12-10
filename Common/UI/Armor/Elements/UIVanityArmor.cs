using PathOfTerraria.Common.UI.Elements;
using ReLogic.Content;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.Armor.Elements;

public sealed class UIVanityArmor : UIArmorPage
{
	public static readonly Asset<Texture2D> VanityFrameTexture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/Inventory/Frame_Vanity", AssetRequestMode.ImmediateLoad);

	public override void OnInitialize()
	{
		base.OnInitialize();

		Width = StyleDimension.FromPixels(UIArmorInventory.ArmorPageWidth);
		Height = StyleDimension.FromPixels(UIArmorInventory.ArmorPageHeight);

		var wings = new UIHoverImageItemSlot(VanityFrameTexture, WingsIconTexture, ref Player.armor, 14, ItemSlot.Context.EquipAccessoryVanity)
		{
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};

		wings.OnMouseOver += UpdateMouseOver;
		wings.OnMouseOut += UpdateMouseOut;

		wings.Predicate = (item, _) => item.wingSlot > 0;

		Append(wings);

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

		Append(helmet);

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

		Append(necklace);

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

		Append(chest);

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

		Append(offhand);

		var leftRing = new UIHoverImageItemSlot(VanityFrameTexture, RingIconTexture, ref Player.armor, 17, ItemSlot.Context.EquipAccessoryVanity)
		{
			HAlign = 0f,
			VAlign = 1f,
			ActiveScale = 1.15f,
			ActiveRotation = MathHelper.ToRadians(1f)
		};

		leftRing.OnMouseOver += UpdateMouseOver;
		leftRing.OnMouseOut += UpdateMouseOut;

		leftRing.Predicate = (item, _) => item.accessory && item.wingSlot <= 0;

		Append(leftRing);

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

		Append(legs);

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

		Append(rightRing);
	}
}