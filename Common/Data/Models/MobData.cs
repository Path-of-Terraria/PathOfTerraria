using System.Collections.Generic;

namespace PathOfTerraria.Common.Data.Models;

public class MobData
{
	public int NetId { get; set; }
	public List<MobDamage> Damage { get; set; }
	public List<MobEntry> Entries { get; set; }
}

public class MobEntry
{
	public float? Scale { get; set; }
	public string Prefix { get; set; }
	public decimal Weight { get; set; }
	public MobStats Stats { get; set; }
	public List<MobDamage> DamageOverrides { get; set; }
	public List<MobEntryAffix> Affixes { get; set; }
	public string Requirements { get; set; }
}

public class MobEntryAffix
{
	public string Name { get; set; }
}

public class MobStats
{
	public int Level { get; set; }
	public int Experience { get; set; }
}

public class MobDamage
{
	public int MinLevel { get; set; }

	public MobElementStats Fire { get; set; }
	public MobElementStats Cold { get; set; }
	public MobElementStats Lightning { get; set; }
}

public class MobElementStats
{
	public int? Added { get; set; }
	public float? Conversion { get; set; }
}