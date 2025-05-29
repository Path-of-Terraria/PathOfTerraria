using Terraria.ID;

namespace PathOfTerraria.Common.Systems.DamageTypes;
internal class ColdDamageType : DamageType
{
	public override int GetBuffType()
	{
		return BuffID.Chilled;  // TODO: could apply other debuffs (e.g. Frozen, Frostburn) by difficulty, damage done or level
	}

	public override int GetBuffDuration()
	{
		return 60 * 5; // TBD
	}
}
