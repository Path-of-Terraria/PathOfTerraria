global using Microsoft.Xna.Framework;
global using Microsoft.Xna.Framework.Graphics;
global using System;
global using Terraria;
global using Terraria.ModLoader;
using PathOfTerraria.API.GraphicsLib;
using PathOfTerraria.Content.Items.Gear;
using PathOfTerraria.Data;
using Terraria.ID;

namespace PathOfTerraria;

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
		if (Main.netMode != NetmodeID.Server)
		{
			PrimitiveDrawing.Init(Main.graphics.GraphicsDevice);
		}
		
		Gear.GenerateGearList();
	}
}