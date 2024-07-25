using System.Collections.Generic;

namespace PathOfTerraria.Common.Data.Models;

public class MobData
{
	public int NetId { get; set; }
	public List<MobEntry> Entries { get; set; }
}