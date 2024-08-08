using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems.Questing;

public abstract class QuestStep
{
	public bool IsDone { get; protected set; }

	/// <summary>
	/// Called every frame on the player. This should be used to complete steps, check conditions, so on and so on.
	/// </summary>
	/// <param name="player">The player that is using the quest.</param>
	/// <returns>Whether the step should complete or not.</returns>
	public abstract bool Track(Player player);

	public virtual string QuestString() { return ""; }
	public virtual string QuestCompleteString() { return "Step completed"; }
	public virtual void Save(TagCompound tag) { }
	public virtual void Load(TagCompound tag) { }

	public virtual void OnComplete()
	{
		IsDone = true;
	}
}
