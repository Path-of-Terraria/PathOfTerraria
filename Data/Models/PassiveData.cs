using System.Collections.Generic;

namespace PathOfTerraria.Data.Models;

public class PassiveData
{
	public int Id { get; set; }
	public int ReferenceId { get; set; }
	public PassivePosition Position { get; set; } // no clue how this works but it probably works fine like this - Position[0] -> x | Position[1] -> y
	public int MaxLevel { get; set; }
	public List<PassiveConnection> Connections { get; set; }
}