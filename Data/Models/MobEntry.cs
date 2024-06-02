using System.Collections.Generic;

namespace PathOfTerraria.Data.Models;

public class MobEntry
{
	public float? Scale { get; set; }
	public string Prefix { get; set; }
	public decimal Weight { get; set; }
	public MobStats Stats { get; set; }
	public List<MobEntryAffix> Affixes { get; set; }
	public string Requirements { get; set; }
}