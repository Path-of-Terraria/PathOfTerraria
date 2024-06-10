using System.Collections.Generic;

namespace PathOfTerraria.Data.Models;

public class PassiveData
{
	public int Id { get; set; }
	public string PassiveName { get; set; }
	public int MaxLevel { get; set; }
	public List<PassiveConnection> Connections { get; set; }
}