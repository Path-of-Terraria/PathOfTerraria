using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;

namespace PathOfTerraria.Content.SkillPassives.SwarmPassives;

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