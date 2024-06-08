using PathOfTerraria.Content.Socketables;
using PathOfTerraria.Core.Systems;
using PathOfTerraria.Core.Systems.Affixes;
using PathOfTerraria.Core.Systems.ModPlayers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace PathOfTerraria.Content.Items.Gear;

internal abstract class Gear : ModItem
{
	private static readonly List<Tuple<float, bool, Type>> AllGear = [];
	// <Drop chance, is unique?, type of item>

	protected GearType GearType;
	protected GearRarity Rarity;
	private GearInfluence _influence;

	public abstract float DropChance { get; }
	public virtual bool IsUnique => false;

	private string _name;
	public virtual int ItemLevel { get; set; }

	private List<GearAffix> _affixes = [];
    private int _implicits = 0;

	private Socketable[] _sockets = []; // [new Imps()];
	private int _selectedSocket = 0;

	public void EquipItem(Player player)
	{
		if (!UseSockets())
		{
			return;
		}

		_sockets.ToList().ForEach(s => s?.OnSocket(player, Item));
	}

	public void UnEquipItem(Player player)
	{
		if (!UseSockets())
		{
			return;
		}

		_sockets.ToList().ForEach(s => s?.OnUnSocket(player, Item));
	}

	public bool IsThisItemActive(Player player)
	{
		return player.inventory[0] == Item || player.armor.Contains(Item);
	}
	
	public static bool IsThisItemActive(Player player, Item item)
	{
		return player.inventory[0] == item || player.armor.Contains(item);
	}

	public bool UseSockets()
	{
		return _sockets.Length != 0;
	}

	public void NextSocket()
	{
		_selectedSocket++;
		if (_selectedSocket >= _sockets.Length)
		{
			_selectedSocket = 0;
		}
	}
	
	public void PrevSocket()
	{
		_selectedSocket--;
		if (_selectedSocket < 0)
		{
			_selectedSocket = _sockets.Length - 1;
		}
	}

	public void Socket(Player player, Socketable item)
	{
		if (_sockets.Length == 0)
		{
			return;
		}

		if (_sockets[_selectedSocket] is not null)
		{
			_sockets[_selectedSocket].OnUnSocket(player, Item);
			Main.mouseItem = _sockets[_selectedSocket].Item;
		}
		else
		{
			Main.mouseItem = new Item();
		}

		_sockets[_selectedSocket] = item;

		if (IsThisItemActive(player))
		{
			item.OnSocket(player, Item);
		}
	}

	public void ShiftClick(Player player)
	{
		if (Main.mouseItem.active && Main.mouseItem.ModItem is Socketable)
		{
			Socket(player, Main.mouseItem.ModItem as Socketable);
		}
		else if (_sockets[_selectedSocket] is not null)
		{
			if (IsThisItemActive(player))
			{
				_sockets[_selectedSocket].OnUnSocket(player, Item);
			}

			Main.mouseItem = _sockets[_selectedSocket].Item;
			_sockets[_selectedSocket] = null;
		}
	}

	public override void Load()
	{
		On_ItemSlot.Draw_SpriteBatch_ItemArray_int_int_Vector2_Color += DrawSpecial;
	}

	private void DrawSpecial(On_ItemSlot.orig_Draw_SpriteBatch_ItemArray_int_int_Vector2_Color orig, SpriteBatch sb,
		Item[] inv, int context, int slot, Vector2 position, Color color)
	{
		if (inv[slot].ModItem is Gear gear && context != 21)
		{
			string rareName = gear.Rarity switch
			{
				GearRarity.Normal => "Normal",
				GearRarity.Magic => "Magic",
				GearRarity.Rare => "Rare",
				GearRarity.Unique => "Unique",
				_ => "Normal"
			};

			Texture2D back = ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/Slots/{rareName}Back")
				.Value;
			Color backcolor = Color.White * 0.75f;

			sb.Draw(back, position, null, backcolor, 0f, default, Main.inventoryScale, SpriteEffects.None, 0f);
			ItemSlot.Draw(sb, ref inv[slot], 21, position);
        
			if (gear._influence == GearInfluence.Solar)
			{
				DrawSolarSlot(sb, position);
			}

			if (gear._influence == GearInfluence.Lunar)
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

	public override void ModifyTooltips(List<TooltipLine> tooltips)
	{
		tooltips.Clear();
		var nameLine = new TooltipLine(Mod, "Name", _name)
		{
			OverrideColor = GetRarityColor(Rarity)
		};
		tooltips.Add(nameLine);

		var rareLine = new TooltipLine(Mod, "Rarity", GetDescriptor(GearType, Rarity, _influence))
		{
			OverrideColor = Color.Lerp(GetRarityColor(Rarity), Color.White, 0.5f)
		};
		tooltips.Add(rareLine);

		var powerLine = new TooltipLine(Mod, "Power", $" Item level: [c/CCCCFF:{ItemLevel}]")
		{
			OverrideColor = new Color(170, 170, 170)
		};
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
		foreach (GearAffix affix in _affixes)
		{
			string text = affix.RequiredInfluence switch
			{
				GearInfluence.Solar => $"[i:{ItemID.IchorBullet}] " + HighlightNumbers($"{affix.GetTooltip(this)}", "FFEE99", "CCB077"),
				GearInfluence.Lunar => $"[i:{ItemID.CrystalBullet}] " + HighlightNumbers($"{affix.GetTooltip(this)}", "BBDDFF", "99AADD"),
				_ => affixIdx < _implicits ?
					$"[i:{ItemID.SilverBullet}] " + HighlightNumbers($"{affix.GetTooltip(this)}", baseColor: "8B8000") :
					$"[i:{ItemID.MusketBall}] " +HighlightNumbers($"{affix.GetTooltip(this)}"),
			};	

			var affixLine = new TooltipLine(Mod, $"Affix{affixIdx}", text);
			tooltips.Add(affixLine);

			affixIdx++;
		}

		if (_sockets.Length > 0)
		{
			tooltips.Add(new TooltipLine(Mod, "Space", " "));
		}
		
		// sockets
		for (int i = 0; i < _sockets.Length; i++)
		{
			string text = "";
			if (_sockets[i] is not null)
			{
				text = _sockets[i].GenerateName();
			}

			var affixLine = new TooltipLine(Mod, $"Socket{i}",
				$"[i:{(i == _selectedSocket ? ItemID.NanoBullet : ItemID.ChlorophyteBullet)}] " + text);
			tooltips.Add(affixLine);
		}

		// change in stats if equipped
		EntityModifier thisItemModifier = new EntityModifier();
		_affixes.ForEach(n => n.ApplyAffix(thisItemModifier, this));

		EntityModifier currentItemModifier = new EntityModifier();
		if (Item.headSlot >= 0 && Main.LocalPlayer.armor[0].active)
		{
			(Main.LocalPlayer.armor[0].ModItem as Gear).ApplyAffixes(currentItemModifier);
		}
		else if (Item.bodySlot >= 0 && Main.LocalPlayer.armor[1].active)
		{
			(Main.LocalPlayer.armor[1].ModItem as Gear).ApplyAffixes(currentItemModifier);
		}
		else if (Item.legSlot >= 0 && Main.LocalPlayer.armor[2].active)
		{
			(Main.LocalPlayer.armor[2].ModItem as Gear).ApplyAffixes(currentItemModifier);
		}
		// missing accessories
		else
		{
			if (Main.LocalPlayer.inventory[0].ModItem is Gear)
			{
				(Main.LocalPlayer.inventory[0].ModItem as Gear).ApplyAffixes(currentItemModifier);
			}
		}

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
	/// Rolls the randomized aspects of this piece of gear, for a given item level
	/// </summary>
	public void Roll(int itemLevel, float dropRarityModifier = 0)
	{
		ItemLevel = itemLevel;

		float rarityModifier = MathF.Pow(dropRarityModifier/10f + 1, -0.5f);
		// ^^ would mean that at about ~30 total quality
		// we would have 50% higher chance of rare magic and rare... ig...
		// numbers subject to change, ofc.

		float rare = (Main.rand.Next(100) - itemLevel / 10f) * rarityModifier;
		Rarity = GearRarity.Normal;

		if (rare < 25 + itemLevel / 10f)
		{
			Rarity = GearRarity.Magic;
		}

		if (rare < 5)
		{
			Rarity = GearRarity.Rare;
		}

		if (IsUnique)
		{
			Rarity = GearRarity.Unique;
		}

		// Only item level 50+ can get influence
		if (itemLevel > 50 && !IsUnique)
		{
			int inf = Main.rand.Next(400) - itemLevel;
			// quality dose not affect influence right now
			// (might not need to, seems to generate plenty often for late game)

			if (inf < 30)
			{
				_influence = Main.rand.NextBool() ? GearInfluence.Solar : GearInfluence.Lunar;
			}
		}

		_selectedSocket = 0;
		int maxSockets = Rarity switch // what to do if we roll less sockets than what we have equipped?
									// maby just not allow to roll if we have any sockets?
		{
			GearRarity.Normal => Main.rand.Next(2),
			GearRarity.Magic => Main.rand.Next(1, 2),
			GearRarity.Rare => Main.rand.Next(1, 3),
			_ => 0,
		};

		_sockets = new Socketable[maxSockets];

		_affixes = GenerateImplicits();

		_implicits = _affixes.Count();

		RollAffixes();

		List<GearAffix> extraAffixes = GenerateAffixes();
		extraAffixes.ForEach(a => a.Roll());

		_affixes.AddRange(extraAffixes);

		PostRoll();
		_name = GenerateName();
	}

	/// <summary>
	/// Selects appropriate random affixes for this item, and applies them
	/// </summary>
	public void RollAffixes()
	{
		if (Rarity == GearRarity.Normal || Rarity == GearRarity.Unique)
		{
			return;
		}

		List<GearAffix> possible = AffixHandler.GetAffixes(GearType, _influence);

		if (possible is null)
		{
			return;
		}

		_affixes.AddRange(Rarity switch
		{
			GearRarity.Magic => Affix.GenerateAffixes(possible, 2),
			GearRarity.Rare => Affix.GenerateAffixes(possible, Main.rand.Next(3, 5)),
			_ => new List<GearAffix>()
		});
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
	public virtual List<GearAffix> GenerateAffixes() { return new(); }

	/// <summary>
	/// Before affix roll, allows you to add the implicit affixes that should exist on this type of gear.
	/// </summary>
	public virtual List<GearAffix> GenerateImplicits() { return new(); }

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
			GearRarity.Normal => Item.Name,
			GearRarity.Magic => $"{prefix} {Item.Name}",
			GearRarity.Rare => $"{prefix} {Item.Name} {suffix}",
			GearRarity.Unique => Item.Name, // uniques might just want to override the GenerateName function
			_ => "Unknown Item"
		};
	}

	public override void UpdateEquip(Player player)
	{
		_sockets.Where(s => s is not null).ToList().ForEach(s => s.UpdateEquip(player, Item));

		ApplyAffixes(player.GetModPlayer<UniversalBuffingPlayer>().UniversalModifier);
	}

	public void ApplyAffixes(EntityModifier entityModifier)
	{
		_affixes.ForEach(n => n.ApplyAffix(entityModifier, this));
	}

	public override void SaveData(TagCompound tag)
	{
		tag["type"] = (int)GearType;
		tag["rarity"] = (int)Rarity;
		tag["influence"] = (int)_influence;

		tag["socketCount"] = _sockets.Length;
		tag["implicits"] = _implicits;

		tag["name"] = _name;
		tag["power"] = ItemLevel;

		List<TagCompound> affixTags = [];
		foreach (GearAffix affix in _affixes)
		{
			var newTag = new TagCompound();
			affix.Save(newTag);
			affixTags.Add(newTag);
		}

		tag["affixes"] = affixTags;

		for (int i = 0; i < _sockets.Length; i++)
		{
			Socketable socket = _sockets[i];
			if (socket is not null)
			{
				var newTag = new TagCompound();
				socket.Save(newTag);
				tag.Add("Socket" + i, newTag);
			}
		}
	}

	public override void LoadData(TagCompound tag)
	{
		GearType = (GearType)tag.GetInt("type");
		Rarity = (GearRarity)tag.GetInt("rarity");
		_influence = (GearInfluence)tag.GetInt("influence");

		_implicits = tag.GetInt("implicits");

		_name = tag.GetString("name");
		ItemLevel = tag.GetInt("power");

		IList<TagCompound> affixTags = tag.GetList<TagCompound>("affixes");

		foreach (TagCompound newTag in affixTags)
		{
			GearAffix g = GearAffix.FromTag(newTag);
			if (g is not null)
			{
				_affixes.Add(g);
			}
		}

		int socketCount = tag.GetInt("socketCount");
		_sockets = new Socketable[socketCount];

		for (int i = 0; i < _sockets.Length; i++)
		{
			if (tag.TryGet("Socket" + i, out TagCompound newTag))
			{
				Socketable g = Socketable.FromTag(newTag);
				if (g is not null)
				{
					_sockets[i] = g;
				}
			}
		}

		PostRoll();
	}

	/// <summary>
	/// Readies all types of gear to be dropped on enemy kill.
	/// </summary>
	/// <param name="pos">Where to spawn the armor</param>
	public static void GenerateGearList()
	{
		AllGear.Clear();
		foreach (Type type in PathOfTerraria.Instance.Code.GetTypes())
		{
			if (type.IsAbstract || !type.IsSubclassOf(typeof(Gear)))
			{
				continue;
			}

			Gear instance = (Gear)Activator.CreateInstance(type);
			AllGear.Add(new(instance.DropChance, instance.IsUnique, type));
		}
	}

	/// <summary>
	/// Spawns a random piece of armor at the given position
	/// </summary>
	/// <param name="pos">Where to spawn the armor</param>
	static readonly MethodInfo Method = typeof(Gear).GetMethod("SpawnGear", BindingFlags.Public | BindingFlags.Static);

	public static void SpawnItem(Vector2 pos, int ilevel = 0, float dropRarityModifier = 0)
	{
		ilevel = ilevel == 0 ? PickItemLevel() : ilevel;  // Pick the item level if not provided

		// Filter AllGear based on item level
		var filteredGear = AllGear.Where(g => 
		{
			var gearInstance = Activator.CreateInstance(g.Item3) as Gear;
			return gearInstance != null && gearInstance.ItemLevel <= ilevel;
		}).ToList();

		// Calculate dropChanceSum based on filtered gear
		float dropChanceSum = filteredGear.Sum(x => x.Item1 * (x.Item2 ? (1f + MathF.Pow(dropRarityModifier / 30f, 2f)) : 1f));
		float choice = Main.rand.NextFloat(dropChanceSum);

		float cumulativeChance = 0;
		foreach (Tuple<float, bool, Type> gear in filteredGear)
		{
			cumulativeChance += gear.Item1 * (gear.Item2 ? (1f + MathF.Pow(dropRarityModifier / 30f, 2f)) : 1f);
			if (choice < cumulativeChance)
			{
				Method.MakeGenericMethod(gear.Item3).Invoke(null, [pos, ilevel, dropRarityModifier]);
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
	public static void SpawnGear<T>(Vector2 pos, int ilevel = 0, float dropRarityModifier = 0) where T : Gear
	{
		var item = new Item();
		item.SetDefaults(ModContent.ItemType<T>());
		var gear = item.ModItem as T;
		gear.Roll(ilevel == 0 ? PickItemLevel() : ilevel, dropRarityModifier);
		Item.NewItem(null, pos, Vector2.Zero, item);
	}
	
	/// <summary>
	/// Selects an appropriate item level for a piece of gear to drop at based on world state
	/// </summary>
	private static int PickItemLevel()
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

	/// <summary>
	/// Returns the intended styling color for a given rarity
	/// </summary>
	/// <param name="rare">The rariy to get the color of</param>
	/// <returns></returns>
	public static Color GetRarityColor(GearRarity rare)
	{
		return rare switch
		{
			GearRarity.Normal => Color.White,
			GearRarity.Magic => new Color(110, 160, 255),
			GearRarity.Rare => new Color(255, 255, 50),
			GearRarity.Unique => new Color(25, 255, 25),
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
	public static string GetDescriptor(GearType type, GearRarity rare, GearInfluence influence)
	{
		string typeName = type switch
		{
			GearType.Sword => "Sword",
			GearType.Spear => "Spear",
			GearType.Bow => "Bow",
			GearType.Gun => "Guns",
			GearType.Staff => "Staff",
			GearType.Tome => "Tome",
			GearType.Helmet => "Helmet",
			GearType.Chestplate => "Chestplate",
			GearType.Leggings => "Leggings",
			GearType.Ring => "Ring",
			GearType.Charm => "Charm",
			_ => ""
		};

		string rareName = rare switch
		{
			GearRarity.Normal => "",
			GearRarity.Magic => "Magic ",
			GearRarity.Rare => "Rare ",
			GearRarity.Unique => "Unique ",
			_ => ""
		};

		string influenceName = influence switch
		{
			GearInfluence.None => "",
			GearInfluence.Solar => "Solar ",
			GearInfluence.Lunar => "Lunar ",
			_ => ""
		};

		return $" {influenceName}{rareName}{typeName}";
	}

	/* // think we can just apply this on equipment update... pretty sure this is for if you hit a certain target or anything like that.
	public override void ModifyHitNPC(Player player, NPC target, ref NPC.HitModifiers modifiers)
	{
		if (!_affixes.Any()) //We don't want to run if there are no affixes to modify anything
		{
			return;
		}

		foreach (GearAffix affix in _affixes)
		{
			switch (affix.GetType().Name)
			{
				case "AddedKnockbackAffix":
					modifiers.Knockback += affix.GetModifierValue(this); // 'this' refers to the current Gear instance
					break;
				case "IncreasedKnockbackAffix": // We want to have the added first, before the multiplier
					modifiers.Knockback *= affix.GetModifierValue(this);
					break;
				case "PiercingAffix":
					modifiers.ArmorPenetration += affix.GetModifierValue(this);
					break;
			}
		}
	}
	*/
}