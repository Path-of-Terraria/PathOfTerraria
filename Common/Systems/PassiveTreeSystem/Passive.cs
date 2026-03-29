using System.Collections.Generic;
using System.Runtime.InteropServices;
using PathOfTerraria.Common.Data.Models;
using PathOfTerraria.Common.Mechanics;
using Terraria.Localization;
using Terraria.ModLoader.Core;

namespace PathOfTerraria.Common.Systems.PassiveTreeSystem;

internal class PassiveLoader : ILoadable
{
	public void Load(Mod mod)
	{
		Passive.LoadPassives();
	}

	public void Unload() { }
}

[Autoload(false)] // Loading is handled in LoadPassives a bit below
public abstract class Passive : Allocatable, ILoadable
{
	public static Dictionary<string, Type> Passives = [];

	internal static Dictionary<string, int> PassiveNameToId = [];
	internal static int MaxId { get; private set; }

	/// <summary> If true, this passive will be given a special UI element that allows players to choose only one of its children. </summary>
	public bool IsChoiceNode { get; set; }

	/// <summary> The internal identifier of this passive. <para/>
	/// This is used to map the JSON data to the correct passive. This is also what's used to grab the texture of this passive.
	/// </summary>
	public override string Name => GetType().Name;

	/// <summary>
	/// This is used to create a reference to the created passive for connections in JSON.
	/// </summary>
	public int ReferenceId;

	/// <summary>
	/// Runtime ID assigned for lookup tables.
	/// </summary>
	public int ID;

	public override string TexturePath => $"{PoTMod.ModName}/Assets/Passives/" + Name;
	/// <summary>
	/// Name to be used in ALL display situations. This is automatically populated by <see cref="Language.GetOrRegister(string, Func{string})"/>.
	/// </summary>
	public override string DisplayName => Language.GetTextValue("Mods.PathOfTerraria.Passives." + Name + ".Name");

	/// <summary>
	/// Tooltip to be used in ALL display situations. This is automatically populated by <see cref="Language.GetOrRegister(string, Func{string})"/>.
	/// </summary>
	public override string DisplayTooltip => Language.GetText($"Mods.PathOfTerraria.Passives.{Name}.Tooltip").Format(Value);

	public int Value;

	void ILoadable.Load(Mod mod)
	{
		OnLoad();
	}

	public virtual void Unload() { }

	public virtual void BuffPlayer(Player player) { }

	public virtual void OnLoad() { }

	public static void LoadPassives()
	{
		Mod mod = PoTMod.Instance;
		int id = 0;

		Passives.Clear();
		PassiveNameToId.Clear();

		foreach (Type type in AssemblyManager.GetLoadableTypes(mod.Code))
		{
			if (type.IsAbstract || !type.IsSubclassOf(typeof(Passive)))
			{
				continue;
			}

			var instance = (Passive)Activator.CreateInstance(type);
			instance.ID = id++;
			mod.AddContent(instance);

			// Automatically registers the given keys for each instance loaded, and sets Name to the class's name and Description to empty if they do not exist.
			Language.GetOrRegister("Mods.PathOfTerraria.Passives." + instance.Name + ".Name", () => type.Name);
			Language.GetOrRegister("Mods.PathOfTerraria.Passives." + instance.Name + ".Tooltip", () => "");

			Passives.Add(instance.Name, type);
			PassiveNameToId.Add(instance.Name, instance.ID);
		}

		MaxId = id - 1;
	}

	public static Passive GetPassiveFromData(PassiveData data)
	{
		if (!Passives.TryGetValue(data.InternalIdentifier, out Type value))
		{
			return null;
		}

		var p = (Passive)Activator.CreateInstance(value);

		if (p is null)
		{
			Console.WriteLine("Failed to create passive from data: " + data.InternalIdentifier);
			return null;
		}

		p.TreePos = new Vector2(data.Position.X, data.Position.Y);
		p.MaxLevel = data.MaxLevel;
		p.ReferenceId = data.ReferenceId;
		p.Value = data.Value;
		p.IsHidden = data.IsHidden;
		p.IsChoiceNode = data.IsChoiceNode;
		p.RequiredAllocatedEdges = data.RequiredAllocatedEdges;

		return p;
	}

	/// <summary>
	/// If this passive is able to be allocated or not
	/// </summary>
	/// <returns></returns>
	public override bool CanAllocate(Player player)
	{
		PassiveTreePlayer passivePlayer = player.GetModPlayer<PassiveTreePlayer>();

		return
			Level < MaxLevel &&
			Main.LocalPlayer.GetModPlayer<PassiveTreePlayer>().Points > 0 &&
			CountRequiredEdges(CollectionsMarshal.AsSpan(passivePlayer.Edges));
	}

	private bool CountRequiredEdges(Span<Edge<Allocatable>> edges)
	{
		int count = 0;

		foreach (Edge<Allocatable> edge in edges)
		{
			if (edge.Contains(this) && edge.Other(this).Level > 0)
			{
				count++;

				if (count >= RequiredAllocatedEdges)
				{
					return true;
				}
			}
		}

		return false;
	}

	/// <summary>
	/// If this passive can be refunded or not
	/// </summary>
	/// <returns></returns>
	public override bool CanDeallocate(Player player)
	{
		PassiveTreePlayer passiveTreeSystem = player.GetModPlayer<PassiveTreePlayer>();

		return Level > 0 && (Level > 1 || passiveTreeSystem.FullyLinkedWithout(this));
	}
}