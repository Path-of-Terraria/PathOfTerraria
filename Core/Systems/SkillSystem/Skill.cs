using FunnyExperience.Content.Items.Gear;

namespace FunnyExperience.Core.Systems.SkillSystem
{
	internal abstract class Skill
	{
		public int Duration;
		public int Timer;

		public int MaxCooldown;
		private readonly int _cooldown;

		private readonly int _manaCost;

		public GearType WeaponType;

		protected Skill(int duration, int timer, int maxCooldown, int cooldown, int manaCost, GearType weaponType)
		{
			Duration = duration;
			Timer = timer;
			MaxCooldown = maxCooldown;
			_cooldown = cooldown;
			_manaCost = manaCost;
			WeaponType = weaponType;
		}

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
			return _cooldown <= 0 && player.CheckMana(_manaCost);
		}
	}
}
