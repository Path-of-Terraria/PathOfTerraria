using PathOfTerraria.Common.Mechanics;
using System.Collections.Generic;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems.Skills;

public abstract class SkillTree : ILoadable
{
	internal readonly record struct Edge(Allocatable Start, Allocatable End)
	{
		public readonly Allocatable Start = Start;
		public readonly Allocatable End = End;

		public bool Contains(Allocatable p)
		{
			return p == Start || p == End;
		}

		/// <summary> Assuming that p is either start or end - Contains returned true. </summary>
		public Allocatable Other(Allocatable p)
		{
			return p == Start ? End : Start;
		}
	}

	public Dictionary<Vector2, Allocatable> Allocatables = [];
	internal List<Edge> Edges = [];

	/// <summary> The number of points available for spending in this skill tree. </summary>
	public int Points;
	/// <summary> The current skill specialization of this tree. </summary>
	public SkillSpecial Specialization;

	public Vector2 Point(Allocatable a)
	{
		foreach (Vector2 key in Allocatables.Keys)
		{
			if (Allocatables[key] == a)
			{
				return key;
			}
		}

		return default;
	}

	/// <summary> Add skill tree elements by position here. </summary>
	public virtual void Populate() { }

	public virtual void SaveData(TagCompound tag)
	{
		//string name = Parent.Name;
		//tag[name + "Names"] = Allocatables.Values.Select(x => x.GetType().FullName).ToList();
	}

	public virtual void LoadData(TagCompound tag)
	{
		/*string name = Parent.Name;

		if (skill != null)
		{
			SkillModifications.Add(skill, new()
			{
				//Augments = tag.GetList<string>(name + "Augments")
				//Passives = tag.GetList<string>(name + "Passives")
			});
		}*/
	}

	public void Load(Mod mod)
	{
		Populate();
	}

	public void Unload() { }
}