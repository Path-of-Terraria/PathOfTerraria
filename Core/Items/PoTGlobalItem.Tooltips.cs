using System.Collections.Generic;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.UI;
using System.Text.RegularExpressions;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.ModPlayers;
using System.Linq;
using Terraria.Localization;
using ReLogic.Content;
using PathOfTerraria.Content.Items.Consumables.Maps;
using PathOfTerraria.Content.Items.Gear;
using PathOfTerraria.Common.Items;

namespace PathOfTerraria.Core.Items;

// Handles modifying and rendering tooltips.

partial class PoTGlobalItem
{
	private sealed class Patcher : ILoadable
	{
		void ILoadable.Load(Mod mod)
		{
			On_ItemSlot.Draw_SpriteBatch_ItemArray_int_int_Vector2_Color += DrawSpecial;
		}

		void ILoadable.Unload() { }
	}

	/// <summary>
	/// Lookup table for textures so we skip some performance.<br/>
	/// Keys:<br/><c>Normal, Magic, Rare, Unique, Favorite</c>
	/// </summary>
	private readonly static Dictionary<string, Asset<Texture2D>> Textures = [];

	#region Modify tooltips and rendering

	public override bool PreDrawTooltipLine(Item item, DrawableTooltipLine line, ref int yOffset)
	{
		// Reduce size of tooltips to fit the "Description"s we add in
		if (line.Name.StartsWith("Tooltip") || line.Name == "SetBonus")
		{
			yOffset = -2;
			line.BaseScale = new Vector2(0.8f);
			return true;
		}

		// Don't mess with tooltip lines that we aren't responsible for.
		if (line.Mod != Mod.Name)
		{
			return true;
		}

		switch (line.Name)
		{
			case "Name":
				yOffset = -2;
				line.BaseScale = new Vector2(1.1f);
				return true;

			case "Rarity" or "Corrupted" or "Cloned":
				yOffset = -8;
				line.BaseScale = new Vector2(0.8f);
				return true;

			case "ItemLevel":
				yOffset = item.ModItem is Map ? -8 : 2;
				line.BaseScale = new Vector2(0.8f);
				return true;

			case "MapTier":
				yOffset = -2;
				line.BaseScale = new Vector2(0.8f);
				return true;

			case "AltUseDescription":
			case "ManaCost":
			case "Description":
			case "ShiftNotice":
			case "SwapNotice":
				yOffset = 2;
				line.BaseScale = new Vector2(0.8f);
				return true;
		}

		if (line.Name.Contains("Affix") || line.Name.Contains("Socket") || line.Name == "Damage" || line.Name == "Defense" || line.Name.StartsWith("Stat"))
		{
			line.BaseScale = new Vector2(0.95f);
			yOffset = line.Name == "Damage" || line.Name == "Defense" || line.Name.StartsWith("Stat") ? 2 : -4;
			return true;
		}

		if (line.Name == "Space")
		{
			line.BaseScale *= 0f;
			yOffset = -20;
			return true;
		}

		if (line.Name.Contains("Change"))
		{
			line.BaseScale = new Vector2(0.75f);
			yOffset = -10;
			return true;
		}

		return base.PreDrawTooltipLine(item, line, ref yOffset);
	}

	public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
	{
		static string Localize(string key)
		{
			return Language.GetTextValue($"Mods.{PoTMod.ModName}.Gear.Tooltips." + key);
		}

		base.ModifyTooltips(item, tooltips);
		var oldTooltips = tooltips.Where(x => x.Name.StartsWith("Tooltip")).ToList();
		var oldStats = tooltips.Where(x => x.Name.StartsWith("Stat")).ToList();

		TooltipLine setBonusLine = tooltips.FirstOrDefault(x => x.Name == "SetBonus");
		TooltipLine nameLine = tooltips.FirstOrDefault(x => x.Name == "ItemName");
		TooltipLine priceLine = tooltips.FirstOrDefault(x => x.FullName == "Terraria/Price");
		TooltipLine materialLine = tooltips.FirstOrDefault(x => x.FullName == "Terraria/Material");

		if (setBonusLine is not null)
		{
			oldTooltips.Add(setBonusLine);
		}

		tooltips.Clear();

		PoTInstanceItemData data = item.GetInstanceData();
		PoTStaticItemData staticData = item.GetStaticData();

		if (data.SpecialName == string.Empty) // Make sure SpecialName generated if it hasn't already for some reason
		{
			data.SpecialName = GenerateName.Invoke(item);
		}

		if (nameLine is null)
		{
			nameLine = new TooltipLine(Mod, "Name", data.SpecialName)
			{
				OverrideColor = GetRarityColor(data.Rarity)
			};
		}
		else
		{
			nameLine.Text = data.SpecialName;
			nameLine.OverrideColor ??= GetRarityColor(data.Rarity);
		}

		AddNewTooltipLine(item, tooltips, nameLine);

		if (data.Corrupted)
		{
			AddNewTooltipLine(item, tooltips, new TooltipLine(Mod, "Corrupted", $" {Localize("Corrupted")}")
			{
				OverrideColor = Color.Lerp(Color.Purple, Color.White, 0.4f)
			});
		}
		
		if (data.Cloned)
		{
			AddNewTooltipLine(item, tooltips, new TooltipLine(Mod, "Cloned", $" {Localize("Cloned")}")
			{
				OverrideColor = Color.Lerp(Color.DarkCyan, Color.White, 0.4f)
			});
		}

		string rarityDesc = GetDescriptor(data.ItemType, data.Rarity, data.Influence);

		if (item.ModItem is GearLocalizationCategory.IItem gear)
		{
			string name = Language.GetTextValue("Mods.PathOfTerraria.Gear." + GearLocalizationCategory.Invoke(item) + ".Name");
			rarityDesc = GetDescriptor(name, data.Rarity, data.Influence);
		}

		if (!string.IsNullOrWhiteSpace(rarityDesc))
		{
			var rarityLine = new TooltipLine(Mod, "Rarity", rarityDesc)
			{
				OverrideColor = Color.Lerp(GetRarityColor(data.Rarity), Color.White, 0.5f)
			};
			AddNewTooltipLine(item, tooltips, rarityLine);
		}

		var itemLevelLine = new TooltipLine(Mod, "ItemLevel", $" {Localize("Level")} [c/CCCCFF:{GetItemLevel.Invoke(item)}]")
		{
			OverrideColor = new Color(170, 170, 170)
		};
		AddNewTooltipLine(item, tooltips, itemLevelLine);

		if (!string.IsNullOrWhiteSpace(staticData.AltUseDescription.Value))
		{
			AddNewTooltipLine(item, tooltips, new TooltipLine(Mod, "AltUseDescription", staticData.AltUseDescription.Value));
		}

		if (!string.IsNullOrWhiteSpace(staticData.Description.Value))
		{
			AddNewTooltipLine(item, tooltips, new(Mod, "Description", staticData.Description.Value));
		}

		if (item.damage > 0)
		{
			// TODO: Slice first space in damage type display name...
			string highlightNumbers = HighlightNumbers(
				$"[{Math.Round(item.damage * 0.85f, 2)}-{Math.Round(item.damage * 1.15f, 2)}] {Localize("Damage")} ({item.DamageType.DisplayName.Value.Trim()})",
				baseColor: "DDDDDD");
			var damageLine = new TooltipLine(Mod, "Damage", $"[i:{ItemID.SilverBullet}] {highlightNumbers}");
			AddNewTooltipLine(item, tooltips, damageLine);
		}
		
		if (item.useTime > 0) // Attack or cast speed
		{
			float aps = 60f / item.useTime;
			aps = (float) Math.Round(aps, 2);
			string apsStr = aps.ToString("0.00");
			string localizeString = item.DamageType == DamageClass.Magic ? "CastSpeed" : "AttackSpeed";
			var attackSpeed = new TooltipLine(Mod, "AttacksPerSecond", $"[i:{ItemID.SilverBullet}] [{apsStr}] {Localize(localizeString)}");
			AddNewTooltipLine(item, tooltips, attackSpeed);
		}
		
		if (item.mana > 0)
		{
			string manaCost = $"{Localize("ManaCost")}: {item.mana}";
			var manaLine = new TooltipLine(Mod, "ManaCost", manaCost)
			{
				OverrideColor = Color.Cyan
			};
			AddNewTooltipLine(item, tooltips, manaLine);
		}

		if (item.defense > 0)
		{
			var def = new TooltipLine(Mod, "Defense", $"[i:{ItemID.SilverBullet}] " + HighlightNumbers($"+{item.defense} {Localize("Defense")}", baseColor: "DDDDDD"));
			AddNewTooltipLine(item, tooltips, def);
		}

		if (item.pick > 0)
		{
			var pick = new TooltipLine(Mod, "Pickaxe", $"[i:{ItemID.SilverBullet}] {HighlightNumbers($"{item.pick} {Localize("Pickaxe")}", baseColor: "DDDDDD")}");
			AddNewTooltipLine(item, tooltips, pick);
		}
		
		if (item.axe > 0)
		{
			var axe = new TooltipLine(Mod, "Axe", $"[i:{ItemID.SilverBullet}] {HighlightNumbers($"{item.axe * 5} {Localize("Axe")}", baseColor: "DDDDDD")}");
			AddNewTooltipLine(item, tooltips, axe);
		}
		
		if (item.hammer > 0)
		{
			var hammer = new TooltipLine(Mod, "Hammer", $"[i:{ItemID.SilverBullet}] {HighlightNumbers($"{item.hammer} {Localize("Hammer")}", baseColor: "DDDDDD")}");
			AddNewTooltipLine(item, tooltips, hammer);
		}
		
		tooltips.AddRange(oldStats);

		if (item.ModItem is Map map && map.Tier > 0)
		{
			AddNewTooltipLine(item, tooltips, new TooltipLine(Mod, "MapTier", $" {Localize("Tier")} [c/CCCCFF:" + map.Tier + "]")
			{
				OverrideColor = new Color(170, 170, 170)
			});
		}

		// Affix tooltips
		InsertAdditionalTooltipLines.Invoke(item, tooltips);
		AffixTooltipsHandler.DefaultColor = Color.Green; // Makes any new affixes from this item show as green as they are new and beneficial by default
		PoTItemHelper.ApplyAffixTooltips(item, Main.LocalPlayer); // Adds in affix tooltips from this item without applying effects
		Main.LocalPlayer.GetModPlayer<UniversalBuffingPlayer>().PrepareComparisonTooltips(tooltips, item);
		AffixTooltipsHandler.DefaultColor = Color.White; // Resets color

		// These don't need AddNewTooltipLine as they're vanilla tooltips
		tooltips.AddRange(oldTooltips);

		if (materialLine is not null)
		{
			tooltips.Add(materialLine);
		}
		
		if (priceLine is not null)
		{
			tooltips.Add(priceLine);
		}
	}

	/// <summary>
	/// Adds the tooltip line if the <paramref name="item"/> is not a Gear item, or if <see cref="Gear.ModifyNewTooltipLine(TooltipLine)"/> returns true.
	/// </summary>
	/// <param name="item">Item referenced.</param>
	/// <param name="tooltips">List of tooltips.</param>
	/// <param name="staticData"></param>
	/// <param name="tooltip"></param>
	private static void AddNewTooltipLine(Item item, List<TooltipLine> tooltips, TooltipLine tooltip)
	{
		if (item.ModItem is not Gear gearItem || gearItem.ModifyNewTooltipLine(tooltip))
		{
			tooltips.Add(tooltip);
		}
	}

	private static string HighlightNumbers(string input, string numColor = "CCCCFF", string baseColor = "A0A0A0")
	{
		Regex regex = NumberHighlightRegex();

		return regex.Replace(input, match =>
		{
			if (match.Groups[1].Success)
			{
				return $"[c/{numColor}:{match.Value}]";
			}

			if (match.Groups[2].Success)
			{
				return $"[c/{baseColor}:{match.Value}]";
			}

			return match.Value;
		});
	}

	public static Color GetRarityColor(ItemRarity rarity)
	{
		return rarity switch
		{
			ItemRarity.Normal => Color.White,
			ItemRarity.Magic => new Color(110, 160, 255),
			ItemRarity.Rare => new Color(255, 255, 50),
			ItemRarity.Unique => new Color(25, 255, 25),
			_ => Color.White,
		};
	}

	/// <summary>
	/// Gets the name of an item in the format Prefix ItemName Suffix.
	/// </summary>
	/// <param name="type">Type of the item.</param>
	/// <param name="rare">Rarity of the item.</param>
	/// <param name="influence">Influence of the item.</param>
	/// <returns>The full name of the item.</returns>
	private static string GetDescriptor(ItemType type, ItemRarity rare, Influence influence)
	{
		string typeName = Language.GetOrRegister("Mods.PathOfTerraria.Gear." + type + ".Name", () => "").Value;
		return GetDescriptor(typeName, rare, influence);
	}

	/// <summary>
	/// Gets the name of an item in the format Prefix ItemName Suffix.
	/// </summary>
	/// <param name="typeName">Base name of the item.</param>
	/// <param name="rare">Rarity of the item.</param>
	/// <param name="influence">Influence of the item.</param>
	/// <returns>The full name of the item.</returns>
	private static string GetDescriptor(string typeName, ItemRarity rare, Influence influence)
	{
		string rareName = Language.GetTextValue("Mods.PathOfTerraria.Gear.Rarity." + rare, () => "");
		string influenceName = Language.GetTextValue("Mods.PathOfTerraria.Gear.Influence." + influence, () => "");
		return $" {influenceName}{rareName}{typeName}";
	}

	[GeneratedRegex(@"(\d+)|(\D+)")]
	private static partial Regex NumberHighlightRegex();
	#endregion

	private static void LoadBackImages()
	{
		Textures.Clear();
		Textures.Add("Normal", ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/Slots/NormalBack"));
		Textures.Add("Magic", ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/Slots/MagicBack"));
		Textures.Add("Rare", ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/Slots/RareBack"));
		Textures.Add("Unique", ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/Slots/UniqueBack"));
		Textures.Add("Favorite", ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/Slots/FavoriteOverlay"));
	}

	#region Special rendering for rarities and influences
	private static void DrawSpecial(On_ItemSlot.orig_Draw_SpriteBatch_ItemArray_int_int_Vector2_Color orig, SpriteBatch sb,
		Item[] inv, int context, int slot, Vector2 position, Color color)
	{
		if (!GearGlobalItem.IsGearItem(inv[slot]) || context == 21 || context == ItemSlot.Context.ChatItem)
		{
			orig(sb, inv, context, slot, position, color);
			return;
		}

		PoTInstanceItemData data = inv[slot].GetInstanceData();

		string rareName = data.Rarity switch
		{
			ItemRarity.Magic => "Magic",
			ItemRarity.Rare => "Rare",
			ItemRarity.Unique => "Unique",
			_ => "Normal"
		};

		Color backColor = Color.White * 0.75f;
		sb.Draw(Textures[rareName].Value, position, null, backColor, 0f, default, Main.inventoryScale, SpriteEffects.None, 0f);

		if (inv[slot].favorited)
		{
			sb.Draw(Textures["Favorite"].Value, position, null, backColor, 0f, default, Main.inventoryScale, SpriteEffects.None, 0f);
		}

		// For some reason this *hard* crashes if it's a HotbarItem.
		// Instead, CountDisplayItem has a manual override.
		CountDisplayItem.ForcedContext = ItemSlot.Context.HotbarItem;
		ItemSlot.Draw(sb, ref inv[slot], ItemSlot.Context.MouseItem, position);
		CountDisplayItem.ForcedContext = -1;

		if (data.Influence == Influence.Solar)
		{
			DrawSolarSlot(sb, position);
		}

		if (data.Influence == Influence.Lunar)
		{
			DrawLunarSlot(sb, position);
		}
	}

	/// <summary>
	/// Draws the shader overlay for solar items
	/// </summary>
	/// <param name="spriteBatch"></param>
	/// <param name="pos"></param>
	private static void DrawSolarSlot(SpriteBatch spriteBatch, Vector2 pos)
	{
		Texture2D tex = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/Slots/SlotMap").Value;

		Effect effect = Filters.Scene["ColoredFire"].GetShader().Shader;

		if (effect is null)
		{
			return;
		}

		effect.Parameters["u_time"].SetValue(Main.GameUpdateCount * 0.015f % 2f);
		effect.Parameters["primary"].SetValue(new Vector3(1, 1, 0.2f) * 0.7f);
		effect.Parameters["primaryScaling"].SetValue(new Vector3(1, 1.3f, 1));
		effect.Parameters["secondary"].SetValue(new Vector3(0.85f, 0.6f, 0.35f) * 0.7f);

		effect.Parameters["sampleTexture"]
			.SetValue(ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/Misc/SwirlNoise").Value);
		effect.Parameters["mapTexture"]
			.SetValue(ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/Misc/SwirlNoise").Value);

		spriteBatch.End();
		spriteBatch.Begin(default, BlendState.Additive, default, default, RasterizerState.CullNone, effect,
			Main.UIScaleMatrix);

		spriteBatch.Draw(tex, pos, null, Color.White, 0, Vector2.Zero, Main.inventoryScale, 0, 0);

		spriteBatch.End();
		spriteBatch.Begin(default, default, default, default, RasterizerState.CullNone, default,
			Main.UIScaleMatrix);
	}

	/// <summary>
	/// Draws the shader overlay for lunar items
	/// </summary>
	/// <param name="spriteBatch"></param>
	/// <param name="pos"></param>
	private static void DrawLunarSlot(SpriteBatch spriteBatch, Vector2 pos)
	{
		Texture2D tex = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/Slots/SlotMap").Value;

		Effect effect = Filters.Scene["LunarEffect"].GetShader().Shader;

		if (effect is null)
		{
			return;
		}

		effect.Parameters["u_time"].SetValue(Main.GameUpdateCount * 0.006f % 2f);
		effect.Parameters["primary"].SetValue(new Vector3(0.4f, 0.8f, 1f) * 0.7f);
		effect.Parameters["primaryScaling"].SetValue(new Vector3(1, 1.1f, 1));
		effect.Parameters["secondary"].SetValue(new Vector3(0.4f, 0.4f, 0.9f) * 0.7f);

		effect.Parameters["sampleTexture"]
			.SetValue(ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/Misc/ShaderNoise").Value);
		effect.Parameters["mapTexture"]
			.SetValue(ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/Misc/ShaderNoise").Value);

		spriteBatch.End();
		spriteBatch.Begin(default, BlendState.Additive, default, default, RasterizerState.CullNone, effect,
			Main.UIScaleMatrix);

		spriteBatch.Draw(tex, pos, null, Color.White, 0, Vector2.Zero, Main.inventoryScale, 0, 0);

		spriteBatch.End();
		spriteBatch.Begin(default, default, default, default, RasterizerState.CullNone, default,
			Main.UIScaleMatrix);
	}
	#endregion
}
