using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Systems.Skills;
using PathOfTerraria.Common.Utilities;
using PathOfTerraria.Content.Skills.Melee;
using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Mechanics;

public abstract class Skill
{
	public int Duration;
	public int MaxCooldown;
	public int Cooldown;
	/// <summary> The default mana cost of this skill.<br/>See <see cref="TotalManaCost"/>. </summary>
	public int ManaCost;
	public ItemType WeaponType = ItemType.None;
	public byte Level = 1;

	/// <summary> The final mana cost of this skill affected by modifications. </summary>
	public int TotalManaCost
	{
		get
		{
			SkillBuff buff = this.GetPower();
			return (int)buff.ManaCost.ApplyTo(ManaCost);
		}
	}

	/// <summary> Attempts to get the skill tree associated with this skill. Returns null if none. </summary>
	public SkillTree Tree
	{
		get
		{
			if (SkillTree.TypeToSkillTree.TryGetValue(GetType(), out SkillTree value))
			{
				return value;
			}

			return null;
		}
	}

	public abstract int MaxLevel { get; }
	public int PassivePoints { get; set; } = 1;

	public virtual string Name => GetType().Name;
	public virtual string Texture => $"{PoTMod.ModName}/Assets/Skills/" + GetTextureName();

	private string GetTextureName()
	{
		if (Tree?.Specialization is not null)
		{
			return Tree.Specialization.Name;
		}

		return Name;
	}

	public virtual LocalizedText DisplayName => Language.GetText("Mods.PathOfTerraria." + GetLocalKey() + ".Name");
	public virtual LocalizedText Description => Language.GetText("Mods.PathOfTerraria." + GetLocalKey() + ".Description");

	private string GetLocalKey()
	{
		if (Tree?.Specialization is not null)
		{
			return "SkillSpecials." + Tree.Specialization.Name;
		}

		return "Skills." + Name;
	}

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
	public abstract void UseSkill(Player player, SkillBuff buff);

	/// <summary>
	/// If this skill should be able to be used. By default this is if the cooldown is over and the player has enough mana.
	/// </summary>
	/// <param name="player">The player trying to use the skill</param>
	/// <returns>If the skill can be used or not</returns>
	public virtual bool CanUseSkill(Player player)
	{
		return Cooldown <= 0 && player.CheckMana(ManaCost);
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
		MaxCooldown = tag.GetShort(nameof(MaxCooldown));
		Cooldown = tag.GetShort(nameof(Cooldown));
		ManaCost = tag.GetShort(nameof(ManaCost));
		WeaponType = (ItemType)tag.GetInt(nameof(WeaponType));
		Level = tag.GetByte(nameof(Level));

		Tree?.LoadData(this, tag);
	}

	public virtual void SaveData(TagCompound tag)
	{
		tag.Add(nameof(Duration), (short)Duration);
		tag.Add(nameof(MaxCooldown), (short)MaxCooldown);
		tag.Add(nameof(Cooldown), (short)Cooldown);
		tag.Add(nameof(ManaCost), (short)ManaCost);
		tag.Add(nameof(WeaponType), (int)WeaponType);
		tag.Add(nameof(Level), Level);

		Tree?.SaveData(this, tag);
	}
}