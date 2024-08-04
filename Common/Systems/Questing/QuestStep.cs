using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems.Questing;

public abstract class QuestStep
{
	public virtual void Track(Player player, Action onCompletion) { }
	public virtual void UnTrack() { }
	public virtual string QuestString() { return ""; }
	public virtual string QuestCompleteString() { return "Step completed"; }
	public virtual void Save(TagCompound tag) { }
	public virtual void Load(TagCompound tag) { }
}
