using PathOfTerraria.Core.Systems.TreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class LongRangePassive : Passive
{
	public override string InternalIdentifier => "IncreasedDistantDamage";
	public override string Name => "Sniper";
	public override string Tooltip => "Increases your damage against distant enemies by 10% per level";
}