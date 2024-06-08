using PathOfTerraria.Content.GUI;
using PathOfTerraria.Core.Loaders.UILoading;
using System.Collections.Generic;
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

	public List<Passive> Nodes = [];
	public List<PassiveEdge> Edges = [];

	public override void OnEnterWorld()
	{
		UILoader.GetUIState<Tree>().RemoveAllChildren();
		UILoader.GetUIState<Tree>().Populated = false;
	}

	public override void UpdateEquips()
	{
		Nodes.ForEach(n => n.BuffPlayer(Player));
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
			Main.blockMouse = UILoader.GetUIState<Tree>().IsVisible &&
						  UILoader.GetUIState<Tree>().GetRectangle().Contains(Main.mouseX, Main.mouseY);
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
		tag["points"] = Points;

		foreach (Passive passive in Nodes)
		{
			tag[passive.GetType().Name] = passive.Level;
		}
	}

	public override void LoadData(TagCompound tag)
	{
		// Reset tree
		Nodes = [];
		Edges = [];

		foreach (Type type in Mod.Code.GetTypes())
		{
			if (type.IsAbstract || !type.IsSubclassOf(typeof(Passive)))
			{
				continue;
			}

			object instance = Activator.CreateInstance(type);
			Nodes.Add(instance as Passive);
		}

		Nodes.ForEach(n => n.Connect(Nodes, Player));

		// Load tree
		Points = tag.GetInt("points");

		foreach (Passive passive in Nodes)
		{
			passive.Level = tag.TryGet(passive.GetType().Name, out int level) ? level : 0;
		}
	}
}