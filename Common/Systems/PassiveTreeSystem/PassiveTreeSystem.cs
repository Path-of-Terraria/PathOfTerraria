using System.Collections.Generic;
using System.Linq;
using PathOfTerraria.Common.Data;
using PathOfTerraria.Common.Data.Models;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.UI;
using PathOfTerraria.Content.Passives;
using PathOfTerraria.Core.UI.SmartUI;
using Terraria.ModLoader.IO;

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

	public List<Passive> ActiveNodes = [];
	public List<Edge> Edges = [];

	private TagCompound _saveData = [];

	public override void OnEnterWorld()
	{
		SmartUiLoader.GetUiState<TreeState>().RemoveAllChildren(); // is this really necessary?
	}

	public void CreateTree()
	{
		ResetNodes();

		ExpModPlayer expPlayer = Main.LocalPlayer.GetModPlayer<ExpModPlayer>();
		Points = expPlayer.EffectiveLevel + ExtraPoints;

		SetTree();
	}

	private void SetTree()
	{
		foreach (Passive passive in ActiveNodes.Where(passive => passive != null))
		{
			passive.Level = _saveData.TryGet(passive.ReferenceId.ToString(), out int level) ? level : passive.Name == "AnchorPassive" ? 1 : 0;

			if (passive is JewelSocket jsPassive)
			{
				if (_saveData.TryGet("_" + passive.ReferenceId, out TagCompound tag))
				{
					jsPassive.LoadJewel(tag);
				}
			}

			if (passive.Name != "AnchorPassive" && passive.Level > 0)
			{
				Points -= passive.Level;
			}
		}
	}

	private void ResetNodes()
	{
		ActiveNodes = [];
		Edges = [];

		Dictionary<int, Passive> passives = [];
		List<PassiveData> data = PassiveRegistry.GetPassiveData();

		data.ForEach(n =>
		{
			passives.Add(n.ReferenceId, Passive.GetPassiveFromData(n));
			if (passives[n.ReferenceId] != null)
			{
				ActiveNodes.Add(passives[n.ReferenceId]);
			}
		});

		data.ForEach(n => n.Connections.ForEach(connection => Edges.Add(new Edge(passives[n.ReferenceId], passives[connection.ReferenceId]))));
	}

	public override void UpdateEquips()
	{
		ActiveNodes.Where(n => n.Level != 0).ToList().ForEach(n => n.BuffPlayer(Player));
	}

	public override void SaveData(TagCompound tag)
	{
		foreach (Passive passive in ActiveNodes)
		{
			if (passive.Level > 0)
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

		_saveData = tag;
	}

	public override void LoadData(TagCompound tag)
	{
		_saveData = tag;
		ExtraPoints = tag.GetInt("extraPoints");

		ResetNodes();
		SetTree();
	}

	internal int GetCumulativeLevel(string internalIdentifier)
	{
		int level = 0;

		foreach (Passive passive in ActiveNodes)
		{
			if (passive.Name == internalIdentifier)
			{
				level += passive.Level;
			}
		}

		return level;
	}

	public bool FullyLinkedWithout(Passive passive)
	{
		HashSet<Allocatable> autoComplete = [];

		foreach (Edge e in Edges)
		{
			if (!e.Contains(passive) || e.Other(passive).Level <= 0)
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

			IEnumerable<Allocatable> add = Edges.Where(e => e.Contains(p) && e.Other(p).Level > 0 && !passed.Contains(e.Other(p))).Select(e => e.Other(p));

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
}
