using System.Collections.Generic;

namespace PathOfTerraria.Data.Models;

public class PassiveData
{
	public string InternalIdentifier { get; set; }
	public int ReferenceId { get; set; }
	public PassivePosition Position { get; set; }
	public int MaxLevel { get; set; }
	public List<PassiveConnection> Connections { get; set; }
}