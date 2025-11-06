using PathOfTerraria.Common.Mechanics;

namespace PathOfTerraria.Common.Systems.Skills;

internal class SkillHooks
{
	/// <summary>
	/// Allows a ModPlayer to run code when any skill is used.
	/// </summary>
	public interface IOnUseSkillPlayer
	{
		void OnUseSkill(Skill skill);
	}
}
