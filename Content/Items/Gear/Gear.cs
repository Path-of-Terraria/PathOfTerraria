using FunnyExperience.Content.Items.Gear.Affixes;
using FunnyExperience.Content.Items.Gear.Armor;
using System.Collections.Generic;
using Terraria.UI;

namespace FunnyExperience.Content.Items.Gear
{
	internal abstract class Gear : ModItem
	{
		public GearType type;
		public GearRarity rarity;
		public GearInfluence influence;

		public string name;
		public int power;

		public List<Affix> affixes = new();

		public override void Load()
		{
			On_ItemSlot.Draw_SpriteBatch_ItemArray_int_int_Vector2_Color += DrawSpecial;
		}

		private void DrawSpecial(On_ItemSlot.orig_Draw_SpriteBatch_ItemArray_int_int_Vector2_Color orig, SpriteBatch sb, Item[] inv, int context, int slot, Vector2 position, Color color)
		{
			if (inv[slot].ModItem is Gear gear && context != 21)
			{
				string rareName = gear.rarity switch
				{
					GearRarity.Normal => "Normal",
					GearRarity.Magic => "Magic",
					GearRarity.Rare => "Rare",
					GearRarity.Unique => "Unique",
					_ => "Normal"
				};

				Texture2D back = ModContent.Request<Texture2D>($"FunnyExperience/Assets/Slots/{rareName}Back").Value;
				Color backcolor = Color.White * 0.75f;

				sb.Draw(back, position, null, backcolor, 0f, default, Main.inventoryScale, SpriteEffects.None, 0f);
				ItemSlot.Draw(sb, ref inv[slot], 21, position);
			}
			else
			{
				orig(sb, inv, context, slot, position, color);
			}
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			tooltips.Clear();

			var nameLine = new TooltipLine(Mod, "Name", name)
			{
				OverrideColor = GetRarityColor(rarity)
			};
			tooltips.Add(nameLine);

			var rareLine = new TooltipLine(Mod, "Rarity", GetDescriptor(type, rarity, influence))
			{
				OverrideColor = Color.Lerp(GetRarityColor(rarity), Color.White, 0.5f)
			};
			tooltips.Add(rareLine);

			var powerLine = new TooltipLine(Mod, "Power", $"Item level {power}")
			{
				OverrideColor = new Color(150, 150, 150)
			};
			tooltips.Add(powerLine);

			foreach (Affix affix in affixes)
			{
				var affixLine = new TooltipLine(Mod, $"Affix{affix.GetHashCode()}", $"* {affix.tooltip}")
				{
					OverrideColor = Color.LightGray
				};

				tooltips.Add(affixLine);
			}
		}

		/// <summary>
		/// Rolls the randomized aspects of this piece of gear, for a given power
		/// </summary>
		public void Roll(int power)
		{
			this.power = power;

			int rare = Main.rand.Next(100) - (int)(power / 10f);
			rarity = GearRarity.Normal;

			if (rare < 25 + (int)(power / 10f))
				rarity = GearRarity.Magic;
			if (rare < 5)
				rarity = GearRarity.Rare;

			// Only power 50+ can get influence
			if (power > 50)
			{
				int inf = Main.rand.Next(400) - power;

				if (inf < 30)
					influence = Main.rand.NextBool() ? GearInfluence.Solar : GearInfluence.Lunar;
			}

			PostRoll();
			name = GenerateName();
		}

		/// <summary>
		/// This occurs after power has been set and rarity/influence has been rolled, Initialize stats here
		/// </summary>
		public virtual void PostRoll()
		{

		}

		/// <summary>
		/// Allows you to customize what this item's name can be
		/// </summary>
		public virtual string GenerateName()
		{
			return "Unnamed Item";
		}

		/// <summary>
		/// Spawns a random piece of armor at the given position
		/// </summary>
		/// <param name="pos">Where to spawn the armor</param>
		public static void SpawnArmor(Vector2 pos)
		{
			int choice = Main.rand.Next(3);

			switch (choice)
			{
				case 0:
					var item = new Item();
					item.SetDefaults(ModContent.ItemType<Helmet>());
					var gear = item.ModItem as Helmet;
					gear.Roll(PickPower());
					Item.NewItem(null, pos, Vector2.Zero, item);
					break;
			}
		}

		/// <summary>
		/// Selects an appropriate random power for a piece of gear to drop at based on world state
		/// </summary>
		public static int PickPower()
		{
			if (NPC.downedMoonlord)
				return Main.rand.Next(150, 201);
			if (NPC.downedAncientCultist)
				return Main.rand.Next(110, 151);
			if (NPC.downedGolemBoss)
				return Main.rand.Next(95, 131);
			if (NPC.downedPlantBoss)
				return Main.rand.Next(80, 121);
			if (NPC.downedMechBossAny)
				return Main.rand.Next(75, 111);
			if (Main.hardMode)
				return Main.rand.Next(50, 91);
			if (NPC.downedBoss3)
				return Main.rand.Next(30, 50);
			if (NPC.downedBoss2)
				return Main.rand.Next(20, 41);
			if (NPC.downedBoss1)
				return Main.rand.Next(10, 26);
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
				GearRarity.Magic => new Color(100, 100, 255),
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
				GearType.Sword => "Broadsword",
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

			return $"{influenceName}{rareName}{typeName}";
		}
	}
}
