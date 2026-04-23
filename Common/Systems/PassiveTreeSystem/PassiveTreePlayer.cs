using PathOfTerraria.Common.Data;
using PathOfTerraria.Common.Data.Models;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Classing;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.UI;
using PathOfTerraria.Common.UI.Guide;
using PathOfTerraria.Content.Passives;
using PathOfTerraria.Content.Passives.Misc;
using PathOfTerraria.Core.UI.SmartUI;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader.IO;
using System.Runtime.InteropServices;

namespace PathOfTerraria.Common.Systems.PassiveTreeSystem;

// ReSharper disable once ClassNeverInstantiated.Global
internal class PassiveTreePlayer : ModPlayer
{
	/// <summary>
	/// This should be equal to your level + any extra points you have.
	/// </summary>
	public int Points;
	
	/// <summary>
	/// For means of getting points that are not equal to your level. Such as quest rewards or other things.
	/// </summary>
	public int ExtraPoints;

	public float[] StrengthByPassive = new float[Passive.MaxId];
	public List<Passive> ActiveNodes = [];
	public List<Edge<Allocatable>> Edges = [];

	private TagCompound _saveData = [];

	public override void OnEnterWorld()
	{
		SmartUiLoader.GetUiState<TreeState>().RemoveAllChildren(); // is this really necessary?
	}

	public void CreateTree()
	{
		ResetNodes();

		ExpModPlayer expPlayer = Player.GetModPlayer<ExpModPlayer>();
		Points = expPlayer.EffectiveLevel + ExtraPoints;

		SetTree(false);
	}

	/// <summary>
	/// Sets or updates the tree according to the player's information.
	/// </summary>
	private void SetTree(bool setStrengths)
	{
		Span<Passive> span = CollectionsMarshal.AsSpan(ActiveNodes);

		foreach (ref Passive passive in span)
		{
			if (passive is null)
			{
				continue;
			}

			int oldLevel = passive.Level;
			passive.Level = passive is AnchorPassive
				? (IsAllowedAnchor(passive) ? 1 : 0)
				: _saveData.TryGet(passive.ReferenceId.ToString(), out int level) ? level : 0;

			if (setStrengths)
			{
				if (oldLevel < passive.Level)
				{
					AllocatePassive(passive, passive.Level - oldLevel, false);
				}
				else if (oldLevel > passive.Level)
				{
					DeallocatePassive(passive, passive.Value, oldLevel - passive.Level);
				}
			}

			if (passive is JewelSocket jsPassive)
			{
				if (_saveData.TryGet("_" + passive.ReferenceId, out TagCompound tag))
				{
					jsPassive.LoadJewel(tag);
				}
			}

			if (passive.Name != "AnchorPassive" && passive.Level > 0 && passive.Name != "MasteryPassive")
			{
				Points -= passive.Level;
			}
		}

		// PruneDisconnectedNodes(setStrengths);
	}

	private void ResetNodes()
	{
		ActiveNodes = [];
		Edges = [];

		Dictionary<int, Passive> passives = [];
		List<PassiveData> data = PassiveRegistry.GetPassiveData();

		data.ForEach(n =>
		{
			var passive = Passive.GetPassiveFromData(n);
			passives.Add(n.ReferenceId, passive);

			if (passives[n.ReferenceId] != null)
			{
				ActiveNodes.Add(passives[n.ReferenceId]);
			}
		});

		data.ForEach(n => n.Connections.ForEach(c =>
		{
			if (passives[n.ReferenceId] != null)
			{
				EdgeFlags flags = 0;
				flags |= c.IsHidden ? EdgeFlags.Hidden : 0;
				flags |= c.EffectsOnly ? EdgeFlags.EffectsOnly : 0;
				Edges.Add(new(passives[n.ReferenceId], passives[c.ReferenceId], flags));
			}
		}));
	}

	public override void UpdateEquips()
	{
		ActiveNodes.Where(n => n.Level != 0).ToList().ForEach(n => n.BuffPlayer(Player));
	}

	public override void SaveData(TagCompound tag)
	{
		foreach (Passive passive in ActiveNodes)
		{
			if (passive is not AnchorPassive && passive.Level > 0)
			{
				tag[passive.ReferenceId.ToString()] = passive.Level;
			}

			if (passive is JewelSocket jsPassive && jsPassive.Socketed is not null)
			{
				TagCompound jewelTag = [];
				jsPassive.SaveJewel(jewelTag);
				tag["_" + passive.ReferenceId] = jewelTag;
			}
		}
		
		tag["extraPoints"] = ExtraPoints;

		_saveData = (TagCompound)tag.Clone();
	}

	public void ResetAllNodes()
	{
		foreach (Passive node in ActiveNodes)
		{
			if (node is AnchorPassive)
			{
				continue;
			}

			if (node.Level > 0)
			{
				Player.GetModPlayer<PassiveTreePlayer>().DeallocatePassive(node, node.Value, node.Level, false);
				node.Level = 0;
			}
		}

		SaveData([]);
	}

	public override void LoadData(TagCompound tag)
	{
		_saveData = tag;
		ExtraPoints = tag.GetInt("extraPoints");

		ResetNodes();
		SetTree(true);
	}

	/// <summary>
	/// Gets the total value of every node of a given type on the passive tree.
	/// </summary>
	internal float GetCumulativeValue<T>() where T : Passive
	{
		return StrengthByPassive[ModContent.GetInstance<T>().ID];
	}

	/// <summary>
	/// Same as <see cref="GetCumulativeValue{T}"/>, but returns false if <paramref name="value"/> is 0.
	/// </summary>
	internal bool TryGetCumulativeValue<T>(out float value) where T : Passive
	{
		value = GetCumulativeValue<T>();
		return value > 0;
	}

	internal bool HasNode<T>() where T : Passive
	{
		foreach (Passive passive in ActiveNodes)
		{
			if (passive is T && passive.Level > 0)
			{
				return true;
			}
		}

		return false;
	}

	public bool FullyLinkedWithout(Passive passive)
	{
		if (!AllAllocatedNodesMeetEdgeRequirementsWithout(passive))
		{
			return false;
		}

		HashSet<Allocatable> autoComplete = [];

		foreach (Edge<Allocatable> e in Edges)
		{
			if (!e.Contains(passive) || GetEffectiveLevel(e.Other(passive)) <= 0)
			{
				continue;
			}

			Tuple<bool, HashSet<Allocatable>> ret = CanFindAnchor(e.Other(passive), autoComplete, passive);

			if (!ret.Item1)
			{
				return false;
			}

			foreach (Allocatable p in ret.Item2)
			{
				if (p != passive)
				{
					autoComplete.Add(p);
				}
			}
		}

		return true;
	}

	private bool AllAllocatedNodesMeetEdgeRequirementsWithout(Allocatable removed)
	{
		foreach (Passive node in ActiveNodes)
		{
			if (node == removed || GetEffectiveLevel(node) <= 0 || node.RequiredAllocatedEdges <= 1)
			{
				continue;
			}

			if (!HasEnoughAllocatedEdgesWithout(node, removed))
			{
				return false;
			}
		}

		return true;
	}

	private bool HasEnoughAllocatedEdgesWithout(Allocatable target, Allocatable removed)
	{
		int activeEdges = 0;

		foreach (Edge<Allocatable> edge in Edges)
		{
			if (!edge.Contains(target))
			{
				continue;
			}

			Allocatable other = edge.Other(target);

			if (other == removed || GetEffectiveLevel(other) <= 0)
			{
				continue;
			}

			if (!CountsTowardRequiredEdges(target, other))
			{
				continue;
			}

			activeEdges++;

			if (activeEdges >= target.RequiredAllocatedEdges)
			{
				return true;
			}
		}

		return false;
	}

	private static bool CountsTowardRequiredEdges(Allocatable target, Allocatable other)
	{
		// Hidden choice children should not count as surrounding prerequisites for their parent hub.
		if (target is Passive { IsChoiceNode: true } && other is Passive { IsHidden: true })
		{
			return false;
		}

		return true;
	}

	private int GetEffectiveLevel(Allocatable allocatable)
	{
		if (allocatable.Level > 0)
		{
			return allocatable.Level;
		}

		if (allocatable is not Passive { IsChoiceNode: true } passive)
		{
			return 0;
		}

		foreach (Edge<Allocatable> edge in Edges)
		{
			if (edge.Start != passive || edge.End is not Passive { IsHidden: true } hiddenPassive)
			{
				continue;
			}

			if (hiddenPassive.Level > 0)
			{
				return hiddenPassive.Level;
			}
		}

		return 0;
	}

	private Tuple<bool, HashSet<Allocatable>> CanFindAnchor(Allocatable from, HashSet<Allocatable> autoComplete, Allocatable removed)
	{
		if (autoComplete.Contains(from) || from.Name == "AnchorPassive")
		{
			return new Tuple<bool, HashSet<Allocatable>>(true, []);
		}

		HashSet<Allocatable> passed = [from, removed];
		List<Allocatable> toCheck = [from];

		while (toCheck.Count != 0)
		{
			Allocatable p = toCheck[0];
			if (Edges.Any(e => e.Contains(p) && autoComplete.Contains(e.Other(p))))
			{
				return new Tuple<bool, HashSet<Allocatable>>(true, passed);
			}

			IEnumerable<Allocatable> add = Edges.Where(e => e.Contains(p) && GetEffectiveLevel(e.Other(p)) > 0 && !passed.Contains(e.Other(p))).Select(e => e.Other(p));

			if (add.Any(p => p.Name == "AnchorPassive"))
			{
				return new Tuple<bool, HashSet<Allocatable>>(true, passed);
			}

			toCheck.AddRange(add);

			passed.Add(p);
			toCheck.RemoveAt(0);
		}

		return new(false, []);
	}

	internal void AllocatePassive(Passive passive, int strength = 1, bool save = true)
	{
		if (passive is MasteryPassive) // Hardcode for this, which doesn't work the same as any other passive
		{
			return;
		}

		Points--;
		Player.GetModPlayer<TutorialPlayer>().TutorialChecks.Add(TutorialCheck.AllocatedPassive);
		int id = Passive.PassiveNameToId[passive.Name];
		StrengthByPassive[id] += strength;

		if (save)
		{
			SaveData([]); // Instantly save the result because _saveData is needed whenever the element reloads - same in DeallocatePassive
		}
	}

	internal void DeallocatePassive(Passive passive, int valueLoss, int pointRefund, bool save = true)
	{
		if (passive is MasteryPassive)
		{
			passive.Level--;
			return;
		}

		int id = Passive.PassiveNameToId[passive.Name];

		Points += pointRefund;
		Player.GetModPlayer<TutorialPlayer>().TutorialChecks.Add(TutorialCheck.DeallocatedPassive);

		ref float str = ref StrengthByPassive[id];

		if (str > 0)
		{
			str = Math.Max(0, str - valueLoss);
		}
#if DEBUG
		else
		{
			Debug.Fail($"Deallocation ran on a passive with no allocation to begin with? {passive.Name}");
		}
#endif

		if (save)
		{
			SaveData([]);
		}
	}

	public bool IsAllowedAnchor(Passive passive)
	{
		if (passive is not AnchorPassive)
		{
			return false;
		}

		List<Passive> anchors = ActiveNodes.Where(n => n is AnchorPassive).ToList();

		if (anchors.Count <= 1)
		{
			return true;
		}

		StarterClass starterClass = Player.GetModPlayer<ClassingPlayer>().Class;

		if (starterClass == StarterClass.None)
		{
			return true;
		}

		int allowedAnchorReferenceId = starterClass switch
		{
			StarterClass.Melee => 0,
			StarterClass.Ranged => -1,
			StarterClass.Magic => -2,
			StarterClass.Summon => -3,
			_ => int.MinValue,
		};

		return passive.ReferenceId == allowedAnchorReferenceId;
	}

	// This method caused mastery nodes to work improperly, and I didn't see any additional functionality.
	// Why was this here? - Gabe
	//private void PruneDisconnectedNodes(bool setStrengths)
	//{
	//	HashSet<Passive> connectedNodes = GetConnectedNodesFromAllowedAnchors();

	//	foreach (Passive passive in ActiveNodes)
	//	{
	//		if (passive is AnchorPassive || passive.Level <= 0 || connectedNodes.Contains(passive))
	//		{
	//			continue;
	//		}
	//
	//		RemovePassiveLevels(passive, setStrengths);
	//	}
	//}

	private HashSet<Passive> GetConnectedNodesFromAllowedAnchors()
	{
		HashSet<Passive> visited = [];
		Queue<Passive> toCheck = new(ActiveNodes.Where(n => n is AnchorPassive && n.Level > 0));

		while (toCheck.Count > 0)
		{
			Passive passive = toCheck.Dequeue();

			if (!visited.Add(passive))
			{
				continue;
			}

			foreach (Passive connected in Edges.Where(e => e.Contains(passive) && e.Other(passive) is Passive { Level: > 0 })
				.Select(e => (Passive)e.Other(passive)))
			{
				if (!visited.Contains(connected))
				{
					toCheck.Enqueue(connected);
				}
			}
		}

		return visited;
	}

	private void RemovePassiveLevels(Passive passive, bool setStrengths)
	{
		int removedLevels = passive.Level;

		if (removedLevels <= 0)
		{
			return;
		}

		passive.Level = 0;

		if (passive is not MasteryPassive)
		{
			Points += removedLevels;
		}

		if (setStrengths && passive is not MasteryPassive)
		{
			int id = Passive.PassiveNameToId[passive.Name];
			StrengthByPassive[id] = Math.Max(0, StrengthByPassive[id] - removedLevels);
		}
	}
}
