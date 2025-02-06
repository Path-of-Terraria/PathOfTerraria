using System.Collections.Generic;
using System.Linq;
using PathOfTerraria.Common.Subworlds.Passes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using PathOfTerraria.Common.Systems.DisableBuilding;
using ReLogic.Graphics;
using SubworldLibrary;
using Terraria.GameContent;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.Subworlds;

/// <summary>
/// This is the base class for all mapping worlds. It sets the width and height of the world to 1000x1000 and disables world saving.<br/>
/// Additionally, it also makes <see cref="StopBuildingPlayer"/> disable world modification.
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

	// We are going to first set the world to be completely flat so we can build on top of that
	public override List<GenPass> Tasks => [new FlatWorldPass()];

	/// <summary>
	/// Forces the time to be the given time, and it to be night/day. Defaults to (-1, true), which ignores this.
	/// </summary>
	public virtual (int time, bool isDay) ForceTime => (-1, true);

	public List<MapAffix> Affixes = null;

	/// <summary>
	/// The tier of the world. This modifies a lot of things:<br/>
	/// Defines the item level of the world, and consequently, the level and type of item that drops from enemies<br/>
	/// Above level 50, buffs enemies' damage and max health; see <see cref="MappingNPC"/>'s SetDefaults<br/>
	/// Above level 50, buffs enemy gear droprate and rarity; see <see cref="Systems.MobSystem.ArpgNPC"/>.
	/// </summary>
	public int AreaLevel = 0;
	
	internal virtual void ModifyDefaultWhitelist(HashSet<int> results, BuildingWhitelist.WhitelistUse use)
	{
	}

#pragma warning disable IDE0060 // Remove unused parameter
	protected static void ResetStep(GenerationProgress progress, GameConfiguration configuration)
#pragma warning restore IDE0060 // Remove unused parameter
	{
		WorldGenerator.CurrentGenerationProgress = progress;
		Main.ActiveWorldFileData.SetSeedToRandom();
		GenVars.structures = new();
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

	internal float TotalWeight()
	{
		if (Affixes is null || Affixes.Count == 0)
		{
			return 0;
		}

		return Affixes.Sum(x => x.Strength);
	}
}