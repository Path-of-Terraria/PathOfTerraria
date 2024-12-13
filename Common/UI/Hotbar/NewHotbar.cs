using ReLogic.Graphics;
using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent;
using Terraria.GameContent.UI;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.UI;
using Terraria.UI.Chat;
using Terraria.Localization;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems;
using Terraria.DataStructures;
using PathOfTerraria.Core.UI.SmartUI;

namespace PathOfTerraria.Common.UI.Hotbar;

internal sealed class NewHotbar : SmartUiState
{
	/// <summary>
	///		Offsets the rendering of buffs to render below our custom hotbar.
	/// </summary>
	private sealed class OffsetBuffRendering : GlobalBuff
	{
		public override bool PreDraw(SpriteBatch spriteBatch, int type, int buffIndex, ref BuffDrawParams drawParams)
		{
			// TODO: Make constant when a good value is found.
			int buffPositionOffsetY = 20;
			drawParams.Position = new Vector2(drawParams.Position.X, drawParams.Position.Y + buffPositionOffsetY);
			drawParams.MouseRectangle.Y += buffPositionOffsetY;
			return true;
		}
	}

	private sealed class Selector
	{
		public float X { get; private set; }

		public float Target { get; set; }

		public void EasePosition()
		{
			X += (Target - X) * 0.33f;
		}
	}

	private readonly Selector specialSelector = new();
	private readonly Selector buildingSelector = new();
	private readonly DynamicSpriteFont _font = FontAssets.DeathText.Value;

	private int _animation;

	public override bool Visible => !Main.playerInventory;

	public override int InsertionIndex(List<GameInterfaceLayer> layers)
	{
		return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		float prog;

		if (Main.LocalPlayer.selectedItem == 0)
		{
			if (_animation > 0)
			{
				_animation--;
			}

			prog = 1 - Ease((20 - _animation) / 20f);
		}
		else
		{
			if (_animation < 20)
			{
				_animation++;
			}

			prog = Ease(_animation / 20f);
		}

		// 20 derived from 20 - 4 because the frame has two pixels of leftmost
		// padding.
		specialSelector.Target = 20 + Utils.Clamp(Main.LocalPlayer.selectedItem, 0, 1) * 62;

		// If the selected item isn't within the target range of slots (less
		// than 2), we set it to 98 for it to rest.
		buildingSelector.Target = Main.LocalPlayer.selectedItem >= 2 ? 24 + 120 + 52 * (MathF.Min(Main.LocalPlayer.selectedItem, 10) - 2) : 98;

		specialSelector.EasePosition();
		buildingSelector.EasePosition();

		DrawSpecial(spriteBatch);
		DrawCombat(spriteBatch, -prog * 80, 1 - prog);
		DrawBuilding(spriteBatch, 80 - prog * 80, prog);
		DrawSelector(spriteBatch, prog);
		DrawHotkeys(spriteBatch, -prog * 80);
		DrawHeldItemName(spriteBatch);
	}

	private static void DrawHeldItemName(SpriteBatch spriteBatch)
	{
		string text = Lang.inter[37].Value; // "Item" when no item is held

		if (Main.LocalPlayer.HeldItem.Name != null && Main.LocalPlayer.HeldItem.Name != string.Empty)
		{
			text = Main.LocalPlayer.HeldItem.AffixName(); // Otherwise the name of the item
		}

		var itemNamePosition = new Vector2(266f - (FontAssets.MouseText.Value.MeasureString(text) / 2f).X, 6f);
		Color itemNameColor = ItemRarity.GetColor(Main.LocalPlayer.HeldItem.rare);
		ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, text, itemNamePosition, itemNameColor, 0f, Vector2.Zero, Vector2.One * 0.9f);
	}

	/// <summary>
	///		Draws the two leftmost slots, which should always be visible and
	///		won't move as part of the hotbar transition.
	/// </summary>
	private void DrawSpecial(SpriteBatch spriteBatch)
	{
		// The inactive texture contains the special, textured "inactive"
		// hotbar slots, which are silver and contain icons for the items they
		// are supposed to hold.  The active texture just contains the color
		// for the background of the hotbar frames.  This is because we use the
		// selector's frame as the hotbar frame.
		// ItemSlot::Draw unfortunately does not provide a scissor/source
		// rectangle API, so we most draw it in this manner:
		// - render active slot textures (hotbar background)
		// - render item slot items,
		// - render inactive slot textures OVER active textures and items,
		//   - these *are* rendered with a source rectangle, allowing us to
		//     cleanly transition within context of the position of the
		//     selector.

		Texture2D specialInactiveCombat = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/HotbarSpecial_Inactive_Combat").Value;
		Texture2D specialInactiveBuilding = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/HotbarSpecial_Inactive_Building").Value;
		Texture2D specialActive = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/HotbarSpecial_Active").Value;
		Main.inventoryScale = 1f; // 36 / 52f * 52f / 36f * 1 computes to 1...

		// Draw active slot textures (hotbar background).
		Main.spriteBatch.Draw(specialActive, new Vector2(20f), null, Color.White);

		// Draw item slot items.
		ItemSlot.Draw(spriteBatch, ref Main.LocalPlayer.inventory[0], 21, new Vector2(24, 30));
		ItemSlot.Draw(spriteBatch, ref Main.LocalPlayer.inventory[1], 21, new Vector2(24 + 62f, 30));

		// Render inactive slot textures OVER active textures and items.
		const int Height = 72;
		float normalizedLeftmostPos = specialSelector.X - 20f;
		float spaceToTheRight = -(specialSelector.X - 22f - 60f);
		int rightXOffset = Math.Clamp((int)MathF.Round(normalizedLeftmostPos), 0, 60);
		int spaceToTheRightRounded = (int)MathF.Round(spaceToTheRight);
		int spaceToTheRightClamped = Math.Clamp(spaceToTheRightRounded, 0, 60);
		int inverseSpaceToTheRight = 60 - spaceToTheRightClamped;
		int hackyTotal = 82 + rightXOffset + spaceToTheRightClamped;
		
		// I'd like to replace this with a more elegant solution, but I don't
		// want to waste time determining what causes strange offsets.
		if (hackyTotal != 142)
		{
			rightXOffset -= hackyTotal > 142 ? hackyTotal - 142 : 142 - hackyTotal; 
		}

		Main.spriteBatch.Draw(specialInactiveCombat, new Vector2(20f), new Rectangle(0, 0, Math.Clamp((int)MathF.Round(normalizedLeftmostPos), 0, 60), Height), Color.White);
		Main.spriteBatch.Draw(specialInactiveBuilding, new Vector2(82f + rightXOffset, 20f), new Rectangle(inverseSpaceToTheRight, 0, spaceToTheRightClamped, Height), Color.White);
	}

	private static void DrawCombat(SpriteBatch spriteBatch, float off, float opacity)
	{
		Texture2D combat = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/HotbarCombat").Value;
		Main.inventoryScale = opacity;

		Main.spriteBatch.Draw(combat, new Vector2(20, 20 + off), null, Color.White * opacity);
		// ItemSlot.Draw(spriteBatch, ref Main.LocalPlayer.inventory[0], 21, new Vector2(24, 30 + off));

		PotionSystem potionPlayer = Main.LocalPlayer.GetModPlayer<PotionSystem>();

		Texture2D bottleTex = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/EmptyPotion").Value;

		// Item textures require loading before use
		Main.instance.LoadItem(ItemID.LesserHealingPotion);
		Main.instance.LoadItem(ItemID.LesserManaPotion);

		Texture2D lifeTexture = TextureAssets.Item[ItemID.LesserHealingPotion].Value;
		Texture2D manaTexture = TextureAssets.Item[ItemID.LesserManaPotion].Value;

		spriteBatch.Draw(bottleTex, new Vector2(471, 40 + off), Color.White * opacity);
		spriteBatch.Draw(bottleTex, new Vector2(523, 40 + off), Color.White * opacity);

		int lifeH = (int)(potionPlayer.HealingLeft / (float)potionPlayer.MaxHealing * lifeTexture.Height);
		int manaH = (int)(potionPlayer.ManaLeft / (float)potionPlayer.MaxMana * lifeTexture.Height);

		spriteBatch.Draw(lifeTexture,
			new Rectangle(471, (int)(40 + off) + lifeTexture.Height - lifeH, bottleTex.Width, lifeH),
			new Rectangle(0, lifeTexture.Height - lifeH, lifeTexture.Width, lifeH), Color.White * opacity);
		spriteBatch.Draw(manaTexture,
			new Rectangle(523, (int)(40 + off) + manaTexture.Height - manaH, bottleTex.Width, manaH),
			new Rectangle(0, manaTexture.Height - manaH, manaTexture.Width, manaH), Color.White * opacity);

		Utils.DrawBorderString(spriteBatch, $"{potionPlayer.HealingLeft}/{potionPlayer.MaxHealing}",
			new Vector2(480, 112 + off),
			(potionPlayer.HealingLeft > 0 ? new Color(255, 200, 200) : Color.Gray) * opacity, 1f * opacity, 0.5f,
			0.5f);
		Utils.DrawBorderString(spriteBatch, $"{potionPlayer.ManaLeft}/{potionPlayer.MaxMana}",
			new Vector2(534, 112 + off),
			(potionPlayer.ManaLeft > 0 ? new Color(200, 220, 255) : Color.Gray) * opacity, 1f * opacity, 0.5f,
			0.5f);

		Texture2D glow = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/GlowSoft").Value;
		if (Main.LocalPlayer.HasBuff(BuffID.PotionSickness))
		{
			spriteBatch.Draw(glow, new Vector2(480, 60 + off), null, Color.Black, 0, glow.Size() / 2f, 1, 0, 0);
			Utils.DrawBorderString(spriteBatch,
				$"{Main.LocalPlayer.buffTime[Main.LocalPlayer.FindBuffIndex(BuffID.PotionSickness)] / 60 + 1}",
				new Vector2(480, 60 + off), Color.LightGray * opacity, 1f * opacity, 0.5f, 0.5f);
		}

		if (Main.LocalPlayer.HasBuff(BuffID.ManaSickness))
		{
			spriteBatch.Draw(glow, new Vector2(534, 60 + off), null, Color.Black, 0, glow.Size() / 2f, 1, 0, 0);
			Utils.DrawBorderString(spriteBatch,
				$"{Main.LocalPlayer.buffTime[Main.LocalPlayer.FindBuffIndex(BuffID.ManaSickness)] / 60 + 1}",
				new Vector2(534, 60 + off), Color.LightGray * opacity, 1f * opacity, 0.5f, 0.5f);
		}

		SkillCombatPlayer skillCombatPlayer = Main.LocalPlayer.GetModPlayer<SkillCombatPlayer>();
		if (skillCombatPlayer.HotbarSkills == null)
		{
			return;
		}

		DrawSkill(spriteBatch, off, opacity, glow, 0);
		DrawSkill(spriteBatch, off, opacity, glow, 1);
		DrawSkill(spriteBatch, off, opacity, glow, 2);
	}

	private static void DrawSkill(SpriteBatch spriteBatch, float off, float opacity, Texture2D glow, int skillIndex)
	{
		SkillCombatPlayer skillCombatPlayer = Main.LocalPlayer.GetModPlayer<SkillCombatPlayer>();

		if (skillCombatPlayer.HotbarSkills[skillIndex] is null)
		{
			return;
		}

		Skill skill = skillCombatPlayer.HotbarSkills[skillIndex];
		Texture2D texture = ModContent.Request<Texture2D>(skill.Texture).Value;
		var skillRect = new Rectangle(268 + 52 * skillIndex, (int)(8 + off) + texture.Height - 25, texture.Width, texture.Height);
		spriteBatch.Draw(texture,
			skillRect,
			new Rectangle(1, 2, texture.Width, texture.Height), Color.White * opacity);

		if (skill.Timer > 0)
		{
			spriteBatch.Draw(glow, new Vector2(291 + 52 * skillIndex, 55 + off), null, Color.Black, 0, glow.Size() / 2f, 1, 0, 0);
			Utils.DrawBorderString(spriteBatch, $"{skill.Timer / 60 + 1}", new Vector2(291 + 52 * skillIndex, 55 + off), Color.LightGray * opacity, 1f * opacity, 0.5f, 0.5f);
		}

		if (skillRect.Contains(Main.MouseScreen.ToPoint()))
		{
			string level = Language.GetText("Mods.PathOfTerraria.Skills.LevelLine").WithFormatArgs(skill.Level, skill.MaxLevel).Value;
			Tooltip.SetName(skill.DisplayName.Value + " " + level);

			string manaCost = Language.GetText("Mods.PathOfTerraria.Skills.ManaLine").WithFormatArgs(skill.ManaCost).Value;
			string weapon = Language.GetText("Mods.PathOfTerraria.Skills.WeaponLine").WithFormatArgs(skill.WeaponType).Value;
			string tooltip = skill.Description.Value + $"\n{manaCost}\n{weapon}";

			if (skill.Duration != 0)
			{
				string duration = Language.GetText("Mods.PathOfTerraria.Skills.DurationLine").WithFormatArgs($"{skill.Duration / 60f:#0.##}").Value;
				tooltip += "\n" + duration;
			}

			string cooldown = Language.GetText("Mods.PathOfTerraria.Skills.CooldownLine").WithFormatArgs($"{skill.MaxCooldown / 60f:#0.##}").Value;
			tooltip += "\n" + cooldown;

			Tooltip.SetTooltip(tooltip);
		}
	}

	private void DrawBuilding(SpriteBatch spriteBatch, float off, float opacity)
	{
		Texture2D building = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/HotbarBuilding").Value;
		Main.inventoryScale = Math.Max(opacity, 0f);

		Main.spriteBatch.Draw(building, new Vector2(20, 20 + off), null, Color.White * opacity);
		// ItemSlot.Draw(spriteBatch, ref Main.LocalPlayer.inventory[1], 21, new Vector2(24 + 62, 30 + off));

		// Let's not draw items if they aren't big enough since they look weird.
		if (opacity < 0.5f)
		{
			return;
		}

		for (int k = 2; k <= 9; k++)
		{
			ItemSlot.Draw(spriteBatch, ref Main.LocalPlayer.inventory[k], 21,
				new Vector2(24 + 124 + 52 * (k - 2), 30 + off), Color.White * opacity);
		}

		if (Main.LocalPlayer.selectedItem > 10)
		{
			Texture2D back = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/HotbarBack").Value;
			spriteBatch.Draw(back, new Vector2(24 + 126 + 52 * 8, 32 + off), null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0);
			ItemSlot.Draw(spriteBatch, ref Main.LocalPlayer.inventory[Main.LocalPlayer.selectedItem], 21, new Vector2(24 + 124 + 52 * 8, 30 + off));
		}
	}

	private void DrawSelector(SpriteBatch spriteBatch, float opacity) {
		Texture2D select = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/HotbarSelector").Value;

		// Render the special selector, which is always visible.
		spriteBatch.Draw(select, new Vector2(specialSelector.X, 20), null, Color.White);

		// Render the building selector, which is conditionally visible.
		spriteBatch.Draw(select, new Vector2(buildingSelector.X, 20), null, Color.White * opacity * (buildingSelector.Target == 98 ? (buildingSelector.X - 98) / 30f : 1f));
	}

	private static float Ease(float input)
	{
		const float c1 = 1.70158f;
		const float c3 = c1 + 1;

		return 1 + c3 * (float)Math.Pow(input - 1, 3) + c1 * (float)Math.Pow(input - 1, 2);
	}

	private void DrawHotkeys(SpriteBatch spriteBatch, float off)
	{
		//Don't draw hotkeys if the player isn't holding the first item in their inventory, as this is when these are visible
		if (!IsHoldingFirstHotbarItem())
		{
			return;
		}

		//Draw Potion Hotkeys
		List<string> quickHealHotkey = PlayerInput.CurrentProfile.InputModes[InputMode.Keyboard].KeyStatus["QuickHeal"];
		List<string> quickManaHotkey = PlayerInput.CurrentProfile.InputModes[InputMode.Keyboard].KeyStatus["QuickMana"];
		if (quickHealHotkey.Count > 0)
		{
			string assignedKey = quickHealHotkey[0];
			DrawLetter(spriteBatch, assignedKey, new Vector2(472, 71 + off), Color.White);
		}

		if (quickManaHotkey.Count <= 0)
		{
			return;
		}

		string assignedManaKey = quickManaHotkey[0];
		DrawLetter(spriteBatch, assignedManaKey, new Vector2(523, 71 + off), Color.White);

		// Draw Skill Hotkeys
		string skill1Key = SkillCombatPlayer.Skill1Keybind.GetAssignedKeys().FirstOrDefault();
		string skill2Key = SkillCombatPlayer.Skill2Keybind.GetAssignedKeys().FirstOrDefault();
		string skill3Key = SkillCombatPlayer.Skill3Keybind.GetAssignedKeys().FirstOrDefault();

		if (!string.IsNullOrEmpty(skill1Key))
		{
			DrawLetter(spriteBatch, skill1Key.Replace("D", ""), new Vector2(285, 71 + off), Color.White);
		}

		if (!string.IsNullOrEmpty(skill2Key))
		{
			DrawLetter(spriteBatch, skill2Key.Replace("D", ""), new Vector2(338, 71 + off), Color.White);
		}

		if (!string.IsNullOrEmpty(skill3Key))
		{
			DrawLetter(spriteBatch, skill3Key.Replace("D", ""), new Vector2(390, 71 + off), Color.White);
		}
	}

	private void DrawLetter(SpriteBatch spriteBatch, string letter, Vector2 position, Color color)
	{
		ChatManager.DrawColorCodedStringWithShadow(spriteBatch, _font, letter, position, color, 0f, Vector2.Zero, new Vector2(0.43f));
	}

	/// <summary>
	/// Checks if the first item in their inventory is being hovered. This is what should draw the combat hotbar
	/// </summary>
	/// <returns></returns>
	private static bool IsHoldingFirstHotbarItem()
	{
		Player player = Main.LocalPlayer;
		Item firstItem = player.inventory[0];
		Item heldItem = Main.LocalPlayer.HeldItem;
		return heldItem == firstItem;
	}
}

public class HijackHotbarClick : ModSystem
{
	public override void Load()
	{
		On_Main.GUIHotbarDrawInner += StopClickOnHotbar;
	}

	private void StopClickOnHotbar(On_Main.orig_GUIHotbarDrawInner orig, Main self)
	{
		bool hbLocked = Main.LocalPlayer.hbLocked; // Lock hotbar for the original method so we don't fight against vanilla
		Main.LocalPlayer.hbLocked = true;
		orig(self);
		Main.LocalPlayer.hbLocked = hbLocked;

		if (Main.LocalPlayer.selectedItem == 0) // If we're on the combat hotbar, don't do any of the following
		{
			DrawMainItemHover(hbLocked);
			DrawPotionHotbarTooltips(hbLocked);

			return;
		}

		Texture2D back = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/HotbarBack").Value;
		DrawBuildingHotbarTooltips(hbLocked, back);
	}

	private static void DrawMainItemHover(bool hbLocked)
	{
		const int FirstSlot = 0;

		if (!hbLocked && !PlayerInput.IgnoreMouseInterface && !Main.LocalPlayer.channel)
		{
			var pos = new Rectangle(26 * (FirstSlot + 1) - 4, 30, 60, 60);

			if (pos.Contains(Main.MouseScreen.ToPoint()))
			{
				Main.LocalPlayer.mouseInterface = true;
				Main.LocalPlayer.cursorItemIconEnabled = false;
				Main.hoverItemName = Main.LocalPlayer.inventory[FirstSlot].AffixName();
				Main.rare = Main.LocalPlayer.inventory[FirstSlot].rare;

				if (Main.mouseLeft && !hbLocked && !Main.blockMouse)
				{
					Main.LocalPlayer.changeItem = FirstSlot;
				}

				if (Main.LocalPlayer.inventory[FirstSlot].stack > 1)
				{
					Main.hoverItemName = Main.hoverItemName + " (" + Main.LocalPlayer.inventory[FirstSlot].stack + ")";
				}
			}
		}
	}

	private static void DrawPotionHotbarTooltips(bool hbLocked)
	{
		if (hbLocked)
		{
			return;
		}

		const int HotbarOffX = 20;
		const int HotbarOffY = 20;
		const int InnerHotbarOffX = 436;
		const int InnerHotbarOffY = 12;
		const int RealHotbarOffX = HotbarOffX + InnerHotbarOffX;
		const int RealHotbarOffY = HotbarOffY + InnerHotbarOffY;
		const int SlotSize = 48;
		const int SeparatorWidth = 4;

		int offX = RealHotbarOffX;
		int offY = RealHotbarOffY;

		for (int i = 0; i < 2; i++)
		{
			var pos = new Rectangle(offX, offY, SlotSize, SlotSize);

			if (pos.Contains(Main.MouseScreen.ToPoint()))
			{
				SetHealthOrManaTooltip(i == 0);
			}

			offX += SlotSize + SeparatorWidth;
		}
	}

	private static void SetHealthOrManaTooltip(bool health)
	{
		string type = health ? "Health" : "Mana";
		PotionSystem potions = Main.LocalPlayer.GetModPlayer<PotionSystem>();

		Tooltip.SetName(Language.GetTextValue($"Mods.PathOfTerraria.Misc.{type}PotionTooltip"));
		Tooltip.SetTooltip(
			Language.GetTextValue($"Mods.PathOfTerraria.Misc.Restores{type}Tooltip", health ? potions.HealPower : potions.ManaPower)
			+ "\n" + Language.GetTextValue($"Mods.PathOfTerraria.Misc.CooldownTooltip", MathF.Round((health ? potions.HealDelay : potions.ManaDelay) / 60f, 2).ToString("0.00")));
	}

	private static void DrawBuildingHotbarTooltips(bool hbLocked, Texture2D back)
	{
		for (int i = 1; i <= 9; i++) // This mimics how Terraria handles clicking on the slots by default. Almost entirely grabbed from the vanilla method this detours.
		{
			if (!hbLocked && !PlayerInput.IgnoreMouseInterface && !Main.LocalPlayer.channel)
			{
				var pos = new Rectangle(52 * (i + 1) - 4, 30, back.Width, back.Height);

				if (pos.Contains(Main.MouseScreen.ToPoint()))
				{
					Main.LocalPlayer.mouseInterface = true;
					Main.LocalPlayer.cursorItemIconEnabled = false;
					Main.hoverItemName = Main.LocalPlayer.inventory[i].AffixName();
					Main.rare = Main.LocalPlayer.inventory[i].rare;

					if (Main.mouseLeft && !hbLocked && !Main.blockMouse)
					{
						Main.LocalPlayer.changeItem = i;
					}

					if (Main.LocalPlayer.inventory[i].stack > 1)
					{
						Main.hoverItemName = Main.hoverItemName + " (" + Main.LocalPlayer.inventory[i].stack + ")";
					}
				}
			}
		}
	}
}