using PathOfTerraria.Content.Items.Gear;

namespace PathOfTerraria.Core.Systems.SkillSystem;

public abstract class Skill(int duration, int timer, int maxCooldown, int cooldown, int manaCost, GearType weaponType)
{
	public int Duration = duration;
	public int Timer = timer;

	public int MaxCooldown = maxCooldown;
	public int Cooldown = cooldown;

	public int ManaCost = manaCost;

	public GearType WeaponType = weaponType;

	public virtual string Name => GetType().Name;
	public virtual string Texture => $"{PathOfTerraria.ModName}/Assets/Skills/" + GetType().Name;

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
		return Timer <= 0 && player.CheckMana(ManaCost);
	}
}