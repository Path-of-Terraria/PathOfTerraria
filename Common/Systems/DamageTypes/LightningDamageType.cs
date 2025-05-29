using Terraria.ID;

namespace PathOfTerraria.Common.Systems.DamageTypes;
internal class LightningDamageType : DamageType
{
	public override int GetBuffType()
	{
		return BuffID.Electrified; // TODO: can apply other debuffs (e.g. custom electric debuff) type by difficulty or level
	}

	public override int GetBuffDuration()
	{
		return 60 * 5; // TBD
	}
}
