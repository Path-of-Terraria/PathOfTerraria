using System.Collections.Generic;
using System.Linq;

namespace FunnyExperience.Core.Systems.TreeSystem
{
	internal abstract class Passive
	{
		public Vector2 TreePos;

		public string Name = "Unknown";
		public string Tooltip = "Who knows what this will do!";

		public int Level;
		public int MaxLevel;

		public int Width = 50;
		public int Height = 50;

		public virtual void BuffPlayer(Player player) { }

		public void Draw(SpriteBatch spriteBatch, Vector2 center)
		{
			Texture2D tex = ModContent.Request<Texture2D>($"{FunnyExperience.ModName}/Assets/PassiveFrameSmall").Value;

			if (ModContent.HasAsset($"{FunnyExperience.ModName}/Assets/Passives/" + GetType().Name))
				tex = ModContent.Request<Texture2D>($"{FunnyExperience.ModName}/Assets/Passives/" + GetType().Name).Value;

			Color color = Color.Gray;

			if (CanAllocate(Main.LocalPlayer))
				color = Color.Lerp(Color.Gray, Color.White, (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.5f + 0.5f);

			if (Level > 0)
				color = Color.White;

			spriteBatch.Draw(tex, center, null, color, 0, tex.Size() / 2f, 1, 0, 0);

			if (MaxLevel > 1)
				Utils.DrawBorderString(spriteBatch, $"{Level}/{MaxLevel}", center + new Vector2(Width / 2f, Height / 2f), color, 1, 0.5f, 0.5f);
		}

		/// <summary>
		/// Called on load to generate the tree edges
		/// </summary>
		/// <param name="all"></param>
		public virtual void Connect(List<Passive> all, Player player) { }

		/// <summary>
		/// Attaches a node to this node, starting at this node and ending at this node.
		/// Remember, the tree is directed!
		/// </summary>
		/// <typeparam name="T">The type of node to connect to</typeparam>
		/// <param name="all">All nodes</param>
		protected void Connect<T>(List<Passive> all, Player player) where T : Passive
		{
			TreePlayer TreeSystem = player.GetModPlayer<TreePlayer>();
			TreeSystem.Edges.Add(new(this, all.FirstOrDefault(n => n is T)));
		}

		/// <summary>
		/// If this passive is able to be allocated or not
		/// </summary>
		/// <returns></returns>
		public bool CanAllocate(Player player)
		{
			TreePlayer TreeSystem = player.GetModPlayer<TreePlayer>();

			return
				Level < MaxLevel &&
				Main.LocalPlayer.GetModPlayer<TreePlayer>().Points > 0 &&
				(TreeSystem.Edges.Any(n => n.Start.Level > 0 && n.End == this) || !TreeSystem.Edges.Any(n => n.End == this));
		}

		/// <summary>
		/// If this passive can be refunded or not
		/// </summary>
		/// <returns></returns>
		public virtual bool CanDeallocate(Player player)
		{
			TreePlayer TreeSystem = player.GetModPlayer<TreePlayer>();

			return
				Level > 0 &&
				!(Level == 1 && TreeSystem.Edges.Any(n => n.End.Level > 0 && n.Start == this && !TreeSystem.Edges.Any(a => a != n && a.End == n.End && a.Start.Level > 0)));
		}
	}
}
