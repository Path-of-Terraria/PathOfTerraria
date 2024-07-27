using PathOfTerraria.Content.Items.Gear;
using ReLogic.Content.Sources;
using System.IO;
using PathOfTerraria.Common;
using PathOfTerraria.Common.Systems.Networking;
using PathOfTerraria.Core.Graphics;
using PathOfTerraria.Core.Sources;
using Terraria.ID;

namespace PathOfTerraria;

public sealed class PathOfTerraria : Mod
{
	public static PathOfTerraria Instance => ModContent.GetInstance<PathOfTerraria>();

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