using System.Collections.Generic;
using System.Linq;
using PathOfTerraria.Common.Subworlds.Passes;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using PathOfTerraria.Common.Systems.BossTrackingSystems;
using PathOfTerraria.Common.Systems.DisableBuilding;
using ReLogic.Content;
using ReLogic.Graphics;
using SubworldLibrary;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.ModLoader.IO;
using Terraria.UI.Chat;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.Subworlds;

/// <summary>
/// This is the base class for all mapping worlds. It sets the width and height of the world to 1000x1000 and disables world saving.<br/>
/// Additionally, it also makes <see cref="StopBuildingPlayer"/> disable world modification, 
/// and enables <see cref="Systems.ModPlayers.LivesSystem.BossDomainLivesPlayer"/>'s life system.
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

	private static int _walkTimer = 0;

	public LocalizedText SubworldName { get; private set; }
	public LocalizedText SubworldDescription { get; private set; }
	public LocalizedText SubworldMining { get; private set; }
	public LocalizedText SubworldPlacing { get; private set; }
	public Asset<Texture2D>[] LoadingBackgrounds = [];

	private string _tip = "";
	private string _fadingInTip = "";
	private int _tipTime = 0;
	private bool needsNetSync;

	public override void Load()
	{
		SubworldName = Language.GetOrRegister("Mods.PathOfTerraria.Subworlds." + GetType().Name + ".DisplayName", () => GetType().Name);
		SubworldDescription = Language.GetOrRegister("Mods.PathOfTerraria.Subworlds." + GetType().Name + ".Description", () => GetType().Name);
		SubworldMining = Language.GetOrRegister("Mods.PathOfTerraria.Subworlds." + GetType().Name + ".Mining", () => "\"{$DefaultMining}\"");
		SubworldPlacing = Language.GetOrRegister("Mods.PathOfTerraria.Subworlds." + GetType().Name + ".Placing", () => "\"{$DefaultPlacing}\"");

		// TODO: Change to throw
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

		TagCompound bossTrackerTag = SubworldSystem.ReadCopiedWorldData<TagCompound>("bossTracker");
		BossTracker.TotalBossesDowned = [.. bossTrackerTag.GetIntArray("total")];
		BossTracker.CachedBossesDowned = [.. bossTrackerTag.GetIntArray("cached")];

		TagCompound worldInfoTag = SubworldSystem.ReadCopiedWorldData<TagCompound>("worldInfo");
		AreaLevel = worldInfoTag.GetInt("level");
		MapTier = worldInfoTag.GetInt("tier");
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

	public override void DrawMenu(GameTime gameTime)
	{
		string statusText = Main.statusText;
		GenerationProgress progress = WorldGenerator.CurrentGenerationProgress;

		Main.spriteBatch.End();
		Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);

		DrawWalkingBackground();

		Main.spriteBatch.End();
		Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);

		if (SubworldSystem.Current is not null)
		{
			DrawStringCentered(Language.GetTextValue("Mods.PathOfTerraria.Subworlds.Entering"), Color.LightGray, new Vector2(0, -360), 0.4f, true);
			DrawStringCentered(SubworldName.Value, Color.White, new Vector2(0, -310), 1.1f, true);
			DrawStringCentered(SubworldDescription.Value, Color.White, new Vector2(0, -250), 0.5f, true);
		}
		else
		{
			DrawStringCentered(Language.GetTextValue("Mods.PathOfTerraria.Subworlds.Exiting"), Color.White, new Vector2(0, -310), 0.9f, true);
		}

		DrawWalkingPlayer();

		if (WorldGen.gen && progress is not null)
		{
			DrawStringCentered(progress.Message, Color.LightGray, new Vector2(0, 150), 0.6f);
			double percentage = progress.Value / progress.CurrentPassWeight * 100f;
			DrawStringCentered($"{percentage:#0.##}%", Color.LightGray, new Vector2(0, 190), 0.7f);
		}

		DrawStringCentered(statusText, Color.White, new Vector2(0, 90));

		if (_tip == "")
		{
			SetTip();
			_tipTime = 0;
		}

		_tipTime++;

		const int TipOffset = 438;

		DrawStringCentered(Language.GetTextValue("Mods.PathOfTerraria.UI.Tips.Title"), Color.White, new Vector2(0, TipOffset - 38), 0.8f);

		if (_tipTime > 300)
		{
			float factor = (_tipTime - 300) / 60f;
			DrawStringCentered(_tip, Color.White * (1 - factor), new Vector2(0, TipOffset), 0.5f);
			DrawStringCentered(_fadingInTip, Color.White * factor, new Vector2(0, TipOffset), 0.5f);

			if (_tipTime == 360)
			{
				SetTip(_fadingInTip);
				_tipTime = 0;
			}
		}
		else
		{
			DrawStringCentered(_tip, Color.White, new Vector2(0, TipOffset), 0.5f);
		}
	}

	private void DrawWalkingBackground()
	{
		_walkTimer++;

		var position = new Vector2(-_walkTimer * 1.3f, Main.screenHeight / 2 + 14);
		var originMod = new Vector2(0, 1);

		if (SubworldSystem.Current is null)
		{
			position = new Vector2(_walkTimer * 1.3f, Main.screenHeight / 2 + 14);
			originMod = new Vector2(1, 1);
		}

		if (LoadingBackgrounds.Length == 0)
		{
			return;
		}

		if (ScrollingBackgroundCount == 1)
		{
			Texture2D tex = LoadingBackgrounds[0].Value;
			Main.spriteBatch.Draw(tex, position, new Rectangle(0, 0, tex.Width * 300, tex.Height), Color.White, 0f, tex.Size() * originMod, 1f, SpriteEffects.None, 0);
		}
		else
		{
			Texture2D tex = ModContent.Request<Texture2D>("PathOfTerraria/Assets/UI/SubworldLoadScreens/DestroyerDomain0").Value;
			Texture2D tex2 = ModContent.Request<Texture2D>("PathOfTerraria/Assets/UI/SubworldLoadScreens/DestroyerDomain1").Value;
			int xOff = 0;

			for (int i = 0; i < 10; ++i)
			{
				Texture2D texture = LoadingBackgrounds[i % ScrollingBackgroundCount].Value;
				Main.spriteBatch.Draw(texture, position, null, Color.White, 0f, tex.Size() * originMod, 1f, SpriteEffects.None, 0);
				xOff += texture.Width;
			}
		}
	}

	private static void DrawWalkingPlayer()
	{
		Player plr = Main.LocalPlayer;
		using var _currentPlr = new Main.CurrentPlayerOverride(plr);

		plr.direction = SubworldSystem.Current is not null ? 1 : -1;
		plr.ResetEffects();
		plr.ResetVisibleAccessories();
		plr.UpdateMiscCounter();
		plr.UpdateDyes();
		plr.PlayerFrame();

		int num = (int)(Main.GlobalTimeWrappedHourly / 0.07f) % 14 + 6;
		plr.bodyFrame.Y = (plr.legFrame.Y = (plr.headFrame.Y = num * 56));
		plr.WingFrame(wingFlap: false);

		Item item = plr.inventory[plr.selectedItem];
		plr.inventory[plr.selectedItem] = new Item(ItemID.None);
		Main.PlayerRenderer.DrawPlayer(Main.Camera, plr, new Vector2(Main.screenWidth / 2f, Main.screenHeight / 2 - 126) + Main.screenPosition, 0f, Vector2.Zero, 0f, 1f);
		plr.inventory[plr.selectedItem] = item;
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

	private static void DrawStringCentered(string test, Color color, Vector2 position = default, float scale = 1f, bool outlined = false)
	{
		Vector2 screenCenter = new Vector2(Main.screenWidth, Main.screenHeight) / 2f + position;
		DynamicSpriteFont font = FontAssets.DeathText.Value;
		Vector2 halfSize = font.MeasureString(test) / 2f * scale;

		if (!outlined)
		{
			Main.spriteBatch.DrawString(font, test, screenCenter - halfSize, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0);
		}
		else
		{
			const int Offset = 6;

			Color shadowColor = Color.Black;
			Color textColor = Color.White;

			Vector2 drawPos = screenCenter - halfSize;
			ChatManager.DrawColorCodedStringShadow(Main.spriteBatch, font, test, drawPos - new Vector2(Offset, 0), shadowColor, 0f, Vector2.Zero, new Vector2(scale));
			ChatManager.DrawColorCodedStringShadow(Main.spriteBatch, font, test, drawPos - new Vector2(0, Offset), shadowColor, 0f, Vector2.Zero, new Vector2(scale));
			ChatManager.DrawColorCodedStringShadow(Main.spriteBatch, font, test, drawPos + new Vector2(Offset, 0), shadowColor, 0f, Vector2.Zero, new Vector2(scale));
			ChatManager.DrawColorCodedStringShadow(Main.spriteBatch, font, test, drawPos + new Vector2(0, Offset), shadowColor, 0f, Vector2.Zero, new Vector2(scale));
			ChatManager.DrawColorCodedString(Main.spriteBatch, font, test, drawPos, textColor, 0f, Vector2.Zero, new Vector2(scale));
		}
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