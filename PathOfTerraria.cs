global using Microsoft.Xna.Framework;
global using Microsoft.Xna.Framework.Graphics;
global using System;
global using Terraria;
global using Terraria.ModLoader;
using PathOfTerraria.API.GraphicsLib;

namespace PathOfTerraria
{
	public class PathOfTerraria : Mod
	{
		public static PathOfTerraria Instance;
		public static readonly string ModName = "PathOfTerraria";

		public PathOfTerraria()
		{
			Instance = this;
		}
		
		public override void Load()
		{
			PrimitiveDrawing.Init(Main.graphics.GraphicsDevice);
		}
	}
}