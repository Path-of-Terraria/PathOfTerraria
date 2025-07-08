using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Systems.MobSystem;
using ReLogic.Content;
using System.Collections.Generic;
using System.IO;
using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems.Affixes;

internal abstract class MobAffix : Affix
{
	public static readonly Dictionary<string, Asset<Texture2D>> MobAffixIconsByAffixName = [];

	public Asset<Texture2D> Icon => MobAffixIconsByAffixName[GetType().AssemblyQualifiedName];

	public virtual ItemRarity MinimumRarity => ItemRarity.Magic;

	/// <summary>
	/// Texture path that points to the icon that shows over an NPC.
	/// </summary>
	public virtual string TexturePath => Mod.Name + "/Assets/AffixIcons/" + GetType().Name;

	/// <summary>
	/// Text for the affix's prefix, such as the Strong in "Strong Blue Slime".
	/// </summary>
	public virtual LocalizedText Prefix => this.GetLocalization("Prefix");

	// would prefer ProgressionLock, but then you'd have to write !Main.moonlordDowned
	// but its mainly for progression
	public virtual float DropQuantityFlat => 0;
	public virtual float DropQuantityMultiplier => 1f;
	public virtual float DropRarityFlat => 0;
	public virtual float DropRarityMultiplier => 1f;

	/// <summary>
	/// Whether this affix can apply to the given NPC. Returns true by default.
	/// </summary>
	/// <param name="npc">The NPC this affix is being added to.</param>
	/// <returns>If the affix can be applied.</returns>
	public virtual bool CanApplyTo(NPC npc)
	{
		return true;
	}

	/// <summary>
	/// Runs after the rarity buff has been applied in <see cref="ArpgNPC.ApplyRarity(NPC, bool)"/>, a method called in SetDefaults. 
	/// This can be used to set values based on the NPC's post-rarity values, such as damage.
	/// </summary>
	public virtual void PostRarity(NPC npc) { }

	/// <summary>
	/// Runs after the rarity buff has been applied in <see cref="ArpgNPC.ApplyRarity(NPC, bool)"/>, a method called in SetDefaults. 
	/// This can be used to set values based on the NPC's pre-rarity values, such as damage.
	/// </summary>
	public virtual void PreRarity(NPC npc) { }

	public virtual bool PreAI(NPC npc) { return true; }
	public virtual void AI(NPC npc) { }
	public virtual void PostAI(NPC npc) { }
	public virtual bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) { return true; }
	public virtual void OnKill(NPC npc) { }
	public virtual bool PreKill(NPC npc) { return true; }

	/// <summary>
	/// Allows you to generate and, if necessary, store localized values. By default, creates the Prefix localization line.
	/// </summary>
	internal override void CreateLocalization()
	{
		// Populate prefix, don't store
		this.GetLocalization("Prefix");
	}

	public override void NetSend(BinaryWriter writer)
	{
		writer.Write(AffixHandler.IndexFromMobAffix(this));

		writer.Write(Value);
		writer.Write(MaxValue);
		writer.Write(MinValue);
	}

	/// <summary>
	/// Generates an affix from a tag, used on load to re-populate affixes.
	/// </summary>
	public static MobAffix FromTag(TagCompound tag)
	{
		var affix = (MobAffix)Activator.CreateInstance(typeof(MobAffix).Assembly.GetType(tag.GetString("type")));

		if (affix is null)
		{
			PoTMod.Instance.Logger.Error($"Could not load affix {tag.GetString("type")}, was it removed?");
			return null;
		}

		affix.Load(tag);
		return affix;
	}
}