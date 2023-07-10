using System.Collections.Generic;
using Terraria.ModLoader.IO;

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

	internal class TreePlayer : ModPlayer
	{
		public int Points;

		public List<Passive> nodes = new();
		public List<PassiveEdge> edges = new();

		public override void OnEnterWorld()
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

			nodes.ForEach(n => n.Connect(nodes, Player));
		}

		public override void Unload()
		{
			nodes = null;
			edges = null;
		}

		public override void UpdateEquips()
		{
			nodes.ForEach(n => n.BuffPlayer(Player));

			if (Player.controlHook && Player.controlQuickHeal)
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

				nodes.ForEach(n => n.Connect(nodes, Player));
			}
		}

		public override void SaveData(TagCompound tag)
		{
			tag["points"] = Points;

			foreach (Passive passive in nodes)
			{
				tag[passive.GetType().Name] = passive.level;
			}
		}

		public override void LoadData(TagCompound tag)
		{
			Points = tag.GetInt("points");

			foreach (Passive passive in nodes)
			{
				if (tag.TryGet(passive.GetType().Name, out int level))
					passive.level = level;
			}
		}
	}
}
