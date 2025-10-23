using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Core.Items;
using PathOfTerraria.Core.UI.SmartUI;
using ReLogic.Content;
using ReLogic.Graphics;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.RGB;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;
using Terraria.UI.Chat;

namespace PathOfTerraria.Common.UI.Hotbar;

public sealed class NewHotbar : SmartUiState
{
	/// <summary>
	///		Offsets the rendering of buffs to render below our custom hotbar.
	/// </summary>
	private sealed class OffsetBuffRendering : GlobalBuff
	{
		public override bool PreDraw(SpriteBatch spriteBatch, int type, int buffIndex, ref BuffDrawParams drawParams)
		{
			const int Offset = 24;

			drawParams.Position = new Vector2(drawParams.Position.X, drawParams.Position.Y + Offset);
			drawParams.MouseRectangle.Y += Offset;
			drawParams.TextPosition.Y += Offset;
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

	public record struct SkillTooltip(string Name, string Text, float Slot);

	public readonly static Dictionary<string, Asset<Texture2D>> Textures = [];

	/// <summary>
	/// Whether the <see cref="Main.LocalPlayer"/> is in "combat mode" (the weapon is selected and skills are displaying).
	/// </summary>
	public static bool LocalCombatMode => InCombatMode(Main.LocalPlayer);

	private readonly Selector specialSelector = new();
	private readonly Selector buildingSelector = new();
	private readonly DynamicSpriteFont _font = FontAssets.DeathText.Value;

	private int _animation;

	public override bool Visible => !Main.playerInventory;

	public static bool InCombatMode(Player player)
	{
		return player.selectedItem == 0;
	}

	public override int InsertionIndex(List<GameInterfaceLayer> layers)
	{
		return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		if (Textures.Count == 0)
		{
			// Spamming ModContent.Request requires a lot of string manipulation, many times per frame,
			// so caching is more efficient long term. It also makes it easier to read & write.

			Textures.Add("InactiveCombat", ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/HotbarSpecial_Inactive_Combat"));
			Textures.Add("InactiveBuilding", ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/HotbarSpecial_Inactive_Building"));
			Textures.Add("SpecialActive", ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/HotbarSpecial_Active"));
			Textures.Add("HotbarCombat", ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/HotbarCombat"));
			Textures.Add("EmptyPotion", ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/EmptyPotion"));
			Textures.Add("GlowSoft", ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/GlowSoft"));
			Textures.Add("HotbarBuilding", ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/HotbarBuilding"));
			Textures.Add("HotbarBack", ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/HotbarBack"));
			Textures.Add("HotbarSelector", ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/HotbarSelector"));
			Textures.Add("SkillDenialIcons", ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/SkillDenialIcons"));
		}

		// Make sure the player doesn't spam smart cursor if hovering over the hotbar
		Rectangle hoverBox = new(20, 20, 546, 78);

		if (hoverBox.Contains(Main.MouseScreen.ToPoint()))
		{
			Main.LocalPlayer.mouseInterface = true;
		}

		float prog;

		if (LocalCombatMode)
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
		Item item = Main.LocalPlayer.HeldItem;

		if (item.Name != null && item.Name != string.Empty)
		{
			text = item.AffixName(); // Otherwise the name of the item
		}

		var itemNamePosition = new Vector2(266f - (FontAssets.MouseText.Value.MeasureString(text) / 2f).X, 6f);
		Color itemNameColor = item.IsAir ? Color.White : ItemTooltips.GetRarityColor(item.GetInstanceData().Rarity);
		ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, text, itemNamePosition, itemNameColor, 0f, Vector2.Zero, Vector2.One * 0.9f);
	}

	/// <summary>
	///		Draws the two leftmost slots, which should always be visible and
	///		won't move as part of the hotbar transition.
	/// </summary>
	private static void DrawSpecial(SpriteBatch spriteBatch)
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
		// - render inactive slot textures for empty slots.

		Texture2D specialInactiveCombat = Textures["InactiveCombat"].Value;
		Texture2D specialInactiveBuilding = Textures["InactiveBuilding"].Value;
		Texture2D specialActive = Textures["SpecialActive"].Value;
		Main.inventoryScale = 1f; // 36 / 52f * 52f / 36f * 1 computes to 1...

		// Draw offhand slot
		Main.spriteBatch.Draw(specialActive, new Vector2(2, -2), new Rectangle(0, 0, 60, 72), Color.White, 0f, Vector2.Zero, 0.7f, SpriteEffects.None, 0);

		if (TryGetKeybindName(GearSwapKeybind.SwapKeybind, true, out string swapKey))
		{
			ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, FontAssets.ItemStack.Value, swapKey, new Vector2(8, 40), Color.White, 0f, Vector2.Zero, new Vector2(0.9f));
		}

		try
		{
			Main.inventoryScale = 0.7f;
			ItemSlot.Draw(spriteBatch, ref Main.LocalPlayer.GetModPlayer<GearSwapManager>().Inventory[0], ItemSlot.Context.HotbarItem, new Vector2(4, 4));
		}
		finally
		{
			Main.inventoryScale = 1f;
		}

		// Draw active slot textures (hotbar background), but only if there is an item in there.
		if (Main.LocalPlayer.inventory[Main.LocalPlayer.selectedItem] is { IsAir: false })
		{
			Main.spriteBatch.Draw(specialActive, new Vector2(20f), null, Color.White);
		}

		// Draw item slot items.
		for (int i = 0; i < 2; i++)
		{
			ItemSlot.Draw(spriteBatch, ref Main.LocalPlayer.inventory[i], ItemSlot.Context.HotbarItem, new Vector2(24 + (i * 62), 30));
		}

		// Draw greyed out icons when empty.
		for (int i = 0; i < 2; i++)
		{
			if (Main.LocalPlayer.inventory[i] is { IsAir: false })
			{
				continue;
			}

			Texture2D tex = i == 0 ? specialInactiveCombat : specialInactiveBuilding;
			var position = new Vector2(20 + (i * 62), 20);

			Main.spriteBatch.Draw(tex, position, Color.White);
		}
	}

	private static void DrawCombat(SpriteBatch spriteBatch, float off, float opacity)
	{
		Texture2D combat = Textures["HotbarCombat"].Value;
		Main.inventoryScale = opacity;

		Main.spriteBatch.Draw(combat, new Vector2(20, 20 + off), null, Color.White * opacity);
		// ItemSlot.Draw(spriteBatch, ref Main.LocalPlayer.inventory[0], 21, new Vector2(24, 30 + off));

		PotionPlayer potionPlayer = Main.LocalPlayer.GetModPlayer<PotionPlayer>();
		Texture2D bottleTex = Textures["EmptyPotion"].Value;

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

		Texture2D glow = Textures["GlowSoft"].Value;
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
		// Texture width is hardcoded as the UI doesn't account for anything larger and anything smaller should be on a 50x50 canvas anyway.
		const int TextureSize = 50;

		SkillCombatPlayer skillCombatPlayer = Main.LocalPlayer.GetModPlayer<SkillCombatPlayer>();
		var skillRect = new Rectangle(268 + 52 * skillIndex, (int)(8 + off) + TextureSize - 25, TextureSize, TextureSize);

		if (skillCombatPlayer.HotbarSkills[skillIndex] is null)
		{
			if (skillRect.Contains(Main.MouseScreen.ToPoint()))
			{
				Tooltip.Create(new TooltipDescription
				{
					Identifier = "Skill",
					SimpleTitle = Language.GetTextValue($"Mods.{PoTMod.ModName}.UI.SkillUI.NoSkill"),
					SimpleSubtitle = Language.GetTextValue($"Mods.{PoTMod.ModName}.UI.SkillUI.ChooseSkill"),
					VisibilityTimeInTicks = 0,
				});
			}

			return;
		}

		SkillFailure failure = default;
		Skill skill = skillCombatPlayer.HotbarSkills[skillIndex];
		var skillCenter = new Vector2(291 + 52 * skillIndex, 55 + off);
		bool cantUse = !skill.CanUseSkill(Main.LocalPlayer, ref failure, true);

		Texture2D texture = ModContent.Request<Texture2D>(skill.Texture).Value;
		spriteBatch.Draw(texture,
			skillRect,
			new Rectangle(1, 2, texture.Width, texture.Height), (cantUse ? Color.Gray : Color.White) * opacity);

		if (skill.Cooldown > 0)
		{
			spriteBatch.Draw(glow, skillCenter, null, Color.Black, 0, glow.Size() / 2f, 1, 0, 0);
			Utils.DrawBorderString(spriteBatch, $"{skill.Cooldown / 60 + 1}", new Vector2(291 + 52 * skillIndex, 55 + off), Color.LightGray * opacity, 1f * opacity, 0.5f, 0.5f);
		}
		else if (cantUse)
		{
			skillCenter += new Vector2(2, 0);

			if (failure.Reason != SkillFailReason.Other)
			{
				int slot = (int)failure.Reason;
				Texture2D tex = Textures["SkillDenialIcons"].Value;
				Rectangle source = new(48 * slot, 0, 46, 46);

				Color color = Color.Lerp(Color.White, Color.Red, MathF.Sin((float)Main.timeForVisualEffects * 0.07f) * 0.25f + 0.25f) * opacity;
				spriteBatch.Draw(tex, skillCenter, source, color, 0f, source.Size() / 2f, 1f, SpriteEffects.None, 0);
			}
			else
			{
				Texture2D tex = Textures["SkillDenialIcons"].Value;
				Rectangle source = new(48, 48, 46, 46);

				Color color = Color.Lerp(Color.White, Color.Red, MathF.Sin((float)Main.timeForVisualEffects * 0.07f) * 0.25f + 0.25f) * opacity;
				spriteBatch.Draw(tex, skillCenter, source, color, 0f, source.Size() / 2f, 1f, SpriteEffects.None, 0);
			}
		}

		if (skillRect.Contains(Main.MouseScreen.ToPoint()))
		{
			DrawSkillHoverTooltips(skill, skillIndex);

			if (Main.mouseRight && Main.mouseRightRelease)
			{
				skillCombatPlayer.HotbarSkills[skillIndex] = null;
			}
		}
	}

	internal static void DrawSkillHoverTooltips(Skill skill, int? skillIndex = null, bool ignoreCanUse = false)
	{
		string level = Language.GetText("Mods.PathOfTerraria.Skills.LevelLine").WithFormatArgs(skill.Level, skill.MaxLevel).Value;
		string title = skill.DisplayName.Value + " " + level;

		SkillFailure failure = default;
		bool canUse = skill.CanUseSkill(Main.LocalPlayer, ref failure, true);

		if (ignoreCanUse)
		{
			canUse = true;
		}

		List<SkillTooltip> tooltips = [];
		string manaPostfix = Main.LocalPlayer.statManaMax2 <= skill.TotalManaCost ? "NotEnoughMana" : "ManaLine";
		SkillTooltip manaCost = new("Mana", Language.GetText("Mods.PathOfTerraria.Skills." + manaPostfix).WithFormatArgs(skill.TotalManaCost).Value, 2);

		if (skill.WeaponType != ItemID.None)
		{
			Color color = canUse && failure.WeaponRejected ? Color.Red : Color.White;
			string text = Language.GetText("Mods.PathOfTerraria.SkillFailReasons.NeedsWeapon").WithFormatArgs(skill.WeaponType).Value;
			tooltips.Add(new("WeaponType", $"[c/{color.Hex3()}:{text}]", 0.5f));
		}

		if (!canUse && failure.Reason == SkillFailReason.Other)
		{
			tooltips.Add(new("Denial", $"[c/FF0000:{failure.Description.Value}]", 0.5f));
		}

		SkillTooltip noKeybindName = new("NoKeybind", Language.GetText("Mods.PathOfTerraria.Skills.NoKeybindLine").Value, 0);
		SkillTooltip keybindLine = new();

		if (skillIndex.HasValue)
		{
			string keybindName = skillIndex switch
			{
				0 => TryGetKeybindName(SkillCombatPlayer.Skill1Keybind, false, out string skill1KeybindName) ? skill1KeybindName : noKeybindName.Text,
				1 => TryGetKeybindName(SkillCombatPlayer.Skill2Keybind, false, out string skill2KeybindName) ? skill2KeybindName : noKeybindName.Text,
				2 => TryGetKeybindName(SkillCombatPlayer.Skill3Keybind, false, out string skill3KeybindName) ? skill3KeybindName : noKeybindName.Text,
				_ => ""
			};

			keybindLine = new("Keybind", Language.GetText("Mods.PathOfTerraria.Skills.KeybindLine").WithFormatArgs(keybindName).Value + "\n", 0);
			tooltips.Add(keybindLine);
		}

		tooltips.Add(manaCost);
		tooltips.Add(new SkillTooltip("Description", skill.Description.Value, 1));

		if (skill.Duration != 0)
		{
			tooltips.Add(new("Duration", Language.GetText("Mods.PathOfTerraria.Skills.DurationLine").WithFormatArgs($"{skill.Duration / 60f:#0.##}").Value, 4));
		}

		tooltips.Add(new("Cooldown", Language.GetText("Mods.PathOfTerraria.Skills.CooldownLine").WithFormatArgs($"{skill.MaxCooldown / 60f:#0.##}").Value, 5));

		if (ItemSlot.ShiftInUse)
		{
			tooltips.Add(new("Tags", GetDisplayTags(skill.Tags(), true), 0.25f));
		}
		else
		{
			tooltips.Add(new("NoTags", $"[c/888888:{Language.GetTextValue("Mods.PathOfTerraria.Skills.ShowTags")}]", 0.25f));
		}

		tooltips.Add(new("Empty", "\n", 0.3f));
		skill.ModifyTooltips(tooltips);
		tooltips.Sort(static (x, y) => x.Slot.CompareTo(y.Slot));

		List<DrawableTooltipLine> lines = [];

		foreach (SkillTooltip tooltip in tooltips)
		{
			TooltipLine line = new(PoTMod.Instance, tooltip.Name, tooltip.Text);
			Vector2 scale = Vector2.One;
			Color color = Color.White;

			if (line.Name is "Tags" or "Keybind" or "NoKeybind" or "NoTags")
			{
				scale = new(0.8f);
			}

			DrawableTooltipLine drawable = new(line, lines.Count, 0, 0, color)
			{
				BaseScale = scale,
			};

			lines.Add(drawable);
		}

		Tooltip.Create(new TooltipDescription
		{
			Identifier = "Skill",
			SimpleTitle = title + GetDisplayTags(skill.Tags(), false),
			Lines = lines,
			VisibilityTimeInTicks = 0,
			Stability = 1,
		});
	}

	private static string GetDisplayTags(SkillTags tags, bool shifting)
	{
		string text = shifting ? Language.GetTextValue("Mods.PathOfTerraria.Skills.TagsLine") + "  " : "";

		foreach (Enum value in Enum.GetValues<SkillTags>())
		{
			var tag = (SkillTags)value;

			// Only show singular tags, not compound tags like Elemental
			if (System.Numerics.BitOperations.PopCount((ulong)tag) == 1 && tags.HasFlag(tag))
			{
				string name = "";
				string tagTex = $"[tex/s1.00,h,y=-4:{PoTMod.ModName}/Assets/UI/SkillTagIcons/{tag}]";

				if (shifting)
				{
					text += $"{tagTex}[c/888888:{Language.GetTextValue("Mods.PathOfTerraria.Skills.Tags." + tag)}], ";
				}
				else
				{
					text += $" {name}{tagTex}";
				}
			}
		}

		if (shifting)
		{
			text = text[..^2];
		}

		text += " ";
		return text[..^1];
	}

	private static void DrawBuilding(SpriteBatch spriteBatch, float off, float opacity)
	{
		Texture2D building = Textures["HotbarBuilding"].Value;
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
			var pos = new Vector2(24 + 124 + 52 * (k - 2), 30 + off);
			ItemSlot.Draw(spriteBatch, ref Main.LocalPlayer.inventory[k], 21, pos, Color.White * opacity);
		}

		if (Main.LocalPlayer.selectedItem > 10)
		{
			Texture2D back = Textures["HotbarBack"].Value;
			spriteBatch.Draw(back, new Vector2(24 + 126 + 52 * 8, 32 + off), null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0);
			ItemSlot.Draw(spriteBatch, ref Main.LocalPlayer.inventory[Main.LocalPlayer.selectedItem], 21, new Vector2(24 + 124 + 52 * 8, 30 + off));
		}
	}

	private void DrawSelector(SpriteBatch spriteBatch, float opacity)
	{
		Texture2D select = Textures["HotbarSelector"].Value;

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
		if (!LocalCombatMode)
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
		if (TryGetKeybindName(SkillCombatPlayer.Skill1Keybind, true, out string skill1KeybindName))
		{
			DrawLetter(spriteBatch, skill1KeybindName, new Vector2(285, 71 + off), Color.White, 0.35f);
		}

		if (TryGetKeybindName(SkillCombatPlayer.Skill2Keybind, true, out string skill2KeybindName))
		{
			DrawLetter(spriteBatch, skill2KeybindName, new Vector2(338, 71 + off), Color.White, 0.35f);
		}

		if (TryGetKeybindName(SkillCombatPlayer.Skill3Keybind, true, out string skill3KeybindName))
		{
			DrawLetter(spriteBatch, skill3KeybindName, new Vector2(390, 71 + off), Color.White, 0.35f);
		}
	}

	private void DrawLetter(SpriteBatch spriteBatch, string letter, Vector2 position, Color color, float scale = 0.43f)
	{
		ChatManager.DrawColorCodedStringWithShadow(spriteBatch, _font, letter, position, color, 0f, Vector2.Zero, new Vector2(scale));
	}

	private static bool TryGetKeybindName(ModKeybind key, bool trim, [NotNullWhen(true)] out string result)
	{
		string name = key.GetAssignedKeys().FirstOrDefault();
		result = null;

		if (string.IsNullOrEmpty(name))
		{
			return false;
		}

		// Remove 'D' from numerical keybinds. "D1" -> "1"
		if (name[0] == 'D' && int.TryParse(name[1].ToString(), out _))
		{
			name = name[1..];
		}

		if (trim)
		{
			result = name.Length > 1 ? $"{name[0]}.." : name[0].ToString();
		}
		else
		{
			result = name;
		}

		return true;
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

		try
		{
			Main.LocalPlayer.hbLocked = true;
			orig(self);
		}
		finally
		{
			Main.LocalPlayer.hbLocked = hbLocked;
		}

		bool combatMode = NewHotbar.LocalCombatMode;

		if (combatMode)
		{
			DrawPotionHotbarTooltips(hbLocked);
		}

		if (NewHotbar.Textures.TryGetValue("HotbarBack", out Asset<Texture2D> back))
		{
			DrawItemHotbarTooltips(hbLocked, combatMode, back.Value);
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

			if (!Main.playerInventory && pos.Contains(Main.MouseScreen.ToPoint()))
			{
				SetHealthOrManaTooltip(i == 0);
			}

			offX += SlotSize + SeparatorWidth;
		}
	}

	private static void SetHealthOrManaTooltip(bool health)
	{
		string type = health ? "Health" : "Mana";
		PotionPlayer potions = Main.LocalPlayer.GetModPlayer<PotionPlayer>();

		Tooltip.Create(new TooltipDescription
		{
			Identifier = type,
			SimpleTitle = Language.GetTextValue($"Mods.PathOfTerraria.Misc.{type}PotionTooltip"),
			SimpleSubtitle = Language.GetTextValue($"Mods.PathOfTerraria.Misc.Restores{type}Tooltip", health ? potions.HealPower : potions.ManaPower)
				+ "\n" + Language.GetTextValue($"Mods.PathOfTerraria.Misc.CooldownTooltip", MathF.Round((health ? potions.HealDelay : potions.ManaDelay) / 60f, 2).ToString("0.00")),
			VisibilityTimeInTicks = 0,
		});
	}

	private static void DrawItemHotbarTooltips(bool hbLocked, bool combatMode, Texture2D back)
	{
		// Combat mode just needs the first two slots handled, otherwise we go through all the slots.
		int start = combatMode ? 0 : 0;
		int end = combatMode ? 1 : 9;

		const int FirstSlot = 0;
		const int NumLargeSlots = 2;
		const int BaseXOffset = 20;
		const int BaseYOffset = 30;
		const int LargeToSmallXOffset = 6;
		(int LargeXSize, int LargeYSize, int LargeXOffset) = (60, 60, 2);
		(int SmallXSize, int SmallYSize, int SmallXOffset) = (back.Width, back.Height, 4);

		if (!PlayerInput.IgnoreMouseInterface && !Main.LocalPlayer.channel && !Main.playerInventory && !hbLocked)
		{
			for (int i = start; i <= end; i++) // This mimics how Terraria handles clicking on the slots by default. Almost entirely grabbed from the vanilla method this detours.
			{
				var pos = new Rectangle(
					BaseXOffset
					+ (Math.Min(i, NumLargeSlots) * (LargeXSize + LargeXOffset))
					+ (i >= NumLargeSlots ? LargeToSmallXOffset : 0)
					+ (Math.Max(0, i - NumLargeSlots) * (SmallXSize + SmallXOffset)),
					BaseYOffset,
					i < NumLargeSlots ? LargeXSize : SmallXSize,
					i < NumLargeSlots ? LargeYSize : SmallYSize
				);

				if (pos.Contains(Main.MouseScreen.ToPoint()))
				{
					bool fullTooltip = combatMode && i == FirstSlot;

					Main.LocalPlayer.mouseInterface = true;
					Main.LocalPlayer.cursorItemIconEnabled = false;
					Main.rare = Main.LocalPlayer.inventory[i].rare;

					if (fullTooltip)
					{
						Main.HoverItem = Main.LocalPlayer.inventory[FirstSlot].Clone();
					}
					else
					{
						Main.hoverItemName = Main.LocalPlayer.inventory[i].AffixName();
					}

					if (Main.mouseLeft && !hbLocked && !Main.blockMouse)
					{
						Main.LocalPlayer.changeItem = i;
					}

					if (!fullTooltip && Main.LocalPlayer.inventory[i].stack > 1)
					{
						Main.hoverItemName = Main.hoverItemName + " (" + Main.LocalPlayer.inventory[i].stack + ")";
					}

					break;
				}
			}
		}
	}
}