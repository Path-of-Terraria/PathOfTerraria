using System.Collections.Generic;
using System.Reflection;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using PathOfTerraria.Content.GUI.Elements;
using PathOfTerraria.Core.Loaders.UILoading;
using PathOfTerraria.Helpers.Extensions;
using ReLogic.Content;
using Terraria.ID;
using Terraria.Localization;
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

		var wings = new UIDynamicItemSlot(frame, icon, $"Mods.{nameof(PathOfTerraria)}.UI.Armor.Tooltips.Wings")
		{
			HAlign = 0f,
			VAlign = 0f
		};

		wings.SetMargin(Margin);

		wings.ItemGetter = (player) => ref player.armor[4];
		wings.Predicate = (item, _) => item.wingSlot > 0;

		Append(wings);
		
		icon = ModContent.Request<Texture2D>(
			$"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Helmet",
			AssetRequestMode.ImmediateLoad
		);

		var helmet = new UIDynamicItemSlot(frame, icon, $"Mods.{nameof(PathOfTerraria)}.UI.Armor.Tooltips.Helmet")
		{
			HAlign = 0.5f,
			VAlign = 0f
		};
		
		helmet.SetMargin(Margin);

		helmet.ItemGetter = (player) => ref player.armor[0];
		helmet.Predicate = (item, _) => item.headSlot > 0;

		Append(helmet);
		
		icon = ModContent.Request<Texture2D>(
			$"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Necklace",
			AssetRequestMode.ImmediateLoad
		);

		var necklace = new UIDynamicItemSlot(frame, icon, $"Mods.{nameof(PathOfTerraria)}.UI.Armor.Tooltips.Necklace")
		{
			HAlign = 1f,
			VAlign = 0f
		};

		necklace.SetMargin(Margin);
		
		necklace.ItemGetter = (player) => ref player.armor[4];
		necklace.Predicate = (item, _) => item.accessory && item.wingSlot <= 0;

		Append(necklace);
		
		icon = ModContent.Request<Texture2D>(
			$"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Left_Hand", 
			AssetRequestMode.ImmediateLoad
		);

		var left = new UIDynamicItemSlot(frame, icon, $"Mods.{nameof(PathOfTerraria)}.UI.Armor.Tooltips.Weapon")
		{
			HAlign = 0f,
			VAlign = 0.5f
		};

		left.SetMargin(Margin);

		left.ItemGetter = (player) => ref player.inventory[0];
		left.Predicate = (item, _) => item.damage > 0;

		Append(left);

		icon = ModContent.Request<Texture2D>(
			$"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Chest", 
			AssetRequestMode.ImmediateLoad
		);

		var chest = new UIDynamicItemSlot(frame, icon, $"Mods.{nameof(PathOfTerraria)}.UI.Armor.Tooltips.Chest")
		{
			HAlign = 0.5f,
			VAlign = 0.5f
		};

		chest.SetMargin(Margin);

		chest.ItemGetter = (player) => ref player.armor[1];
		chest.Predicate = (item, _) => item.bodySlot > 0;

		Append(chest);

		icon = ModContent.Request<Texture2D>(
			$"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Right_Hand", 
			AssetRequestMode.ImmediateLoad
		);

		var right = new UIDynamicItemSlot(frame, icon, $"Mods.{nameof(PathOfTerraria)}.UI.Armor.Tooltips.Offhand")
		{
			HAlign = 1f,
			VAlign = 0.5f
		};

		right.SetMargin(Margin);
		
		right.ItemGetter = (player) => ref player.inventory[2];
		right.Predicate = (item, _) => item.pick > 0;

		Append(right);
		
		icon = ModContent.Request<Texture2D>(
			$"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Ring", 
			AssetRequestMode.ImmediateLoad
		);

		var leftRing = new UIDynamicItemSlot(frame, icon, $"Mods.{nameof(PathOfTerraria)}.UI.Armor.Tooltips.Ring")
		{
			HAlign = 0f,
			VAlign = 1f
		};

		leftRing.SetMargin(Margin);
		
		leftRing.ItemGetter = (player) => ref player.armor[5];
		leftRing.Predicate = (item, _) => item.accessory && item.wingSlot <= 0;

		Append(leftRing);
		
		icon = ModContent.Request<Texture2D>(
			$"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Legs", 
			AssetRequestMode.ImmediateLoad
		);

		var legs = new UIDynamicItemSlot(frame, icon, $"Mods.{nameof(PathOfTerraria)}.UI.Armor.Tooltips.Legs")
		{
			HAlign = 0.5f,
			VAlign = 1f
		};

		legs.SetMargin(Margin);
		
		legs.ItemGetter = (player) => ref player.armor[2];
		legs.Predicate = (item, _) => item.legSlot > 0;

		Append(legs);
		
		icon = ModContent.Request<Texture2D>(
			$"{nameof(PathOfTerraria)}/Assets/GUI/Inventory/Ring", 
			AssetRequestMode.ImmediateLoad
		);

		var rightRing = new UIDynamicItemSlot(frame, icon, $"Mods.{nameof(PathOfTerraria)}.UI.Armor.Tooltips.Ring")
		{
			HAlign = 1f,
			VAlign = 1f
		};

		rightRing.SetMargin(Margin);
		
		rightRing.ItemGetter = (player) => ref player.armor[6];
		rightRing.Predicate = (item, _) => item.accessory && item.wingSlot <= 0;

		Append(rightRing);
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