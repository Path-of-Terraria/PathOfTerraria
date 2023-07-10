using System.Collections.Generic;

namespace FunnyExperience.Core.Systems.TreeSystem
{
	internal class PassiveEdge
	{
		public Passive start;
		public Passive end;

		public PassiveEdge(Passive start, Passive end)
		{
			this.start = start;
			this.end = end;
		}
	}

	internal class TreeSystem : ModSystem
	{
		public static List<Passive> nodes;
		public static List<PassiveEdge> edges;

		public override void Load()
		{
			nodes = new();
			edges = new();

			foreach (Type type in Mod.Code.GetTypes())
			{
				if (!type.IsAbstract && type.IsSubclassOf(typeof(Passive)))
				{
					object instance = Activator.CreateInstance(type);
					nodes.Add(instance as Passive);
				}
			}

			nodes.ForEach(n => n.Connect(nodes));
		}

		public override void Unload()
		{
			nodes = null;
			edges = null;
		}
	}

	internal class TreePlayer : ModPlayer
	{
		public List<Passive> passives = new();

		public int Points;

		public override void UpdateEquips()
		{
			passives.ForEach(n => n.BuffPlayer(Player));
		}
	}
}
