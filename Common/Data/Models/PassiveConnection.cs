using PathOfTerraria.Common.Mechanics;

namespace PathOfTerraria.Common.Data.Models;

public class PassiveConnection
{
	public int ReferenceId { get; set; }
	public bool IsHidden { get; set; }
	public bool EffectsOnly { get; set; }
}