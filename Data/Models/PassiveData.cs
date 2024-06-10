using PathOfTerraria.Core.Systems.TreeSystem;
using System.Collections.Generic;

namespace PathOfTerraria.Data.Models;

public class PassiveData
{
	public int Id { get; set; }
	public string Passive { get; set; }
	public List<int> Position { get; set; } // no clue how this works but it probably works fine like this - Position[0] -> x | Position[1] -> y
	public int MaxLevel { get; set; }
	public List<int> Connections { get; set; }
}