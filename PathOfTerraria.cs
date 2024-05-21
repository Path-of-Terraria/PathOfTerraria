global using Microsoft.Xna.Framework;
global using Microsoft.Xna.Framework.Graphics;
global using System;
global using Terraria;
global using Terraria.ModLoader;

namespace PathOfTerraria
{
	public class PathOfTerraria : Mod
	{
		public static PathOfTerraria Instance;
		public static string ModName = "PathOfTerraria";

		public PathOfTerraria()
		{
			Instance = this;
		}
	}
}