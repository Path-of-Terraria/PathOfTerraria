using PathOfTerraria.Common.NPCs.Dialogue;
using PathOfTerraria.Common.Systems.Synchronization;
using PathOfTerraria.Core.Sources;
using ReLogic.Content.Sources;
using System.Diagnostics;
using System.IO;
using Terraria.UI.Chat;

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

	private bool loadedNpcUtilsBestiaryHelper;
	private bool loadedNpcUtilsMod;

	public override void Load()
	{
		base.Load();

		NPCUtils.NPCUtils.AutoloadModBannersAndCritters(this);
		loadedNpcUtilsMod = true;
		NPCUtils.NPCUtils.TryLoadBestiaryHelper();
		loadedNpcUtilsBestiaryHelper = true;

		Debug.Assert(Name == ModName, "Internal mod name does not match expected constant.");
	}

	public override void Unload()
	{
		if (loadedNpcUtilsBestiaryHelper) { NPCUtils.NPCUtils.UnloadBestiaryHelper(); }
		if (loadedNpcUtilsMod) { NPCUtils.NPCUtils.UnloadMod(this); }
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