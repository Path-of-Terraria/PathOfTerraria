using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Systems.Skills;
using PathOfTerraria.Common.UI.Hotbar;
using PathOfTerraria.Common.Utilities;
using PathOfTerraria.Content.Skills.Melee;
using System.Collections.Generic;
using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Mechanics;

/// <summary>
/// Standardized reasons for a skill to fail, + other for all other use cases. Does not contain "OnCooldown" or similar, as that's handled seperately.
/// </summary>
public enum SkillFailReason
{
	NotEnoughMana,
	NeedsMelee,
	NeedsRanged,
	NeedsMagic,
	NeedsSummon,
	Other
}

/// <summary> Contains a reason for a skill to fail in any context. </summary>
/// <param name="reason"></param>
/// <param name="context"></param>
public readonly struct SkillFailure(SkillFailReason reason, string context = null)
{
	/// <summary> Whether <see cref="Reason"/> is a weapons requirement. </summary>
	public bool WeaponRejected => Reason is SkillFailReason.NeedsMelee or SkillFailReason.NeedsRanged or SkillFailReason.NeedsMagic or SkillFailReason.NeedsSummon;
	public readonly LocalizedText Description => GetDescription();

	public readonly SkillFailReason Reason = reason;
	private readonly string _context = context;

	private readonly LocalizedText GetDescription()
	{
		const string path = $"Mods.{PoTMod.ModName}.SkillFailReasons.";
		string value = Reason switch
		{ 
			SkillFailReason.NeedsMelee => ItemType.Melee.ToString(),
			SkillFailReason.NeedsRanged => ItemType.Ranged.ToString(),
			SkillFailReason.NeedsMagic => ItemType.Magic.ToString(),
			SkillFailReason.NeedsSummon => ItemType.Summoner.ToString(),
			_ => null
		};

		if (value != null && _context == null)
		{
			return Language.GetText(path + "NeedsWeapon").WithFormatArgs(value);
		}

		return Language.GetText(path + (_context ?? Reason.ToString()));
	}
}

public abstract partial class Skill
{
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

	public virtual LocalizedText DisplayName => Language.GetText("Mods.PathOfTerraria." + GetLocalKey() + ".Name");
	public virtual LocalizedText Description => Language.GetText("Mods.PathOfTerraria." + GetLocalKey() + ".Description");

	public int Duration;
	public int MaxCooldown;
	public int Cooldown;
	
	/// <summary> The default mana cost of this skill.<br/>See <see cref="TotalManaCost"/>. </summary>
	public int ManaCost;
	
	public ItemType WeaponType = ItemType.None;
	public byte Level = 1;

	private Vector2 _size;

	private string GetTextureName()
	{
		return Tree?.Specialization?.Name ?? Name;
	}

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

	/// <summary> What this skill actually does.<br/>Consumes mana based on <see cref="TotalManaCost"/> and applies <see cref="MaxCooldown"/> by default. </summary>
	/// <param name="player">The player using the skill</param>
	public virtual void UseSkill(Player player)
	{
		player.CheckMana(TotalManaCost, true);
		Cooldown = TotalCooldown;
	}

	/// <summary>
	/// If this skill should be able to be used. By default this is if the cooldown is over and the player has enough mana.
	/// </summary>
	/// <param name="player">The player trying to use the skill</param>
	/// <returns>If the skill can be used or not</returns>
	/// <param name="failReason">The reason this skill failed to be used. Used for the icons over the UI.</param>
	/// <param name="justChecking">If this is being called to "just check" - i.e., don't do any functionality aside from true/false and <paramref name="failReason"/>.</param>
	public virtual bool CanUseSkill(Player player, ref SkillFailure failReason, bool justChecking = true)
	{
		if (!CanEquipSkill(player, ref failReason))
		{
			return false;
		}

		if (!player.CheckMana(TotalManaCost))
		{
			failReason = new SkillFailure(SkillFailReason.NotEnoughMana);
			return false;
		}

		return Cooldown <= 0;
	}

	/// <summary>
	/// Whether the player can have the current skill equipped or not.<br/>
	/// Returns true by default.<br/>
	/// If you need to check if a skill can be equipped, use <see cref="CanEquipSkill(Player, ref SkillFailure)"/> instead.
	/// </summary>
	/// <param name="player">The player who is trying to equip the skill, or has the skill equipped.</param>
	/// <returns>If the player can have this skill equipped.</returns>
	protected virtual bool ProtectedCanEquip(Player player, ref SkillFailure failReason)
	{
		return true;
	}

	/// <summary>
	/// Whether the skill can be equipped. Runs <see cref="ProtectedCanEquip(Player, ref SkillFailure)"/> and checks if the player has enough mana to use the skill.
	/// </summary>
	/// <param name="player">The player who is trying to equip the skill, or has the skill equipped.</param>
	/// <returns>If the player can have this skill equipped.</returns>
	public bool CanEquipSkill(Player player, ref SkillFailure failReason)
	{
		if (!ProtectedCanEquip(player, ref failReason))
		{
			return false;
		}

		if (player.statManaMax2 < TotalManaCost)
		{
			failReason = new SkillFailure(SkillFailReason.NotEnoughMana);
			return false;
		}

		return true;
	}

	public virtual void LoadData(TagCompound tag, Player loadingPlayer)
	{
		Duration = tag.GetShort(nameof(Duration));
		MaxCooldown = tag.GetShort(nameof(MaxCooldown));
		Cooldown = tag.GetShort(nameof(Cooldown));
		ManaCost = tag.GetShort(nameof(ManaCost));
		WeaponType = (ItemType)tag.GetInt(nameof(WeaponType));
		Level = tag.GetByte(nameof(Level));

		Tree?.LoadData(this, tag, loadingPlayer);
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

	public virtual void ModifyTooltips(List<NewHotbar.SkillTooltip> tooltips)
	{
	}
}