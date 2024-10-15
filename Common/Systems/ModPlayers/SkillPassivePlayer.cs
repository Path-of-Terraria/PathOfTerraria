using System.Collections.Generic;
using System.Linq;
using PathOfTerraria.Common.Mechanics;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems.ModPlayers;

public class SkillPassivePlayer : ModPlayer
{
	// A dictionary to store the acquired passive points for each skill.
	public Dictionary<Skill, int> AcquiredPassivePoints = [];

	// A dictionary to store the allocated passive points for each skill.
	public Dictionary<Skill, int> AllocatedPassivePoints = [];

	// A dictionary to store the actual allocated skill passives for each skill.
	public Dictionary<Skill, Dictionary<string, SkillPassive>> AllocatedPassives = [];

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

	public bool AllocatePassivePoint(Skill skill, SkillPassive passive, bool fromLoad = false)
	{
		if (passive.Level > passive.MaxLevel)
		{
			return false; // Passive is not leveled or already maxed
		}
		
		AllocatedPassivePoints.TryAdd(skill, 0);
		AcquiredPassivePoints.TryAdd(skill, 0);
		
		if (!fromLoad && AllocatedPassivePoints[skill] >= AcquiredPassivePoints[skill])
		{
			return false; // Not enough points or already allocated
		}

		if (!AllocatedPassives.ContainsKey(skill))
		{
			AllocatedPassives[skill] = [];
		}

		AllocatedPassives[skill].Add(passive.Name, passive);

		if (!fromLoad)
		{
			AllocatedPassivePoints[skill]++;
		}

		return true;
	}

	public void DeallocatePassivePoint(Skill skill, SkillPassive passive)
	{
		if (passive.Name == "Anchor")
		{
			return; // Anchor passive cannot be unallocated
		}
		
		if (AllocatedPassives.TryGetValue(skill, out Dictionary<string, SkillPassive> value) && value.ContainsValue(passive))
		{
			value.Remove(passive.Name);
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
		var acquiredPointKeys = AcquiredPassivePoints.Keys.Select(skill => skill.GetType().FullName).ToList();
		var acquiredPointValues = AcquiredPassivePoints.Values.ToList();
		tag["AcquiredPointKeys"] = acquiredPointKeys;
		tag["AcquiredPointValues"] = acquiredPointValues;

		var allocatedPointKeys = AllocatedPassivePoints.Keys.Select(skill => skill.GetType().FullName).ToList();
		var allocatedPointValues = AllocatedPassivePoints.Values.ToList();
		tag["AllocatedPointKeys"] = allocatedPointKeys;
		tag["AllocatedPointValues"] = allocatedPointValues;

		var allocatedPassiveKeys = AllocatedPassives.Keys.Select(skill => skill.GetType().FullName).ToList();
		var allocatedPassiveValues = AllocatedPassives.Values
			.Select(passives => passives.Select(p => p.Value.GetType().FullName).ToList()).ToList();
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
				var skill = Skill.GetAndPrepareSkill(Mod.Code.GetType(acquiredPointKeys[i]));
				AcquiredPassivePoints[skill] = acquiredPointValues[i];
			}
		}

		if (tag.ContainsKey("AllocatedPointKeys") && tag.ContainsKey("AllocatedPointValues"))
		{
			List<string> allocatedPointKeys = tag.Get<List<string>>("AllocatedPointKeys");
			List<int> allocatedPointValues = tag.Get<List<int>>("AllocatedPointValues");
			for (int i = 0; i < allocatedPointKeys.Count; i++)
			{
				var skill = Skill.GetAndPrepareSkill(Mod.Code.GetType(allocatedPointKeys[i]));
				AllocatedPassivePoints[skill] = allocatedPointValues[i];
			}
		}

		if (tag.ContainsKey("AllocatedPassiveKeys") && tag.ContainsKey("AllocatedPassiveValues"))
		{
			List<string> allocatedPassiveKeys = tag.Get<List<string>>("AllocatedPassiveKeys");
			List<List<string>> allocatedPassiveValues = tag.Get<List<List<string>>>("AllocatedPassiveValues");

			for (int i = 0; i < allocatedPassiveKeys.Count; i++)
			{
				var skill = Skill.GetAndPrepareSkill(Mod.Code.GetType(allocatedPassiveKeys[i]));
				AllocatedPassives.Add(skill, []);

				var passives = allocatedPassiveValues[i]
					.Select(name => skill.Passives.FirstOrDefault(p => p.GetType().FullName == name)).ToList();

				foreach (SkillPassive passive in passives)
				{
					if (passive is null)
					{
						continue;
					}

					AllocatePassivePoint(skill, passive, true);
				}
			}
		}
	}
}