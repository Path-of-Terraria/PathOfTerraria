using ReLogic.Content.Sources;
using System.IO;
using JetBrains.Annotations;
using PathOfTerraria.Common.Systems.Networking;
using PathOfTerraria.Core.Sources;

namespace PathOfTerraria;

/// <summary>
///		Path of Terraria <see cref="Mod"/> implementation.
/// </summary>
[UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature | ImplicitUseKindFlags.Access)]
public sealed class PoTMod : Mod
{
	/// <summary>
	///		The internal name of the mod.
	/// </summary>
	public const string ModName = "PathOfTerraria";

	/// <summary>
	///		A static reference to the current instance of the mod.
	/// </summary>
	internal static PoTMod Instance => ModContent.GetInstance<PoTMod>();

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