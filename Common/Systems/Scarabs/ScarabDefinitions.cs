using System.Collections.Generic;
using System.IO;
using System.Linq;
using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.Subworlds.MappingAreas;
using PathOfTerraria.Content.Items.Mapping.Scarabs;
using SubworldLibrary;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems.Scarabs;

internal enum ScarabGrade : byte
{
	Carved,
	Gilded,
	Prismatic,
	Unique,
}

internal enum ScarabFamily : byte
{
	Binding,
	Conflux,
	Devotion,
	Infestation,
	Nemeses,
	WardedWealth,
	Cartography,
	Sovereignty,
	Peril,
}

internal enum ScarabKind : byte
{
	Binding,
	InfernalConflux,
	GlacialConflux,
	CelestialConflux,
	Devotion,
	Infestation,
	Nemeses,
	WardedWealth,
	Cartography,
	Sovereignty,
	Peril,
	RunicMenagerie,
	TriuneConflux,
	CrownedTyrant,
}

internal readonly record struct ScarabEntry(ScarabKind Kind, ScarabGrade Grade)
{
	public ScarabFamily Family => ScarabCatalog.GetFamily(Kind);

	public TagCompound Save()
	{
		return new TagCompound
		{
			[nameof(Kind)] = (byte)Kind,
			[nameof(Grade)] = (byte)Grade,
		};
	}

	public static ScarabEntry Load(TagCompound tag)
	{
		return new ScarabEntry((ScarabKind)tag.GetByte(nameof(Kind)), (ScarabGrade)tag.GetByte(nameof(Grade)));
	}

	public void NetSend(BinaryWriter writer)
	{
		writer.Write((byte)Kind);
		writer.Write((byte)Grade);
	}

	public static ScarabEntry NetReceive(BinaryReader reader)
	{
		return new ScarabEntry((ScarabKind)reader.ReadByte(), (ScarabGrade)reader.ReadByte());
	}
}

internal static class ScarabCatalog
{
	public static bool IsValid(ScarabEntry entry)
	{
		return Enum.IsDefined(entry.Kind) && Enum.IsDefined(entry.Grade)
			&& DomainScarab.ItemTypeFor(entry.Kind, entry.Grade) > 0;
	}

	public static ScarabFamily GetFamily(ScarabKind kind)
	{
		return kind switch
		{
			ScarabKind.Binding or ScarabKind.RunicMenagerie => ScarabFamily.Binding,
			ScarabKind.InfernalConflux or ScarabKind.GlacialConflux or ScarabKind.CelestialConflux or ScarabKind.TriuneConflux => ScarabFamily.Conflux,
			ScarabKind.Devotion => ScarabFamily.Devotion,
			ScarabKind.Infestation => ScarabFamily.Infestation,
			ScarabKind.Nemeses => ScarabFamily.Nemeses,
			ScarabKind.WardedWealth => ScarabFamily.WardedWealth,
			ScarabKind.Cartography => ScarabFamily.Cartography,
			ScarabKind.Sovereignty or ScarabKind.CrownedTyrant => ScarabFamily.Sovereignty,
			ScarabKind.Peril => ScarabFamily.Peril,
			_ => throw new ArgumentOutOfRangeException(nameof(kind)),
		};
	}

	public static Color GetColor(ScarabKind kind)
	{
		return GetFamily(kind) switch
		{
			ScarabFamily.Binding => new Color(155, 88, 220),
			ScarabFamily.Conflux => kind switch
			{
				ScarabKind.InfernalConflux => new Color(232, 80, 45),
				ScarabKind.GlacialConflux => new Color(75, 180, 235),
				ScarabKind.CelestialConflux => new Color(224, 196, 75),
				_ => new Color(200, 120, 235),
			},
			ScarabFamily.Devotion => new Color(252, 230, 145),
			ScarabFamily.Infestation => new Color(126, 190, 70),
			ScarabFamily.Nemeses => new Color(220, 82, 68),
			ScarabFamily.WardedWealth => new Color(230, 166, 55),
			ScarabFamily.Cartography => new Color(80, 165, 230),
			ScarabFamily.Sovereignty => new Color(205, 55, 55),
			ScarabFamily.Peril => new Color(125, 45, 150),
			_ => Color.White,
		};
	}

	public static int GetPower(ScarabGrade grade)
	{
		return grade switch
		{
			ScarabGrade.Carved => 1,
			ScarabGrade.Gilded => 2,
			ScarabGrade.Prismatic => 3,
			_ => 4,
		};
	}

	public static string DescribeEffect(ScarabEntry entry)
	{
		int power = GetPower(entry.Grade);
		return entry.Kind switch
		{
			ScarabKind.Binding => $"+{power} Runebound prisons",
			ScarabKind.InfernalConflux => $"+{power} Infernal rifts",
			ScarabKind.GlacialConflux => $"+{power} Glacial rifts",
			ScarabKind.CelestialConflux => $"+{power} Celestial rifts",
			ScarabKind.Devotion => $"+{power} guarded shrines",
			ScarabKind.Infestation => $"{(entry.Grade == ScarabGrade.Carved ? 15 : entry.Grade == ScarabGrade.Gilded ? 30 : 50)}% more base packs",
			ScarabKind.Nemeses => $"+{power + 1} rare captains",
			ScarabKind.WardedWealth => $"+{power} Warded Caches",
			ScarabKind.Cartography => "additional map-boss map drops",
			ScarabKind.Sovereignty => $"empowered boss; +{power} reward rolls",
			ScarabKind.Peril => $"+{(entry.Grade == ScarabGrade.Carved ? 10 : entry.Grade == ScarabGrade.Gilded ? 20 : 35)}% map-affix effect and strength",
			ScarabKind.RunicMenagerie => "+3 empowered Runebound prisons",
			ScarabKind.TriuneConflux => "one empowered rift of every kind",
			ScarabKind.CrownedTyrant => "tyrant boss; reinforcements; +4 reward rolls",
			_ => entry.Kind.ToString(),
		};
	}

	public static int GetThreat(ScarabEntry entry)
	{
		int power = GetPower(entry.Grade);
		return entry.Kind switch
		{
			ScarabKind.Infestation or ScarabKind.Devotion or ScarabKind.Cartography => power,
			ScarabKind.Binding or ScarabKind.InfernalConflux or ScarabKind.GlacialConflux or ScarabKind.CelestialConflux
				or ScarabKind.Nemeses or ScarabKind.WardedWealth => power * 2,
			ScarabKind.Sovereignty or ScarabKind.Peril => power * 3,
			ScarabKind.RunicMenagerie or ScarabKind.TriuneConflux => 8,
			ScarabKind.CrownedTyrant => 12,
			_ => power,
		};
	}

	public static int GetItemType(ScarabKind kind, ScarabGrade grade)
	{
		return DomainScarab.ItemTypeFor(kind, grade);
	}

	public static int RollDropType(int mapTier, bool allowUnique)
	{
		if (allowUnique && mapTier >= 8 && Main.rand.NextBool(200))
		{
			return Main.rand.Next(3) switch
			{
				0 => GetItemType(ScarabKind.RunicMenagerie, ScarabGrade.Unique),
				1 => GetItemType(ScarabKind.TriuneConflux, ScarabGrade.Unique),
				_ => GetItemType(ScarabKind.CrownedTyrant, ScarabGrade.Unique),
			};
		}

		ScarabGrade grade;
		int roll = Main.rand.Next(100);
		if (mapTier >= 8)
		{
			grade = roll < 10 ? ScarabGrade.Prismatic : roll < 48 ? ScarabGrade.Gilded : ScarabGrade.Carved;
		}
		else if (mapTier >= 4)
		{
			grade = roll < 3 ? ScarabGrade.Prismatic : roll < 28 ? ScarabGrade.Gilded : ScarabGrade.Carved;
		}
		else
		{
			grade = roll < 8 ? ScarabGrade.Gilded : ScarabGrade.Carved;
		}

		ScarabKind[] normalKinds =
		[
			ScarabKind.Binding,
			ScarabKind.InfernalConflux,
			ScarabKind.GlacialConflux,
			ScarabKind.CelestialConflux,
			ScarabKind.Devotion,
			ScarabKind.Infestation,
			ScarabKind.Nemeses,
			ScarabKind.WardedWealth,
			ScarabKind.Cartography,
			ScarabKind.Sovereignty,
			ScarabKind.Peril,
		];

		return GetItemType(Main.rand.Next(normalKinds), grade);
	}

	public static int RollNormalType(ScarabGrade grade)
	{
		ScarabKind[] kinds =
		[
			ScarabKind.Binding, ScarabKind.InfernalConflux, ScarabKind.GlacialConflux, ScarabKind.CelestialConflux,
			ScarabKind.Devotion, ScarabKind.Infestation, ScarabKind.Nemeses, ScarabKind.WardedWealth,
			ScarabKind.Cartography, ScarabKind.Sovereignty, ScarabKind.Peril,
		];
		return GetItemType(Main.rand.Next(kinds), grade);
	}
}

internal sealed class ScarabSystem : ModSystem
{
	private static readonly List<ScarabEntry> activeScarabs = [];

	public static IReadOnlyList<ScarabEntry> ActiveScarabs => activeScarabs;
	public static int HighestCompletedMapTier { get; private set; }
	public static int UnlockedSlotCount => HighestCompletedMapTier switch
	{
		>= 10 => 4,
		>= 7 => 3,
		>= 4 => 2,
		>= 1 => 1,
		_ => 0,
	};

	public override void ClearWorld()
	{
		activeScarabs.Clear();
		HighestCompletedMapTier = 0;
	}

	public override void SaveWorldData(TagCompound tag)
	{
		if (HighestCompletedMapTier > 0)
		{
			tag[nameof(HighestCompletedMapTier)] = HighestCompletedMapTier;
		}
	}

	public override void LoadWorldData(TagCompound tag)
	{
		HighestCompletedMapTier = Math.Clamp(tag.GetInt(nameof(HighestCompletedMapTier)), 0,
			Content.Items.Consumables.Maps.Map.MaxMapTier);
	}

	public override void NetSend(BinaryWriter writer)
	{
		writer.Write((byte)HighestCompletedMapTier);
	}

	public override void NetReceive(BinaryReader reader)
	{
		HighestCompletedMapTier = Math.Clamp((int)reader.ReadByte(), 0,
			Content.Items.Consumables.Maps.Map.MaxMapTier);
	}

	public static void SetActive(IEnumerable<ScarabEntry> entries)
	{
		activeScarabs.Clear();
		var families = new HashSet<ScarabFamily>();
		foreach (ScarabEntry entry in entries)
		{
			if (!ScarabCatalog.IsValid(entry))
			{
				continue;
			}

			ScarabFamily family = entry.Family;
			if (!families.Add(family))
			{
				continue;
			}

			activeScarabs.Add(entry);
			if (activeScarabs.Count == 4)
			{
				break;
			}
		}
	}

	public static void ClearActive()
	{
		activeScarabs.Clear();
	}

	public static void RecordMapCompletion(int tier)
	{
		HighestCompletedMapTier = Math.Max(HighestCompletedMapTier, Math.Clamp(tier, 0, Content.Items.Consumables.Maps.Map.MaxMapTier));
	}

	public static bool Has(ScarabKind kind)
	{
		return activeScarabs.Any(entry => entry.Kind == kind);
	}

	public static ScarabEntry? FindFamily(ScarabFamily family)
	{
		foreach (ScarabEntry entry in activeScarabs)
		{
			if (entry.Family == family)
			{
				return entry;
			}
		}

		return null;
	}

	public static int Power(ScarabFamily family)
	{
		return FindFamily(family) is { } entry ? ScarabCatalog.GetPower(entry.Grade) : 0;
	}

	public static TagCompound SaveActive()
	{
		return new TagCompound
		{
			["entries"] = activeScarabs.Select(entry => entry.Save()).ToArray(),
			[nameof(HighestCompletedMapTier)] = HighestCompletedMapTier,
		};
	}

	public static void LoadActive(TagCompound tag)
	{
		SetActive(tag.TryGet("entries", out TagCompound[] entries) ? entries.Select(ScarabEntry.Load) : []);
		HighestCompletedMapTier = Math.Max(HighestCompletedMapTier, tag.GetInt(nameof(HighestCompletedMapTier)));
	}

	public static void WriteActive(BinaryWriter writer)
	{
		writer.Write((byte)activeScarabs.Count);
		foreach (ScarabEntry entry in activeScarabs)
		{
			entry.NetSend(writer);
		}
	}

	public static void ReadActive(BinaryReader reader)
	{
		activeScarabs.Clear();
		int count = reader.ReadByte();
		for (int i = 0; i < count; i++)
		{
			ScarabEntry entry = ScarabEntry.NetReceive(reader);
			if (i < 4)
			{
				activeScarabs.Add(entry);
			}
		}
	}

	public static bool IsExplorationMap()
	{
		return SubworldSystem.Current is MappingWorld and IExplorationWorld and not RavencrestSubworld;
	}
}
