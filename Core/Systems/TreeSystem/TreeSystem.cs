using FunnyExperience.Content.GUI;
using FunnyExperience.Core.Loaders.UILoading;
using System.Collections.Generic;
using Terraria.ModLoader.IO;

namespace FunnyExperience.Core.Systems.TreeSystem
{
	internal class PassiveEdge
	{
		public readonly Passive Start;
		public readonly Passive End;

		public PassiveEdge(Passive start, Passive end)
		{
			Start = start;
			End = end;
		}
	}

	internal class TreePlayer : ModPlayer
	{
		public int Points;

		public List<Passive> Nodes = new();
		public List<PassiveEdge> Edges = new();

		public override void OnEnterWorld()
		{
			UILoader.GetUIState<Tree>().RemoveAllChildren();
			UILoader.GetUIState<Tree>().Populated = false;
		}

		public override void UpdateEquips()
		{
			Nodes.ForEach(n => n.BuffPlayer(Player));
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
			Nodes = new List<Passive>();
			Edges = new List<PassiveEdge>();

			foreach (Type type in Mod.Code.GetTypes())
			{
				if (!type.IsAbstract && type.IsSubclassOf(typeof(Passive)))
				{
					object instance = Activator.CreateInstance(type);
					Nodes.Add(instance as Passive);
				}
			}

			Nodes.ForEach(n => n.Connect(Nodes, Player));

			// Load tree
			Points = tag.GetInt("points");

			foreach (Passive passive in Nodes)
			{
				if (tag.TryGet(passive.GetType().Name, out int level))
					passive.Level = level;
				else
					passive.Level = 0;
			}
		}
	}
}
