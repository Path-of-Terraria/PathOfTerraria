global using Microsoft.Xna.Framework;
global using Microsoft.Xna.Framework.Graphics;
global using System;
global using Terraria;
global using Terraria.ModLoader;
using PathOfTerraria.API.GraphicsLib;
using PathOfTerraria.Content.Items.Gear;
using PathOfTerraria.Core.Sources;
using PathOfTerraria.Core.Systems.Networking;
using ReLogic.Content.Sources;
using System.IO;
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

		Core.PoTItem.GenerateItemList();
	}

	public override void HandlePacket(BinaryReader reader, int whoAmI)
	{
		Networking.HandlePacket(reader);
	}

	public override IContentSource CreateDefaultContentSource()
	{
		// Use our own SmartContentSource which wraps IContentSource with additional
		// behavior.
		var source = new SmartContentSource(base.CreateDefaultContentSource());

		// Redirects requests for ModName/Content/... to ModName/Assets/...
		source.AddDirectoryRedirect("Content", "Assets");

		return source;
	}
}