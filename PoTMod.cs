using PathOfTerraria.Common.Systems.Synchronization;
using PathOfTerraria.Core.Sources;
using ReLogic.Content.Sources;
using System.Diagnostics;
using System.IO;
using Wayfarer.API;

namespace PathOfTerraria;

/// <summary>
///		Path of Terraria <see cref="Mod"/> implementation.
/// </summary>
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

	/// <summary>
	///		Determines if any cheat mod is active. This is used to load certain cheat items, such as the spawn triggers for boss domains.
	/// </summary>
	public static bool CheatModEnabled => ModLoader.HasMod("CheatSheet") || ModLoader.HasMod("HerosMod") || ModLoader.HasMod("DragonLens");

	public override void Load()
	{
		base.Load();

		NPCUtils.NPCUtils.AutoloadModBannersAndCritters(this);
		NPCUtils.NPCUtils.TryLoadBestiaryHelper();
		
		Debug.Assert(Name == ModName, "Internal mod name does not match expected contsant.");
	}

	public override void Unload()
	{
		NPCUtils.NPCUtils.UnloadBestiaryHelper();
		NPCUtils.NPCUtils.UnloadMod(this);
		WayfarerAPI.Shutdown();
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