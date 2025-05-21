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

	private bool _justSpawned = false;

	/// <summary> Similar to <see cref="ModProjectile.OnSpawn"/> but called on all clients. </summary>
	public virtual void SyncedSpawn() { }

	/// <summary><inheritdoc cref="ModProjectile.PreAI"/><para/>Additionally sets one-time stat effects and calls <see cref="SyncedSpawn"/> by default.</summary>
	/// <returns><inheritdoc cref="ModProjectile.PreAI"/></returns>
	public override bool PreAI()
	{
		if (!_justSpawned)
		{
			int originalCrit = Math.Max(Projectile.OriginalCritChance, 4);
			Projectile.CritChance = Skill.GetTotalCritChance(originalCrit);

			SyncedSpawn();
			_justSpawned = true;
		}

		return true;
	}
}