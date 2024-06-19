using PathOfTerraria.Content.Items.Gear;
using PathOfTerraria.Core.Systems.Affixes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.IO;
using Terraria.Graphics.Effects;
using Terraria.UI;
using System.Reflection;
using PathOfTerraria.Core.Systems;
using System.Text.RegularExpressions;
using Terraria.ID;
using PathOfTerraria.Content.Socketables;
using Terraria.GameContent.Items;
using PathOfTerraria.Core.Systems.ModPlayers;
using System.Security.Cryptography;
using PathOfTerraria.Core.Systems.TreeSystem;

namespace PathOfTerraria.Core;
internal abstract class PoTItem : ModItem
{
	/// <summary>
	/// Spawns a random piece of armor at the given position.
	/// </summary>
	private static readonly MethodInfo SpawnItemMethod = typeof(PoTItem).GetMethod("SpawnItem", BindingFlags.Public | BindingFlags.Static);

	private static readonly List<Tuple<float, Rarity, Type>> AllItems = [];
	// <Drop chance, item rarity, type of item>

	private static bool AddedDetour = false;

	public ItemType ItemType;
	public Rarity Rarity;
	public Influence Influence;

	public abstract float DropChance { get; }
	public virtual bool IsUnique => false;

	private string _name;
	protected int InternalItemLevel;
	public virtual int ItemLevel
	{
		get => InternalItemLevel;
		set => InternalItemLevel = value;
	}

	public virtual int MinDropItemLevel => 0;

	protected List<ItemAffix> Affixes = [];
	private int _implicits = 0;

	/// <summary>
	/// Called in <see cref="SetDefaults"/> before <see cref="Roll(int, float)"/> is called.<br/>
	/// This ensures Gear will always be set up properly. Otherwise, this works exactly like <see cref="ModItem.SetDefaults"/>.
	/// </summary>
	public virtual void Defaults() { }

	public virtual void InsertAdditionalTooltipLines(List<TooltipLine> tooltips, EntityModifier thisItemModifier) { }
	public virtual void SwapItemModifiers(EntityModifier SawpItemModifier) { }

	public override void ModifyTooltips(List<TooltipLine> tooltips)
	{
		tooltips.Clear();
		var nameLine = new TooltipLine(Mod, "Name", _name)
		{
			OverrideColor = GetRarityColor(Rarity)
		};
		tooltips.Add(nameLine);

		var rareLine = new TooltipLine(Mod, "Rarity", GetDescriptor(ItemType, Rarity, Influence))
		{
			OverrideColor = Color.Lerp(GetRarityColor(Rarity), Color.White, 0.5f)
		};
		tooltips.Add(rareLine);

		TooltipLine powerLine;
		if (ItemType == ItemType.Map)
		{
			powerLine = new TooltipLine(Mod, "Power", $" Tier: [c/CCCCFF:{ItemLevel}]")
			{
				OverrideColor = new Color(170, 170, 170)
			};
		}
		else
		{
			powerLine = new TooltipLine(Mod, "Power", $" Item level: [c/CCCCFF:{ItemLevel}]")
			{
				OverrideColor = new Color(170, 170, 170)
			};
		}

		tooltips.Add(powerLine);

		if (Item.damage > 0)
		{
			var damageLine = new TooltipLine(Mod, "Damage",
				$"[i:{ItemID.SilverBullet}] " + HighlightNumbers(
					$"[{Math.Round(Item.damage * 0.8f, 2)}-{Math.Round(Item.damage * 1.2f, 2)}] Damage ({Item.DamageType.DisplayName})",
					baseColor: "DDDDDD"));
			tooltips.Add(damageLine);
		}

		if (Item.defense > 0)
		{
			var defenseLine = new TooltipLine(Mod, "Defense",
				$"[i:{ItemID.SilverBullet}] " + HighlightNumbers($"+{Item.defense} Defense", baseColor: "DDDDDD"));
			tooltips.Add(defenseLine);
		}

		// custom affixes
		int affixIdx = 0;
		foreach (ItemAffix affix in Affixes)
		{
			string text = affix.RequiredInfluence switch
			{
				Influence.Solar => $"[i:{ItemID.IchorBullet}] " + HighlightNumbers($"{affix.GetTooltip(this)}", "FFEE99", "CCB077"),
				Influence.Lunar => $"[i:{ItemID.CrystalBullet}] " + HighlightNumbers($"{affix.GetTooltip(this)}", "BBDDFF", "99AADD"),
				_ => affixIdx < _implicits ?
					$"[i:{ItemID.SilverBullet}] " + HighlightNumbers($"{affix.GetTooltip(this)}", baseColor: "8B8000") :
					$"[i:{ItemID.MusketBall}] " + HighlightNumbers($"{affix.GetTooltip(this)}"),
			};

			var affixLine = new TooltipLine(Mod, $"Affix{affixIdx}", text);
			tooltips.Add(affixLine);

			affixIdx++;
		}

		// change in stats if equipped
		EntityModifier thisItemModifier = new EntityModifier();
		ApplyAffixes(thisItemModifier);

		InsertAdditionalTooltipLines(tooltips, thisItemModifier);

		EntityModifier currentItemModifier = new EntityModifier();
		SwapItemModifiers(currentItemModifier);

		List<string> red = new();
		List<string> green = new();
		currentItemModifier.GetDifference(thisItemModifier).ForEach(s => {
			if (s[0] == '-' || s.Contains("decrease"))
			{
				red.Add(s);
			}
			else
			{
				green.Add(s);
			}
		});
		if (red.Count + green.Count > 0)
		{
			tooltips.Add(new TooltipLine(Mod, "Space", " "));
		}

		foreach (string changes in green)
		{
			tooltips.Add(new TooltipLine(Mod, $"Change{affixIdx}", $"[c/00FF00:{changes}]"));
		}

		foreach (string changes in red)
		{
			tooltips.Add(new TooltipLine(Mod, $"Change{affixIdx}", $"[c/FF0000:{changes}]"));
		}
	}

	public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset)
	{
		if (line.Mod == Mod.Name && line.Name == "Name")
		{
			yOffset = -2;
			line.BaseScale = Vector2.One * 1.1f;
			return true;
		}

		if (line.Mod == Mod.Name && line.Name == "Rarity")
		{
			yOffset = -8;
			line.BaseScale = Vector2.One * 0.8f;
			return true;
		}

		if (line.Mod == Mod.Name && line.Name == "Power")
		{
			yOffset = 2;
			line.BaseScale = Vector2.One * 0.8f;
			return true;
		}

		if (line.Mod == Mod.Name &&
			(line.Name.Contains("Affix") || line.Name.Contains("Socket") || line.Name == "Damage" || line.Name == "Defense"))
		{
			line.BaseScale = Vector2.One * 0.95f;

			if (line.Name == "Damage" || line.Name == "Defense")
			{
				yOffset = 2;
			}
			else
			{
				yOffset = -4;
			}

			return true;
		}

		if (line.Mod == Mod.Name && line.Name == "Space")
		{
			line.BaseScale *= 0f;
			yOffset = -20;

			return true;
		}

		if (line.Mod == Mod.Name && line.Name.Contains("Change"))
		{
			line.BaseScale = Vector2.One * 0.75f;
			yOffset = -10;

			return true;
		}

		return true;
	}

	/// <summary>
	/// Adds chat tags to darken non-numerical text
	/// </summary>
	/// <param name="input"></param>
	/// <returns></returns>
	public static string HighlightNumbers(string input, string numColor = "CCCCFF", string baseColor = "A0A0A0")
	{
		// Define the regular expression pattern
		string pattern = @"(\d+)|(\D+)";

		// Create a regular expression object
		var regex = new Regex(pattern);

		// Perform the transformation
		string transformedString = regex.Replace(input, match =>
		{
			if (match.Groups[1].Success) // Numeric group
			{
				return $"[c/{numColor}:{match.Value}]";
			}

			if (match.Groups[2].Success) // Non-numeric group
			{
				return $"[c/{baseColor}:{match.Value}]";
			}

			return match.Value;
		});

		return transformedString;
	}

	/// <summary>
	/// Returns the intended styling color for a given rarity
	/// </summary>
	/// <param name="rare">The rariy to get the color of</param>
	/// <returns></returns>
	public static Color GetRarityColor(Rarity rare)
	{
		return rare switch
		{
			Rarity.Normal => Color.White,
			Rarity.Magic => new Color(110, 160, 255),
			Rarity.Rare => new Color(255, 255, 50),
			Rarity.Unique => new Color(25, 255, 25),
			_ => Color.White,
		};
	}

	/// <summary>
	/// Returns a string describing an item based on it's type, rarity, and influence
	/// </summary>
	/// <param name="type"></param>
	/// <param name="rare"></param>
	/// <param name="influence"></param>
	/// <returns></returns>
	public static string GetDescriptor(ItemType type, Rarity rare, Influence influence)
	{
		string typeName = type switch
		{
			ItemType.Sword => "Sword",
			ItemType.Spear => "Spear",
			ItemType.Bow => "Bow",
			ItemType.Gun => "Guns",
			ItemType.Staff => "Staff",
			ItemType.Tome => "Tome",
			ItemType.Helmet => "Helmet",
			ItemType.Chestplate => "Chestplate",
			ItemType.Leggings => "Leggings",
			ItemType.Ring => "Ring",
			ItemType.Charm => "Charm",
			_ => ""
		};

		string rareName = rare switch
		{
			Rarity.Normal => "",
			Rarity.Magic => "Magic ",
			Rarity.Rare => "Rare ",
			Rarity.Unique => "Unique ",
			_ => ""
		};

		string influenceName = influence switch
		{
			Influence.None => "",
			Influence.Solar => "Solar ",
			Influence.Lunar => "Lunar ",
			_ => ""
		};

		return $" {influenceName}{rareName}{typeName}";
	}

	public sealed override void SetDefaults()
	{
		Defaults();
		Roll(PickItemLevel());
	}

	public virtual void ExtraRolls() { }

	/// <summary>
	/// Rolls the randomized aspects of this piece of gear, for a given item level
	/// </summary>
	public void Roll(int itemLevel)
	{
		ItemLevel = itemLevel;

		// Only item level 50+ gear can get influence
		if (InternalItemLevel > 50 && !IsUnique && (ItemType & ItemType.AllGear) == ItemType.AllGear)
		{
			int inf = Main.rand.Next(400) - InternalItemLevel;
			// quality dose not affect influence right now
			// (might not need to, seems to generate plenty often for late game)

			if (inf < 30)
			{
				Influence = Main.rand.NextBool() ? Influence.Solar : Influence.Lunar;
			}
		}

		RollAffixes();

		PostRoll();
		_name = GenerateName();
	}

	/// <summary>
	/// Readies all types of gear to be dropped on enemy kill.
	/// </summary>
	/// <param name="pos">Where to spawn the armor</param>
	public static void GenerateItemList()
	{
		AllItems.Clear();
		foreach (Type type in PathOfTerraria.Instance.Code.GetTypes())
		{
			if (type.IsAbstract || !type.IsSubclassOf(typeof(PoTItem)))
			{
				continue;
			}

			PoTItem instance = (PoTItem)Activator.CreateInstance(type);

			if (instance.IsUnique)
			{
				AllItems.Add(new(instance.DropChance, Rarity.Unique, type));
			}
			else
			{
				if (!type.IsSubclassOf(typeof(Jewel)))
				{
					AllItems.Add(new(instance.DropChance * 0.70f, Rarity.Normal, type));
				}
				AllItems.Add(new(instance.DropChance * 0.25f, Rarity.Magic, type));
				AllItems.Add(new(instance.DropChance * 0.05f, Rarity.Rare, type));
			}
		}
	}

	private const float _magicFindPowerDecrease = 100f;
	private static float ApplyRarityModifier(float chance, float dropRarityModifier)
	{
		// this is just some arbitrary function from chat gpt, modified a little...
		// it is pretty hard to get all this down when we dont know all the items we will have n such;

		chance *= 100f; // to make it effective on <0.1; it works... ok?
		float powerDecrease = chance * (1 + dropRarityModifier / _magicFindPowerDecrease) / (1 + chance * dropRarityModifier / _magicFindPowerDecrease);
		return powerDecrease;
	}

	public static void SpawnRandomItem(Vector2 pos, int ilevel = 0, float dropRarityModifier = 0)
	{
		ilevel = ilevel == 0 ? PickItemLevel() : ilevel;  // Pick the item level if not provided
		dropRarityModifier += ilevel / 10f; // the effect of item level on "magic find"

		// Filter AllGear based on item level
		var filteredGear = AllItems.Where(g => 
		{
			var gearInstance = Activator.CreateInstance(g.Item3) as PoTItem;
			return gearInstance != null && gearInstance.MinDropItemLevel <= ilevel;
		}).ToList();

		// Calculate dropChanceSum based on filtered gear
		float dropChanceSum = filteredGear.Sum((Tuple<float, Rarity, Type> x) => ApplyRarityModifier(x.Item1, dropRarityModifier));
		float choice = Main.rand.NextFloat(dropChanceSum);

		float cumulativeChance = 0;
		foreach (Tuple<float, Rarity, Type> item in filteredGear)
		{
			cumulativeChance += ApplyRarityModifier(item.Item1, dropRarityModifier);
			if (choice < cumulativeChance)
			{
				SpawnItemMethod.MakeGenericMethod(item.Item3).Invoke(null, [pos, ilevel, item.Item2]);
				return;
			}
		}
	}

	/// <summary>
	/// Spawns a random piece of gear of the given base type at the given position
	/// </summary>
	/// <typeparam name="T">The type of gear to drop</typeparam>
	/// <param name="pos">Where to drop it in the world</param>
	/// <param name="ilevel">The item level of the item to spawn</param>
	/// <param name="dropRarityModifier">Rolls an item with a drop rarity modifier</param>
	public static void SpawnItem<T>(Vector2 pos, int ilevel = 0, Rarity rarity = Rarity.Normal) where T : PoTItem
	{
		var item = new Item();
		item.SetDefaults(ModContent.ItemType<T>());
		T gear = item.ModItem as T;
		gear.Rarity = rarity;
		gear.Roll(ilevel == 0 ? PickItemLevel() : ilevel);
		Item.NewItem(null, pos, Vector2.Zero, item);
	}
	
	/// <summary>
	/// Selects an appropriate item level for a piece of gear to drop at based on world state
	/// </summary>
	internal static int PickItemLevel()
	{
		if (NPC.downedMoonlord)
		{
			return Main.rand.Next(150, 201);
		}

		if (NPC.downedAncientCultist)
		{
			return Main.rand.Next(110, 151);
		}

		if (NPC.downedGolemBoss)
		{
			return Main.rand.Next(95, 131);
		}

		if (NPC.downedPlantBoss)
		{
			return Main.rand.Next(80, 121);
		}

		if (NPC.downedMechBossAny)
		{
			return Main.rand.Next(75, 111);
		}

		if (Main.hardMode)
		{
			return Main.rand.Next(50, 91);
		}

		if (NPC.downedBoss3)
		{
			return Main.rand.Next(30, 50);
		}

		if (NPC.downedBoss2)
		{
			return Main.rand.Next(20, 41);
		}

		if (NPC.downedBoss1)
		{
			return Main.rand.Next(10, 26);
		}

		return Main.rand.Next(5, 21);
	}

	public override void Load()
	{
		if (!AddedDetour)
		{
			On_ItemSlot.Draw_SpriteBatch_ItemArray_int_int_Vector2_Color += DrawSpecial;
			AddedDetour = true;
		}
	}

	public override void Unload()
	{
		AddedDetour = false;
	}

	private void DrawSpecial(On_ItemSlot.orig_Draw_SpriteBatch_ItemArray_int_int_Vector2_Color orig, SpriteBatch sb,
		Item[] inv, int context, int slot, Vector2 position, Color color)
	{
		if (inv[slot].ModItem is Gear gear && context != 21)
		{
			string rareName = gear.Rarity switch
			{
				Rarity.Normal => "Normal",
				Rarity.Magic => "Magic",
				Rarity.Rare => "Rare",
				Rarity.Unique => "Unique",
				_ => "Normal"
			};

			Texture2D back = ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/Slots/{rareName}Back")
				.Value;
			Color backcolor = Color.White * 0.75f;

			sb.Draw(back, position, null, backcolor, 0f, default, Main.inventoryScale, SpriteEffects.None, 0f);
			ItemSlot.Draw(sb, ref inv[slot], 21, position);

			if (gear.Influence == Influence.Solar)
			{
				DrawSolarSlot(sb, position);
			}

			if (gear.Influence == Influence.Lunar)
			{
				DrawLunarSlot(sb, position);
			}
		}
		else
		{
			orig(sb, inv, context, slot, position, color);
		}
	}

	/// <summary>
	/// Draws the shader overlay for solar items
	/// </summary>
	/// <param name="spriteBatch"></param>
	/// <param name="pos"></param>
	private void DrawSolarSlot(SpriteBatch spriteBatch, Vector2 pos)
	{
		Texture2D tex = ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/Slots/SlotMap").Value;

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
			.SetValue(ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/Misc/SwirlNoise").Value);
		effect.Parameters["mapTexture"]
			.SetValue(ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/Misc/SwirlNoise").Value);

		spriteBatch.End();
		spriteBatch.Begin(default, BlendState.Additive, default, default, RasterizerState.CullNone, effect,
			Main.GameViewMatrix.TransformationMatrix);

		spriteBatch.Draw(tex, pos, null, Color.White, 0, Vector2.Zero, Main.inventoryScale, 0, 0);

		spriteBatch.End();
		spriteBatch.Begin(default, default, default, default, RasterizerState.CullNone, default,
			Main.GameViewMatrix.TransformationMatrix);
	}

	/// <summary>
	/// Draws the shader overlay for lunar items
	/// </summary>
	/// <param name="spriteBatch"></param>
	/// <param name="pos"></param>
	private void DrawLunarSlot(SpriteBatch spriteBatch, Vector2 pos)
	{
		Texture2D tex = ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/Slots/SlotMap").Value;

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
			.SetValue(ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/Misc/ShaderNoise").Value);
		effect.Parameters["mapTexture"]
			.SetValue(ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/Misc/ShaderNoise").Value);

		spriteBatch.End();
		spriteBatch.Begin(default, BlendState.Additive, default, default, RasterizerState.CullNone, effect,
			Main.GameViewMatrix.TransformationMatrix);

		spriteBatch.Draw(tex, pos, null, Color.White, 0, Vector2.Zero, Main.inventoryScale, 0, 0);

		spriteBatch.End();
		spriteBatch.Begin(default, default, default, default, RasterizerState.CullNone, default,
			Main.GameViewMatrix.TransformationMatrix);
	}

	/// <summary>
	/// This occurs after power has been set and rarity/influence has been rolled, Initialize stats here
	/// </summary>
	public virtual void PostRoll()
	{
	}

	/// <summary>
	/// Applies after affixes have been placed, this is mainly for unique items.
	/// </summary>
	public virtual List<ItemAffix> GenerateAffixes() { return new(); }

	/// <summary>
	/// Before affix roll, allows you to add the implicit affixes that should exist on this type of gear.
	/// </summary>
	public virtual List<ItemAffix> GenerateImplicits() { return new(); }

	/// <summary>
	/// Allows you to customize what prefixes this items can have, only visual
	/// </summary>
	public virtual string GenerateSuffix() { return ""; }

	/// <summary>
	/// Allows you to customize what suffixes this items can have, only visual
	/// </summary>
	public virtual string GeneratePrefix() { return ""; }

	/// <summary>
	/// Allows you to customize what this item's name can be
	/// </summary>
	public virtual string GenerateName()
	{
		string prefix = GeneratePrefix();
		string suffix = GenerateSuffix();

		return Rarity switch
		{
			Rarity.Normal => Item.Name,
			Rarity.Magic => $"{prefix} {Item.Name}",
			Rarity.Rare => $"{prefix} {Item.Name} {suffix}",
			Rarity.Unique => Item.Name, // uniques might just want to override the GenerateName function
			_ => "Unknown Item"
		};
	}

	public virtual void ExtraUpdateEquips(Player player) { }

	public override void UpdateEquip(Player player)
	{
		ExtraUpdateEquips(player);

		ApplyAffixes(player.GetModPlayer<UniversalBuffingPlayer>().UniversalModifier);
	}

	public void ApplyAffixes(EntityModifier entityModifier)
	{
		Affixes.ForEach(n => n.ApplyAffix(entityModifier, this));
	}

	public override void SaveData(TagCompound tag)
	{
		tag["type"] = (int)ItemType;
		tag["rarity"] = (int)Rarity;
		tag["influence"] = (int)Influence;

		tag["implicits"] = _implicits;

		tag["name"] = _name;
		tag["power"] = InternalItemLevel;

		List<TagCompound> affixTags = [];
		foreach (ItemAffix affix in Affixes)
		{
			var newTag = new TagCompound();
			affix.Save(newTag);
			affixTags.Add(newTag);
		}

		tag["affixes"] = affixTags;
	}

	public override void LoadData(TagCompound tag)
	{
		ItemType = (ItemType)tag.GetInt("type");
		Rarity = (Rarity)tag.GetInt("rarity");
		Influence = (Influence)tag.GetInt("influence");

		_implicits = tag.GetInt("implicits");

		_name = tag.GetString("name");
		InternalItemLevel = tag.GetInt("power");

		IList<TagCompound> affixTags = tag.GetList<TagCompound>("affixes");

		foreach (TagCompound newTag in affixTags)
		{
			ItemAffix g = Affix.FromTag<ItemAffix>(newTag);
			if (g is not null)
			{
				Affixes.Add(g);
			}
		}

		PostRoll();
	}

	public virtual int GetAffixCount(Rarity rarity)
	{
		return Rarity switch
		{
			Rarity.Magic => 2,
			Rarity.Rare => Main.rand.Next(3, 5),
			_ => 0
		};
	}

	/// <summary>
	/// Selects appropriate random affixes for this item, and applies them
	/// </summary>
	public void RollAffixes()
	{
		Affixes = GenerateImplicits();

		_implicits = Affixes.Count();

		if (Rarity == Rarity.Normal || Rarity == Rarity.Unique)
		{
			return;
		}

		List<ItemAffix> possible = AffixHandler.GetAffixes(this);

		if (possible is null)
		{
			return;
		}

		int AffixCount = GetAffixCount(Rarity);

		Affixes.AddRange(Affix.GenerateAffixes(possible, AffixCount));

		List<ItemAffix> extraAffixes = GenerateAffixes();
		extraAffixes.ForEach(a => a.Roll());

		Affixes.AddRange(extraAffixes);
	}
}
