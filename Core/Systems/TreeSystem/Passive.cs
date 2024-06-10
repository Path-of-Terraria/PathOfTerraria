using System.Collections.Generic;
using System.Linq;
using PathOfTerraria.Content.Items.Gear;
using PathOfTerraria.Core.Systems.ModPlayers;
using PathOfTerraria.Data.Models;
using Steamworks;
using SteelSeries.GameSense.DeviceZone;

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

	public int Id;

	public virtual string Name => "Unknown";
	public virtual string Tooltip => "Who knows what this will do!";

	public int Level;
	public int MaxLevel;

	private Vector2 _size;
	public Vector2 Size
	{
		get
		{
			if (_size == Vector2.Zero)
			{
				Texture2D tex = ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/PassiveFrameSmall", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;

				if (ModContent.HasAsset($"{PathOfTerraria.ModName}/Assets/Passives/" + GetType().Name))
				{
					tex = ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/Passives/" + GetType().Name, ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
				}

				_size = tex.Size();
			}

			return _size;
		}
	}

	public virtual void BuffPlayer(Player player) { }

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
			Passives.Add(instance.Name, type);
		}
	}

	public static Passive GetPassiveFromData(PassiveData data)
	{
		if (!Passives.ContainsKey(data.Passive))
		{
			return null;
		}

		Passive p = (Passive)Activator.CreateInstance(Passives[data.Passive]);

		p.TreePos = new(data.Position.Count > 0 ? data.Position[0] : 0, data.Position.Count > 1 ? data.Position[1] : 0);
		p.MaxLevel = data.MaxLevel;
		p.Id = data.Id;

		return p;
	}

	public void Draw(SpriteBatch spriteBatch, Vector2 center)
	{
		Texture2D tex = ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/PassiveFrameSmall").Value;

		if (ModContent.HasAsset($"{PathOfTerraria.ModName}/Assets/Passives/" + GetType().Name))
		{
			tex = ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/Passives/" + GetType().Name).Value;
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
			(treeSystem.Edges.Any(n => n.Start.Level > 0 && n.End == this) || treeSystem.Edges.All(n => n.End != this));
	}

	/// <summary>
	/// If this passive can be refunded or not
	/// </summary>
	/// <returns></returns>
	public virtual bool CanDeallocate(Player player)
	{
		TreePlayer treeSystem = player.GetModPlayer<TreePlayer>();

		return
			Level > 0 &&
			!(Level == 1 && treeSystem.Edges.Any(n => n.End.Level > 0 && n.Start == this && !treeSystem.Edges.Any(a => a != n && a.End == n.End && a.Start.Level > 0)));
	}
}