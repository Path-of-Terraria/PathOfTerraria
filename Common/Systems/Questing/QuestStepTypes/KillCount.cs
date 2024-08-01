using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems.Questing.QuestStepTypes;

internal class KillCount(Func<NPC, bool> includes, int count, Func<string, string> displayText) : QuestStep
{
	public KillCount(int npcId, int count, Func<string, string> displayText) : this(
		(NPC npcKilled) => npcKilled.type == npcId, count, displayText)
	{
	}

	private int _remaining = count;
	private OnKillByDelegate tracker;

	public override string QuestString()
	{
		return displayText(count - _remaining + "/" + count);
	}

	public override string QuestCompleteString()
	{
		return displayText(count + "/" + count);
	}

	public override void Track(Player player, Action onCompletion)
	{
		tracker = (NPC n, Player p) =>
		{
			if (p == player && includes(n))
			{
				_remaining--;
			}

			if (_remaining <= 0)
			{
				onCompletion();
			}
		};

		PathOfTerrariaNpcEvents.OnKillByEvent += tracker;
	}

	public override void UnTrack()
	{
		PathOfTerrariaNpcEvents.OnKillByEvent -= tracker;
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