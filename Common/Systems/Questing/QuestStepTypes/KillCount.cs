using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems.Questing.QuestStepTypes;

/// <summary>
/// Counts up kills that fit under the <paramref name="includes"/> condition.
/// </summary>
/// <param name="includes">Whether a kill counts.</param>
/// <param name="count">How many kills to track.</param>
/// <param name="displayText">The text to display.</param>
internal class KillCount(Func<NPC, bool> includes, int count, LocalizedText displayText) : QuestStep
{
	public KillCount(int npcId, int count, LocalizedText displayText) : this(
		(NPC npcKilled) => npcKilled.netID == npcId, count, displayText)
	{
	}

	private readonly LocalizedText Display = displayText;
	private readonly int MaxRemaining = count;
	private readonly Func<NPC, bool> countsAsKill = includes;

	private int _remaining = count;

	public override string DisplayString()
	{
		return Display.WithFormatArgs(MaxRemaining - _remaining + "/" + MaxRemaining).Value;
	}

	public override void OnKillNPC(Player player, NPC target, NPC.HitInfo hitInfo, int damageDone)
	{
		if (player.whoAmI == Main.myPlayer && countsAsKill(target))
		{
			_remaining--;
		}
	}

	public override bool Track(Player player)
	{
		return _remaining <= 0;
	}

	public override void Save(TagCompound tag)
	{
		tag.Add("remaining", _remaining);
	}

	public override void Load(TagCompound tag)
	{
		_remaining = tag.GetInt("remaining");
	}
}