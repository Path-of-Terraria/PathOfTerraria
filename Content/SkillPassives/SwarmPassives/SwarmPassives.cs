using PathOfTerraria.Common;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Projectiles;
using PathOfTerraria.Common.Systems.ElementalDamage;
using PathOfTerraria.Common.Systems.Skills;
using PathOfTerraria.Content.Buffs.ElementalBuffs;
using PathOfTerraria.Content.Projectiles.Utility;
using PathOfTerraria.Content.Skills.Summon;
using PathOfTerraria.Content.SkillTrees;
using Terraria.ID;

namespace PathOfTerraria.Content.SkillPassives.SwarmPassives;

internal class AggressiveChill(SkillTree tree) : SkillPassive(tree);

internal class BiggerBrood(SkillTree tree) : SkillPassive(tree)
{
	public override object[] TooltipArguments => ["1"];
}

internal class CarapaceCracker(SkillTree tree) : SkillPassive(tree)
{
	internal class CrackedCarapaceDebuff : ModBuff
	{
		public override void SetStaticDefaults()
		{
			Main.debuff[Type] = true;
		}
	}

	internal class CrackedCarapaceNPC : GlobalNPC
	{
		public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
		{
			if (npc.HasBuff<CrackedCarapaceDebuff>())
			{
				modifiers.DefenseEffectiveness *= 0.5f;
			}
		}
	}
}

internal class CarnivorousLarvae(SkillTree tree) : SkillPassive(tree)
{
	public override object[] TooltipArguments => ["2"];
}

internal class ColdBlooded(SkillTree tree) : SkillPassive(tree)
{
	public override object[] TooltipArguments => ["1"];
}

internal class CombustableGuts(SkillTree tree) : SkillPassive(tree)
{
	public override object[] TooltipArguments => ["2"];
}

internal class Eggsplosion(SkillTree tree) : SkillPassive(tree);

internal class FrostbiteMandibles(SkillTree tree) : SkillPassive(tree)
{
	public override object[] TooltipArguments => ["10"];
}

internal class Gestation(SkillTree tree) : SkillPassive(tree)
{
	public override object[] TooltipArguments => ["10"];
}

internal class HeartierExplosions(SkillTree tree) : SkillPassive(tree)
{
	public override object[] TooltipArguments => ["2"];
}

internal class IceVenom(SkillTree tree) : SkillPassive(tree);

internal class InfectedDetonation(SkillTree tree) : SkillPassive(tree)
{
	// % chance to explode, % of max health used as damage
	public override object[] TooltipArguments => ["10", "10"];
}

internal class OverheatingBugs(SkillTree tree) : SkillPassive(tree);

internal class QuickerHatching(SkillTree tree) : SkillPassive(tree)
{
	public override object[] TooltipArguments => ["20"];
}

internal class ShatteringCarapace(SkillTree tree) : SkillPassive(tree);

internal class ShockingEmergence(SkillTree tree) : SkillPassive(tree);

internal class SuperheatedBugs(SkillTree tree) : SkillPassive(tree)
{
	public override object[] TooltipArguments => ["15"];
}

internal class ThermalConversion(SkillTree tree) : SkillPassive(tree)
{
	public override object[] TooltipArguments => ["0.5"];
}

internal class ViciousBites(SkillTree tree) : SkillPassive(tree);

internal class VolatileInsects(SkillTree tree) : SkillPassive(tree)
{
	public override object[] TooltipArguments => ["300"];
}
