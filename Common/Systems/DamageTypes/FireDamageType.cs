using Terraria.ID;

namespace PathOfTerraria.Common.Systems.DamageTypes;
internal class FireDamageType : DamageType
{
	public override int GetBuffType()
	{
		return BuffID.OnFire; // TODO: can apply other debuffs (e.g. OnFire3) by difficulty, damage done or level
	}

	public override int GetBuffDuration()
	{
		return 60 * 5; // TBD
	}
}
