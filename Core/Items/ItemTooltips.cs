using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Items;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.DisableBuilding;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Content.Items.Consumables.Maps;
using PathOfTerraria.Content.Items.Gear;
using PathOfTerraria.Utilities.Xna;
using ReLogic.Content;
using Terraria.Graphics.Effects;
using Terraria.Localization;
using Terraria.UI;

namespace PathOfTerraria.Core.Items;

// Handles modifying and rendering tooltips.

public sealed partial class ItemTooltips : GlobalItem
{
	private sealed class Patcher : ILoadable
	{
		void ILoadable.Load(Mod mod)
		{
			On_ItemSlot.Draw_SpriteBatch_ItemArray_int_int_Vector2_Color += DrawSpecial;
		}

		void ILoadable.Unload() { }
	}

	// Commonly used colors.
	public static class Colors
	{
		public static Color DefaultText => ColorUtils.FromHexRgb(0x_dddddd);
		public static Color DefaultNumber => ColorUtils.FromHexRgb(0x_a1bdbd);
		public static Color Positive => ColorUtils.FromHexRgb(0x_99e550);
		public static Color Negative => ColorUtils.FromHexRgb(0x_ac3232);
		public static Color Levels => ColorUtils.FromHexRgb(0x_d8b47a);
		// Accents
		public static Color StatsAccent => ColorUtils.FromHexRgb(0x_a1bdbd);
		public static Color AffixAccent => ColorUtils.FromHexRgb(0x_ff2222);

		public static Color Corrupt => Color.Lerp(Color.Purple, Color.White, 0.4f);
		public static Color Cloned => ColorUtils.FromHexRgb(0x_37946e);
		public static Color ManaCost => ColorUtils.FromHexRgb(0x_5fcde4);
	}

	public static string ColoredDot(Color color)
	{
		return $"[c/{ColorUtils.ToHexRGB(color)}:◙]";
	}

	/// <summary>
	/// Lookup table for textures so we skip some performance.<br/>
	/// Keys:<br/><c>Normal, Magic, Rare, Unique, Favorite</c>
	/// </summary>
	private readonly static Dictionary<string, Asset<Texture2D>> Textures = [];

	public override void Load()
	{
		LoadBackImages();
	}

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
			case "Description":
			case "ShiftNotice":
			case "SwapNotice":
			case "BuildBlocked":
			case "ShootBlocked":
				yOffset = 2;
				line.BaseScale = new Vector2(0.8f);
				return true;
		}

		if (line.Name.Contains("Affix") || line.Name.Contains("Socket") || line.Name.StartsWith("Stat")
			|| line.Name is "Damage" or "Defense" or "AttacksPerSecond" or "CriticalStrikeChance" or "ManaCost")
		{
			line.BaseScale = new Vector2(0.95f);
			yOffset = -4;
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

		var oldTooltips = tooltips.Where(x => x.Name.StartsWith("Tooltip")).ToList();
		var oldStats = tooltips.Where(x => x.Name.StartsWith("Stat")).ToList();

		TooltipLine setBonusLine = tooltips.FirstOrDefault(x => x.Name == "SetBonus");
		TooltipLine nameLine = tooltips.FirstOrDefault(x => x.Name == "ItemName");
		TooltipLine priceLine = tooltips.FirstOrDefault(x => x.FullName == "Terraria/Price");
		TooltipLine materialLine = tooltips.FirstOrDefault(x => x.FullName == "Terraria/Material");
		TooltipLine placeableLine = tooltips.FirstOrDefault(x => x.FullName == "Terraria/Placeable");

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
				OverrideColor = Colors.Corrupt,
			});
		}
		
		if (data.Cloned)
		{
			AddNewTooltipLine(item, tooltips, new TooltipLine(Mod, "Cloned", $" {Localize("Cloned")}")
			{
				OverrideColor = Colors.Cloned
			});
		}

		if (data.ItemType != ItemType.None || data.Rarity > ItemRarity.Normal)
		{
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
		}

		if (GetItemLevel.Invoke(item) is > 0 and int level)
		{
			var itemLevelLine = new TooltipLine(Mod, "ItemLevel", $" {Localize("Level")} [c/{ColorUtils.ToHexRGB(Colors.Levels)}:{level}]")
			{
				OverrideColor = Colors.Levels,
			};
			AddNewTooltipLine(item, tooltips, itemLevelLine);
		}

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
			Player player = Main.LocalPlayer;
    
			// Get the weapon's damage type
			DamageClass damageClass = item.DamageType;
    
			// Calculate the actual damage the player would deal with this weapon
			StatModifier damage = player.GetTotalDamage(damageClass);
			int baseDamage = item.damage;
			
			float finalDamage = damage.ApplyTo(baseDamage);
    
			//15% var
			float minDamage = finalDamage * 0.85f;
			float maxDamage = finalDamage * 1.15f;
    
			string highlightNumbers = HighlightNumbers($"[{Math.Round(minDamage, 2)}-{Math.Round(maxDamage, 2)}] {Localize("Damage")} ({item.DamageType.DisplayName.Value.Trim()})");
			var damageLine = new TooltipLine(Mod, "Damage", $"{ColoredDot(Colors.StatsAccent)} {highlightNumbers}");
			AddNewTooltipLine(item, tooltips, damageLine);
		}
		
		if (item.useTime > 0 && item.damage > 0) 
		{
			//We want to ensure default attack speed is not shown on any of the below gear/items.
			if (data.ItemType != ItemType.Helmet && 
			    data.ItemType != ItemType.Chestplate && 
			    data.ItemType != ItemType.Leggings && 
			    data.ItemType != ItemType.Ring && 
			    data.ItemType != ItemType.Amulet &&
			    data.ItemType != ItemType.Accessories &&
			    data.ItemType != ItemType.Map &&
			    data.ItemType != ItemType.Shield &&
			    item.wingSlot <= 0)
			{
				Player player = Main.LocalPlayer;
				// Get the player's attack speed multiplier
				float useTimeMultiplier = player.GetTotalAttackSpeed(item.DamageType);
		
				// Calculate effective use time with player bonuses
				float effectiveUseTime = item.useTime / useTimeMultiplier;
		
				// Calculate attacks per second with bonuses
				float aps = 60f / effectiveUseTime;

				aps = (float) Math.Round(aps, 2);
				string apsStr = aps.ToString("0.00");
				string localizeString = item.DamageType == DamageClass.Magic ? "CastSpeed" : "AttackSpeed";
				var attackSpeed = new TooltipLine(Mod, "AttacksPerSecond",
					$"{ColoredDot(Colors.StatsAccent)} {HighlightNumbers($"[{apsStr}]")} {Localize(localizeString)}"
				);
				AddNewTooltipLine(item, tooltips, attackSpeed);
			}
		}
		
		// Add weapon critical strike chance
		if (item.damage > 0 && !item.accessory)
		{
			Player player = Main.LocalPlayer;
        
			// Calculate the actual crit chance for this weapon
			//(I dont think we are changing base crit of weapons, but this fool proofs the future if we plan to.)
			float baseCritChance = item.crit;
			float playerCritChance = player.GetTotalCritChance(item.DamageType);
			float totalCritChance = baseCritChance + playerCritChance;
        
			// Add the tooltip line
			var critLine = new TooltipLine(Mod, "CriticalStrikeChance",
				$"{ColoredDot(Colors.StatsAccent)} {HighlightNumbers($"[{totalCritChance:0}%]")} Critical Strike Chance"
			);

			AddNewTooltipLine(item, tooltips, critLine);;
		}

		if (item.mana > 0)
		{
			string manaCost = $"{ColoredDot(Colors.StatsAccent)} {HighlightNumbers($"[{item.mana}]")} [c/{Colors.ManaCost.ToHexRGB()}:{Localize("ManaCost")}]";
			var manaLine = new TooltipLine(Mod, "ManaCost", manaCost);
			AddNewTooltipLine(item, tooltips, manaLine);
		}

		if (item.defense > 0)
		{
			var def = new TooltipLine(Mod, "Defense", $"{ColoredDot(Colors.StatsAccent)} {HighlightNumbers($"+{item.defense} {Localize("Defense")}")}");
			AddNewTooltipLine(item, tooltips, def);
		}

		if (item.pick > 0)
		{
			var pick = new TooltipLine(Mod, "Pickaxe", $"{ColoredDot(Colors.StatsAccent)} {HighlightNumbers($"{item.pick} {Localize("Pickaxe")}")}");
			AddNewTooltipLine(item, tooltips, pick);
		}
		
		if (item.axe > 0)
		{
			var axe = new TooltipLine(Mod, "Axe", $"{ColoredDot(Colors.StatsAccent)} {HighlightNumbers($"{item.axe * 5} {Localize("Axe")}")}");
			AddNewTooltipLine(item, tooltips, axe);
		}
		
		if (item.hammer > 0)
		{
			var hammer = new TooltipLine(Mod, "Hammer", $"{ColoredDot(Colors.StatsAccent)} {HighlightNumbers($"{item.hammer} {Localize("Hammer")}")}");
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

		if (StopBuildingPlayer.InvalidItemsToUse.Contains(item.type))
		{
			AddNewTooltipLine(item, tooltips, new TooltipLine(Mod, "BuildBlocked", Localize("BuildBlocked")) { OverrideColor = new Color(220, 110, 110) });
		}

		if (StopBuildingProjectiles.ProjectileAliasingsByItemId.TryGetValue(item.type, out StopBuildingProjectiles.ProjectileAlias alias))
		{
			string text = Language.GetTextValue("Mods.PathOfTerraria.Gear.Tooltips.ShootBlocked", Lang.GetItemName(alias.AltItemId), alias.AltItemId);
			AddNewTooltipLine(item, tooltips, new TooltipLine(Mod, "ShootBlocked", text) { OverrideColor = new Color(220, 110, 110) });
		}

		// Affix tooltips
		var tooltipsHandler = AffixTooltips.CollectAffixTooltips(item, Main.LocalPlayer);
		InsertAdditionalTooltipLines.Invoke(item, tooltips);
		tooltipsHandler.ModifyTooltips(tooltips, item, Main.LocalPlayer);

		// These don't need AddNewTooltipLine as they're vanilla tooltips
		tooltips.AddRange(oldTooltips);

		if (materialLine is not null)
		{
			tooltips.Add(materialLine);
		}

		if (placeableLine is not null)
		{
			tooltips.Add(placeableLine);
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

	public static string HighlightNumbers(string input, Color? numColor = null, Color? baseColor = null)
	{
		numColor ??= Colors.DefaultNumber;
		baseColor ??= Colors.DefaultText;

		Regex regex = NumberHighlightRegex();

		return regex.Replace(input, match =>
		{
			if (match.Groups[1].Success)
			{
				return $"[c/{ColorUtils.ToHexRGB(numColor.Value)}:{match.Value}]";
			}

			if (match.Groups[2].Success)
			{
				return $"[c/{ColorUtils.ToHexRGB(baseColor.Value)}:{match.Value}]";
			}

			return match.Value;
		});
	}

	public static Color GetRarityColor(ItemRarity rarity)
	{
		return rarity switch
		{
			ItemRarity.Normal => Color.White,
			ItemRarity.Magic => ColorUtils.FromHexRgb(0x_639bff),
			ItemRarity.Rare => ColorUtils.FromHexRgb(0x_ffe46e),
			ItemRarity.Unique => ColorUtils.FromHexRgb(0x_5dff46),
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

	[GeneratedRegex(@"([\d\%\-\+]+)|([^\d\%\-\+]+)")]
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
