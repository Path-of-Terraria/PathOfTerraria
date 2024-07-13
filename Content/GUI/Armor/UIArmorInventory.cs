using System.Collections.Generic;
using System.Reflection;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using PathOfTerraria.Content.GUI.Armor.Elements;
using PathOfTerraria.Content.GUI.UIItemSlots;
using PathOfTerraria.Core.Loaders.UILoading;
using PathOfTerraria.Helpers.Extensions;
using ReLogic.Content;
using Terraria.ID;
using Terraria.UI;

namespace PathOfTerraria.Content.GUI;

public sealed class UIArmorInventory : UIState
{
	private const float Margin = 4f;

	private const int HorizontalSlots = 3;
	private const int VerticalSlots = 3;
	
	public override void OnInitialize()
	{
		Player player = Main.CurrentPlayer;

		Asset<Texture2D> frame = ModContent.Request<Texture2D>(
			$"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Frame",
			AssetRequestMode.ImmediateLoad
		);
		
		Left.Set(Main.screenWidth, 0f);
		Top.Set(Main.mapStyle == 1 ? 446f : 190f, 0f);
		
		Width.Set(frame.Width() * HorizontalSlots + Margin * HorizontalSlots, 0f);
		Height.Set(frame.Height() * VerticalSlots + Margin * VerticalSlots, 0f);
		
		Asset<Texture2D> icon = ModContent.Request<Texture2D>(
			$"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Wings",
			AssetRequestMode.ImmediateLoad
		);

		var wings = new UIArmorItemSlot(frame, icon) { InventoryIndex = 3 };

		wings.SetMargin(Margin);

		wings.HAlign = 0f;
		wings.VAlign = 0f;

		wings.InsertionPredicate = (item, _) => item.wingSlot > 0;

		Append(wings);
		
		icon = ModContent.Request<Texture2D>(
			$"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Helmet",
			AssetRequestMode.ImmediateLoad
		);
		
		var helmet = new UIArmorItemSlot(frame, icon) { InventoryIndex = 0 };
		
		helmet.SetMargin(Margin);

		helmet.HAlign = 0.5f;
		helmet.VAlign = 0f;

		helmet.InsertionPredicate = (item, _) => item.headSlot > 0;

		Append(helmet);
		
		icon = ModContent.Request<Texture2D>(
			$"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Necklace",
			AssetRequestMode.ImmediateLoad
		);

		var necklace = new UIArmorItemSlot(frame, icon) { InventoryIndex = 4 };

		necklace.SetMargin(Margin);

		necklace.HAlign = 1f;
		necklace.VAlign = 0f;

		necklace.InsertionPredicate = (item, _) => item.accessory && item.wingSlot <= 0;

		Append(necklace);
		
		icon = ModContent.Request<Texture2D>(
			$"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Left_Hand", 
			AssetRequestMode.ImmediateLoad
		);

		var left = new UIArmorItemSlot(frame, icon);

		left.SetMargin(Margin);

		left.HAlign = 0f;
		left.VAlign = 0.5f;

		Append(left);

		icon = ModContent.Request<Texture2D>(
			$"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Chest", 
			AssetRequestMode.ImmediateLoad
		);

		var chest = new UIArmorItemSlot(frame, icon) { InventoryIndex = 1 };

		chest.SetMargin(Margin);

		chest.HAlign = 0.5f;
		chest.VAlign = 0.5f;

		chest.InsertionPredicate = (item, _) => item.bodySlot > 0;

		Append(chest);

		icon = ModContent.Request<Texture2D>(
			$"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Right_Hand", 
			AssetRequestMode.ImmediateLoad
		);
		
		var right = new UIArmorItemSlot(frame, icon);

		right.SetMargin(Margin);

		right.HAlign = 1f;
		right.VAlign = 0.5f;

		Append(right);
		
		icon = ModContent.Request<Texture2D>(
			$"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Ring", 
			AssetRequestMode.ImmediateLoad
		);

		var leftRing = new UIArmorItemSlot(frame, icon) { InventoryIndex = 5 };

		leftRing.SetMargin(Margin);

		leftRing.HAlign = 0f;
		leftRing.VAlign = 1f;
		
		leftRing.InsertionPredicate = (item, _) =>  item.accessory && item.wingSlot <= 0;

		Append(leftRing);
		
		icon = ModContent.Request<Texture2D>(
			$"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Legs", 
			AssetRequestMode.ImmediateLoad
		);

		var legs = new UIArmorItemSlot(frame, icon) { InventoryIndex = 2 };

		legs.SetMargin(Margin);

		legs.HAlign = 0.5f;
		legs.VAlign = 1f;

		legs.InsertionPredicate = (item, _) => item.legSlot > 0;

		Append(legs);
		
		icon = ModContent.Request<Texture2D>(
			$"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Ring", 
			AssetRequestMode.ImmediateLoad
		);

		var rightRing = new UIArmorItemSlot(frame, icon) { InventoryIndex = 6 };

		rightRing.SetMargin(Margin);

		rightRing.HAlign = 1f;
		rightRing.VAlign = 1f;
		
		rightRing.InsertionPredicate = (item, _) => item.accessory && item.wingSlot <= 0;

		Append(rightRing);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		base.Draw(spriteBatch);
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		var inArmorInventory = Main.playerInventory && Main.EquipPage == 0;
		
		var left = inArmorInventory ? Main.screenWidth - Width.Pixels - 40f : Main.screenWidth;
		var top = Main.mapStyle == 1 ? 446f : 190f;
		
		Left.Set(MathHelper.Lerp(Left.Pixels, left, 0.2f), 0f);
		Top.Set(MathHelper.Lerp(Top.Pixels, top, 0.2f), 0f);
		
		Recalculate();
	}
}