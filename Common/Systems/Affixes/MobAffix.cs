using PathOfTerraria.Common.Enums;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems.Affixes;

internal abstract class MobAffix : Affix
{
	public virtual ItemRarity MinimumRarity => ItemRarity.Magic;
	public virtual bool Allowed => true;
	
	// would prefer ProgressionLock, but then you'd have to write !Main.moonlordDowned
	// but its mainly for progression
	public virtual float DropQuantityFlat => 0;
	public virtual float DropQuantityMultiplier => 1f;
	public virtual float DropRarityFlat => 0;
	public virtual float DropRarityMultiplier => 1f;

	/// <summary>
	/// after the rarity buff has been applied
	/// </summary>
	public virtual void PostRarity(NPC npc) { }

	/// <summary>
	/// before the rarity buff has been applied
	/// </summary>
	public virtual void PreRarity(NPC npc) { }

	public virtual bool PreAi(NPC npc) { return true; }
	public virtual void Ai(NPC npc) { }
	public virtual void PostAi(NPC npc) { }
	public virtual bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) { return true; }
	public virtual void OnKill(NPC npc) { }
	public virtual bool PreKill(NPC npc) { return true; }

	/// <summary>
	/// Generates an affix from a tag, used on load to re-populate affixes
	/// </summary>
	/// <param name="tag"></param>
	/// <returns></returns>
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