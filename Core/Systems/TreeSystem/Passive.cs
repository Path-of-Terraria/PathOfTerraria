using System.Collections.Generic;
using System.Linq;

namespace FunnyExperience.Core.Systems.TreeSystem
{
	internal abstract class Passive
	{
		public Vector2 treePos;

		public string name = "Unknown";
		public string tooltip = "Who knows what this will do!";

		public int level;
		public int maxLevel;

		public int width = 50;
		public int height = 50;

		public virtual void BuffPlayer(Player player) { }

		public virtual void Draw(SpriteBatch spriteBatch, Vector2 center)
		{
			Texture2D tex = ModContent.Request<Texture2D>("FunnyExperience/Assets/PassiveFrameSmall").Value;

			if (ModContent.HasAsset("FunnyExperience/Assets/Passives/" + GetType().Name))
				tex = ModContent.Request<Texture2D>("FunnyExperience/Assets/Passives/" + GetType().Name).Value;

			Color color = Color.Gray;

			if (CanAllocate())
				color = Color.Lerp(Color.Gray, Color.White, (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.5f + 0.5f);

			if (level > 0)
				color = Color.White;

			spriteBatch.Draw(tex, center, null, color, 0, tex.Size() / 2f, 1, 0, 0);

			Utils.DrawBorderString(spriteBatch, $"{level}/{maxLevel}", center + new Vector2(width / 2f, height / 2f), Color.White, 1, 0.5f, 0.5f);
		}

		/// <summary>
		/// Called on load to generate the tree edges
		/// </summary>
		/// <param name="all"></param>
		public virtual void Connect(List<Passive> all) { }

		/// <summary>
		/// Attaches a node to this node, starting at this node and ending at this node.
		/// Remember, the tree is directed!
		/// </summary>
		/// <typeparam name="T">The type of node to connect to</typeparam>
		/// <param name="all">All nodes</param>
		public void Connect<T>(List<Passive> all) where T : Passive
		{
			TreeSystem.edges.Add(new(this, all.FirstOrDefault(n => n is T)));
		}

		/// <summary>
		/// If this passive is unlocked or not
		/// </summary>
		/// <returns></returns>
		public bool CanAllocate()
		{
			return
				level < maxLevel &&
				Main.LocalPlayer.GetModPlayer<TreePlayer>().Points > 0 &&
				(TreeSystem.edges.Any(n => n.start.level > 0 && n.end == this) || !TreeSystem.edges.Any(n => n.end == this));
		}

		/// <summary>
		/// If this passive can be refunded or not
		/// </summary>
		/// <returns></returns>
		public bool CanDeallocate()
		{
			return
				level > 0 &&
				!(level == 1 && TreeSystem.edges.Any(n => n.end.level > 0 && n.start == this));
		}
	}
}
