using System.Collections.Generic;
using System.Linq;
using PathOfTerraria.Common.Loaders.UILoading;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems;
using PathOfTerraria.Common.Systems.ModPlayers;
using ReLogic.Graphics;
using Terraria.GameContent;
using Terraria.GameContent.UI;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;
using Terraria.UI.Chat;

namespace PathOfTerraria.Common.UI;

internal class NewHotbar : SmartUIState
{
	private int _animation;

	private float _selectorX;
	private float _selectorTarget;
	private readonly DynamicSpriteFont _font = FontAssets.DeathText.Value;

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

		if (Main.LocalPlayer.selectedItem >= 2)
		{
			_selectorTarget = 24 + 120 + 52 * (MathF.Min(Main.LocalPlayer.selectedItem, 10) - 2);
		}
		else
		{
			_selectorTarget = 98;
		}

		_selectorX += (_selectorTarget - _selectorX) * 0.33f;

		DrawCombat(spriteBatch, -prog * 80, 1 - prog);
		DrawBuilding(spriteBatch, 80 - prog * 80, prog);
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

	private static void DrawCombat(SpriteBatch spriteBatch, float off, float opacity)
	{
		Texture2D combat = ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/GUI/HotbarCombat").Value;
		Main.inventoryScale = 36 / 52f * 52f / 36f * opacity;

		Main.spriteBatch.Draw(combat, new Vector2(20, 20 + off), null, Color.White * opacity);
		ItemSlot.Draw(spriteBatch, ref Main.LocalPlayer.inventory[0], 21, new Vector2(24, 30 + off));

		PotionSystem potionPlayer = Main.LocalPlayer.GetModPlayer<PotionSystem>();

		Texture2D bottleTex = ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/GUI/EmptyPotion").Value;
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

		Texture2D glow = ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/GUI/GlowSoft").Value;
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

		SkillPlayer skillPlayer = Main.LocalPlayer.GetModPlayer<SkillPlayer>();
		if (skillPlayer.Skills == null)
		{
			return;
		}

		if (skillPlayer.Skills[0] != null)
		{
			DrawSkill(spriteBatch, off, opacity, glow, 0);
		}

		if (skillPlayer.Skills[1] != null)
		{
			DrawSkill(spriteBatch, off, opacity, glow, 1);
		}

		if (skillPlayer.Skills[2] != null)
		{
			DrawSkill(spriteBatch, off, opacity, glow, 2);
		}
	}

	private static void DrawSkill(SpriteBatch spriteBatch, float off, float opacity, Texture2D glow, int skillIndex)
	{
		SkillPlayer skillPlayer = Main.LocalPlayer.GetModPlayer<SkillPlayer>();
		Skill skill = skillPlayer.Skills[skillIndex];
		Texture2D texture = ModContent.Request<Texture2D>(skill.Texture).Value;
		spriteBatch.Draw(texture,
			new Rectangle(268 + 52 * skillIndex, (int)(8 + off) + texture.Height - 25, texture.Width, texture.Height),
			new Rectangle(1, 2, texture.Width, texture.Height), Color.White * opacity);

		if (skill.Timer > 0)
		{
			spriteBatch.Draw(glow, new Vector2(291 + 52 * skillIndex, 55 + off), null, Color.Black, 0, glow.Size() / 2f, 1, 0, 0);
			Utils.DrawBorderString(spriteBatch, $"{skill.Timer / 60 + 1}", new Vector2(291 + 52 * skillIndex, 55 + off), Color.LightGray * opacity, 1f * opacity, 0.5f, 0.5f);
		}
	}

	private void DrawBuilding(SpriteBatch spriteBatch, float off, float opacity)
	{
		Texture2D building = ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/GUI/HotbarBuilding").Value;
		Main.inventoryScale = 36 / 52f * 52f / 36f * opacity;

		Main.spriteBatch.Draw(building, new Vector2(20, 20 + off), null, Color.White * opacity);
		ItemSlot.Draw(spriteBatch, ref Main.LocalPlayer.inventory[1], 21, new Vector2(24 + 62, 30 + off));

		for (int k = 2; k <= 9; k++)
		{
			ItemSlot.Draw(spriteBatch, ref Main.LocalPlayer.inventory[k], 21,
				new Vector2(24 + 124 + 52 * (k - 2), 30 + off));
		}

		Texture2D select = ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/GUI/HotbarSelector").Value;
		Main.spriteBatch.Draw(select, new Vector2(_selectorX, 21 + off), null,
			Color.White * opacity * (_selectorTarget == 98 ? (_selectorX - 98) / 30f : 1));

		if (Main.LocalPlayer.selectedItem > 10)
		{
			Texture2D back = ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/GUI/HotbarBack").Value;
			spriteBatch.Draw(back, new Vector2(24 + 126 + 52 * 8, 32 + off), null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0);
			ItemSlot.Draw(spriteBatch, ref Main.LocalPlayer.inventory[Main.LocalPlayer.selectedItem], 21, new Vector2(24 + 124 + 52 * 8, 30 + off));
		}
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
		string skill1Key = SkillPlayer.Skill1Keybind.GetAssignedKeys().FirstOrDefault();
		string skill2Key = SkillPlayer.Skill2Keybind.GetAssignedKeys().FirstOrDefault();
		string skill3Key = SkillPlayer.Skill3Keybind.GetAssignedKeys().FirstOrDefault();

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
		Texture2D back = ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/GUI/HotbarBack").Value;

		if (Main.LocalPlayer.selectedItem == 0) // If we're on the combat hotbar, don't do any of the following
		{
			DrawMainItemHover(hbLocked);
			DrawPotionHotbarTooltips(hbLocked);

			return;
		}

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
				Main.LocalPlayer.mouseInterface = true;
				Main.LocalPlayer.cursorItemIconEnabled = false;
				Main.hoverItemName = GetHealthOrManaTooltip(i == 0);
			}

			offX += SlotSize + SeparatorWidth;
		}
	}

	private static string GetHealthOrManaTooltip(bool health)
	{
		string type = health ? "Health" : "Mana";
		PotionSystem potions = Main.LocalPlayer.GetModPlayer<PotionSystem>();

		return Language.GetTextValue($"Mods.PathOfTerraria.Misc.{type}PotionTooltip")
			+ "\n" + Language.GetTextValue($"Mods.PathOfTerraria.Misc.Restores{type}Tooltip", health ? potions.HealPower : potions.ManaPower)
			+ "\n" + Language.GetTextValue($"Mods.PathOfTerraria.Misc.CooldownTooltip", MathF.Round((health ? potions.HealDelay : potions.ManaDelay) / 60f, 2).ToString("0.00"));
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