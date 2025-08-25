using System.Collections.Generic;
using System.Linq;
using PathOfTerraria.Common.Subworlds.Passes;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using PathOfTerraria.Common.Systems.BossTrackingSystems;
using PathOfTerraria.Common.Systems.DisableBuilding;
using ReLogic.Graphics;
using SubworldLibrary;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.Subworlds;

/// <summary>
/// This is the base class for all mapping worlds. It sets the width and height of the world to 1000x1000 and disables world saving.<br/>
/// Additionally, it also makes <see cref="StopBuildingPlayer"/> disable world modification, and enables <see cref="Systems.ModPlayers.LivesSystem.BossDomainLivesPlayer"/>'s life system.
/// </summary>
public abstract class MappingWorld : Subworld
{
	public override int Width => 1000;
	public override int Height => 1000;

	public override bool ShouldSave => false;
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

	// We are going to first set the world to be completely flat so we can build on top of that
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

	private string _tip = "";
	private string _fadingInTip = "";
	private int _tipTime = 0;
	
	internal virtual void ModifyDefaultWhitelist(HashSet<int> results, BuildingWhitelist.WhitelistUse use, List<FramedTileBlockers> blockers)
	{
	}

#pragma warning disable IDE0060 // Remove unused parameter
	protected static void ResetStep(GenerationProgress progress, GameConfiguration configuration)
#pragma warning restore IDE0060 // Remove unused parameter
	{
		WorldGenerator.CurrentGenerationProgress = progress;
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
		bossTrackerTag.Add("total", (int[])[.. BossTracker.TotalBossesDowned]);
		bossTrackerTag.Add("cached", (int[])[.. BossTracker.CachedBossesDowned]);
		SubworldSystem.CopyWorldData("bossTracker", bossTrackerTag);

		TagCompound worldInfoTag = [];
		worldInfoTag.Add("level", AreaLevel);
		worldInfoTag.Add("tier", MapTier);
		worldInfoTag.Add("affixes", (TagCompound[])[.. Affixes.Select(x => x.SaveAs())]);
		SubworldSystem.CopyWorldData("worldInfo", worldInfoTag);
	}

	public override void ReadCopiedMainWorldData()
	{
		base.ReadCopiedMainWorldData();
		ReadConsistentInfo();
	}

	private static void ReadConsistentInfo()
	{
		TagCompound tag = SubworldSystem.ReadCopiedWorldData<TagCompound>("tracker");
		var tracker = MappingDomainSystem.TiersDownedTracker.Load(tag);
		ModContent.GetInstance<MappingDomainSystem>().Tracker = tracker;

		TagCompound bossTrackerTag = SubworldSystem.ReadCopiedWorldData<TagCompound>("bossTracker");
		BossTracker.TotalBossesDowned = [.. bossTrackerTag.GetIntArray("total")];
		BossTracker.CachedBossesDowned = [.. bossTrackerTag.GetIntArray("cached")];

		TagCompound worldInfoTag = SubworldSystem.ReadCopiedWorldData<TagCompound>("worldInfo");
		AreaLevel = worldInfoTag.GetInt("level");
		MapTier = worldInfoTag.GetInt("tier");

		Affixes.Clear();
		TagCompound[] affixes = worldInfoTag.Get<TagCompound[]>("affixes");

		foreach (TagCompound affixTag in affixes)
		{
			Affixes.Add(Affix.FromTag<MapAffix>(affixTag));
		}
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

	public override void DrawMenu(GameTime gameTime)
	{
		string statusText = Main.statusText;
		GenerationProgress progress = WorldGenerator.CurrentGenerationProgress;

		if (WorldGen.gen && progress is not null)
		{
			DrawStringCentered(progress.Message, Color.LightGray, new Vector2(0, 60), 0.6f);
			double percentage = progress.Value / progress.CurrentPassWeight * 100f;
			DrawStringCentered($"{percentage:#0.##}%", Color.LightGray, new Vector2(0, 120), 0.7f);
		}

		DrawStringCentered(statusText, Color.White);

		if (_tip == "")
		{
			SetTip();
			_tipTime = 0;
		}

		_tipTime++;

		DrawStringCentered(Language.GetTextValue("Mods.PathOfTerraria.UI.Tips.Title"), Color.White, new Vector2(0, 300), 0.8f);

		if (_tipTime > 300)
		{
			float factor = (_tipTime - 300) / 60f;
			DrawStringCentered(_tip, Color.White * (1 - factor), new Vector2(0, 338), 0.5f);
			DrawStringCentered(_fadingInTip, Color.White * factor, new Vector2(0, 338), 0.5f);

			if (_tipTime == 360)
			{
				SetTip(_fadingInTip);
				_tipTime = 0;
			}
		}
		else
		{
			DrawStringCentered(_tip, Color.White, new Vector2(0, 338), 0.5f);
		}
	}

	/// <summary>
	/// Sets the tip to <paramref name="text"/>, or if <paramref name="text"/> is null, any random tip.
	/// </summary>
	private void SetTip(string text = null)
	{
		const int MaxTips = 31;

		_tip = text ?? Language.GetTextValue("Mods.PathOfTerraria.UI.Tips." + Main.rand.Next(MaxTips));

		do
		{
			_fadingInTip = Language.GetTextValue("Mods.PathOfTerraria.UI.Tips." + Main.rand.Next(MaxTips));
		} while (_tip == _fadingInTip);
	}

	private static void DrawStringCentered(string statusText, Color color, Vector2 position = default, float scale = 1f)
	{
		Vector2 screenCenter = new Vector2(Main.screenWidth, Main.screenHeight) / 2f + position;
		Vector2 halfSize = FontAssets.DeathText.Value.MeasureString(statusText) / 2f * scale;
		Main.spriteBatch.DrawString(FontAssets.DeathText.Value, statusText, screenCenter - halfSize, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0);
	}

	internal int ModifyExperience(int experience)
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