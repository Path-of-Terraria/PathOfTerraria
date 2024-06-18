using PathOfTerraria.Content.GUI;
using PathOfTerraria.Content.Passives;
using PathOfTerraria.Core.Loaders.UILoading;
using PathOfTerraria.Core.Systems.ModPlayers;
using PathOfTerraria.Data;
using PathOfTerraria.Data.Models;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Core.Systems.TreeSystem;

internal class PassiveEdge(Passive start, Passive end)
{
	public readonly Passive Start = start;
	public readonly Passive End = end;
}

// ReSharper disable once ClassNeverInstantiated.Global
internal class TreePlayer : ModPlayer
{
	public int Points;

	public List<Passive> ActiveNodes = [];
	public List<PassiveEdge> Edges = [];

	private TagCompound _saveData = [];

	public override void OnEnterWorld()
	{
		UILoader.GetUIState<PassiveTree>().RemoveAllChildren(); // is this really necessary?
		UILoader.GetUIState<PassiveTree>().CurrentDisplayClass = PlayerClass.None;
	}

	public void CreateTree()
	{
		ClassModPlayer mp = Main.LocalPlayer.GetModPlayer<ClassModPlayer>();

		ActiveNodes = [];
		Edges = [];

		Dictionary<int, Passive> passives = [];

		List<PassiveData> data = PassiveRegistry.TryGetPassiveData(mp.SelectedClass);

		data.ForEach(n => { passives.Add(n.ReferenceId, Passive.GetPassiveFromData(n)); ActiveNodes.Add(passives[n.ReferenceId]); });
		data.ForEach(n => n.Connections.ForEach(connection => Edges.Add(new(passives[n.ReferenceId], passives[connection.ReferenceId]))));

		ExpModPlayer expPlayer = Main.LocalPlayer.GetModPlayer<ExpModPlayer>();

		Points = expPlayer.EffectiveLevel;

		foreach (Passive passive in ActiveNodes)
		{
			passive.Level = _saveData.TryGet(passive.ReferenceId.ToString(), out int level) ? level : passive.InternalIdentifier == "Anchor" ? 1 : 0;
			// standard is id 1 is anchor for now.
			// no handeling for multiple anchors..
			
			if (passive is JewelSocket jsPassive)
			{
				if (_saveData.TryGet("_" + passive.ReferenceId.ToString(), out TagCompound tag))
				{
					jsPassive.LoadJewel(tag);
				}
			}

			if (passive.InternalIdentifier != "Anchor")
			{
				Points -= passive.Level;
			}
		}
	}

	public override void UpdateEquips()
	{
		ActiveNodes.Where(n => n.Level != 0).ToList().ForEach(n => n.BuffPlayer(Player));
	}

	private bool _blockMouse = false;
	private bool _lastState = false;
	
	public override void PreUpdate()
	{
		if (!Main.mouseLeft)
		{
			_blockMouse = false;
		}

		if (!_lastState && Main.mouseLeft)
		{
			Main.blockMouse = UILoader.GetUIState<PassiveTree>().IsVisible &&
						  UILoader.GetUIState<PassiveTree>().GetRectangle().Contains(Main.mouseX, Main.mouseY);
		}
		else
		{
			_blockMouse = UILoader.GetUIState<ExpBar>().GetRectangle().Contains(Main.mouseX, Main.mouseY);
		}

		Main.blockMouse = Main.blockMouse || _blockMouse;
		_lastState = Main.mouseLeft;
	}

	public override void SaveData(TagCompound tag)
	{
		foreach (Passive passive in ActiveNodes)
		{
			tag[passive.ReferenceId.ToString()] = passive.Level;
			if (passive is JewelSocket jsPassive && jsPassive.Socketed is not null)
			{
				TagCompound jewelTag = new TagCompound();
				jsPassive.SaveJewel(jewelTag);
				tag["_" + passive.ReferenceId.ToString()] = jewelTag;
			}
		}
	}

	public override void LoadData(TagCompound tag)
	{
		_saveData = tag;
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
}