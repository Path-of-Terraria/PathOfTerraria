﻿using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems.Questing;

public abstract class QuestStep
{
	public virtual int LineCount => 1;
	public virtual bool NoUI => false;

	public bool IsDone { get; internal set; }

	/// <summary>
	/// Called every frame on the player. This should be used to complete steps, check conditions, so on and so on.
	/// </summary>
	/// <param name="player">The player that is using the quest.</param>
	/// <returns>Whether the step should complete or not.</returns>
	public abstract bool Track(Player player);

	/// <summary>
	/// Used to display in the Quest book. Should use a backing <see cref="LocalizedText"/> for proper localization.
	/// </summary>
	/// <returns>The display string.</returns>
	public virtual string DisplayString() { return ""; }

	public virtual void Save(TagCompound tag) { }
	public virtual void Load(TagCompound tag) { }
	public virtual void OnKillNPC(Player player, NPC target, NPC.HitInfo hitInfo, int damageDone) { }

	public virtual void OnComplete()
	{
		IsDone = true;
	}

	public override string ToString()
	{
		return DisplayString();
	}
}
