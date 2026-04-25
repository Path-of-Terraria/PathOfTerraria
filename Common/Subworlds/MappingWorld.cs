using PathOfTerraria.Common.Config;
using PathOfTerraria.Common.Subworlds.Passes;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using PathOfTerraria.Common.Systems.BossTrackingSystems;
using PathOfTerraria.Common.Systems.DisableBuilding;
using PathOfTerraria.Common.Systems.Synchronization;
using PathOfTerraria.Common.Systems.Synchronization.Handlers;
using PathOfTerraria.Common.UI;
using ReLogic.Content;
using SubworldLibrary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.Subworlds;

#nullable enable

/// <summary>
/// Contains persistent data for subworlds to use. This, by default, contains only a <see cref="BossDowned"/> bool, used by most domains,<br/>
/// but it is a class so it can be inherited to add arbitrary additional data.<br/><b>Most</b> of the code relating to this will not run on multiplayer clients,
/// so be careful.
/// </summary>
public class PersistentData
{
	public bool BossDowned = false;

	/// <summary>
	/// Automatically checks if the corresponding boss(es) have been downed, and sets <see cref="BossDowned"/> to true if so.
	/// </summary>
	public void CheckDowned<T>(params int[] id) where T : BossDomainSubworld
	{
		if (id.Length == 0)
		{
			throw new ArgumentException("There must be at least 1 IDs passed to CheckDowned.");
		}

		if (BossTracker.DownedInDomain<T>(id))
		{
			BossDowned = true;
		}
	}
}

/// <summary>
/// This is the base class for all mapping worlds. It sets the width and height of the world to 1000x1000 and enables world saving (or uses the configurable option in Debug).<br/>
/// Additionally, it also makes <see cref="StopBuildingPlayer"/> disable world modification,
/// and enables <see cref="Systems.ModPlayers.LivesSystem.BossDomainLivesPlayer"/>'s life system.
/// </summary>
public abstract class MappingWorld : Subworld
{
	public class MappingWorldInfo : ModSystem
	{
		public override void PostUpdateEverything()
		{
			if (SubworldSystem.Current is not null || !CloseActiveMapDeviceAfterFailedRun)
			{
				return;
			}

			if (HasActiveMapDevice())
			{
				if (!CloseMapDevicePortalHandler.TryClosePortal(ActiveMapDevicePosition))
				{
					return;
				}
			}

			ClearActiveMapDevice();
		}

		public override void SaveWorldData(TagCompound tag)
		{
			tag.Add("lastPath", LastSubworldSavePath);
			tag.Add("timesKeys", (string[])[.. TimesEnteredByDomain.Keys]);
			tag.Add("timesValues", (int[])[.. TimesEnteredByDomain.Values]);
		}

		public override void LoadWorldData(TagCompound tag)
		{
			LastSubworldSavePath = null;

			if (tag.TryGet("lastPath", out string value))
			{
				LastSubworldSavePath = value;
			}

			if (SubworldSystem.Current is null && tag.TryGet("timesKeys", out string[] times))
			{
				TimesEnteredByDomain = [];
				int[] values = tag.GetIntArray("timesValues");

				for (int i = 0; i < times.Length; ++i)
				{
					TimesEnteredByDomain.Add(times[i], values[i]);
				}
			}
		}
	}

	internal class DeleteOnServerHandler : Handler
	{
		public static void Send()
		{
			ModPacket packet = Networking.GetPacket<DeleteOnServerHandler>();
			packet.Send();
		}

		internal override void Receive(BinaryReader reader, byte sender)
		{
			DeleteSavedSubworld();
		}
	}

	internal class SetLastSubworldPathHandler : Handler
	{
		public static void Send(string value)
		{
			ModPacket packet = Networking.GetPacket<SetLastSubworldPathHandler>();
			packet.Write(value);
			packet.Send();
		}

		internal override void Receive(BinaryReader reader, byte sender)
		{
			string value = reader.ReadString();

			LastSubworldSavePath = value;
		}
	}

	public override int Width => 1000;
	public override int Height => 1000;

	public override bool ShouldSave =>
#if DEBUG
		ModContent.GetInstance<DeveloperConfig>().SaveSubworlds;
#else
		true;
#endif

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

	public static List<MapAffix> Affixes = null!;

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

	internal static string? LastSubworldSavePath { get; private set; }

	private static Point16 ActiveMapDevicePosition = new(-1, -1);
	private static bool CloseActiveMapDeviceAfterFailedRun;

	/// <summary>
	/// How many times a given subworld has been entered, indexed by <see cref="Mod.Name"/>/<see cref="Subworld"/>.GetType().Name.
	/// </summary>
	internal static Dictionary<string, int> TimesEnteredByDomain = [];

	internal static Dictionary<string, PersistentData> PersistentDomainInfo = [];

	public LocalizedText? SubworldName { get; private set; }
	public LocalizedText? SubworldDescription { get; private set; }
	public LocalizedText? SubworldMining { get; private set; }
	public LocalizedText? SubworldPlacing { get; private set; }

	/// <summary>
	/// The loading backgrounds for this <see cref="MappingWorld"/>. Used by <see cref="SubworldLoadingScreen"/>.
	/// </summary>
	public Asset<Texture2D>[] LoadingBackgrounds = [];

	private bool needsNetSync;

	public static int GetTimesEntered<T>() where T : Subworld
	{
		return GetTimesEntered(ModContent.GetInstance<T>());
	}

	public static int GetTimesEntered(Subworld subworld)
	{
		if (TimesEnteredByDomain.TryGetValue(subworld.FullName, out int value))
		{
			return value;
		}

		return 0;
	}

	/// <summary>
	/// Gets the amount of times this subworld has been entered.
	/// </summary>
	public int GetTimesEntered()
	{
		return GetTimesEntered(this);
	}

	public static PersistentData? GetData<T>() where T : Subworld
	{
		return GetData(ModContent.GetInstance<T>());
	}

	public static PersistentData? GetData(Subworld subworld)
	{
		if (PersistentDomainInfo.TryGetValue(subworld.FullName, out PersistentData? value))
		{
			return value;
		}

		if (SubworldSystem.IsActive(subworld.FullName))
		{
			PersistentDomainInfo.Add(subworld.FullName, new());
			return PersistentDomainInfo[subworld.FullName];
		}

		return null;
	}

	/// <summary>
	/// Gets the persistent data saved with this domain, if any. While a domain is active, this value should not be null.
	/// </summary>
	public PersistentData? GetData()
	{
		return GetData(this);
	}

	internal static void SetActiveMapDevice(Point16 position)
	{
		ActiveMapDevicePosition = position;
		CloseActiveMapDeviceAfterFailedRun = false;
	}

	internal static void RequestCloseActiveMapDeviceAfterFailedRun()
	{
		CloseActiveMapDeviceAfterFailedRun = true;
	}

	internal static void ClearActiveMapDevice()
	{
		ActiveMapDevicePosition = new(-1, -1);
		CloseActiveMapDeviceAfterFailedRun = false;
	}

	private static bool HasActiveMapDevice()
	{
		return ActiveMapDevicePosition.X >= 0 && ActiveMapDevicePosition.Y >= 0;
	}

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

	}

	public override void OnLoad()
	{
		if (SubworldSystem.Current is not null)
		{
			LastSubworldSavePath = TryGetCurrentPath();

			if (!TimesEnteredByDomain.TryAdd(FullName, 1))
			{
				TimesEnteredByDomain[FullName]++;
			}

			if (Main.netMode == NetmodeID.Server && SubworldSystem.Current is not null)
			{
				ModPacket packet = Networking.GetPacket<SetLastSubworldPathHandler>();
				packet.Write(LastSubworldSavePath!);
				Networking.SendPacketToMainServer(packet);
			}
		}

		Mod.Logger.Debug("Running" + " " + TimesEnteredByDomain[FullName]);
	}

	private static string? TryGetCurrentPath()
	{
		try
		{
			if (SubworldSystem.Current is null || SubworldSystem.Current.FileName is null)
			{
				return null;
			}

			WorldFileData main = GetMain(ModContent.GetInstance<SubworldSystem>());

			if (main is null)
			{
				return null;
			}

			return SubworldSystem.CurrentPath;
		}
		catch (NullReferenceException)
		{
			return null;
		}
	}

	[UnsafeAccessor(UnsafeAccessorKind.StaticField, Name = "main")]
	public static extern ref WorldFileData GetMain(SubworldSystem sys);

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
		if (!string.IsNullOrWhiteSpace(LastSubworldSavePath))
		{
			worldInfoTag.Add("subworldSavePath", LastSubworldSavePath);
		}

		if (HasActiveMapDevice())
		{
			worldInfoTag.Add("activeMapDeviceX", ActiveMapDevicePosition.X);
			worldInfoTag.Add("activeMapDeviceY", ActiveMapDevicePosition.Y);
		}

		if (CloseActiveMapDeviceAfterFailedRun)
		{
			worldInfoTag.Add("closeActiveMapDeviceAfterFailedRun", true);
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
		LastSubworldSavePath = worldInfoTag.TryGet("subworldSavePath", out string subworldSavePath) ? subworldSavePath : null;
		CloseActiveMapDeviceAfterFailedRun = worldInfoTag.GetBool("closeActiveMapDeviceAfterFailedRun");
		ActiveMapDevicePosition = worldInfoTag.TryGet("activeMapDeviceX", out short activeMapDeviceX) && worldInfoTag.TryGet("activeMapDeviceY", out short activeMapDeviceY)
			? new Point16(activeMapDeviceX, activeMapDeviceY)
			: new Point16(-1, -1);
		Affixes = [];

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
	}

	internal static void DeleteSavedSubworld(Subworld? subworld = null)
	{
		if (Main.netMode != NetmodeID.MultiplayerClient)
		{
			if (LastSubworldSavePath is { Length: > 0 } path)
			{
				DeleteSubworldFiles(path);
			}

			if (subworld is not null && TryGetSavePath(subworld) is { Length: > 0 } destinationPath && destinationPath != LastSubworldSavePath)
			{
				DeleteSubworldFiles(destinationPath);
			}
		}
		else if (Main.netMode == NetmodeID.MultiplayerClient)
		{
			DeleteOnServerHandler.Send();
		}

		LastSubworldSavePath = null;
	}

	private static string? TryGetSavePath(Subworld subworld)
	{
		try
		{
			WorldFileData main = GetMain(ModContent.GetInstance<SubworldSystem>()) ?? Main.ActiveWorldFileData;

			if (main is null)
			{
				return null;
			}

			string worldPath = main.IsCloudSave ? Main.CloudWorldPath : Main.WorldPath;
			return Path.Combine(worldPath, main.UniqueId.ToString(), subworld.FileName + ".wld");
		}
		catch (NullReferenceException)
		{
			return null;
		}
	}

	private static void DeleteSubworldFiles(string path)
	{
		DeleteSubworldFile(path);
		DeleteSubworldFile(path + ".bak");
		DeleteSubworldFile(Path.ChangeExtension(path, ".twld"));
		DeleteSubworldFile(Path.ChangeExtension(path, ".twld") + ".bak");
	}

	private static void DeleteSubworldFile(string path)
	{
		if (!File.Exists(path))
		{
			return;
		}

		try
		{
			File.Delete(path);
		}
		catch
		{
			PoTMod.Instance.Logger.Error($"[DeleteSavedSubworld] Failed to delete saved subworld file at '{path}'.");
		}
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
