﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PathOfTerraria.Data.Models;
using Terraria.DataStructures;
using Terraria.Localization;

namespace PathOfTerraria.Core.Systems.TreeSystem;

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

internal abstract class Passive
{
	public static Dictionary<string, Type> Passives = [];
	
	public Vector2 TreePos;

	/// <summary>
	/// This is used to map the JSON data to the correct passive.
	/// This is also what's used to grab the texture of this passive.
	/// </summary>
	public virtual string InternalIdentifier => "NONE";
	
	// This is used to create a reference to the created passive for connections
	public int ReferenceId;

	/// <summary>
	/// Name to be used in ALL display situations. This is automatically populated by <see cref="Language.GetOrRegister(string, Func{string})"/>.
	/// </summary>
	public virtual string DisplayName => Language.GetTextValue("Mods.PathOfTerraria.Passives." + InternalIdentifier + ".Name");

	/// <summary>
	/// Tooltip to be used in ALL display situations. This is automatically populated by <see cref="Language.GetOrRegister(string, Func{string})"/>.
	/// </summary>
	public virtual string DisplayTooltip => Language.GetTextValue("Mods.PathOfTerraria.Passives." + InternalIdentifier + ".Tooltip");

	public int Level;
	public int MaxLevel;

	private Vector2 _size;
	
	public Vector2 Size
	{
		get
		{
			if (_size != Vector2.Zero)
			{
				return _size;
			}

			Texture2D tex = ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/GUI/PassiveFrameSmall", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;

			if (ModContent.HasAsset($"{PathOfTerraria.ModName}/Assets/Passives/" + InternalIdentifier))
			{
				tex = ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/Passives/" + InternalIdentifier, ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
			}

			_size = tex.Size();

			return _size;
		}
	}

	public virtual void BuffPlayer(Player player) { }

	public virtual void OnLoad() { }

	public static void LoadPassives()
	{
		Passives.Clear();

		foreach (Type type in PathOfTerraria.Instance.Code.GetTypes())
		{
			if (type.IsAbstract || !type.IsSubclassOf(typeof(Passive)))
			{
				continue;
			}

			var instance = (Passive)Activator.CreateInstance(type);
			instance.OnLoad();

			// Automatically registers the given keys for each instance loaded, and sets Name to the class's name and Description to empty if they do not exist.
			Language.GetOrRegister("Mods.PathOfTerraria.Passives." + instance.InternalIdentifier + ".Name", () => type.Name);
			Language.GetOrRegister("Mods.PathOfTerraria.Passives." + instance.InternalIdentifier + ".Tooltip", () => "");

			Passives.Add(instance.InternalIdentifier, type);
		}
	}

	public static Passive GetPassiveFromData(PassiveData data)
	{
		if (!Passives.TryGetValue(data.InternalIdentifier, out Type value))
		{
			return null;
		}

		var p = (Passive) Activator.CreateInstance(value);

		p.TreePos = new Vector2(data.Position.X, data.Position.Y);
		p.MaxLevel = data.MaxLevel;
		p.ReferenceId = data.ReferenceId;

		return p;
	}

	public void Draw(SpriteBatch spriteBatch, Vector2 center)
	{
		Texture2D tex = ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/GUI/PassiveFrameSmall").Value;

		if (ModContent.HasAsset($"{PathOfTerraria.ModName}/Assets/Passives/" + InternalIdentifier))
		{
			tex = ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/Passives/" + InternalIdentifier).Value;
		}

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
	public bool CanAllocate(Player player)
	{
		TreePlayer treeSystem = player.GetModPlayer<TreePlayer>();

		return
			Level < MaxLevel &&
			Main.LocalPlayer.GetModPlayer<TreePlayer>().Points > 0 &&
			treeSystem.Edges.Any(e => e.Contains(this) && e.Other(this).Level > 0);
	}

	/// <summary>
	/// If this passive can be refunded or not
	/// </summary>
	/// <returns></returns>
	public virtual bool CanDeallocate(Player player)
	{
		if (InternalIdentifier == "Anchor")
		{
			return false;
		}

		TreePlayer treeSystem = player.GetModPlayer<TreePlayer>();

		return Level > 0 && (Level > 1 || treeSystem.FullyLinkedWithout(this));
	}
}