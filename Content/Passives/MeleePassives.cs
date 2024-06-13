using PathOfTerraria.Core;
using PathOfTerraria.Core.Systems.TreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class CloseRangePassive : Passive
{
	public override string InternalIdentifier => "IncreasedCloseDamage";
}

internal class BleedPassive : Passive
{
	public override string InternalIdentifier => "BleedingDamageOverTime";
}

internal class DamageReductionPassive : Passive
{
	public override string InternalIdentifier => "IncreasedDamageReduction";

	public override void BuffPlayer(Player player)
	{
		player.endurance += 0.025f * Level;
	}
}

internal class DamageReflectionPassive : Passive
{
	public override string InternalIdentifier => "AddedContactDamageReflection";

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