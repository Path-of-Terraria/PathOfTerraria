using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using PathOfTerraria.Common.UI.Components;
using PathOfTerraria.Common.UI.Elements;
using PathOfTerraria.Content.NPCs.Mapping.Forest;
using PathOfTerraria.Core.UI;
using PathOfTerraria.Core.UI.SmartUI;
using PathOfTerraria.Utilities.Terraria;
using PathOfTerraria.Utilities.Xna;
using ReLogic.Content;
using ReLogic.OS;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using Terraria.ModLoader.UI;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;
using FixedUIScrollbar = Terraria.GameContent.UI.Elements.FixedUIScrollbar;

#nullable enable
#pragma warning disable IDE0023 // Use block body for conversion operator.
#pragma warning disable IDE0053 // Use expression body for lambda expression
#pragma warning disable IDE0028, IDE0306 // Collection initialization can be 'simplified'.

namespace PathOfTerraria.Common.Encounters;

internal struct EncounterBox(Encounter handle, EncounterDescription description)
{
	public Encounter Handle = handle;
	public object Description = description with { Waves = null! };
	public List<WaveBox> Waves = new(description.Waves.Select(w => (WaveBox)w));
	public static explicit operator EncounterBox(Encounter src) => new(src, src.Description);
}
internal struct WaveBox(EncounterWave wave, SpawnBox[] spawns)
{
	public object Wave = wave with { Spawns = null! };
	public List<SpawnBox> Spawns = new(spawns);
	public static explicit operator WaveBox(EncounterWave src) => new(src, src.Spawns.Select(s => (SpawnBox)s).ToArray());
}
internal struct SpawnBox(EnemySpawn spawn, SpawnPlacement? placement)
{
	public object Spawn = spawn;
	public object Placement = placement ?? EncounterEditor.DefaultPlacement.WithDefaults(spawn.NpcType.Type);
	public static explicit operator SpawnBox(EnemySpawn src) => new(src, src.SpawnPlacement);
}

[Autoload(Side = ModSide.Client)]
internal sealed class EncounterEditor : ModSystem
{
	// C# unions never.
	public enum DragKind
	{
		EncounterOrigin,
		EncounterArea,
		SpawnPosition,
	}
	public struct DragContext()
	{
		public required DragKind Kind;
		public required RectangleDrag Drag;
		public bool? MouseLeftValueToStopAt = false;
		public bool? MouseRightValueToStopAt;
	}

	private static bool wasWritingTextInPreviousTick;
	private static LegacyGameInterfaceLayer? gizmosLayer;
#if DEBUG
	private static ModKeybind keyToggleEncounterDebugging = null!;
#endif

	public static SpawnBox BoxedSpawn;
	public static EncounterBox[] BoxedEncounters = []; // Sparse array, may contain zeroed values.

	public static SpawnPlacement DefaultPlacement { get; } = new() { Area = default, CollisionSize = default };
	public static EncounterEditorState State { get; private set; } = null!;
	public static DragContext? ActiveDrag { get; set; }
	public static bool PlacementMode { get; set; }

	public static bool WritingText => PlayerInput.WritingText | wasWritingTextInPreviousTick;

	public override void Load()
	{
#if DEBUG
		keyToggleEncounterDebugging = KeybindLoader.RegisterKeybind(Mod, "ToggleEncounterDebugging", Microsoft.Xna.Framework.Input.Keys.NumPad4);
#endif
	}

	public override void PostSetupContent()
	{
		State = SmartUiLoader.GetUiState<EncounterEditorState>();
	}

	public override void PreUpdateEntities()
	{
#if DEBUG
		if (keyToggleEncounterDebugging.JustPressed)
		{
			State.Toggle();
		}
#endif

		if (!State.Visible) { return; }

		HandlePlacement();
		DragObjects();
		ExportData();
	}

	public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
	{
		if (State.Visible)
		{
			FixMouseInterface();
			gizmosLayer ??= new LegacyGameInterfaceLayer($"{nameof(PathOfTerraria)}: Encounter Gizmos", DrawGizmos, InterfaceScaleType.Game);
			layers.Insert(Math.Max(0, layers.FindIndex(l => l.Name.Equals("Vanilla: Mouse Text")) - 1), gizmosLayer);
		}
	}

	public override void PostDrawInterface(SpriteBatch spriteBatch)
	{
		wasWritingTextInPreviousTick = PlayerInput.WritingText;
	}

	public override void ClearWorld()
	{
		State.Visible = false;
	}

	/// <summary> Updates internal box copies from the provided encounter description. Returns whether any operation has occurred. </summary>
	public static bool ImportData(bool onlyNewEntries = false)
	{
		// Create a tree of boxed objects.
		bool didAnything = false;
		Array.Resize(ref BoxedEncounters, (int)EnemyEncounters.Capacity);

		foreach (Encounter encounter in EnemyEncounters.IterateEncounters())
		{
			if (!onlyNewEntries || BoxedEncounters[encounter.Index].Description == null)
			{
				BoxedEncounters[encounter.Index] = (EncounterBox)encounter;
				didAnything = true;
			}
		}

		return didAnything;
	}

	/// <summary> Synchronizes all encounters' data with their internal boxed copies. </summary>
	public static void ExportData()
	{
		bool needsRebuild = false;

		needsRebuild |= ImportData(onlyNewEntries: true);

		for (int encounterIndex = 0; encounterIndex < BoxedEncounters.Length; encounterIndex++)
		{
			ref EncounterBox encounterBox = ref BoxedEncounters[encounterIndex];

			if (!encounterBox.Handle.IsValid) { continue; }

			if (encounterBox.Waves == default) { continue; }

			bool isDifferent = false;
			List<WaveBox> waveBoxes = encounterBox.Waves;
			ref readonly EncounterDescription refDesc = ref encounterBox.Handle.Description;
			ref EncounterDescription boxDesc = ref Unsafe.Unbox<EncounterDescription>(encounterBox.Description!);
			EncounterDescription dstDesc = boxDesc with { Waves = refDesc.Waves };

			// Update wave destination array sizes.
			if (refDesc.Waves.Length != waveBoxes.Count)
			{
				(EncounterWave[] waves, int oldLength, int newLength) = (refDesc.Waves, refDesc.Waves.Length, waveBoxes.Count);
				Array.Resize(ref waves, newLength);
				for (int i = oldLength; i < newLength; i++) { waves[i] = ((EncounterWave)waveBoxes[i].Wave) with { Spawns = [] }; }

				(dstDesc.Waves, isDifferent, needsRebuild) = (waves, true, true);
			}

			for (int waveIndex = 0, leastWaves = Math.Min(waveBoxes.Count, refDesc.Waves.Length); waveIndex < leastWaves; waveIndex++)
			{
				List<SpawnBox> spawnBoxes = waveBoxes[waveIndex].Spawns;
				ref EncounterWave refWave = ref refDesc.Waves[waveIndex];
				ref EncounterWave boxWave = ref Unsafe.Unbox<EncounterWave>(waveBoxes[waveIndex].Wave);
				EncounterWave dstWave = boxWave with { Spawns = refWave.Spawns };

				// Update wave contents.
				if (refWave != dstWave)
				{
					refWave = dstWave;
					isDifferent = true;
				}

				// Update spawn destination array sizes.
				if (refWave.Spawns.Length != spawnBoxes.Count)
				{
					(EnemySpawn[] spawns, int oldLength, int newLength) = (refWave.Spawns, refWave.Spawns.Length, spawnBoxes.Count);
					Array.Resize(ref spawns, newLength);
					for (int i = oldLength; i < newLength; i++) { spawns[i] = (EnemySpawn)spawnBoxes[i].Spawn; }

					(refWave.Spawns, isDifferent, needsRebuild) = (spawns, true, true);
				}

				// Update spawn destination array contents.
				for (int spawnIndex = 0; spawnIndex < refWave.Spawns.Length; spawnIndex++)
				{
					ref EnemySpawn refSpawn = ref refWave.Spawns[spawnIndex];
					ref EnemySpawn boxSpawn = ref Unsafe.Unbox<EnemySpawn>(spawnBoxes[spawnIndex].Spawn);
					ref SpawnPlacement boxPlacement = ref Unsafe.Unbox<SpawnPlacement>(spawnBoxes[spawnIndex].Placement);

					if (boxSpawn != refSpawn)
					{
						needsRebuild |= boxSpawn.NpcType.Type != refSpawn.NpcType.Type;
						(refSpawn, isDifferent) = (boxSpawn, true);
					}

					if (refSpawn.SpawnPlacement.HasValue && refSpawn.SpawnPlacement.Value != boxPlacement)
					{
						(refSpawn.SpawnPlacement, isDifferent) = (boxPlacement, true);
					}
				}
			}

			isDifferent |= refDesc != dstDesc;

			if (isDifferent)
			{
				// Sanitize.
				dstDesc.Identifier = !dstDesc.Identifier.All(char.IsAsciiLetterOrDigit) ? new string(dstDesc.Identifier.Where(char.IsAsciiLetterOrDigit).ToArray()) : dstDesc.Identifier;
				dstDesc.Identifier = dstDesc.Identifier.Length == 0 ? refDesc.Identifier : dstDesc.Identifier;

				needsRebuild |= dstDesc.Identifier != refDesc.Identifier;
				encounterBox.Handle.ModifyDescription(dstDesc);
			}
		}

		if (needsRebuild)
		{
			State.Rebuild();
		}
	}

	public static void AddEncounter()
	{
		Vector2 basePosition = Main.LocalPlayer.Center;
		Point16 basePoint = basePosition.ToTileCoordinates16();
		var dummyWave = new EncounterWave { Spawns = [] };

		Encounter encounter = EnemyEncounters.CreateEncounter(new EncounterDescription
		{
			Identifier = "NewEncounter",
			ActivationRange = 512f,
			ActivationOrigin = basePosition,
			SpawnArea = new Rectangle(basePoint.X - 32, basePoint.Y - 16, 64, 32),
			SpawnOrigin = basePoint,
			Waves = [dummyWave],
		});

		encounter.SetPaused(true);
		State.SetSelections(encounter, 0, -1);
	}

	public static void RemoveEncounter(Encounter encounter)
	{
		encounter.Remove();
		State.Rebuild();
	}

	public static void SaveEncounter(Encounter encounter)
	{
		string defaultPath = Path.Combine(Path.GetFullPath(PoTMod.Instance.SourceFolder is { Length: > 0 } src ? src : "."), (encounter.Description.Identifier ?? "Encounter") + EncounterIO.Extension);

		if (nativefiledialog.NFD_SaveDialog(EncounterIO.Extension[1..], defaultPath, out string? outPath) == nativefiledialog.nfdresult_t.NFD_OKAY)
		{
			try
			{
				File.WriteAllText(outPath!, EncounterIO.ToJson(encounter));
			}
			catch (Exception e)
			{
				Main.NewText($"Error saving encounter: {e.Message}", Color.MediumVioletRed);
			}
		}
	}

	public static void LoadEncounter(Encounter encounter)
	{
		string defaultPath = Path.Combine(Path.GetFullPath(PoTMod.Instance.SourceFolder is { Length: > 0 } src ? src : "."), (encounter.Description.Identifier ?? "Encounter") + EncounterIO.Extension);

		if (nativefiledialog.NFD_OpenDialog(EncounterIO.Extension[1..], defaultPath, out string? inPath) == nativefiledialog.nfdresult_t.NFD_OKAY)
		{
			try
			{
				EncounterDescription description = EncounterIO.FromJson(File.ReadAllText(inPath!));
				BoxedEncounters[encounter.Index] = new(encounter, description);
			}
			catch (Exception e)
			{
				Main.NewText($"Error loading encounter: {e.Message}", Color.MediumVioletRed);
			}
		}
	}

	public static void AddWave(Encounter encounter, int? insertionIndex, WaveBox? value)
	{
		if (!encounter.IsValid) { return; }
		List<WaveBox> list = BoxedEncounters[encounter.Index].Waves;
		list.Insert(insertionIndex ?? list.Count, value ?? ((WaveBox)new EncounterWave { Spawns = [] }));
	}

	public static void RemoveWave(Encounter encounter, int waveIndex)
	{
		if (!encounter.IsValid) { return; }

		List<WaveBox> list = BoxedEncounters[encounter.Index].Waves;
		if (waveIndex >= list.Count || list.Count <= 1) { return; }

		if (State.SelectedEncounter == encounter && State.SelectedWave >= waveIndex)
		{
			State.SetSelections(State.SelectedEncounter, State.SelectedWave > waveIndex ? (State.SelectedWave - 1) : -1, -1);
		}

		list.RemoveAt(waveIndex);
	}

	public static void RemoveSpawn(Encounter encounter, int waveIndex, int spawnIndex)
	{
		if (spawnIndex >= BoxedEncounters[encounter.Index].Waves[waveIndex].Spawns.Count) { return; }

		// This needs to run before removal, as it can trigger recreation of the boxed tree.
		if (State.SelectedEncounter == encounter && State.SelectedWave == waveIndex && State.SelectedSpawn >= spawnIndex)
		{
			State.SetSelections(State.SelectedEncounter, State.SelectedWave, State.SelectedSpawn > spawnIndex ? (State.SelectedSpawn - 1) : -1);
		}

		BoxedEncounters[encounter.Index].Waves[waveIndex].Spawns.RemoveAt(spawnIndex);
	}

	private static void FixMouseInterface()
	{
		foreach (UIElement child in State.Children)
		{
			if (child.IsMouseHovering)
			{
				Main.LocalPlayer.mouseInterface = true;
			}
		}
	}

	private static void HandlePlacement()
	{
		if (PlacementMode && State is { SelectedEncounter: { IsValid: true } encounter, SelectedWave: int selectedWave })
		{
			if (ConsumeMouseClick(0))
			{
				var spawnCopy = (EnemySpawn)(State.CurrentSpawnBox.Spawn);
				spawnCopy.SpawnPosition = Main.MouseWorld;
				spawnCopy.SpawnPlacement = null;

				BoxedEncounters[encounter.Index].Waves[selectedWave].Spawns.Add((SpawnBox)spawnCopy);
				State.Rebuild(immediate: true);
			}
			else if (ConsumeMouseClick(1))
			{
				PlacementMode = false;
				State.Rebuild(immediate: true);
			}
		}
	}

	private static void DragObjects()
	{
		if (ActiveDrag is not { } ctx) { return; }

		static void Reset()
		{
			ActiveDrag = null;
			State.Rebuild(immediate: true);
		}

		if (Main.mouseLeft == ctx.MouseLeftValueToStopAt || Main.mouseRight == ctx.MouseRightValueToStopAt)
		{
			Reset();
			return;
		}

		Vector2 mouseWorld = Main.MouseWorld;
		(Vector2 targetPos, Vector2 targetSize) = ctx.Drag.Calculate(mouseWorld);
		(Point16 targetTilePos, Point16 targetTileSize) = (targetPos.ToTileCoordinates16(), targetSize.ToTileCoordinates16());

		switch (ctx.Kind)
		{
			case DragKind.EncounterOrigin when State is { SelectedEncounter: { IsValid: true } encounter }:
				// Refuse placement into solid tiles.
				if (targetTilePos.X >= 0 && targetTilePos.Y >= 0 && targetTilePos.X < Main.maxTilesX && targetTilePos.Y < Main.maxTilesY)
				{
					Tile tile = Main.tile[targetTilePos];
					if (tile.HasTile && Main.tileSolid[tile.TileType]) { break; }
				}

				ref EncounterDescription dsc = ref Unsafe.Unbox<EncounterDescription>(BoxedEncounters[encounter.Index].Description);
				Point16 originOffset = targetTilePos - dsc.SpawnOrigin;
				Rectangle newSpawnArea = dsc.SpawnArea;
				newSpawnArea.X += originOffset.X;
				newSpawnArea.Y += originOffset.Y;
				dsc.ActivationOrigin = targetTilePos.ToWorldCoordinates();
				dsc.SpawnOrigin = targetTilePos;
				dsc.SpawnArea = newSpawnArea;
				break;
			case DragKind.EncounterArea when State is { SelectedEncounter: { IsValid: true } encounter }:
				Unsafe.Unbox<EncounterDescription>(BoxedEncounters[encounter.Index].Description).SpawnArea = new Rectangle
				{
					X = targetTilePos.X,
					Y = targetTilePos.Y,
					Width = targetTileSize.X,
					Height = targetTileSize.Y,
				};
				break;
			case DragKind.SpawnPosition when State is { SelectedEncounter: { IsValid: true } encounter, SelectedWave: int waveIndex, SelectedSpawn: int spawnIndex }:
				ref EnemySpawn existingSpawn = ref Unsafe.Unbox<EnemySpawn>(BoxedEncounters[encounter.Index].Waves[waveIndex].Spawns[spawnIndex].Spawn);
				existingSpawn.SpawnPosition = targetPos;
				existingSpawn.SpawnPlacement = null;
				break;
			default:
				Reset();
				break;
		}
	}

	private static bool DrawGizmos()
	{
		SpriteBatch sb = Main.spriteBatch;
		EncounterEditorState state = SmartUiLoader.GetUiState<EncounterEditorState>();

		static float Pulse(float scale)
		{
			return (MathF.Sin(Main.GameUpdateCount * scale) + 1f) * 0.5f;
		}

		float activeAnim = Pulse(0.075f);
		float inactiveAnim = Pulse(0.03f);
		Point mouseScreenPoint = Main.MouseScreen.ToPoint();
		Point mouseWorldPoint = Main.MouseWorld.ToPoint();
		Point mouseTilePoint = Main.MouseWorld.ToTileCoordinates();
		bool allowMouseInteractions = !ActiveDrag.HasValue && !PlacementMode && Main.mouseLeftRelease && !Main.LocalPlayer.mouseInterface;

		if (PlacementMode)
		{
			ref readonly EnemySpawn spawn = ref Unsafe.Unbox<EnemySpawn>(state.CurrentSpawnBox.Spawn);
			RenderSpawnGizmo(sb, in spawn, Main.MouseWorld, Color.Lerp(Color.White, Color.YellowGreen, Pulse(0.2f)), default);
			
			Main.instance.MouseText($"Left click to place\nRight click to cancel");
		}

		foreach (Encounter encounter in EnemyEncounters.IterateEncounters())
		{
			ref readonly EncounterDescription description = ref encounter.Description;
			bool isEncounterSelected = state.SelectedEncounter == encounter;

			// Draw spawn origin.
			Texture2D originTexture = TextureAssets.NpcHeadBoss[19].Value;
			Vector2 originPos = description.SpawnOrigin.ToWorldCoordinates() - Main.screenPosition;
			var originColor = Color.Lerp(Color.MediumPurple, Color.White, activeAnim);
			sb.Draw(originTexture, originPos, null, originColor, 0f, originTexture.Size() * 0.5f, 1.5f, 0, 0f);

			if (allowMouseInteractions && Main.MouseWorld.DistanceSQ(description.SpawnOrigin.ToWorldCoordinates()) < 32f * 32f)
			{
				Main.LocalPlayer.mouseInterface = true;
				Main.instance.MouseText($"Encounter: {description.Identifier} (#{encounter.Index}v{encounter.Version})\n[Click and hold to move]");

				// Select and drag the encounter when clicked on.
				if (ConsumeMouseClick(0))
				{
					if (encounter != state.SelectedEncounter)
					{
						state.SetSelections(encounter, -1, -1);
					}

					ActiveDrag = new DragContext
					{
						Kind = DragKind.EncounterOrigin,
						Drag = new(encounter.Description.ActivationOrigin, default, Main.MouseWorld, Vector2.One, default),
					};
				}
			}

			// Draw activation range.

			Vector2 screenSpaceOrigin = encounter.Description.ActivationOrigin - Main.screenPosition;
			DebugRendering.DrawCircle(Main.spriteBatch, screenSpaceOrigin, encounter.Description.ActivationRange, Color.White.MultiplyRGBA(new(Vector4.One * 0.5f)), resolution: 64);

			// Draw spawn area.

			Color areaColor = (isEncounterSelected ? Color.Gold : Color.IndianRed).MultiplyRGBA(new Color(Vector4.One * MathHelper.Lerp(0.25f, 0.35f, activeAnim)));
			Vector4Int tileArea = new(description.SpawnArea.Left, description.SpawnArea.Top, description.SpawnArea.Right, description.SpawnArea.Bottom);
			Vector4Int worldArea = tileArea * TileUtils.TileSizeInPixels;

			for (int i = 0; i < 4; i++)
			{
				var dstRect = new Rectangle
				{
					X = (i is not 3 ? worldArea.X : (worldArea.Z - TileUtils.TileSizeInPixels)) - (int)Main.screenPosition.X,
					Y = (i is not 1 ? worldArea.Y : (worldArea.W - TileUtils.TileSizeInPixels)) - (int)Main.screenPosition.Y,
					Width = i < 2 ? (worldArea.Z - worldArea.X) : TileUtils.TileSizeInPixels,
					Height = i < 2 ? TileUtils.TileSizeInPixels : (worldArea.W - worldArea.Y),
				};

				sb.Draw(TextureAssets.BlackTile.Value, dstRect, areaColor);
			}

			// Interact with the spawn area edges.
			Vector4Int tileAreaInner = new(tileArea.X + 1, tileArea.Y + 1, tileArea.Z - 2, tileArea.W - 2);
			Rectangle tileAreaRect = new(tileArea.X, tileArea.Y, tileArea.Z - tileArea.X, tileArea.W - tileArea.Y);
			Rectangle tileAreaRectInner = new(tileAreaInner.X, tileAreaInner.Y, tileAreaInner.Z - tileAreaInner.X, tileAreaInner.W - tileAreaInner.Y);
			if (allowMouseInteractions && tileAreaRect.Contains(mouseTilePoint) && !tileAreaRectInner.Contains(mouseTilePoint))
			{
				Main.LocalPlayer.mouseInterface = true;
				Main.instance.MouseText($"{description.Identifier}'s Spawn Area\n{description.SpawnArea}\n[Click and hold to resize]");

				if (ConsumeMouseClick(0))
				{
					if (encounter != state.SelectedEncounter)
					{
						state.SetSelections(encounter, -1, -1);
					}

					Vector2 areaWorldPos = description.SpawnArea.TopLeft() * TileUtils.TileSizeInPixels;
					Vector2 areaWorldSize = description.SpawnArea.Size() * TileUtils.TileSizeInPixels;
					var resizeSigns = new Vector2
					(
						(mouseTilePoint.X == tileArea.X) ? -1f : ((mouseTilePoint.X == tileArea.Z - 1) ? 1f : 0f),
						(mouseTilePoint.Y == tileArea.Y) ? -1f : ((mouseTilePoint.Y == tileArea.W - 1) ? 1f : 0f)
					);

					ActiveDrag = new DragContext
					{
						Kind = DragKind.EncounterArea,
						Drag = new RectangleDrag(areaWorldPos, areaWorldSize, Main.MouseWorld, Vector2.Zero, resizeSigns),
					};
				}
			}

			// Draw manually assigned enemy spawn points for every wave.

			for (int waveIndex = 0; waveIndex < encounter.Description.Waves.Length; waveIndex++)
			{
				ref readonly EncounterWave wave = ref encounter.Description.Waves[waveIndex];
				bool isWaveCurrent = waveIndex == encounter.Instance.WaveIndex;
				bool isWaveSelected = isEncounterSelected && state.SelectedWave == waveIndex;

				for (int spawnIndex = 0; spawnIndex < wave.Spawns.Length; spawnIndex++)
				{
					ref readonly EnemySpawn spawn = ref wave.Spawns[spawnIndex];
					bool isSpawnSelected = isWaveSelected && state.SelectedSpawn == spawnIndex;

					if (spawn.SpawnPosition is not { } spawnPoint) { continue; }

					Color fgColor = new(Vector4.One * MathHelper.Lerp(0.50f, 0.75f, isEncounterSelected ? activeAnim : inactiveAnim));
					Color bgColor = isSpawnSelected ? Color.GreenYellow : (isWaveSelected ? Color.Orange : new Color(Color.DarkRed.ToVector4() * 0.5f));
					Rectangle npcRect = RenderSpawnGizmo(sb, in spawn, spawnPoint, fgColor, bgColor);

					// Mouse interaction. Select and drag the spawn when clicked on.
					if (allowMouseInteractions && npcRect.Inflated(8, 8).Contains(mouseScreenPoint))
					{
						Main.LocalPlayer.mouseInterface = true;
						Main.instance.MouseText($"Spawn #{spawnIndex} ({ContentSamples.NpcsByNetId[spawn.NpcType.Type].TypeName})\n[Click and hold to move]\n[Right click to remove]");

						if (ConsumeMouseClick(0))
						{
							state.SetSelections(encounter, waveIndex, spawnIndex);
							ActiveDrag = new DragContext
							{
								Kind = DragKind.SpawnPosition,
								Drag = new(spawnPoint, default, Main.MouseWorld, Vector2.One, default),
							};
						}
						else if (ConsumeMouseClick(1))
						{
							RemoveSpawn(state.SelectedEncounter, waveIndex, spawnIndex);
						}
					}
				}
			}
		}

		return true;
	}

	private static Rectangle RenderSpawnGizmo(SpriteBatch sb, ref readonly EnemySpawn spawn, Vector2 spawnPoint, Color color, Color backgroundColor)
	{
		// Prepare.
		NPC sample = ContentSamples.NpcsByNetId[spawn.NpcType.Type];
		
		Main.instance.LoadNPC(sample.type);
		Texture2D texture = TextureAssets.Npc[sample.type].Value;

		Rectangle srcRect = new SpriteFrame(1, (byte)Main.npcFrameCount[sample.type]).GetSourceRectangle(texture);
		Rectangle dstRect = new
		(
			(int)(spawnPoint.X - (srcRect.Width * 0.5f) - Main.screenPosition.X),
			(int)(spawnPoint.Y - (srcRect.Height * 0.5f) - Main.screenPosition.Y),
			sample.width,
			sample.height
		);
		
		Color opaqueColor = sample.color.A == 0 ? Color.White : new(sample.color.ToVector4() / (sample.color.A / (float)byte.MaxValue));
		Color spawnColor = opaqueColor.MultiplyRGBA(color);

		// Draw hitbox background.
		sb.Draw(TextureAssets.BlackTile.Value, dstRect, backgroundColor);
		// Draw NPC sprite.
		sb.Draw(texture, dstRect.Center(), srcRect, spawnColor, 0f, srcRect.Size() * 0.5f, 1f, 0, 0f);

		return dstRect;
	}

	private static bool ConsumeMouseClick(int button)
	{
		ref bool pressed = ref (button == 0 ? ref Main.mouseLeft : ref Main.mouseRight);
		ref bool notReleased = ref (button == 0 ? ref Main.mouseLeftRelease : ref Main.mouseRightRelease);

		if (pressed && notReleased)
		{
			notReleased = false;
			return true;
		}

		return false;
	}
}

internal sealed class EncounterEditorCommand : ModCommand
{
	public override string Command => "potEncounterEditor";
	public override CommandType Type => CommandType.Chat;

	public override void Action(CommandCaller caller, string input, string[] args)
	{
		// Encounters are a server-side concept, no packets exist to synchronize information about them from the client to server.
		if (Main.netMode != NetmodeID.SinglePlayer)
		{
			throw new UsageException("This command is only available in singleplayer.");
		}

		SmartUiLoader.GetUiState<EncounterEditorState>().Toggle();
	}
}

/// <summary>
/// A big encounter editor GUI.
/// <br/> Due to limitations of internally used ModConfig code, operates on boxed copies of data.
/// </summary>
[Autoload(Side = ModSide.Client)]
internal sealed class EncounterEditorState : SmartUiState
{
	public enum SpawningMode
	{
		EditingExisting,
		PlacingCopy,
		EditingNew,
		PlacingNew,
	}

	private const int ScrollbarWidth = 20;
	private const int ScrollbarOffset = 4;

	private string? lastSearchString;
	private bool rebuildEnqueuePending;

	// Selection
	public Encounter SelectedEncounter { get; private set; }
	public int SelectedWave { get; private set; } = -1;
	public int SelectedSpawn { get; private set; } = -1;
	// Encounters & Waves
	public bool EncountersVisible { get; set; } = true;
	public UIEditableText EncountersSearchBar { get; private set; } = null!;
	// Spawning
	public bool SpawningVisible { get; set; } = true;
	public bool EditingExistingSpawn => SelectedEncounter.IsValid && SelectedWave >= 0 && SelectedSpawn >= 0;
	public SpawnBox CurrentSpawnBox => EditingExistingSpawn ? EncounterEditor.BoxedEncounters[SelectedEncounter.Index].Waves[SelectedWave].Spawns[SelectedSpawn] : EncounterEditor.BoxedSpawn;
	public SpawningMode CurrentSpawningMode => EncounterEditor.PlacementMode
		? (EditingExistingSpawn ? SpawningMode.PlacingCopy : SpawningMode.PlacingNew)
		: (EditingExistingSpawn ? SpawningMode.EditingExisting : SpawningMode.EditingNew);

#if DEBUG
	public override bool Visible
	{
		// Hide when alt-tabbed to prevent hot reload crashes.
		get => base.Visible && Main.hasFocus;
		set => base.Visible = value;
	}
#endif

	public override int InsertionIndex(List<GameInterfaceLayer> layers)
	{
		return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Cursor")) - 1;
	}

	public override void SafeUpdate(GameTime gameTime)
	{
		if (rebuildEnqueuePending && !EncounterEditor.WritingText)
		{
			Rebuild();
		}
	}

	public void Toggle()
	{
		if (!Visible)
		{
			EncountersVisible = true;
			SpawningVisible = true;
		}

		SetEnabled(!Visible);
	}

	public void SetEnabled(bool value)
	{
		if (Visible == value)
		{
			return;
		}

		Visible = value;

		if (Visible)
		{
			Rebuild();
		}
		else
		{
			RemoveAllChildren();
		}
	}

	public void Rebuild(bool immediate = false)
	{
		rebuildEnqueuePending = false;

		if (!immediate)
		{
			Main.QueueMainThreadAction(() => Rebuild(immediate: true));
			return;
		}

		// Delay rebuilds when the player may be typing text into an element here.
		if (EncounterEditor.WritingText)
		{
			rebuildEnqueuePending = true;
			return;
		}

		EncounterEditor.ExportData();
		EncounterEditor.ImportData();
		RemoveAllChildren();

		if (!EncountersVisible && !SpawningVisible)
		{
			Visible = false;
		}

		if (EncountersVisible) { BuildEncountersWindow(); }

		if (SpawningVisible) { BuildSpawningWindow(); }
	}

	public void SetSelections(Encounter encounter, int waveIndex, int spawnIndex, bool noRebuild = false)
	{
		// Update indices.
		SelectedEncounter = encounter;
		SelectedWave = (encounter.IsValid && waveIndex >= 0) ? Math.Min(waveIndex, (encounter.Description.Waves.Length - 1)) : -1;
		SelectedSpawn = (encounter.IsValid && SelectedWave >= 0 && spawnIndex >= 0) ? Math.Min(spawnIndex, encounter.Description.Waves[SelectedWave].Spawns.Length - 1) : -1;

		// Update the placement mode spawn data.
		ref SpawnBox spawnBox = ref EncounterEditor.BoxedSpawn;
		if (SelectedSpawn >= 0)
		{
			EnemySpawn spawn = encounter.Description.Waves[SelectedWave].Spawns[SelectedSpawn];
			spawnBox.Spawn = spawn;
			spawnBox.Placement = spawn.SpawnPlacement ?? EncounterEditor.DefaultPlacement.WithDefaults(spawn.NpcType.Type);
		}
		else
		{
			int npcType = NPCID.Zombie;
			spawnBox.Spawn ??= new EnemySpawn {
				NpcType = new(npcType),
				SpawnPlacement = null,
				SpawnPosition = Main.LocalPlayer.Center,
			};
			spawnBox.Placement ??= ((EnemySpawn)spawnBox.Spawn).SpawnPlacement ?? EncounterEditor.DefaultPlacement.WithDefaults(npcType);
		}

		if (!noRebuild) { Rebuild(); }
	}

	private UIElement BuildEncountersWindow()
	{
		var padding = new Vector4(8f, 40f, 8f, 8f);

		UIElement window = this.AddElement(new UIPanel(), e =>
		{
			e.MinWidth.Set(+512f, 0f);
			e.MinHeight.Set(+300f, 0f);

			e.SetDimensions(x: (0.0f, +32), y: (1.00f, -(600 + 32)), width: (0.0f, +512), height: (0f, +600));

			e.AddComponent(new UIPersistent("Encounters_MainWindow"));
			e.AddComponent(new UIMouseDrag(canMove: true, canResize: true));
		});
		// Header.
		window.AddElement(new UIText("Encounters"), e =>
		{
			e.SetDimensions(x: (0.0f, +padding.X), y: (0.00f, +4));
			e.IgnoresMouseInteraction = true;
		});
		// Close button.
		window.AddElement(new UIImageButton(ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/CloseButton")), e =>
		{
			e.SetDimensions(x: (1.0f, -34), y: (0.00f, -3), width: (0.0f, +38), height: (0f, +38));
			e.SetVisibility(1f, 0.8f);
			e.OnLeftClick += (evt, self) =>
			{
				EncountersVisible = false;
				Rebuild();
			};
		});

		// 'Add' button.
		window.AddElement(new UIButton<string>("Add New"), e =>
		{
			e.SetDimensions(x: (0.0f, +padding.X + 96f), y: (0.00f, +0), width: (0.0f, +80), height: (0f, +32));
			e.OnLeftClick += (evt, self) => EncounterEditor.AddEncounter();
		});

		// Search bar

		UIPanel searchBG = window.AddElement(new UIPanel(), e =>
		{
			e.SetDimensions(x: (0.0f, +192), y: (0.00f, +0), width: (1.0f, -232), height: (0f, +32));
		});
		EncountersSearchBar = window.AddElement(EncountersSearchBar ?? new UIEditableText(backingText: "Search..."), e =>
		{
			e.SetDimensions(x: (0.0f, +192 + 8f), y: (0.00f, +0), width: (1.0f, -232 - 16f), height: (0f, +32));

			if (EncountersSearchBar != null)
			{
				return;
			}

			e.CurrentValue = lastSearchString;
			e.OnUpdate += uiElement =>
			{
				if (uiElement is UIEditableText e && e.CurrentValue != lastSearchString)
				{
					Rebuild();
					lastSearchString = e.CurrentValue;
				}
			};
		});

		UIPersistent? scrollbarPersistence = null;
		UIElement listArea = window.AddElement(new UIElement(), e =>
		{
			e.SetDimensions(x: (0.00f, +padding.X), y: (0.00f, +padding.Y), width: (1.00f, -(padding.X + padding.Z)), height: (1.00f, -(padding.Y + padding.W)));
		});
		UIList list = listArea.AddElement(new UIList(), e =>
		{
			e.ManualSortMethod = _ => { };
			e.SetDimensions(x: (0.00f, +0), y: (0.00f, +0), width: (1.00f, -(ScrollbarWidth + ScrollbarOffset)), height: (1.00f, +0));
		});
		UIScrollbar scrollbar = listArea.AddElement(new FixedUIScrollbar(UserInterface), e =>
		{
			e.SetDimensions(x: (1.00f, -ScrollbarWidth), y: (0.00f, +6), width: (0.00f, +ScrollbarWidth), height: (1.00f, -12));
			list.SetScrollbar(e);
			
			scrollbarPersistence = e.AddComponent(new UIPersistent("EncounterDetailsScrollBar"));
		});

		PopulateListWithEncounterControls(list);
		PopulateListWithEncounterDetails(list);

		window.Recalculate();
		scrollbarPersistence?.Import(scrollbar);

		return window;
	}

	private void PopulateListWithEncounterControls(UIList list)
	{
		StringBuilder stringBuilder = new();
		int encounterIndex = -1;

		foreach (EncounterBox encounterBox in EncounterEditor.BoxedEncounters)
		{
			if (encounterBox is not { Handle: { IsValid: true } encounter }) { continue; }

			ref readonly EncounterDescription description = ref encounter.Description;

			if (!string.IsNullOrEmpty(EncountersSearchBar.CurrentValue) && !description.Identifier.Contains(EncountersSearchBar.CurrentValue, StringComparison.InvariantCultureIgnoreCase))
			{
				continue;
			}

			encounterIndex++;

			// Select the first encounter in the list if none is selected.
			if (!SelectedEncounter.IsValid)
			{
				SetSelections(encounter, -1, -1, noRebuild: true);
			}

			// Selectable Panel
			float buttonX = 0f;
			UIPanel itemPanel = list.AddElement(new UIPanel(), e =>
			{
				e.SetDimensions(x: (0.0f, +0), y: (0.1f, +128 * encounterIndex), width: (1.0f, +0), height: (0f, +80));

				Color defaultBorderColor = e.BorderColor = Color.PaleVioletRed;
				e.OnUpdate += self => ((UIPanel)self).BorderColor = SelectedEncounter == encounter ? Color.Gold : defaultBorderColor;
				e.OnLeftClick += (self, evt) => SetSelections(encounter, -1, -1);
			});

			// Name
			itemPanel.AddElement(new UIText($"[c/{Color.YellowGreen.ToHexRGB()}:{description.Identifier}] (#{encounterIndex})"), e =>
			{
				e.SetDimensions(x: (0.0f, +0), y: (0.0f, +0), width: (0.0f, +0), height: (0f, +128));
				e.IgnoresMouseInteraction = true;
			});

			// State info panel
			itemPanel.AddElement(new UIButton<string>(""), e =>
			{
				e.BackgroundColor = e.HoverPanelColor = Color.IndianRed;
				e.AltPanelColor = e.AltHoverPanelColor = Color.Orange;
				e.AltHoverBorderColor = e.HoverBorderColor = e.BorderColor;
				e.UseAltColors = () => encounter.IsValid && encounter.Instance.State is not EncounterState.NotStarted && !encounter.Instance.IsPaused;

				e.SetDimensions(x: (0.0f, +buttonX), y: (0.0f, +24), width: (0.0f, +128), height: (0f, +32));
				e.AddComponent(new UIDynamicText(() =>
				{
					if (!encounter.IsValid) { return string.Empty; }

					return encounter.Instance.State switch
					{
						_ when encounter.Instance.IsPaused => "Paused",
						EncounterState.InProgress => $"Wave {encounter.Instance.WaveIndex + 1} of {encounter.Description.Waves.Length}",
						EncounterState.NotStarted => "Inactive",
						EncounterState.Completed => "Completed",
						_ => throw new NotImplementedException(),
					};
				}));

				buttonX += e.Width.Pixels + 8f;
			});

			// 'Start'/'Complete' button
			itemPanel.AddElement(new UIButton<string>("Start"), e =>
			{
				e.SetDimensions(x: (0.0f, +buttonX), y: (0.0f, +24), width: (0.0f, +80), height: (0f, +32));
				e.AddComponent(new UIDynamicText(() =>
				{
					if (!encounter.IsValid) { return string.Empty; }

					return encounter.Instance.State is EncounterState.NotStarted or EncounterState.Completed ? "Start" : "Complete";
				}));

				e.OnLeftClick += (evt, self) =>
				{
					if (!encounter.IsValid) { return; }

					if (encounter.Instance.State is EncounterState.NotStarted or EncounterState.Completed) { encounter.Start(); } else { encounter.Complete(); }
				};

				buttonX += e.Width.Pixels + 2f;
			});
			// 'Pause'/'Resume' button
			itemPanel.AddElement(new UIButton<string>("Pause"), e =>
			{
				e.SetDimensions(x: (0.0f, +buttonX), y: (0.0f, +24), width: (0.0f, +80), height: (0f, +32));
				e.AddComponent(new UIDynamicText(() =>
				{
					if (!encounter.IsValid) { return string.Empty; }

					return !encounter.Instance.IsPaused ? "Pause" : "Resume";
				}));
				e.OnLeftClick += (evt, self) => encounter.SetPaused(!encounter.Instance.IsPaused);

				buttonX += e.Width.Pixels + 2f;
			});
			// 'Reset' button
			itemPanel.AddElement(new UIButton<string>("Reset"), e =>
			{
				e.SetDimensions(x: (0.0f, +buttonX), y: (0.0f, +24), width: (0.0f, +80), height: (0f, +32));
				e.OnLeftClick += (evt, self) =>
				{
					if (!encounter.IsValid) { return; }

					bool wasPaused = encounter.Instance.IsPaused;
					encounter.Reset();
					encounter.SetPaused(wasPaused);
				};

				buttonX += e.Width.Pixels + 2f;
			});
			// Removal button
			itemPanel.AddElement(new UIImageButton(ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/CloseButton")), e =>
			{
				e.SetDimensions(x: (1.0f, -33f), y: (0.0f, +20), width: (0.0f, +38), height: (0f, +38));
				e.SetVisibility(1f, 0.8f);
				e.OnLeftClick += (evt, self) => EncounterEditor.RemoveEncounter(encounter);
			});
		}

		// Notify that no encounters exist.
		if (encounterIndex < 0)
		{
			list.AddElement(new UITextPanel<string>("No encounters exist. Click 'Add New' above."), e =>
			{
				e.SetDimensions(x: (0.0f, +0), y: (0.1f, +128 * 0), width: (1.0f, +0), height: (0f, +40));
			});
		}
	}

	private void PopulateListWithEncounterDetails(UIList list)
	{
		if (!SelectedEncounter.IsValid)
		{
			if (EnemyEncounters.Count != 0)
			{
				list.AddElement(new UITextPanel<string>("Click an encounter panel to select it."), e =>
				{
					e.SetDimensions(x: (0.0f, +0), y: (0.1f, +128 * 0), width: (1.0f, +0), height: (0f, +40));
				});
			}

			return;
		}

		// Encounter properties:
		list.AddElement(new UIText($"Encounter: {SelectedEncounter.Description.Identifier}"));

		// Save/Load.
		list.AddElement(new UIElement(), e =>
		{
			e.SetDimensions(width: (1.0f, +0), height: (0f, +24));

			e.AddElement(new UIButton<string>($"Save Encounter"), e =>
			{
				e.SetDimensions(width: (0.5f, +0), height: (1.0f, +0));
				e.OnLeftClick += (evt, e) => Main.QueueMainThreadAction(() => EncounterEditor.SaveEncounter(SelectedEncounter));
			});
			e.AddElement(new UIButton<string>($"Load Encounter"), e =>
			{
				e.SetDimensions(x: (0.5f, +0), width: (0.5f, +0), height: (1.0f, +0));
				e.OnLeftClick += (evt, e) => Main.QueueMainThreadAction(() => EncounterEditor.LoadEncounter(SelectedEncounter));
			});
		});

		// Add options for all elements, reusing ModConfig code.
		foreach (PropertyInfo property in typeof(EncounterDescription).GetProperties())
		{
			// Skip properties handled in other ways.
			if (property.Name is { }
				and not nameof(EncounterDescription.Waves)
				and not nameof(EncounterDescription.SpawnArea)
				and not nameof(EncounterDescription.SpawnOrigin)
				and not nameof(EncounterDescription.ActivationOrigin))
			{
				AddConfigElement(list, new(property), EncounterEditor.BoxedEncounters[SelectedEncounter.Index].Description);
			}
		}

		// Waves properties:
		PopulateListWithAllWaveControls(list, SelectedEncounter);
	}

	private void PopulateListWithAllWaveControls(UIList list, Encounter encounter)
	{
		for (int waveIndex = 0; waveIndex < encounter.Description.Waves.Length; waveIndex++)
		{
			list.AddElement(new UIText($"Wave #{waveIndex}"));

			PopulateListWithSingleWaveControls(list, encounter, waveIndex);
		}

		list.AddElement(new UIButton<string>($"New Wave"), e =>
		{
			e.SetDimensions(width: (1f, +0f), height: (0f, +24f));

			e.OnLeftClick += (evt, e) => EncounterEditor.AddWave(SelectedEncounter, null, null);
		});
	}

	private void PopulateListWithSingleWaveControls(UIList list, Encounter encounter, int waveIndex)
	{
		ref readonly EncounterWave wave = ref encounter.Description.Waves[waveIndex];
		bool isWaveSelected = SelectedWave == waveIndex;

		int gridButtonSize = 48;
		int gridRows = 3;
		int gridHeight = (gridButtonSize * gridRows) + 12;
		int panelHeight = (gridHeight + 64);

		UIPanel wavePanel = list.AddElement(new UIButton<string>(string.Empty), e =>
		{
			// Temporary dimensions.
			e.SetDimensions(width: (0f, +list.GetInnerDimensions().Width), height: (0f, +1024));

			if (isWaveSelected)
			{
				e.BackgroundColor = Color.DarkGoldenrod;
				e.HoverPanelColor = Color.Goldenrod;
			}

			e.OnLeftClick += (evt, e) =>
			{
				if (SelectedEncounter.IsValid && SelectedWave != waveIndex) { SetSelections(SelectedEncounter, waveIndex, -1); }
			};
		});
		UIList wavePanelList = wavePanel.AddElement(new UIList(), e =>
		{
			e.ManualSortMethod = _ => { };
			e.SetDimensions(width: (1f, +0), height: (1f, +0));
		});

		// Selected wave properties:

		foreach (PropertyInfo property in typeof(EncounterWave).GetProperties())
		{
			// Skip properties handled in other ways.
			if (property.Name is not nameof(EncounterWave.Spawns))
			{
				AddConfigElement(wavePanelList, new(property), EncounterEditor.BoxedEncounters[SelectedEncounter.Index].Waves[waveIndex].Wave);
			}
		}

		// Wave spawn selectors:

		wavePanelList.AddElement(new UIText("Spawns"), e =>
		{
			e.IgnoresMouseInteraction = true;
		});
		UIGrid grid = wavePanelList.AddElement(new UIGrid(), e =>
		{
			e.SetDimensions(width: (1f, +0f), height: (1f, +0));
		});

		if (wave.Spawns.Length == 0)
		{
			grid.AddElement(new UIButton<string>("N/A"), e =>
			{
				(e.HoverPanelColor, e.HoverBorderColor) = (e.BackgroundColor, e.BorderColor);
				e.HoverText = "Add new spawns using the spawning window";
				e.SetDimensions(width: (0f, +gridButtonSize), height: (0f, +gridButtonSize));
			});
		}

		for (int spawnIndex = 0; spawnIndex < wave.Spawns.Length; spawnIndex++)
		{
			int spawnIndexCopy = spawnIndex;
			bool isSpawnSelected = isWaveSelected && spawnIndexCopy == SelectedSpawn;
			EnemySpawn spawn = wave.Spawns[spawnIndex];

			int type = ContentSamples.NpcsByNetId[spawn.NpcType.Type].type;
			Main.instance.LoadNPC(type);
			Asset<Texture2D> texture = TextureAssets.Npc[type];
			Rectangle frame = new SpriteFrame(1, (byte)Main.npcFrameCount[type]).GetSourceRectangle(texture.Value);

			UISortableElement sortable = grid.AddElement(new UISortableElement(spawnIndex));
			UIButton<string> container = sortable.AddElement(new UIButton<string>(string.Empty), e =>
			{
				e.SetDimensions(width: (0f, +gridButtonSize), height: (0f, +gridButtonSize));
				e.OverflowHidden = true;
				e.BorderColor = spawn.SpawnPosition.HasValue ? Color.Orange : Color.LightSkyBlue;
				e.HoverText = $"Spawn #{spawnIndexCopy} ({spawn.NpcType.DisplayName})\nRight click to remove";

				if (isSpawnSelected)
				{
					e.BackgroundColor = ColorUtils.FromHexRgb(0x798fde);
					e.HoverPanelColor = ColorUtils.FromHexRgb(0x7ea9c2);
				}

				e.OnLeftClick += (evt, e) =>
				{
					if (evt.Target == e) { SetSelections(SelectedEncounter, waveIndex, spawnIndexCopy); }
				};
				e.OnRightClick += (evt, e) =>
				{
					if (evt.Target != e || !SelectedEncounter.IsValid) { return; }

					EncounterEditor.RemoveSpawn(SelectedEncounter, waveIndex, spawnIndexCopy);
				};
			});

			container.AddElement(new UIImageFramed(texture, frame), e =>
			{
				e.SetDimensions(x: (0.5f, -(frame.Width * 0.5f)), y: (0.5f, -(frame.Height * 0.5f)), width: (0f, +0f), height: (0f, +0f));
				e.IgnoresMouseInteraction = true;
			});

			sortable.CopyDimensionsFrom(container);
		}

		// Removal button
		if (encounter.Description.Waves.Length >= 2)
		{
			wavePanelList.AddElement(new UIButton<string>($"Remove Wave"), e =>
			{
				e.SetDimensions(width: (1f, +0f), height: (0f, +24f));
				e.OnLeftClick += (evt, self) => EncounterEditor.RemoveWave(encounter, waveIndex);
			});
		}

		// Update sizes.
		grid.Recalculate();
		grid.Height.Set(grid.GetTotalHeight(), 0f);
		wavePanelList.Recalculate();
		wavePanelList.Height.Set(wavePanelList.GetTotalHeight(), 0f);
		wavePanel.Height.Set(wavePanelList.GetTotalHeight() + wavePanel.PaddingTop + wavePanel.PaddingBottom, 0f);

		// Silly workaround for the buggy list implementation seemingly not recalculating its inner element.
		list.Remove(wavePanel);
		list.Add(wavePanel);
	}

	private UIElement BuildSpawningWindow()
	{
		var padding = new Vector4(0f, 40f, 0f, 8f);

		UIElement window = this.AddElement(new UIPanel(), e =>
		{
			e.MinWidth.Set(+300f, 0f);
			e.MinHeight.Set(+300f, 0f);

			float alpha = e.BackgroundColor.A / (float)byte.MaxValue;

			if (EncounterEditor.PlacementMode || EncounterEditor.ActiveDrag is { Kind: EncounterEditor.DragKind.SpawnPosition })
			{
				e.BackgroundColor = Color.DarkSlateGray.MultiplyRGBA(new(Vector4.One * alpha));
			}
			else if (CurrentSpawningMode == SpawningMode.EditingExisting)
			{
				e.BackgroundColor = Color.PaleVioletRed.MultiplyRGBA(new(Vector4.One * alpha));
			}

			e.SetDimensions(x: (1.0f, -(512 + 32)), y: (1.0f, -(300 + 32)), width: (0.0f, +512), height: (0.00f, +300));

			e.AddComponent(new UIPersistent("Encounters_SpawningWindow"));
			e.AddComponent(new UIMouseDrag(canMove: true, canResize: true));
		});
		// Header.
		window.AddElement(new UIText(""), e =>
		{
			e.SetDimensions(x: (0.0f, +padding.X), y: (0.00f, +4));
			e.AddComponent(new UIDynamicText(() => CurrentSpawningMode switch
			{
				SpawningMode.EditingExisting when EncounterEditor.ActiveDrag is { Kind: EncounterEditor.DragKind.SpawnPosition } => "Moving Existing...",
				SpawningMode.PlacingNew or SpawningMode.PlacingCopy => "Placing Spawn...",
				SpawningMode.EditingExisting => "Editing Spawn",
				SpawningMode.EditingNew => "New Spawn",
				_ => "",
			}));
			e.IgnoresMouseInteraction = true;
		});
		// Close button.
		window.AddElement(new UIImageButton(ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/CloseButton")), e =>
		{
			e.SetDimensions(x: (1.0f, -34), y: (0.00f, -3), width: (0.0f, +38), height: (0f, +38));
			e.SetVisibility(1f, 0.8f);
			e.OnLeftClick += (evt, self) =>
			{
				SpawningVisible = false;
				Rebuild();
			};
		});

		UIElement innerArea = window.AddElement(new UIElement(), e =>
		{
			e.SetDimensions(x: (0.00f, +padding.X), y: (0.00f, +padding.Y), width: (1.00f, -(padding.X + padding.Z)), height: (1.00f, -(padding.Y + padding.W)));
		});

		UIPersistent? scrollbarPersistence = null;
		UIList list = innerArea.AddElement(new UIList(), e =>
		{
			e.ManualSortMethod = _ => { };
			e.SetDimensions(x: (0.00f, +0), y: (0.00f, +0), width: (1.00f, -(ScrollbarWidth + ScrollbarOffset)), height: (1.00f, +0));
		});
		UIScrollbar scrollbar = innerArea.AddElement(new FixedUIScrollbar(UserInterface), e =>
		{
			e.SetDimensions(x: (1.00f, -ScrollbarWidth), y: (0.00f, +6), width: (0.00f, +ScrollbarWidth), height: (1.00f, -12));
			list.SetScrollbar(e);

			scrollbarPersistence = e.AddComponent(new UIPersistent("SpawningWindowScrollbar"));
		});

		if (!SelectedEncounter.IsValid || SelectedWave < 0)
		{
			list.AddElement(new UIText("No encounter or wave selected."));
		}
		else
		{
			PopulateListWithSpawnControls(list, CurrentSpawnBox);
		}

		window.Recalculate();
		scrollbarPersistence?.Import(scrollbar);

		return window;
	}

	private void PopulateListWithSpawnControls(UIList list, SpawnBox spawnBox)
	{
		// Editing->Adding one-way mode toggle.
		list.AddElement(new UIText("Control"), e =>
		{
			const float LeftF = 0.25f;
			const float WidthF = 0.75f * 0.5f;

			(e.TextOriginX, e.TextOriginY) = (0f, 0.5f);
			e.SetDimensions(x: (0f, +0f), width: (1f, +0f), height: (0f, +32f));

			// Place/Add/Duplicate button.
			e.AddElement(new UIButton<string>(""), e =>
			{
				e.SetDimensions(x: (LeftF, +0f), width: (WidthF, +0f), height: (1f, +0f));
				e.AddComponent(new UIDynamicText(() =>
				{
					return CurrentSpawningMode switch
					{
						SpawningMode.EditingExisting => "Copy",
						SpawningMode.EditingNew => "Add",
						_ => "",
					};
				}));

				e.OnLeftClick += (evt, e) =>
				{
					if (evt.Target != e || !SelectedEncounter.IsValid || SelectedWave < 0) { return; }

					if (CurrentSpawningMode is SpawningMode.EditingNew or SpawningMode.EditingExisting)
					{
						bool existing = CurrentSpawningMode == SpawningMode.EditingExisting;
						var spawnCopy = (EnemySpawn)spawnBox.Spawn;

						if (spawnCopy.SpawnPlacement.HasValue)
						{
							List<SpawnBox> spawns = EncounterEditor.BoxedEncounters[SelectedEncounter.Index].Waves[SelectedWave].Spawns;
							int index = SelectedSpawn >= 0 ? (SelectedSpawn + 1) : spawns.Count;
							spawns.Insert(index, (SpawnBox)spawnCopy);
						}
						else
						{
							EncounterEditor.PlacementMode = true;
						}

						Rebuild();
					}
				};
			});

			// Stop/Cancel button.
			e.AddElement(new UIButton<string>(""), e =>
			{
				e.SetDimensions(x: (LeftF + WidthF, +0f), width: (WidthF, +0f), height: (1f, +0f));
				e.AddComponent(new UIDynamicText(() => CurrentSpawningMode switch
				{
					SpawningMode.EditingExisting => "Stop Editing",
					SpawningMode.PlacingNew => "Cancel Placement",
					_ => ""
				}));

				e.OnLeftClick += (evt, e) =>
				{
					if (evt.Target != e || !SelectedEncounter.IsValid) { return; }

					SetSelections(SelectedEncounter, SelectedWave, -1);
				};
			});
		});

		foreach (PropertyInfo property in typeof(EnemySpawn).GetProperties())
		{
			// Skip properties handled in other ways.
			if (property.Name is not nameof(EnemySpawn.SpawnPosition) and not nameof(EnemySpawn.SpawnPlacement))
			{
				AddConfigElement(list, new(property), spawnBox.Spawn);
			}
		}

		// Manual/Automatic placement toggle.
		list.AddElement(new UIText(""), e =>
		{
			(e.TextOriginX, e.TextOriginY) = (0.0f, 0.5f);
			e.SetDimensions(width: (1f, +0f), height: (0f, +32f));
			e.AddComponent(new UIDynamicText
			(
				() => SelectedEncounter.IsValid && Unsafe.Unbox<EnemySpawn>(spawnBox.Spawn).SpawnPosition.HasValue ? "Manual Placement" : "Automatic Placement"
			));

			e.AddElement(new UIButton<string>(""), e =>
			{
				e.SetDimensions(x: (0.5f, +0f), width: (0.5f, +0f), height: (1f, +0f));
				e.AddComponent(new UIDynamicText
				(
					() => SelectedEncounter.IsValid && Unsafe.Unbox<EnemySpawn>(spawnBox.Spawn).SpawnPosition.HasValue ? "Use Dynamic Placement" : "Use Manual Position"
				));
				e.OnLeftClick += (evt, e) =>
				{
					if (!SelectedEncounter.IsValid) { return; }

					ref EnemySpawn spawn = ref Unsafe.Unbox<EnemySpawn>(spawnBox.Spawn);
					bool makingDynamic = !spawn.SpawnPlacement.HasValue;

					spawn.SpawnPlacement = makingDynamic ? (spawnBox.Placement is SpawnPlacement placement ? placement : EncounterEditor.DefaultPlacement.WithDefaults(spawn.NpcType.Type)) : null;
					spawn.SpawnPosition = makingDynamic ? null : Main.LocalPlayer.Center;

					// If the spawn exists, define a position for it now.
					if (!makingDynamic && SelectedSpawn >= 0)
					{
						EncounterEditor.ActiveDrag = new()
						{
							Kind = EncounterEditor.DragKind.SpawnPosition,
							Drag = new RectangleDrag(Main.MouseWorld, default, Main.MouseWorld, Vector2.One, Vector2.Zero),
							MouseLeftValueToStopAt = true,
							MouseRightValueToStopAt = true,
						};
					}

					Rebuild();
				};
			});
		});

		ref EnemySpawn spawn = ref Unsafe.Unbox<EnemySpawn>(spawnBox.Spawn);

		if (spawn.SpawnPlacement.HasValue)
		{
			foreach (PropertyInfo property in typeof(SpawnPlacement).GetProperties())
			{
				if (property.Name is nameof(SpawnPlacement.Area) or nameof(SpawnPlacement.AreaOrigin) or nameof(SpawnPlacement.CollisionSize)) { continue; }

				AddConfigElement(list, new(property), spawnBox.Placement);
			}
		}
	}

	/// <summary> Adds a config element, omitting the sortable wrapper. </summary>
	private static void AddConfigElement(UIList list, PropertyFieldWrapper memberInfo, object item)
	{
		int yPos = 0;
		int order = 0;
		(UIElement container, UIElement element) = ConfigManager.WrapIt(list, ref yPos, memberInfo, item, order);
		list.Remove(container);
		list.Add(element);
	}
}
