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
	NotEnoughLife,
	NeedsMelee,
	NeedsRanged,
	NeedsMagic,
	NeedsSummon,
	Other
}

/// <summary> Contains a reason for a skill to fail in any context. </summary>
/// <param name="reason"></param>
/// <param name="context"></param>
public readonly struct SkillFailure(SkillFailReason reason, string context = null,  params object[] formatArgs)
{
	/// <summary> Whether <see cref="Reason"/> is a weapons requirement. </summary>
	public bool WeaponRejected => Reason is SkillFailReason.NeedsMelee or SkillFailReason.NeedsRanged or SkillFailReason.NeedsMagic or SkillFailReason.NeedsSummon;
	public readonly LocalizedText Description => GetDescription();

	public readonly SkillFailReason Reason = reason;
	private readonly string _context = context;
	private readonly object[] _formatArgs = formatArgs;

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

		LocalizedText text = Language.GetText(path + (_context ?? Reason.ToString()));
		return (_formatArgs is { Length: > 0 }) ? text.WithFormatArgs(_formatArgs) : text;
	}
}

public enum SkillCost
{
	None = 0,
	ManaUse,
	HealthUse,

	/// <summary>
	/// Requires being an Aura skill.
	/// </summary>
	ManaDrainPerSecond,

	/// <inheritdoc cref="ManaDrainPerSecond"/>
	LifeDrainPerSecond,

	/// <inheritdoc cref="ManaDrainPerSecond"/>
	ManaReserve,

	/// <inheritdoc cref="ManaDrainPerSecond"/>
	HealthReserve
}

public readonly record struct SkillFunctionalityInfo(bool AuraSkill, bool ToggleAlwaysOn, SkillCost Cost);

public abstract partial class Skill : ILoadable
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

	/// <summary>
	/// Defines how the skill is used - if it's an Aura (toggleable) skill, if it's "always on" if/when toggled, and what cost it has for usage.
	/// </summary>
	public virtual SkillFunctionalityInfo Functionality => new(false, false, SkillCost.ManaUse);

	public bool AuraToggled => Functionality.AuraSkill && _toggled;

	public int Duration;
	public int MaxCooldown;
	public int Cooldown;

	/// <summary> 
	/// The default resource cost of this skill.<br/>
	/// See <see cref="TotalResourceCost"/>.<br/><br/>
	/// This is flat for the following <see cref="SkillCost"/>s:<br/>
	/// <see cref="SkillCost.ManaUse"/> - <see cref="SkillCost.HealthUse"/> - <see cref="SkillCost.ManaDrainPerSecond"/> - <see cref="SkillCost.LifeDrainPerSecond"/><br/><br/>
	/// And is a proportion (out of 100) for the following:<br/>
	/// <see cref="SkillCost.ManaReserve"/> - <see cref="SkillCost.HealthReserve"/>
	/// </summary>
	public int ResourceCost;
	
	public ItemType WeaponType = ItemType.None;
	public byte Level = 1;

	private Vector2 _size;
	private bool _toggled;

	public void Load(Mod mod)
	{
	}

	public void Unload()
	{
	}

	public Skill Clone()
	{
		return (Skill)MemberwiseClone();
	}

	/// <summary>
	/// The tags this skill applies to.
	/// </summary>
	public abstract SkillTags Tags();

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

	/// <summary>
	/// What this skill actually does.<br/>
	/// By default, consumes the appropriate based on <see cref="TotalResourceCost"/> and <see cref="Functionality"/>.Cost and applies <see cref="MaxCooldown"/>.<br/>
	/// For <see cref="AuraToggled"/>-able skills, this will also toggle the effect.
	/// This only runs on the local client.
	/// </summary>
	/// <param name="player">The player using the skill</param>
	public void UseSkill(Player player)
	{
		_toggled = !_toggled;

		PreUseSkill(player);

		int cooldown = TotalCooldown;
		ModifyCooldown(player, ref cooldown);

		if (Functionality.Cost is SkillCost.ManaUse or SkillCost.ManaDrainPerSecond)
		{
			player.CheckMana(TotalResourceCost, true);
			Cooldown = cooldown;
		}
		else if (Functionality.Cost is SkillCost.HealthUse or SkillCost.LifeDrainPerSecond)
		{
			player.statLife -= TotalResourceCost;
			Cooldown = cooldown;
		}
		
		// Health/ManaReserve is handled inSkillResourcingPlaying

		//player.MaxManaRegenDelay was way too slow for some reason compared to when using a mag weapon? Idk why. But 60 seems right
		player.manaRegenDelay = player.maxRegenDelay;
		InternalUseSkill(player);
	}

	/// <summary>
	/// Allows you to modify the cooldown the skill uses before it's set.
	/// </summary>
	protected virtual void ModifyCooldown(Player player, ref int cooldown)
	{
	}

	/// <summary>
	/// Runs before <see cref="InternalUseSkill(Player)"/> is ran. Useful for child classes that define pre-skill use behaviour, such as <see cref="SummonSkill"/>.
	/// </summary>
	protected virtual void PreUseSkill(Player player)
	{
	}

	/// <summary>
	/// Internal code ran by <see cref="UseSkill(Player)"/>. This avoids needing to reimplement either <c>base.UseSkill</c> or its respective functionality per use.
	/// </summary>
	protected virtual void InternalUseSkill(Player player)
	{
	}

	/// <summary>
	/// Ran every frame while this Aura (toggleable) skill is active. Also provided are two timers - one modifiable and another static.
	/// </summary>
	/// <param name="drainTimer">Per-player-per-skill timer that controls how often the resource for this skill is checked. This is reset every 60 ticks, and can be modified.</param>
	/// <param name="staticTimer">Per-player-per-skill timer for ease of use. This will increase forever, and is reset to 0 when the skill is inactive.</param>
	public virtual void ActiveUse(Player player, ref float drainTimer, float staticTimer)
	{
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

		if (!CheckResourceUsage(player, ref failReason))
		{
			return false;
		}

		return Cooldown <= 0;
	}

	public bool CheckResourceUsage(Player player, ref SkillFailure failReason)
	{
		if (Functionality.Cost is SkillCost.HealthReserve or SkillCost.ManaReserve)
		{
			// These are separate and do not have "on-use" checks
			return true;
		}

		if (Functionality.Cost is SkillCost.ManaUse or SkillCost.ManaDrainPerSecond && !player.CheckMana(TotalResourceCost))
		{
			failReason = new SkillFailure(SkillFailReason.NotEnoughMana);
			return false;
		}
		else if (Functionality.Cost is SkillCost.HealthUse or SkillCost.LifeDrainPerSecond && player.statLife < TotalResourceCost)
		{
			failReason = new SkillFailure(SkillFailReason.NotEnoughLife);
			return false;
		}

		return true;
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

		if (player.statManaMax2 < TotalResourceCost)
		{
			failReason = new SkillFailure(SkillFailReason.NotEnoughMana);
			return false;
		}

		return true;
	}

	public virtual void LoadData(TagCompound tag, Player loadingPlayer)
	{
		//Duration = tag.GetShort(nameof(Duration));
		//MaxCooldown = tag.GetShort(nameof(MaxCooldown));
		//ResourceCost = tag.GetShort(nameof(ResourceCost));
		//WeaponType = (ItemType)tag.GetInt(nameof(WeaponType));
		Level = tag.GetByte(nameof(Level));

		LevelTo(Level);

		Cooldown = tag.GetShort(nameof(Cooldown));
		Tree?.LoadData(this, tag, loadingPlayer);
	}

	public virtual void SaveData(TagCompound tag)
	{
		//tag.Add(nameof(Duration), (short)Duration);
		//tag.Add(nameof(MaxCooldown), (short)MaxCooldown);
		//tag.Add(nameof(ResourceCost), (short)ResourceCost);
		//tag.Add(nameof(WeaponType), (int)WeaponType);
		tag.Add(nameof(Cooldown), (short)Cooldown);
		tag.Add(nameof(Level), Level);

		Tree?.SaveData(this, tag);
	}

	public virtual void ModifyTooltips(List<NewHotbar.SkillTooltip> tooltips)
	{
	}
}