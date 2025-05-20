using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.ModPlayers;

namespace PathOfTerraria.Common.Projectiles;

/// <summary> Helper for projectiles bound to a particular player skill. See <see cref="Skill"/>. </summary>
/// <typeparam name="T"> The skill this projectile is associated with. </typeparam>
internal abstract class SkillProjectile<T> : ModProjectile where T : Skill
{
	public Skill Skill
	{
		get
		{
			if (Main.player[Projectile.owner].GetModPlayer<SkillCombatPlayer>().TryGetSkill<T>(out Skill skill))
			{
				return skill;
			}

			return Skill.GetAndPrepareSkill<T>();
		}
	}
}