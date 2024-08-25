using System.Collections.Generic;
using System.Linq;
using PathOfTerraria.Common.Mechanics;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems.ModPlayers;

public class SkillPassivePlayer : ModPlayer
{
	// A dictionary to store the acquired passive points for each skill.
	public Dictionary<Skill, int> AcquiredPassivePoints = new();

	// A dictionary to store the allocated passive points for each skill.
	public Dictionary<Skill, int> AllocatedPassivePoints = new();

	// A dictionary to store the actual allocated skill passives for each skill.
	public Dictionary<Skill, List<SkillPassive>> AllocatedPassives = new();

	public override void Initialize()
	{
		AcquiredPassivePoints.Clear();
		AllocatedPassivePoints.Clear();
		AllocatedPassives.Clear();
	}

	public void AwardPassivePoint(Skill skill)
	{
		AcquiredPassivePoints.TryAdd(skill, 0);

		AcquiredPassivePoints[skill]++;
	}

	public bool AllocatePassivePoint(Skill skill, SkillPassive passive)
	{
		if (AllocatedPassivePoints.ContainsKey(skill) && AllocatedPassivePoints[skill] < AcquiredPassivePoints[skill])
		{
			if (!AllocatedPassives.ContainsKey(skill))
			{
				AllocatedPassives[skill] = new List<SkillPassive>();
			}

			if (AllocatedPassives[skill].Contains(passive))
			{
				return false; // Already allocated this passive
			}

			AllocatedPassives[skill].Add(passive);
			AllocatedPassivePoints[skill]++;
			return true;
		}

		return false; // Not enough points or already allocated
	}

	public void DeallocatePassivePoint(Skill skill, SkillPassive passive)
	{
		if (AllocatedPassives.ContainsKey(skill) && AllocatedPassives[skill].Contains(passive))
		{
			AllocatedPassives[skill].Remove(passive);
			AllocatedPassivePoints[skill]--;
		}
	}

	public int GetAvailablePoints(Skill skill)
	{
		if (AcquiredPassivePoints.TryGetValue(skill, out int point))
		{
			return point - AllocatedPassivePoints.GetValueOrDefault(skill, 0);
		}

		return 0;
	}

	public override void SaveData(TagCompound tag)
	{
		var acquiredPointKeys = AcquiredPassivePoints.Keys.Select(skill => skill.Name).ToList();
		var acquiredPointValues = AcquiredPassivePoints.Values.ToList();
		tag["AcquiredPointKeys"] = acquiredPointKeys;
		tag["AcquiredPointValues"] = acquiredPointValues;

		var allocatedPointKeys = AllocatedPassivePoints.Keys.Select(skill => skill.Name).ToList();
		var allocatedPointValues = AllocatedPassivePoints.Values.ToList();
		tag["AllocatedPointKeys"] = allocatedPointKeys;
		tag["AllocatedPointValues"] = allocatedPointValues;

		var allocatedPassiveKeys = AllocatedPassives.Keys.Select(skill => skill.Name).ToList();
		var allocatedPassiveValues = AllocatedPassives.Values.Select(
			passives => passives.Select(p => p.ReferenceId).ToList()
		).ToList();
		tag["AllocatedPassiveKeys"] = allocatedPassiveKeys;
		tag["AllocatedPassiveValues"] = allocatedPassiveValues;
	}

	public override void LoadData(TagCompound tag)
	{
		AcquiredPassivePoints.Clear();
		AllocatedPassivePoints.Clear();
		AllocatedPassives.Clear();

		if (tag.ContainsKey("AcquiredPointKeys") && tag.ContainsKey("AcquiredPointValues"))
		{
			List<string> acquiredPointKeys = tag.Get<List<string>>("AcquiredPointKeys");
			List<int> acquiredPointValues = tag.Get<List<int>>("AcquiredPointValues");
			for (int i = 0; i < acquiredPointKeys.Count; i++)
			{
				var skill = Skill.GetAndPrepareSkill(Type.GetType(acquiredPointKeys[i]));
				AcquiredPassivePoints[skill] = acquiredPointValues[i];
			}
		}

		if (tag.ContainsKey("AllocatedPointKeys") && tag.ContainsKey("AllocatedPointValues"))
		{
			List<string> allocatedPointKeys = tag.Get<List<string>>("AllocatedPointKeys");
			List<int> allocatedPointValues = tag.Get<List<int>>("AllocatedPointValues");
			for (int i = 0; i < allocatedPointKeys.Count; i++)
			{
				var skill = Skill.GetAndPrepareSkill(Type.GetType(allocatedPointKeys[i]));
				AllocatedPassivePoints[skill] = allocatedPointValues[i];
			}
		}

		if (tag.ContainsKey("AllocatedPassiveKeys") && tag.ContainsKey("AllocatedPassiveValues"))
		{
			List<string> allocatedPassiveKeys = tag.Get<List<string>>("AllocatedPassiveKeys");
			List<List<int>> allocatedPassiveValues = tag.Get<List<List<int>>>("AllocatedPassiveValues");
			for (int i = 0; i < allocatedPassiveKeys.Count; i++)
			{
				var skill = Skill.GetAndPrepareSkill(Type.GetType(allocatedPassiveKeys[i]));
				AllocatedPassives[skill] = allocatedPassiveValues[i].Select(
					id => skill.Passives.FirstOrDefault(p => p.ReferenceId == id)
				).ToList();
			}
		}
	}
}