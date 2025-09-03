using System.Collections.Generic;
using System.Linq;
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

	public void Unload()
	{
	}
}

public abstract class Passive : Allocatable
{
	public static Dictionary<string, Type> Passives = [];

	/// <summary> The internal identifier of this passive. <para/>
	/// This is used to map the JSON data to the correct passive. This is also what's used to grab the texture of this passive.
	/// </summary>
	public override string Name => GetType().Name;
	
	// This is used to create a reference to the created passive for connections
	public int ReferenceId;

	public override string TexturePath => $"{PoTMod.ModName}/Assets/Passives/" + Name;
	/// <summary>
	/// Name to be used in ALL display situations. This is automatically populated by <see cref="Language.GetOrRegister(string, Func{string})"/>.
	/// </summary>
	public override string DisplayName => Language.GetTextValue("Mods.PathOfTerraria.Passives." + Name + ".Name");

	/// <summary>
	/// Tooltip to be used in ALL display situations. This is automatically populated by <see cref="Language.GetOrRegister(string, Func{string})"/>.
	/// </summary>
	public override string DisplayTooltip => string.Format(Language.GetTextValue($"Mods.PathOfTerraria.Passives.{Name}.Tooltip"), Value);

	public int Value;

	public virtual void BuffPlayer(Player player) { }

	public virtual void OnLoad() { }

	public static void LoadPassives()
	{
		Passives.Clear();

		foreach (Type type in AssemblyManager.GetLoadableTypes(PoTMod.Instance.Code))
		{
			if (type.IsAbstract || !type.IsSubclassOf(typeof(Passive)))
			{
				continue;
			}

			var instance = (Passive)Activator.CreateInstance(type);
			instance.OnLoad();

			// Automatically registers the given keys for each instance loaded, and sets Name to the class's name and Description to empty if they do not exist.
			Language.GetOrRegister("Mods.PathOfTerraria.Passives." + instance.Name + ".Name", () => type.Name);
			Language.GetOrRegister("Mods.PathOfTerraria.Passives." + instance.Name + ".Tooltip", () => "");

			Passives.Add(instance.Name, type);
		}
	}

	public static Passive GetPassiveFromData(PassiveData data)
	{
		if (!Passives.TryGetValue(data.InternalIdentifier, out Type value))
		{
			return null;
		}

		var p = (Passive) Activator.CreateInstance(value);

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
		p.RequiredAllocatedEdges = data.RequiredAllocatedEdges;

		return p;
	}

	public override void Draw(SpriteBatch spriteBatch, Vector2 center)
	{
		Texture2D tex = Texture.Value;
		Color color = Color.Gray;

		if (CanAllocate(Main.LocalPlayer))
		{
			color = Color.Lerp(Color.Gray, Color.White, (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.5f + 0.5f);
		}

		if (Level > 0)
		{
			color = Color.White;
		}

		spriteBatch.Draw(tex, center, null, color, 0, Size / 2f, 1, 0, 0);

		if (MaxLevel > 1)
		{
			Utils.DrawBorderString(spriteBatch, $"{Level}/{MaxLevel}", center + Size / 2f, color, 1, 0.5f, 0.5f);
		}
	}

	/// <summary>
	/// If this passive is able to be allocated or not
	/// </summary>
	/// <returns></returns>
	public override bool CanAllocate(Player player)
	{
		PassiveTreePlayer passiveTreeSystem = player.GetModPlayer<PassiveTreePlayer>();

		return
			Level < MaxLevel &&
			Main.LocalPlayer.GetModPlayer<PassiveTreePlayer>().Points > 0 &&
			passiveTreeSystem.Edges.Count(e => e.Contains(this) && e.Other(this).Level > 0) >= RequiredAllocatedEdges;
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