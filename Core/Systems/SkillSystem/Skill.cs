using PathOfTerraria.Content.Items.Gear;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Core.Systems.SkillSystem;

public abstract class Skill(int duration, int timer, int maxCooldown, int cooldown, int manaCost, GearType weaponType, byte level = 1)
{
	public int Duration = duration;
	public int Timer = timer;

	public int MaxCooldown = maxCooldown;
	public int Cooldown = cooldown;

	public int ManaCost = manaCost;

	public GearType WeaponType = weaponType;
	public byte Level = level;

	public virtual string Name => GetType().Name;
	public virtual string Texture => $"{PathOfTerraria.ModName}/Assets/Skills/" + GetType().Name;

	/// <summary>
	/// Constructor that should be used in Activator.CreateInstance only. Otherwise, use <c>new Skill(int, int, int, int, int, GearType, byte)</c>.
	/// </summary>
	public Skill() : this(0, 0, 0, 0, 0, 0, 0)
	{
	}

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

	public virtual void LoadData(TagCompound tag)
	{
		// Skill(int duration, int timer, int maxCooldown, int cooldown, int manaCost, GearType weaponType, int level = -1)
		Duration = tag.GetShort(nameof(Duration));
		Timer = tag.GetShort(nameof(Timer));
		MaxCooldown = tag.GetShort(nameof(MaxCooldown));
		Cooldown = tag.GetShort(nameof(Cooldown));
		ManaCost = tag.GetShort(nameof(ManaCost));
		WeaponType = (GearType)tag.GetInt(nameof(WeaponType));
		Level = tag.GetByte(nameof(Level));
	}

	public virtual void SaveData(TagCompound tag)
	{
		tag.Add(nameof(Duration), (short)Duration);
		tag.Add(nameof(Timer), (short)Timer);
		tag.Add(nameof(MaxCooldown), (short)MaxCooldown);
		tag.Add(nameof(Cooldown), (short)Cooldown);
		tag.Add(nameof(ManaCost), (short)ManaCost);
		tag.Add(nameof(WeaponType), (int)WeaponType);
		tag.Add(nameof(Level), Level);
	}
}