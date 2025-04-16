﻿using PathOfTerraria.Common.Systems.Skills;
using Terraria.Localization;

namespace PathOfTerraria.Common.Mechanics;

public abstract class SkillPassive(SkillTree tree) : Allocatable(tree)
{
	public override string TexturePath => $"{PoTMod.ModName}/Assets/SkillPassives/" + Name;
	public override string Tooltip => Language.GetTextValue("Mods.PathOfTerraria.SkillPassives." + Name + ".Description");

	public int Level;
	public int MaxLevel;

	public override void Draw(SpriteBatch spriteBatch, Vector2 position)
	{
		Texture2D texture = Texture.Value;
		spriteBatch.Draw(texture, position - texture.Size() / 2, Color.White);
	}

	/// <summary> The effects of this skill passive. </summary>
	public virtual void PassiveEffects() { }

	public override void OnAllocate(Player player)
	{
		Level++;
	}

	public override void OnDeallocate(Player player)
	{
		Level--;
	}

	/*public Skill Skill { get; set; } = null;
	public virtual List<SkillPassive> Connections { get; set; }
	public abstract int ReferenceId { get; }
	public abstract Vector2 TreePos { get; }
	public abstract int MaxLevel { get; }

	public virtual string Name => GetType().Name;
	public virtual string Texture => $"{PoTMod.ModName}/Assets/SkillPassives/" + Name;
	
	// New constructor

	public virtual string DisplayName => Language.GetTextValue("Mods.PathOfTerraria.SkillPassives." + Name + ".Name");
	public virtual string Description => Language.GetTextValue("Mods.PathOfTerraria.SkillPassives." + Name + ".Description");
	
	/// <summary>
	/// Tooltip to be used in ALL display situations. This is automatically populated by <see cref="Language.GetOrRegister(string, Func{string})"/>.
	/// </summary>
	public virtual string DisplayTooltip => Language.GetTextValue("Mods.PathOfTerraria.SkillPassives." + Name + ".Tooltip");

	public Vector2 Size
	{
		get
		{
			if (_size != Vector2.Zero)
			{
				return _size;
			}

			_size = StringUtils.GetSizeOfTexture($"Assets/SkillPassives/{Name}") ?? StringUtils.GetSizeOfTexture("Assets/UI/PassiveFrameSmall") ?? new Vector2();

			return _size;
		}
	}

	public int Level;

	private Vector2 _size;

	public SkillPassive(Skill skill)
	{
		Skill = skill;

		// Stops a StackOverflow since SkillPassiveAnchor is a child of SkillPassive, calls base()
		if (this is not SkillPassiveAnchor)
		{
			Connections = [new SkillPassiveAnchor(skill)];
		}
	}

	public void Draw(SpriteBatch spriteBatch, Vector2 center)
	{
		Texture2D tex = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/PassiveFrameSmall").Value;

		if (ModContent.HasAsset($"{PoTMod.ModName}/Assets/SkillPassives/" + Name))
		{
			tex = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/SkillPassives/" + Name).Value;
		}

		Color color = Color.Gray;

		if (CanAllocate())
		{
			color = Color.Lerp(Color.Gray, Color.White, (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.5f + 0.5f);
		}

		if (Level > 0)
		{
			color = Color.White;
		}

		spriteBatch.Draw(tex, center, null, color, 0, Size / 2f, 1, 0, 0);

		if (MaxLevel > 1)
		{
			Utils.DrawBorderString(spriteBatch, $"{Level}/{MaxLevel}", center + Size / 2f, color, 1, 0.5f, 0.5f);
		}
	}

	public abstract void LevelTo(byte level);

	protected void IncreaseLevel()
	{
		LevelTo((byte)(Level + 1));
	}

	public virtual void LoadData(TagCompound tag)
	{
		Level = tag.GetByte(nameof(Level));
	}

	public virtual void SaveData(TagCompound tag)
	{
		tag.Add(nameof(Level), Level);
	}

	public bool HasAllocated()
	{
		return Level > 0;
	}
	
	/// <summary>
	/// If this passive is able to be allocated or not
	/// </summary>
	/// <returns></returns>
	public bool CanAllocate()
	{
		bool baseCheck = Name == "Anchor" ||
			Level < MaxLevel &&
			Skill.Edges.Any(e => e.Contains(this) && e.Other(this).Level > 0);

		return !AnyBlockers(this) && baseCheck && InternalCanAllocate();
	}

	internal static bool AnyBlockers(SkillPassive skill)
	{
		if (skill is SkillPassiveAnchor)
		{
			return false;
		}

		foreach (KeyValuePair<string, SkillPassiveBlocker> blocker in SkillPassiveBlocker.LoadedBlockers)
		{
			if (blocker.Value.BlockAllocation(skill))
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Additional conditions for if the skill passive can be allocated.
	/// </summary>
	/// <returns>True if the passive can be allocated.</returns>
	protected virtual bool InternalCanAllocate()
	{
		return true;
	}

	/// <summary>
	/// Called when the passive is allocated.
	/// </summary>
	public virtual void OnAllocate()
	{
	}

	/// <summary>
	/// If this passive can be refunded or not
	/// </summary>
	/// <returns></returns>
	public virtual bool CanDeallocate()
	{
		return ReferenceId != 0;

		//TOOD
		//PassiveTreePlayer passiveTreeSystem = player.GetModPlayer<PassiveTreePlayer>();

		//return Level > 0 && (Level > 1 || passiveTreeSystem.FullyLinkedWithout(this));
	}*/
}