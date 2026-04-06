using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Items;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.DisableBuilding;
using PathOfTerraria.Content.Items.Consumables.Maps;
using PathOfTerraria.Content.Items.Consumables.Maps.ExplorableMaps;
using PathOfTerraria.Content.Items.Consumables.Maps.BossMaps;
using PathOfTerraria.Content.Items.Gear;
using PathOfTerraria.Utilities.Xna;
using ReLogic.Content;
using Terraria.Graphics.Effects;
using Terraria.Localization;
using Terraria.UI;
using SubworldLibrary;
using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.Systems.ElementalDamage;
using Terraria.ID;
using Terraria.GameContent.RGB;
using PathOfTerraria.Common.UI;
using Terraria.GameContent;
using System.Net.Http.Headers;

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
		public static Color FireDamage => ColorUtils.FromHexRgb(0xFF6A6A); 
		public static Color ColdDamage => ColorUtils.FromHexRgb(0x5FCDE4);
		public static Color LightningDamage => ColorUtils.FromHexRgb(0xFFF68F); 
		public static Color ChaosDamage => ColorUtils.FromHexRgb(0xC084FC);
		// Accents
		public static Color StatsAccent => ColorUtils.FromHexRgb(0x_a1bdbd);
		public static Color AffixAccent => ColorUtils.FromHexRgb(0x_ff2222);

		public static Color Implicit => ColorUtils.FromHexRgb(0x_72abcb);
		public static Color Corrupt => Color.Lerp(Color.Purple, Color.White, 0.4f);
		public static Color Cloned => ColorUtils.FromHexRgb(0x_37946e);
		public static Color ManaCost => ColorUtils.FromHexRgb(0x_5fcde4);
	}

	private static int _seperatorCount = 0;

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

		On_ItemSlot.MouseHover_ItemArray_int_int += FixSetBonusTooltipNotAppearing;
	}

	private void FixSetBonusTooltipNotAppearing(On_ItemSlot.orig_MouseHover_ItemArray_int_int orig, Item[] inv, int context, int slot)
	{
		orig(inv, context, slot);

		// The original code checks for the context and if slot <= 2, since all vanilla armor slots are indexes <= 2
		if (context == ItemSlot.Context.EquipArmor)
		{
			inv[slot].wornArmor = true;
		}
	}

	#region Modify tooltips and rendering

	public override bool PreDrawTooltipLine(Item item, DrawableTooltipLine line, ref int yOffset)
	{
		// Handle vanilla lines we want before the mod check
		switch (line.Name)
		{
			case "FishingPower":
			case "Consumable":
			case "Material": 
			case "Placeable":
			case "Price":
				yOffset = -2;
				line.BaseScale = new Vector2(0.9f);
				return true;
		}

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

		if (line.Name.StartsWith("Separator"))
		{
			yOffset = 12;

			if (Tooltip.RenderingTooltip is not { } cache || Tooltip.SuppressDrawing)
			{
				return Tooltip.SuppressDrawing;
			}

			Vector2 pos = new(line.X, line.Y - 2);
			float width = cache.OuterSize.X - 28;
			Texture2D tex = Textures["Separator"].Value;
			float middleWidth = width - 20;
			var middleScale = new Vector2(middleWidth / 100f, 1);
			Color col = Color.Gray;

			Main.spriteBatch.Draw(tex, pos, new Rectangle(0, 0, 10, 6), col);
			Main.spriteBatch.Draw(tex, pos + new Vector2(10, 0), new Rectangle(10, 0, 100, 6), col, 0f, Vector2.Zero, middleScale, SpriteEffects.None, 0);
			Main.spriteBatch.Draw(tex, pos + new Vector2(middleWidth + 6, 0), new Rectangle(110, 0, 10, 6), col);

			// Return true - draws nothing (aka ""), but this is necessary to add the yOffset properly
			return true;
		}

		switch (line.Name)
		{
			case "Name":
				yOffset = -2;
				line.BaseScale = new Vector2(1.1f);
				return true;

			case "Rarity" or "Corrupted" or "Cloned":
				yOffset = -2;
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
			|| line.Name is "Defense" or "AttacksPerSecond" or "CriticalStrikeChance" or "ManaCost" || line.Name.StartsWith("Damage"))
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

		_seperatorCount = 0;

		var oldTooltips = tooltips.Where(x => x.Name.StartsWith("Tooltip")).ToList();
		var oldStats = tooltips.Where(x => x.Name.StartsWith("Stat")).ToList();

		TooltipLine favoriteLine = tooltips.FirstOrDefault(x => x.Name == "Favorite");
		TooltipLine needsBaitLine = tooltips.FirstOrDefault(x => x.Name == "NeedsBait");
		TooltipLine baitPowerLine = tooltips.FirstOrDefault(x => x.Name == "BaitPower");
		TooltipLine equipableLine = tooltips.FirstOrDefault(x => x.Name == "Equipable");
		TooltipLine vanityLine = tooltips.FirstOrDefault(x => x.Name == "Vanity");
		TooltipLine wellFedExpertLine = tooltips.FirstOrDefault(x => x.Name == "WellFedExpert");
		TooltipLine buffTimeLine = tooltips.FirstOrDefault(x => x.Name == "BuffTime");
		TooltipLine expertLine = tooltips.FirstOrDefault(x => x.Name == "Expert");
		TooltipLine masterLine = tooltips.FirstOrDefault(x => x.Name == "Master");
		TooltipLine fishingPowerLine = tooltips.FirstOrDefault(x => x.Name == "FishingPower");
		TooltipLine consumableLine = tooltips.FirstOrDefault(x => x.Name == "Consumable");
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

		//if (data.Rarity > ItemRarity.Normal && data.NameAffix.Empty) // Make sure SpecialName generated if it hasn't already for some reason
		//{
		//	data.NameAffix = GenerateNameAffixes.Invoke(item);
		//}

		if (nameLine is null)
		{
			nameLine = new TooltipLine(Mod, "Name", GenerateName.Invoke(item))
			{
				OverrideColor = GetRarityColor(data.Rarity)
			};
		}
		else
		{
			nameLine.Text = GenerateName.Invoke(item);
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

			// Override the type descriptor for maps to distinguish Explorable vs Boss maps
			if (item.ModItem is Map)
			{
				string mapTypeName = null;
				if (item.ModItem is ExplorableMap)
				{
					mapTypeName = Language.GetTextValue("Mods.PathOfTerraria.Gear.ExplorableMap.Name");
				}
				else if (item.ModItem is BossMap)
				{
					mapTypeName = Language.GetTextValue("Mods.PathOfTerraria.Gear.BossMap.Name");
				}

				if (!string.IsNullOrWhiteSpace(mapTypeName))
				{
					rarityDesc = GetDescriptor(mapTypeName, data.Rarity, data.Influence);
				}
			}

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
			AddSeparator(item, tooltips);
		}

		if (!string.IsNullOrWhiteSpace(staticData.AltUseDescription.Value))
		{
			AddNewTooltipLine(item, tooltips, new TooltipLine(Mod, "AltUseDescription", staticData.AltUseDescription.Value));
		}

		if (!string.IsNullOrWhiteSpace(staticData.Description.Value))
		{
			AddNewTooltipLine(item, tooltips, new(Mod, "Description", staticData.Description.Value));
			AddSeparator(item, tooltips);
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
    
			// 15% variability, default range
			float minDamage = finalDamage * 0.85f;
			float maxDamage = finalDamage * 1.15f;

			// Base values for the base tooltip
			float baseMinDamage = minDamage;
			float baseMaxDamage = maxDamage;
			
			// Calculate total flat damage to add to main tooltip
			float totalFlatDamage = player.GetModPlayer<ElementalPlayer>().Container.Sum(x => x.GetFlatDamage(0));
			
			// We are doing calcs and storing it here, so tooltip placement order isnt messed up, and the final damage tooltip is accurate
			List<TooltipLine> elementLines = [];

			foreach (ElementInstance instance in player.GetModPlayer<ElementalPlayer>().Container)
			{
				int flat = (int)Math.Round(instance.GetFlatDamage(0));
				string elementName = Language.GetTextValue("Mods.PathOfTerraria.Misc.Damage", instance.ElementDisplayName.Value.ToLower().Trim());
				float baseWeaponConversion = item?.type > ItemID.None ? ElementalWeaponSets.GetElementStrength(item.type, instance.Type) : 0f;

				float elementDamage = finalDamage * (baseWeaponConversion + instance.GetTotalConversion(0)) * instance.Multiplier;

				float eleMinDamage = (elementDamage * 0.85f) + flat;
				float eleMaxDamage = (elementDamage * 1.15f) + flat;
				
				if (eleMinDamage > 0)
				{
					// pick element color based on type
					Color elementColor = instance.Type.ElementColor();

					// numbers tinted with element color
					string highlightNumbers = HighlightNumbers((eleMinDamage == eleMaxDamage) ? $"[{Math.Round(eleMinDamage, 2)}]" : $"[{Math.Round(eleMinDamage, 2)}-{Math.Round(eleMaxDamage, 2)}]", elementColor);
					var newDamageLine = new TooltipLine(Mod, "Damage" + instance.Type, $"    {ColoredDot(Colors.StatsAccent)} {highlightNumbers} {elementName}");
					elementLines.Add(newDamageLine);
				}
			}

			// Elemental multiplier for the weapon's base damage
			float totalMultiplier = 1 + player.GetModPlayer<ElementalPlayer>().Container.Sum(x => x.GetTotalConversion(0));
			string topHighlightNumbers = HighlightNumbers(
				$"[{Math.Round((minDamage * totalMultiplier) + totalFlatDamage, 2)}-{Math.Round((maxDamage * totalMultiplier) + totalFlatDamage, 2)}] {item.DamageType.DisplayName.Value.Trim()}"
			);

			if (Main.keyState.PressingShift())
			{
				topHighlightNumbers += $" ({HighlightNumbers($"[{baseMinDamage:#0.#}-{baseMaxDamage:#0.#}]")} [c/{Colors.DefaultNumber.ToHexRGB()}:base])";
			}

			var damageLine = new TooltipLine(
				Mod,
				"Damage",
				$"{ColoredDot(Colors.StatsAccent)} {topHighlightNumbers}"
			);
			
			//Add the actual damage line
			AddNewTooltipLine(item, tooltips, damageLine);
			
			// Then add all elemental lines
			foreach (TooltipLine line in elementLines)
			{
				AddNewTooltipLine(item, tooltips, line);
			}
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
			// Base crit of all weapons atm is 4 (+ any added we have)
			float baseCritChance = item.crit;
			float playerCritChance = player.GetTotalCritChance(item.DamageType);
			float totalCritChance = baseCritChance + playerCritChance;
        
			// Add the tooltip line
			var critLine = new TooltipLine(Mod, "CriticalStrikeChance",
				$"{ColoredDot(Colors.StatsAccent)} {HighlightNumbers($"[{totalCritChance:0}%]")} Critical Strike Chance"
			);

			AddNewTooltipLine(item, tooltips, critLine);
		}

		if (item.mana > 0)
		{
			Player player = Main.LocalPlayer;
			// Calculate the real mana cost
			int effectiveManaCost = (int)Math.Round(item.mana * player.manaCost);

			string manaCost = $"{ColoredDot(Colors.StatsAccent)} {HighlightNumbers($"[{effectiveManaCost}]")} [c/{Colors.ManaCost.ToHexRGB()}:{Localize("ManaCost")}]";
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

		if (StopBuildingPlayer.InvalidItemsToUse.Contains(item.type) && SubworldSystem.Current is MappingWorld)
		{
			AddNewTooltipLine(item, tooltips, new TooltipLine(Mod, "BuildBlocked", Localize("BuildBlocked")) { OverrideColor = new Color(220, 110, 110) });
		}

		if (StopBuildingPlayer.NerfedDomainItems.ContainsKey(item.type) && SubworldSystem.Current is MappingWorld)
		{
			AddNewTooltipLine(item, tooltips, new TooltipLine(Mod, "WeaponNerfed", Localize("WeaponNerfed")) { OverrideColor = new Color(255, 160, 160) });
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
		
		if (favoriteLine is not null)
		{
			favoriteLine.OverrideColor = Color.Gold;
			tooltips.Add(favoriteLine);
		}

		if (needsBaitLine is not null)
		{
			needsBaitLine.OverrideColor = Color.LightBlue;
			tooltips.Add(needsBaitLine);
		}

		if (baitPowerLine is not null)
		{
			baitPowerLine.OverrideColor = Color.LightBlue;
			tooltips.Add(baitPowerLine);
		}

		if (equipableLine is not null)
		{
			equipableLine.OverrideColor = Color.LightBlue;
			tooltips.Add(equipableLine);
		}

		if (vanityLine is not null)
		{
			vanityLine.OverrideColor = Color.LightGray;
			tooltips.Add(vanityLine);
		}

		if (wellFedExpertLine is not null)
		{
			wellFedExpertLine.OverrideColor = Color.Orange;
			tooltips.Add(wellFedExpertLine);
		}

		if (buffTimeLine is not null)
		{
			buffTimeLine.OverrideColor = Color.LightGreen;
			tooltips.Add(buffTimeLine);
		}

		if (expertLine is not null)
		{
			expertLine.OverrideColor = new Color(255, 175, 0);
			tooltips.Add(expertLine);
		}

		if (masterLine is not null)
		{
			masterLine.OverrideColor = new Color(255, 0, 255);
			tooltips.Add(masterLine);
		}

		if (fishingPowerLine is not null)
		{
			fishingPowerLine.OverrideColor = Color.LightBlue;
			tooltips.Add(fishingPowerLine);
		}
		
		if (consumableLine is not null)
		{
			consumableLine.OverrideColor = Color.LightGray;
			tooltips.Add(consumableLine);
		}

		if (materialLine is not null)
		{
			materialLine.OverrideColor = Color.LightGray;
			tooltips.Add(materialLine);
		}

		if (placeableLine is not null)
		{
			placeableLine.OverrideColor = Color.LightGray;
			tooltips.Add(placeableLine);
		}

		if (priceLine is not null)
		{
			priceLine.OverrideColor = Color.LightGray;
			tooltips.Add(priceLine);
		}
	}

	/// <summary>
	/// Adds a simple separator into the tooltips. This is done using the texture tag.
	/// </summary>
	internal static void AddSeparator(Item item, List<TooltipLine> lines)
	{
		var tooltip = new TooltipLine(PoTMod.Instance, "Separator" + _seperatorCount, "");

		if (item is not null)
		{
			AddNewTooltipLine(item, lines, tooltip);
		}
		else
		{
			lines.Add(tooltip);
		}

		_seperatorCount++;
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
		Textures.Add("Separator", ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/Separator"));
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
