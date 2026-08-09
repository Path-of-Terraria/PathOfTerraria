namespace PathOfTerraria.Common.Systems.ItemStats;

internal interface IDefenseRangeItem
{
	(int Minimum, int Maximum) DefenseRange { get; }
}

internal interface IEnergyShieldRangeItem
{
	(int Minimum, int Maximum) EnergyShieldRange { get; }
}
