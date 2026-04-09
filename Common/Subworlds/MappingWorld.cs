using System.Collections.Generic;
using System.Linq;
using PathOfTerraria.Common.Subworlds.Passes;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using PathOfTerraria.Common.Systems.BossTrackingSystems;
using PathOfTerraria.Common.Systems.DisableBuilding;
using PathOfTerraria.Common.UI;
using PathOfTerraria.Common.UI.SubworldHelp;
using PathOfTerraria.Content.Tiles.Furniture;
using ReLogic.Content;
using SubworldLibrary;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.Subworlds;

/// <summary>
/// This is the base class for all mapping worlds. It sets the width and height of the world to 1000x1000 and disables world saving.<br/>
/// Additionally, it also makes <see cref="StopBuildingPlayer"/> disable world modification, 
/// and enables <see cref="Systems.ModPlayers.LivesSystem.BossDomainLivesPlayer"/>'s life system.
/// </summary>
public abstract class MappingWorld : Subworld
{
	/// <summary> How many times this subworld type has been started on this client or server. </summary>
	public uint TimesEntered { get; private set; }

	public override int Width => 1000;
	public override int Height => 1000;

	public override bool ShouldSave => !ClosePortalOnReturn;
	public override bool NoPlayerSaving => false;
	
	/// <summary>
	/// These tiles are allowed to be mined by the player using a pickaxe.
	/// </summary>
	public virtual int[] WhitelistedMiningTiles => [];

	/// <summary>
	/// These tiles are allowed to be cut by the player with melee or projectiles.
	/// </summary>
	public virtual int[] WhitelistedCutTiles => [];

	/// <summary>
	/// These tiles are allowed to be placed by the player. These should also be in <see cref="WhitelistedMiningTiles"/>.
	/// </summary>
	public virtual int[] WhitelistedPlaceableTiles => [];

	/// <summary>
	/// These tiles are allowed to explode, by the player or otherwise.
	/// </summary>
	public virtual int[] WhitelistedExplodableTiles => [];

	/// <summary>
	/// The amount of scrolling backgrounds this domain has. Defaults to 1.
	/// </summary>
	public virtual int ScrollingBackgroundCount => 1;

	public override List<GenPass> Tasks => [new FlatWorldPass()];

	/// <summary>
	/// Forces the time to be the given time, and it to be night/day. Defaults to (-1, true), which ignores this.
	/// </summary>
	public virtual (int time, bool isDay) ForceTime => (-1, true);

	public static List<MapAffix> Affixes = null;

	/// <summary>
	/// The level of the world. This modifies a lot of things:<br/>
	/// Defines the item level of the world, and consequently, the level and type of item that drops from enemies<br/>
	/// Above level 50, buffs enemies' damage and max health; see <see cref="MappingNPC"/>'s SetDefaults<br/>
	/// Above level 50, buffs enemy gear droprate and rarity; see <see cref="Systems.MobSystem.ArpgNPC"/>.
	/// </summary>
	public static int AreaLevel = 0;

	/// <summary>
	/// The map tier. This is the unconverted version of <see cref="AreaLevel"/>; the level should be used more often.<br/>
	/// This is kept as the map tier is used for a couple of things, namely <see cref="MappingDomainSystem.Tracker"/>.
	/// </summary>
	public static int MapTier = 0;
	public static Point16 ActiveMapDevicePosition = new(-1, -1);
	public static bool ClosePortalOnReturn;
	/// <summary>
	/// The file path of the last saved subworld, used to delete the save when the portal closes.
	/// </summary>
	internal static string LastSubworldSavePath;
	private static bool pendingPortalStateApply;
	private static bool pendingPortalActive;
	private static int pendingPortalUsesLeft;
	private static Item pendingPortalMap = new();
	private static (int Id, int Amount)? pendingPortalInjection;

	public LocalizedText SubworldName { get; private set; }
	public LocalizedText SubworldDescription { get; private set; }
	public LocalizedText SubworldMining { get; private set; }
	public LocalizedText SubworldPlacing { get; private set; }

	/// <summary>
	/// The loading backgrounds for this <see cref="MappingWorld"/>. Used by <see cref="SubworldLoadingScreen"/>.
	/// </summary>
	public Asset<Texture2D>[] LoadingBackgrounds = [];

	private bool needsNetSync;

	public override void Load()
	{
		SubworldName = Language.GetOrRegister("Mods.PathOfTerraria.Subworlds." + GetType().Name + ".DisplayName", () => GetType().Name);
		SubworldDescription = Language.GetOrRegister("Mods.PathOfTerraria.Subworlds." + GetType().Name + ".Description", () => GetType().Name);
		SubworldMining = Language.GetOrRegister("Mods.PathOfTerraria.Subworlds." + GetType().Name + ".Mining", () => "{$DefaultMining}");
		SubworldPlacing = Language.GetOrRegister("Mods.PathOfTerraria.Subworlds." + GetType().Name + ".Placing", () => "{$DefaultPlacing}");

		if (!Main.dedServ)
		{
			LoadLoadingScreens();
		}
	}

	public override void OnEnter()
	{
		TimesEntered++;

		// Capture the save file path early so it can be deleted when the portal is closed.
		// CurrentPath is only valid while the subworld is active.
		LastSubworldSavePath = SubworldSystem.CurrentPath;
	}


	private void LoadLoadingScreens()
	{
		if (ScrollingBackgroundCount == 1 && ModContent.RequestIfExists("PathOfTerraria/Assets/UI/SubworldLoadScreens/" + GetType().Name, out Asset<Texture2D> image))
		{
			LoadingBackgrounds = [image];
		}
		else if (ScrollingBackgroundCount > 1)
		{
			LoadingBackgrounds = new Asset<Texture2D>[ScrollingBackgroundCount];

			for (int i = 0; i < ScrollingBackgroundCount; i++)
			{
				if (ModContent.RequestIfExists($"PathOfTerraria/Assets/UI/SubworldLoadScreens/{GetType().Name}_{i}", out Asset<Texture2D> background))
				{
					LoadingBackgrounds[i] = background;
				}
			}
		}
	}

	internal virtual void ModifyDefaultWhitelist(HashSet<int> results, BuildingWhitelist.WhitelistUse use, List<FramedTileBlockers> blockers)
	{
	}

#pragma warning disable IDE0060 // Remove unused parameter
	protected static void ResetStep(GenerationProgress progress, GameConfiguration configuration)
#pragma warning restore IDE0060 // Remove unused parameter
	{
		WorldGenerator.CurrentGenerationProgress = progress;
		progress.CurrentPassWeight = 1;
		Main.ActiveWorldFileData.SetSeedToRandom();
		GenVars.structures = new();
		WorldGen._genRandSeed = Main.rand.Next();

		int rand = Main.rand.Next(10, 30);
		
		for (int i = 0; i < rand; ++i)
		{
			WorldGen.genRand.Next();
		}
	}

	public override void CopyMainWorldData()
	{
		base.CopyMainWorldData();

		CopyConsistentInfo();
	}

	public static void SetActiveMapDevice(Point16 position)
	{
		ActiveMapDevicePosition = position;
		ClosePortalOnReturn = false;
	}

	public static void MarkActivePortalForClosure()
	{
		if (HasActiveMapDevice())
		{
			ClosePortalOnReturn = true;
		}
	}

	public static void ClearActiveMapDevice()
	{
		ActiveMapDevicePosition = new Point16(-1, -1);
		ClosePortalOnReturn = false;
	}

	/// <summary>
	/// Deletes the saved subworld file so the next portal entry generates a fresh world.
	/// </summary>
	internal static void DeleteSavedSubworld()
	{
		if (LastSubworldSavePath is { Length: > 0 } && System.IO.File.Exists(LastSubworldSavePath))
		{
			try
			{
				System.IO.File.Delete(LastSubworldSavePath);
			}
			catch
			{
				// Best-effort cleanup; file may be locked or already removed.
			}
		}

		LastSubworldSavePath = null;
	}

	private static void CopyConsistentInfo()
	{
		TagCompound trackerTag = [];
		ModContent.GetInstance<MappingDomainSystem>().Tracker.Save(trackerTag);
		SubworldSystem.CopyWorldData("tracker", trackerTag);

		TagCompound bossTrackerTag = [];
		BossTracker.WriteConsistentInfo(bossTrackerTag);
		SubworldSystem.CopyWorldData("bossTracker", bossTrackerTag);

		TagCompound eventTrackerTag = [];
		EventTracker.WriteConsistentInfo(eventTrackerTag);
		SubworldSystem.CopyWorldData("eventTracker", eventTrackerTag);

		TagCompound worldInfoTag = [];
		worldInfoTag.Add("level", AreaLevel);
		worldInfoTag.Add("tier", MapTier);
		worldInfoTag.Add("closePortalOnReturn", ClosePortalOnReturn);

		if (HasActiveMapDevice())
		{
			worldInfoTag.Add("activeMapDeviceX", ActiveMapDevicePosition.X);
			worldInfoTag.Add("activeMapDeviceY", ActiveMapDevicePosition.Y);

			// When called from CopySubworldData, the map device entity doesn't exist in the subworld.
			// Fall back to the cached pending values that were read when entering.
			if (TileEntity.ByPosition.TryGetValue(ActiveMapDevicePosition, out TileEntity tileEntity) && tileEntity is MapDeviceEntity device)
			{
				worldInfoTag.Add("activeMapPortalActive", device.PortalActive);
				worldInfoTag.Add("activeMapPortalUsesLeft", device.PortalUsesLeft);

				if (device.ActiveMap is { IsAir: false })
				{
					worldInfoTag.Add("activeMapPortalItem", ItemIO.Save(device.ActiveMap));
				}

				if (device.Injection is { } injection)
				{
					worldInfoTag.Add("activeMapInjectionId", injection.Id);
					worldInfoTag.Add("activeMapInjectionAmount", injection.Amount);
				}
			}
			else
			{
				worldInfoTag.Add("activeMapPortalActive", pendingPortalActive);
				worldInfoTag.Add("activeMapPortalUsesLeft", pendingPortalUsesLeft);

				if (pendingPortalMap is { IsAir: false })
				{
					worldInfoTag.Add("activeMapPortalItem", ItemIO.Save(pendingPortalMap));
				}

				if (pendingPortalInjection is { } injection)
				{
					worldInfoTag.Add("activeMapInjectionId", injection.Id);
					worldInfoTag.Add("activeMapInjectionAmount", injection.Amount);
				}
			}
		}

		if (Affixes is not null && Affixes.Count > 0)
		{
			worldInfoTag.Add("affixes", (TagCompound[])[.. Affixes.Select(x => x.SaveAs())]);
		}
		
		SubworldSystem.CopyWorldData("worldInfo", worldInfoTag);
	}

	public override void ReadCopiedMainWorldData()
	{
		base.ReadCopiedMainWorldData();
		ReadConsistentInfo();
	}

	public override void Update()
	{
		base.Update();

		if (needsNetSync && Main.netMode == NetmodeID.Server)
		{
			NetMessage.SendData(MessageID.WorldData);
			needsNetSync = false;
		}
	}

	private void ReadConsistentInfo()
	{
		TagCompound tag = SubworldSystem.ReadCopiedWorldData<TagCompound>("tracker");
		var tracker = MappingDomainSystem.TiersDownedTracker.Load(tag);
		ModContent.GetInstance<MappingDomainSystem>().Tracker = tracker;

		BossTracker.ReadConsistentInfo(SubworldSystem.ReadCopiedWorldData<TagCompound>("bossTracker"));
		EventTracker.ReadConsistentInfo(SubworldSystem.ReadCopiedWorldData<TagCompound>("eventTracker"));

		TagCompound worldInfoTag = SubworldSystem.ReadCopiedWorldData<TagCompound>("worldInfo");
		AreaLevel = worldInfoTag.GetInt("level");
		MapTier = worldInfoTag.GetInt("tier");
		ClosePortalOnReturn = worldInfoTag.GetBool("closePortalOnReturn");
		Affixes = [];

		if (worldInfoTag.ContainsKey("activeMapDeviceX") && worldInfoTag.ContainsKey("activeMapDeviceY"))
		{
			ActiveMapDevicePosition = new Point16(worldInfoTag.GetShort("activeMapDeviceX"), worldInfoTag.GetShort("activeMapDeviceY"));
		}
		else
		{
			ActiveMapDevicePosition = new Point16(-1, -1);
		}

		pendingPortalActive = worldInfoTag.GetBool("activeMapPortalActive");
		pendingPortalUsesLeft = worldInfoTag.GetInt("activeMapPortalUsesLeft");
		pendingPortalMap = worldInfoTag.TryGet("activeMapPortalItem", out TagCompound activeMapTag) ? ItemIO.Load(activeMapTag) : new();
		pendingPortalInjection = worldInfoTag.ContainsKey("activeMapInjectionId")
			? (worldInfoTag.GetInt("activeMapInjectionId"), worldInfoTag.GetInt("activeMapInjectionAmount"))
			: null;

		if (worldInfoTag.TryGet("affixes", out TagCompound[] affixes))
		{
			foreach (TagCompound affixTag in affixes)
			{
				Affixes.Add(Affix.FromTag<MapAffix>(affixTag));
			}
		}

		needsNetSync |= Main.netMode == NetmodeID.Server;
	}

	public override void CopySubworldData()
	{
		base.CopySubworldData();
		CopyConsistentInfo();

	}

	public override void ReadCopiedSubworldData()
	{
		base.ReadCopiedSubworldData();
		ReadConsistentInfo();
		pendingPortalStateApply = HasActiveMapDevice();
	}

	internal static bool HasActiveMapDevice()
	{
		return ActiveMapDevicePosition.X >= 0 && ActiveMapDevicePosition.Y >= 0;
	}

	internal static void TryApplyTrackedPortalState()
	{
		if (!pendingPortalStateApply || !HasActiveMapDevice())
		{
			return;
		}

		if (!TileEntity.ByPosition.TryGetValue(ActiveMapDevicePosition, out TileEntity tileEntity) || tileEntity is not MapDeviceEntity device)
		{
			return;
		}

		if (ClosePortalOnReturn)
		{
			device.TryClosingPortal();
			pendingPortalStateApply = false;
			return;
		}

		device.PortalActive = pendingPortalActive;
		device.PortalUsesLeft = pendingPortalUsesLeft;
		device.StoredMap = new();
		device.ActiveMap = pendingPortalMap.Clone();
		device.Injection = pendingPortalInjection;

		if (Main.netMode == NetmodeID.Server)
		{
			MapDeviceSync.Send(device.ID, MapDeviceSync.Flags.FullSync);
		}

		pendingPortalStateApply = false;
	}

	public override void DrawMenu(GameTime gameTime)
	{
		SubworldLoadingScreen.DrawLoading(this);
	}

	public virtual void ModifyLoadScreenDescripton(ref string input)
	{
	}

	/// <summary>
	/// Allows dynamic modification of the tooltips displayed in the Subworld Help UI element under the inventory.<br/>
	/// Valid names for finding indexes are:<br/>
	/// <b>Info</b> - "Info" header<br/>
	/// <b>Name</b> - Name of the domain<br/>
	/// <b>Desc</b> - Description of the domain<br/>
	/// <b>Mining</b> - Mining blocklist<br/>
	/// <b>Placing</b> - Placing blocklist<br/>
	/// <b>Tier</b> - Only appears if this is a high enough level to have a tier (level 45+). Displays the map's current tier<br/><br/>
	/// The following only appear when the map have affixes<br/>
	/// <b>AffixHeading</b> - Affix header<br/>
	/// <b>MapAffix0..N</b> - Affix information for the given slot<br/>
	/// <b>ModifierStrength</b> - "Map Strength" modifier, which is the value used for calculating benefits<br/>
	/// <b>ExpMod</b> - Experience modifier<br/>
	/// <b>RateMod</b> - Drop rate modifier<br/>
	/// <b>RarityMod</b> - Drop rarity modifier<br/>
	/// </summary>
	public virtual void ModifyHelpTooltips(List<DrawableTooltipLine> lines, Vector2 scale)
	{
	}

	internal static int ModifyExperience(int experience)
	{
		return experience + (int)(TotalWeight() / 200f * experience);
	}

	internal static float TotalWeight()
	{
		if (Affixes is null || Affixes.Count == 0)
		{
			return 0;
		}

		return Affixes.Sum(x => x.Strength);
	}
}

internal sealed class MappingPortalReturnSystem : ModSystem
{
	public override void PostUpdateWorld()
	{
		if (SubworldSystem.Current is MappingWorld)
		{
			return;
		}

		MappingWorld.TryApplyTrackedPortalState();
	}
}
