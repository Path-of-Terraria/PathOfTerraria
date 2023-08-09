using FunnyExperience.Content.Items.Gear;

namespace FunnyExperience.Core.Systems.SkillSystem
{
	internal abstract class Skill
	{
		public int duration;
		public int timer;

		public int maxCooldown;
		public int cooldown;

		public int manaCost;

		public GearType weaponType;

		public virtual string Name => GetType().Name;
		public virtual string Texture => $"{FunnyExperience.ModName}/Assets/Skills/" + GetType().Name;

		/// <summary>
		/// What this skill actually does
		/// </summary>
		/// <param name="player">The player using the skill</param>
		public abstract void UseSkill(Player player);

		/// <summary>
		/// Gets the description of this skill
		/// </summary>
		/// <param name="player">The player that has the skill</param>
		/// <returns>The skill's description</returns>
		public abstract string GetDescription(Player player);

		/// <summary>
		/// If this skill should be able to be used. By default this is if the cooldown is over and the player has enough mana.
		/// </summary>
		/// <param name="player">The player trying to use the skill</param>
		/// <returns>If the skill can be used or not</returns>
		public virtual bool CanUseSkill(Player player)
		{
			return cooldown <= 0 && player.CheckMana(manaCost);
		}
	}
}
