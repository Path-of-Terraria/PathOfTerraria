using PathOfTerraria.Core;
using PathOfTerraria.Core.Systems.TreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class CloseRangePassive : Passive
{
	public override string InternalIdentifier => "IncreasedCloseDamage";
	public override string Name => "Close Combatant";
	public override string Tooltip => "Increases your damage against nearby enemies by 10% per level";
}

internal class BleedPassive : Passive
{
	public override string InternalIdentifier => "BleedingDamageOverTime";
	public override string Name => "Crimson Dance";
	public override string Tooltip => "Your melee attacks inflict bleeding, dealing 5 damage per second per level";
}

internal class DamageReductionPassive : Passive
{
	public override string InternalIdentifier => "IncreasedDamageReduction";
	public override string Name => "Iron Will";
	public override string Tooltip => "Gain 0.25% damage reduction per level.";

	public override void BuffPlayer(Player player)
	{
		player.endurance += 0.025f * Level;
	}
}

internal class DamageReflectionPassive : Passive
{
	public override string InternalIdentifier => "AddedContactDamageReflection";
	public override string Name => "Thorny Exterior";
	public override string Tooltip => "Reflect 10 more damage to enemies that deal contact damage per level.";

	public override void OnLoad()
	{
		PathOfTerrariaPlayerEvents.OnHitByNPCEvent += ReflectHitByNPC;
	}

	private void ReflectHitByNPC(Player player, NPC npc, Player.HurtInfo hurtInfo)
	{
		int dir = player.Center.X < npc.Center.X ? 1 : -1;
		npc.SimpleStrikeNPC(10 * player.GetModPlayer<TreePlayer>().GetCumulativeLevel(InternalIdentifier), dir, false, 1);
	}
}