using System.Collections.Generic;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core.Items;
using SubworldLibraryCommunityFork;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear;

internal class GearAlternatives
{
	internal static Dictionary<int, int> GearToVanillaAlternative = [];
	internal static Dictionary<int, int> VanillaAlternativeToGear = [];

	public static bool Register(int gear, int vanilla)
	{
		if (!GearToVanillaAlternative.ContainsKey(gear) && !VanillaAlternativeToGear.ContainsKey(vanilla))
		{
			GearToVanillaAlternative.Add(gear, vanilla);
			VanillaAlternativeToGear.Add(vanilla, gear);
			return true;
		}

		return false;
	}

	public static bool HasGear(int vanilla)
	{
		return VanillaAlternativeToGear.ContainsKey(vanilla);
	}
}

internal class GearAlternativeGlobalItem : ModSystem
{
	public override void PostAddRecipes()
	{
		foreach (Recipe recipe in Main.recipe)
		{
			if (GearAlternatives.HasGear(recipe.createItem.type))
			{
				recipe.ReplaceResult(GearAlternatives.VanillaAlternativeToGear[recipe.createItem.type], recipe.createItem.stack);
			}
		}
	}
}

internal class GearAlternativeChestReplacement : ModSystem
{
	private static class ContainerStyle
	{
		public const int Shadow = 3;
		public const int LockedShadow = 4;
		public const int RichMahogany = 8;
		public const int Ivy = 10;
		public const int Frozen = 11;
		public const int Skyware = 13;
		public const int Lihzahrd = 16;
		public const int Water = 17;
		public const int Jungle = 18;
		public const int Corruption = 19;
		public const int Crimson = 20;
		public const int Hallowed = 21;
		public const int Ice = 22;
		public const int LockedJungle = 23;
		public const int LockedCorruption = 24;
		public const int LockedCrimson = 25;
		public const int LockedHallowed = 26;
		public const int LockedIce = 27;
	}

	private const int SurfaceChestLevel = 5;
	private const int UndergroundChestLevel = 10;
	private const int BiomeChestLevel = 15;
	private const int DungeonChestLevel = 30;
	private const int ShadowChestLevel = 40;
	private const int HardmodeBiomeChestLevel = 50;
	private const int TempleChestLevel = 60;

	public override void PostWorldGen()
	{
		foreach (Chest chest in Main.chest)
		{
			if (chest is null)
			{
				continue;
			}

			int chestLevel = SubworldSystem.Current is null
				? GetOverworldChestLevel(chest)
				: PoTItemHelper.PickItemLevel();

			foreach (Item item in chest.item)
			{
				if (GearAlternatives.VanillaAlternativeToGear.TryGetValue(item.type, out int value) && !item.IsAir)
				{
					item.SetDefaults(value);
					item.stack = 1;

					PoTInstanceItemData data = item.GetInstanceData();
					data.Rarity = ItemRarity.Magic;

					if (WorldGen.genRand.NextBool(10))
					{
						data.Rarity = ItemRarity.Rare;
					}
				}

				// Chest contents are created before their final location is available, so gear defaults
				// to level 1 during world generation. Roll it again once the chest has been placed.
				if (GearGlobalItem.IsGearItem(item) && item.GetInstanceData().RealLevel <= 1)
				{
					PoTItemHelper.Roll(item, chestLevel);
				}
			}
		}
	}

	private static int GetOverworldChestLevel(Chest chest)
	{
		Tile tile = Framing.GetTileSafely(chest.x, chest.y);

		if (!tile.HasTile || !TileID.Sets.BasicChest[tile.TileType])
		{
			return UndergroundChestLevel;
		}

		if (tile.TileType != TileID.Containers)
		{
			return IsDungeonChest(tile)
				? DungeonChestLevel
				: chest.y < Main.worldSurface ? SurfaceChestLevel : UndergroundChestLevel;
		}

		int style = tile.TileFrameX / 36;
		return style switch
		{
			ContainerStyle.Jungle or ContainerStyle.Corruption or ContainerStyle.Crimson
				or ContainerStyle.Hallowed or ContainerStyle.Ice
				or ContainerStyle.LockedJungle or ContainerStyle.LockedCorruption or ContainerStyle.LockedCrimson
				or ContainerStyle.LockedHallowed or ContainerStyle.LockedIce => HardmodeBiomeChestLevel,
			ContainerStyle.Shadow or ContainerStyle.LockedShadow => ShadowChestLevel,
			ContainerStyle.Lihzahrd => TempleChestLevel,
			ContainerStyle.RichMahogany or ContainerStyle.Ivy or ContainerStyle.Frozen => BiomeChestLevel,
			ContainerStyle.Skyware or ContainerStyle.Water => UndergroundChestLevel,
			_ when IsDungeonChest(tile) => DungeonChestLevel,
			_ when chest.y < Main.worldSurface => SurfaceChestLevel,
			_ => UndergroundChestLevel,
		};
	}

	private static bool IsDungeonChest(Tile tile)
	{
		return tile.WallType < Main.wallDungeon.Length && Main.wallDungeon[tile.WallType];
	}
}
