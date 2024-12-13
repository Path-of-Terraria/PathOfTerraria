using System.Collections.Generic;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Utilities;
using PathOfTerraria.Content.SkillPassives;
using PathOfTerraria.Content.Skills.Melee;
using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Mechanics;

public class SkillPassiveEdge(SkillPassive start, SkillPassive end)
{
	public readonly SkillPassive Start = start;
	public readonly SkillPassive End = end;

	public bool Contains(SkillPassive p)
	{
		return p == Start || p == End;
	}

	/// <summary>
	/// Assuming that p is either start or end - Contains returned true.
	/// </summary>
	public SkillPassive Other(SkillPassive p)
	{
		return p == Start ? End : Start;
	}
}

public abstract class Skill
{
	public int Duration;
	public int Timer;
	public int MaxCooldown;
	public int Cooldown;
	public int ManaCost;
	public ItemType WeaponType = ItemType.None;
	public byte Level = 1;
	public abstract List<SkillPassive> Passives { get; }
	public List<SkillPassive> ActiveNodes = [];
	public List<SkillPassiveEdge> Edges = [];

	public abstract int MaxLevel { get; }
	public int PassivePoints { get; set; } = 1;

	public virtual string Name => GetType().Name;
	public virtual string Texture => $"{PoTMod.ModName}/Assets/Skills/" + GetType().Name;

	public virtual LocalizedText DisplayName => Language.GetText("Mods.PathOfTerraria.Skills." + Name + ".Name");
	public virtual LocalizedText Description => Language.GetText("Mods.PathOfTerraria.Skills." + Name + ".Description");

	/// <summary>
	/// Creates a default instance of the given <see cref="Skill"/> at <see cref="Level"/> 1.
	/// </summary>
	/// <param name="type">The type of skill to generate.</param>
	/// <returns>The newly generated skill.</returns>
	public static Skill GetAndPrepareSkill(Type type)
	{
		if (type is null)
		{
			ModContent.GetInstance<PoTMod>().Logger.Error($"Loading Skill not found. Defaulting to Berserk.");
			return new Berserk();
		}

		var skill = Activator.CreateInstance(type) as Skill;

		if (skill is null)
		{
			ModContent.GetInstance<PoTMod>().Logger.Error($"Skill of type {type} not found. Defaulting to Berserk.");
			return new Berserk();
		}

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
	
	public override bool Equals(object obj)
	{
		if (obj is Skill otherSkill)
		{
			return Name == otherSkill.Name;
		}

		return false;
	}
	
	public override int GetHashCode()
	{
		return Name.GetHashCode(); // Again, you can use other properties here if needed
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

	/// <summary>
	/// Whether the player can have the current skill equipped or not.<br/>
	/// This will remove the skill automatically if they have it equipped but the condition is false.
	/// Returns true by default.
	/// </summary>
	/// <param name="player">The player who is trying to equip or has the skill equipped.</param>
	/// <returns>If the player can have this skill equipped.</returns>
	public virtual bool CanEquipSkill(Player player)
	{
		return true;
	}
	
	private Vector2 _size;
	
	public Vector2 Size
	{
		get
		{
			if (_size != Vector2.Zero)
			{
				return _size;
			}

			_size = StringUtils.GetSizeOfTexture($"Assets/Skills/{GetType().Name}") ?? new Vector2();
				
			return _size;
		}
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
	
	public void CreateTree()
	{
		Edges = [];
		ActiveNodes =
		[
			new SkillPassiveAnchor(this)
		];

		foreach (SkillPassive passive in Passives)
		{
			if (passive.Connections == null)
			{
				continue;
			}
			
			foreach (SkillPassive connection in passive.Connections)
			{
				Edges.Add(new SkillPassiveEdge(passive, connection));
			}
			
			if (passive.ReferenceId != 0) //Not anchor
			{
				PassivePoints -= passive.Level;
			}
			
			ActiveNodes.Add(passive);
		}
	}
}