using System.Collections.Generic;
using System.IO;
using System.Linq;
using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.Subworlds.MappingAreas;
using PathOfTerraria.Content.Items.Mapping.Sigils;
using SubworldLibrary;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems.Sigils;

internal enum SigilGrade : byte
{
	Carved,
	Gilded,
	Prismatic,
	Unique,
}

internal enum SigilFamily : byte
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

internal enum SigilKind : byte
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

internal readonly record struct SigilEntry(SigilKind Kind, SigilGrade Grade)
{
	public SigilFamily Family => SigilCatalog.GetFamily(Kind);

	public TagCompound Save()
	{
		return new TagCompound
		{
			[nameof(Kind)] = (byte)Kind,
			[nameof(Grade)] = (byte)Grade,
		};
	}

	public static SigilEntry Load(TagCompound tag)
	{
		return new SigilEntry((SigilKind)tag.GetByte(nameof(Kind)), (SigilGrade)tag.GetByte(nameof(Grade)));
	}

	public void NetSend(BinaryWriter writer)
	{
		writer.Write((byte)Kind);
		writer.Write((byte)Grade);
	}

	public static SigilEntry NetReceive(BinaryReader reader)
	{
		return new SigilEntry((SigilKind)reader.ReadByte(), (SigilGrade)reader.ReadByte());
	}
}

internal static class SigilCatalog
{
	public static bool IsValid(SigilEntry entry)
	{
		return Enum.IsDefined(entry.Kind) && Enum.IsDefined(entry.Grade)
			&& DomainSigil.ItemTypeFor(entry.Kind, entry.Grade) > 0;
	}

	public static SigilFamily GetFamily(SigilKind kind)
	{
		return kind switch
		{
			SigilKind.Binding or SigilKind.RunicMenagerie => SigilFamily.Binding,
			SigilKind.InfernalConflux or SigilKind.GlacialConflux or SigilKind.CelestialConflux or SigilKind.TriuneConflux => SigilFamily.Conflux,
			SigilKind.Devotion => SigilFamily.Devotion,
			SigilKind.Infestation => SigilFamily.Infestation,
			SigilKind.Nemeses => SigilFamily.Nemeses,
			SigilKind.WardedWealth => SigilFamily.WardedWealth,
			SigilKind.Cartography => SigilFamily.Cartography,
			SigilKind.Sovereignty or SigilKind.CrownedTyrant => SigilFamily.Sovereignty,
			SigilKind.Peril => SigilFamily.Peril,
			_ => throw new ArgumentOutOfRangeException(nameof(kind)),
		};
	}

	public static Color GetColor(SigilKind kind)
	{
		return GetFamily(kind) switch
		{
			SigilFamily.Binding => new Color(155, 88, 220),
			SigilFamily.Conflux => kind switch
			{
				SigilKind.InfernalConflux => new Color(232, 80, 45),
				SigilKind.GlacialConflux => new Color(75, 180, 235),
				SigilKind.CelestialConflux => new Color(224, 196, 75),
				_ => new Color(200, 120, 235),
			},
			SigilFamily.Devotion => new Color(252, 230, 145),
			SigilFamily.Infestation => new Color(126, 190, 70),
			SigilFamily.Nemeses => new Color(220, 82, 68),
			SigilFamily.WardedWealth => new Color(230, 166, 55),
			SigilFamily.Cartography => new Color(80, 165, 230),
			SigilFamily.Sovereignty => new Color(205, 55, 55),
			SigilFamily.Peril => new Color(125, 45, 150),
			_ => Color.White,
		};
	}

	public static int GetPower(SigilGrade grade)
	{
		return grade switch
		{
			SigilGrade.Carved => 1,
			SigilGrade.Gilded => 2,
			SigilGrade.Prismatic => 3,
			_ => 4,
		};
	}

	public static string DescribeEffect(SigilEntry entry)
	{
		int power = GetPower(entry.Grade);
		return entry.Kind switch
		{
			SigilKind.Binding => $"+{power} Runebound prisons",
			SigilKind.InfernalConflux => $"+{power} Infernal rifts",
			SigilKind.GlacialConflux => $"+{power} Glacial rifts",
			SigilKind.CelestialConflux => $"+{power} Celestial rifts",
			SigilKind.Devotion => $"+{power} guarded shrines",
			SigilKind.Infestation => $"{(entry.Grade == SigilGrade.Carved ? 15 : entry.Grade == SigilGrade.Gilded ? 30 : 50)}% more base packs",
			SigilKind.Nemeses => $"+{power + 1} rare captains",
			SigilKind.WardedWealth => $"+{power} Warded Caches",
			SigilKind.Cartography => "additional map-boss map drops",
			SigilKind.Sovereignty => $"empowered boss; +{power} reward rolls",
			SigilKind.Peril => $"+{(entry.Grade == SigilGrade.Carved ? 10 : entry.Grade == SigilGrade.Gilded ? 20 : 35)}% map-affix effect and strength",
			SigilKind.RunicMenagerie => "+3 empowered Runebound prisons",
			SigilKind.TriuneConflux => "one empowered rift of every kind",
			SigilKind.CrownedTyrant => "tyrant boss; reinforcements; +4 reward rolls",
			_ => entry.Kind.ToString(),
		};
	}

	public static int GetThreat(SigilEntry entry)
	{
		int power = GetPower(entry.Grade);
		return entry.Kind switch
		{
			SigilKind.Infestation or SigilKind.Devotion or SigilKind.Cartography => power,
			SigilKind.Binding or SigilKind.InfernalConflux or SigilKind.GlacialConflux or SigilKind.CelestialConflux
				or SigilKind.Nemeses or SigilKind.WardedWealth => power * 2,
			SigilKind.Sovereignty or SigilKind.Peril => power * 3,
			SigilKind.RunicMenagerie or SigilKind.TriuneConflux => 8,
			SigilKind.CrownedTyrant => 12,
			_ => power,
		};
	}

	public static int GetItemType(SigilKind kind, SigilGrade grade)
	{
		return DomainSigil.ItemTypeFor(kind, grade);
	}

	public static int RollDropType(int mapTier, bool allowUnique)
	{
		if (allowUnique && mapTier >= 8 && Main.rand.NextBool(200))
		{
			return Main.rand.Next(3) switch
			{
				0 => GetItemType(SigilKind.RunicMenagerie, SigilGrade.Unique),
				1 => GetItemType(SigilKind.TriuneConflux, SigilGrade.Unique),
				_ => GetItemType(SigilKind.CrownedTyrant, SigilGrade.Unique),
			};
		}

		SigilGrade grade;
		int roll = Main.rand.Next(100);
		if (mapTier >= 8)
		{
			grade = roll < 10 ? SigilGrade.Prismatic : roll < 48 ? SigilGrade.Gilded : SigilGrade.Carved;
		}
		else if (mapTier >= 4)
		{
			grade = roll < 3 ? SigilGrade.Prismatic : roll < 28 ? SigilGrade.Gilded : SigilGrade.Carved;
		}
		else
		{
			grade = roll < 8 ? SigilGrade.Gilded : SigilGrade.Carved;
		}

		SigilKind[] normalKinds =
		[
			SigilKind.Binding,
			SigilKind.InfernalConflux,
			SigilKind.GlacialConflux,
			SigilKind.CelestialConflux,
			SigilKind.Devotion,
			SigilKind.Infestation,
			SigilKind.Nemeses,
			SigilKind.WardedWealth,
			SigilKind.Cartography,
			SigilKind.Sovereignty,
			SigilKind.Peril,
		];

		return GetItemType(Main.rand.Next(normalKinds), grade);
	}

	public static int RollNormalType(SigilGrade grade)
	{
		SigilKind[] kinds =
		[
			SigilKind.Binding, SigilKind.InfernalConflux, SigilKind.GlacialConflux, SigilKind.CelestialConflux,
			SigilKind.Devotion, SigilKind.Infestation, SigilKind.Nemeses, SigilKind.WardedWealth,
			SigilKind.Cartography, SigilKind.Sovereignty, SigilKind.Peril,
		];
		return GetItemType(Main.rand.Next(kinds), grade);
	}
}

internal sealed class SigilSystem : ModSystem
{
	private static readonly List<SigilEntry> activeSigils = [];

	public static IReadOnlyList<SigilEntry> ActiveSigils => activeSigils;
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
		activeSigils.Clear();
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

	public static void SetActive(IEnumerable<SigilEntry> entries)
	{
		activeSigils.Clear();
		var families = new HashSet<SigilFamily>();
		foreach (SigilEntry entry in entries)
		{
			if (!SigilCatalog.IsValid(entry))
			{
				continue;
			}

			SigilFamily family = entry.Family;
			if (!families.Add(family))
			{
				continue;
			}

			activeSigils.Add(entry);
			if (activeSigils.Count == 4)
			{
				break;
			}
		}
	}

	public static void ClearActive()
	{
		activeSigils.Clear();
	}

	public static void RecordMapCompletion(int tier)
	{
		HighestCompletedMapTier = Math.Max(HighestCompletedMapTier, Math.Clamp(tier, 0, Content.Items.Consumables.Maps.Map.MaxMapTier));
	}

	public static bool Has(SigilKind kind)
	{
		return activeSigils.Any(entry => entry.Kind == kind);
	}

	public static SigilEntry? FindFamily(SigilFamily family)
	{
		foreach (SigilEntry entry in activeSigils)
		{
			if (entry.Family == family)
			{
				return entry;
			}
		}

		return null;
	}

	public static int Power(SigilFamily family)
	{
		return FindFamily(family) is { } entry ? SigilCatalog.GetPower(entry.Grade) : 0;
	}

	public static TagCompound SaveActive()
	{
		return new TagCompound
		{
			["entries"] = activeSigils.Select(entry => entry.Save()).ToArray(),
			[nameof(HighestCompletedMapTier)] = HighestCompletedMapTier,
		};
	}

	public static void LoadActive(TagCompound tag)
	{
		SetActive(tag.TryGet("entries", out TagCompound[] entries) ? entries.Select(SigilEntry.Load) : []);
		HighestCompletedMapTier = Math.Max(HighestCompletedMapTier, tag.GetInt(nameof(HighestCompletedMapTier)));
	}

	public static void WriteActive(BinaryWriter writer)
	{
		writer.Write((byte)activeSigils.Count);
		foreach (SigilEntry entry in activeSigils)
		{
			entry.NetSend(writer);
		}
	}

	public static void ReadActive(BinaryReader reader)
	{
		activeSigils.Clear();
		int count = reader.ReadByte();
		for (int i = 0; i < count; i++)
		{
			SigilEntry entry = SigilEntry.NetReceive(reader);
			if (i < 4)
			{
				activeSigils.Add(entry);
			}
		}
	}

	public static bool IsExplorationMap()
	{
		return SubworldSystem.Current is MappingWorld and IExplorationWorld and not RavencrestSubworld;
	}
}
