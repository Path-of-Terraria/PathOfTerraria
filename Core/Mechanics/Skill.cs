﻿using Terraria.ModLoader.IO;

namespace PathOfTerraria.Core.Mechanics;

public abstract class Skill
{
	public int Duration;
	public int Timer;
	public int MaxCooldown;
	public int Cooldown;
	public int ManaCost;
	public ItemType WeaponType = ItemType.None;
	public byte Level = 1;

	public abstract int MaxLevel { get; }

	public virtual string Name => GetType().Name;
	public virtual string Texture => $"{PathOfTerraria.ModName}/Assets/Skills/" + GetType().Name;

	/// <summary>
	/// Creates a default instance of the given <see cref="Skill"/> with 0 for all ctor parameters, aside from 1 for <see cref="Level"/>.
	/// </summary>
	/// <param name="type">The type of skill to generate.</param>
	/// <returns>The newly generated skill.</returns>
	public static Skill GetAndPrepareSkill(Type type)
	{
		var skill = Activator.CreateInstance(type) as Skill;
		skill.LevelTo(1);
		return skill;
	}

	/// <inheritdoc cref="GetAndPrepareSkill(Type)"/>
	public static Skill GetAndPrepareSkill<T>()
	{
		return GetAndPrepareSkill(typeof(T));
	}

	public abstract void LevelTo(byte level);

	protected void IncreaseLevel()
	{
		LevelTo((byte)(Level + 1));
	}

	/// <summary>
	/// What this skill actually does
	/// </summary>
	/// <param name="player">The player using the skill</param>
	public abstract void UseSkill(Player player);

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
		Duration = tag.GetShort(nameof(Duration));
		Timer = tag.GetShort(nameof(Timer));
		MaxCooldown = tag.GetShort(nameof(MaxCooldown));
		Cooldown = tag.GetShort(nameof(Cooldown));
		ManaCost = tag.GetShort(nameof(ManaCost));
		WeaponType = (ItemType)tag.GetInt(nameof(WeaponType));
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