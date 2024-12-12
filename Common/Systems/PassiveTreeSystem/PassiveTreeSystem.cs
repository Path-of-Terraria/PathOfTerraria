using System.Collections.Generic;
using System.Linq;
using PathOfTerraria.Common.Data;
using PathOfTerraria.Common.Data.Models;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.TreeSystem;
using PathOfTerraria.Common.UI;
using PathOfTerraria.Content.Passives;
using PathOfTerraria.Core.UI.SmartUI;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems.PassiveTreeSystem;

public class PassiveEdge(Passive start, Passive end)
{
	public readonly Passive Start = start;
	public readonly Passive End = end;

	public bool Contains(Passive p)
	{
		return p == Start || p == End;
	}

	/// <summary>
	/// Assuming that p is either start or end - Contains returned true.
	/// </summary>
	public Passive Other(Passive p)
	{
		return p == Start ? End : Start;
	}
}

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
	public List<PassiveEdge> Edges = [];

	private TagCompound _saveData = [];

	public override void OnEnterWorld()
	{
		SmartUiLoader.GetUiState<TreeState>().RemoveAllChildren(); // is this really necessary?
	}

	public void CreateTree()
	{
		ActiveNodes = [];
		Edges = [];

		Dictionary<int, Passive> passives = [];
		List<PassiveData> data = PassiveRegistry.GetPassiveData();

		data.ForEach(n => 
		{
			passives.Add(n.ReferenceId, Passive.GetPassiveFromData(n)); 
			ActiveNodes.Add(passives[n.ReferenceId]); 
		});

		data.ForEach(n => n.Connections.ForEach(connection => Edges.Add(new PassiveEdge(passives[n.ReferenceId], passives[connection.ReferenceId]))));

		ExpModPlayer expPlayer = Main.LocalPlayer.GetModPlayer<ExpModPlayer>();
		Points = expPlayer.EffectiveLevel + ExtraPoints;
		foreach (Passive passive in ActiveNodes)
		{
			passive.Level = _saveData.TryGet(passive.ReferenceId.ToString(), out int level) ? level : passive.InternalIdentifier == "AnchorPassive" ? 1 : 0;
			// standard is id 1 is anchor for now.
			// no handling for multiple anchors..
			
			if (passive is JewelSocket jsPassive)
			{
				if (_saveData.TryGet("_" + passive.ReferenceId, out TagCompound tag))
				{
					jsPassive.LoadJewel(tag);
				}
			}

			if (passive.InternalIdentifier != "AnchorPassive" && passive.Level > 0)
			{
				Points -= passive.Level;
			}
		}
	}

	public override void UpdateEquips()
	{
		ActiveNodes.Where(n => n.Level != 0).ToList().ForEach(n => n.BuffPlayer(Player));
	}

	public override void SaveData(TagCompound tag)
	{
		foreach (Passive passive in ActiveNodes)
		{
			tag[passive.ReferenceId.ToString()] = passive.Level;
			if (passive is JewelSocket jsPassive && jsPassive.Socketed is not null)
			{
				TagCompound jewelTag = [];
				jsPassive.SaveJewel(jewelTag);
				tag["_" + passive.ReferenceId] = jewelTag;
			}
		}
		
		tag["extraPoints"] = ExtraPoints;
	}

	public override void LoadData(TagCompound tag)
	{
		_saveData = tag;
		ExtraPoints = tag.GetInt("extraPoints");
	}

	internal int GetCumulativeLevel(string internalIdentifier)
	{
		int level = 0;

		foreach (Passive passive in ActiveNodes)
		{
			if (passive.InternalIdentifier == internalIdentifier)
			{
				level += passive.Level;
			}
		}

		return level;
	}

	public bool FullyLinkedWithout(Passive passive)
	{
		HashSet<Passive> autoComplete = [];

		foreach (PassiveEdge e in Edges)
		{
			if (!e.Contains(passive) || e.Other(passive).Level <= 0)
			{
				continue;
			}

			Tuple<bool, HashSet<Passive>> ret = CanFindAnchor(e.Other(passive), autoComplete, passive);

			if (!ret.Item1)
			{
				return false;
			}

			foreach (Passive p in ret.Item2)
			{
				if (p != passive)
				{
					autoComplete.Add(p);
				}
			}
		}

		return true;
	}

	private Tuple<bool, HashSet<Passive>> CanFindAnchor(Passive from, HashSet<Passive> autoComplete, Passive removed)
	{
		if (autoComplete.Contains(from) || from.InternalIdentifier == "AnchorPassive")
		{
			return new Tuple<bool, HashSet<Passive>>(true, []);
		}

		HashSet<Passive> passed = [from, removed];
		List<Passive> toCheck = [from];

		while (toCheck.Count != 0)
		{
			Passive p = toCheck[0];
			if (Edges.Any(e => e.Contains(p) && autoComplete.Contains(e.Other(p))))
			{
				return new Tuple<bool, HashSet<Passive>>(true, passed);
			}

			IEnumerable<Passive> add = Edges.Where(e => e.Contains(p) && e.Other(p).Level > 0 && !passed.Contains(e.Other(p))).Select(e => e.Other(p));

			if (add.Any(p => p.InternalIdentifier == "AnchorPassive"))
			{
				return new Tuple<bool, HashSet<Passive>>(true, passed);
			}

			toCheck.AddRange(add);

			passed.Add(p);
			toCheck.RemoveAt(0);
		}

		return new(false, []);
	}
}
