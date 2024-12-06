using PathOfTerraria.Common.Subworlds.Passes;
using PathOfTerraria.Common.Systems;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.IO;
using Terraria.WorldBuilding;
using ReLogic.Graphics;
using SubworldLibrary;
using PathOfTerraria.Common.Systems.DisableBuilding;

namespace PathOfTerraria.Common.Subworlds;

/// <summary>
/// This is used to stop bosses from doing special death effects (like King Slime spawning the town slime, WoF spawning hardmode) automatically,<br/>
/// using a nicer loading screen dialogue, and setting <see cref="WorldGenerator.CurrentGenerationProgress"/> as it's not set by default.<br/>
/// The death effect system is in <see cref="BossTracker.CachedBossesDowned"/>.
/// </summary>
public abstract class BossDomainSubworld : MappingWorld
{
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
	/// The level of dropped <see cref="Content.Items.Gear.Gear"/> in the domain. 0 will roll default level formula.
	/// </summary>
	public virtual int DropItemLevel => 0;

	/// <summary>
	/// Forces the time to be the given time, and it to be night/day. Defaults to (-1, true), which ignores this.
	/// </summary>
	public virtual (int time, bool isDay) ForceTime => (-1, true);

	// We are going to first set the world to be completely flat so we can build on top of that
	public override List<GenPass> Tasks => [new FlatWorldPass()];

#pragma warning disable IDE0060 // Remove unused parameter
	protected static void ResetStep(GenerationProgress progress, GameConfiguration configuration)
#pragma warning restore IDE0060 // Remove unused parameter
	{
		WorldGenerator.CurrentGenerationProgress = progress;
		Main.ActiveWorldFileData.SetSeedToRandom();
		GenVars.structures = new();
	}

	public override void OnEnter()
	{
		base.OnEnter();

		SubworldSystem.noReturn = true;
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

	internal virtual void ModifyDefaultWhitelist(HashSet<int> results, BuildingWhitelist.WhitelistUse use)
	{
	}
}
