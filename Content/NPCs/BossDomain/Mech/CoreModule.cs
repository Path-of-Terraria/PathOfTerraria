using NPCUtils;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using PathOfTerraria.Common.NPCs.Pathfinding;
using PathOfTerraria.Content.Scenes;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria.GameContent.Bestiary;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.BossDomain.Mech;

internal class CoreModule : ModNPC
{
	private ref float State => ref NPC.ai[0];
	private ref float PlateCount => ref NPC.ai[1];

	private readonly Pathfinder _pathfinder = new(5);

	private readonly int[] _plates = new int[4];

	public override bool IsLoadingEnabled(Mod mod)
	{
		return false;
	}

	public override void SetDefaults()
	{
		NPC.Size = new Vector2(86);
		NPC.aiStyle = -1;
		NPC.lifeMax = 5000;
		NPC.defense = 50;
		NPC.damage = 20;
		NPC.HitSound = SoundID.NPCHit4;
		NPC.noGravity = true;
		NPC.knockBackResist = 0;
		NPC.value = Item.buyPrice(0, 0, 5);
		NPC.dontTakeDamage = true;

		SpawnModBiomes = [ModContent.GetInstance<MechBiome>().Type];

		NPC.TryEnableComponent<NPCHitEffects>(c =>
		{
			c.AddDust(new(DustID.MinecartSpark, 6, null, static x => x.scale = 10));
			c.AddDust(new(DustID.MinecartSpark, 20, NPCHitEffects.OnDeath, static x => x.scale = 10));

			//c.AddGore(new NPCHitEffects.GoreSpawnParameters($"{PoTMod.ModName}/{Name}_0", 1, NPCHitEffects.OnDeath));
			//c.AddGore(new NPCHitEffects.GoreSpawnParameters($"{PoTMod.ModName}/{Name}_1", 1, NPCHitEffects.OnDeath));
			//c.AddGore(new NPCHitEffects.GoreSpawnParameters(GoreID.Smoke1, 1, NPCHitEffects.OnDeath));
			//c.AddGore(new NPCHitEffects.GoreSpawnParameters(GoreID.Smoke2, 1, NPCHitEffects.OnDeath));
			//c.AddGore(new NPCHitEffects.GoreSpawnParameters(GoreID.Smoke3, 1, NPCHitEffects.OnDeath));
		});
	}

	public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
	{
		bestiaryEntry.AddInfo(this, "");
	}

	public override bool? CanFallThroughPlatforms()
	{
		return true;
	}

	public override void AI()
	{
		if (State == 0)
		{
			for (int i = 0; i < _plates.Length; ++i)
			{
				_plates[i] = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<CoreModulePlate>(), 0, NPC.whoAmI, i);
			}

			PlateCount = 4;
			State = 1;
		}
		else if (State == 1)
		{
			NPC.TargetClosest();

			Player target = Main.player[NPC.target];
			_pathfinder.CheckDrawPath(NPC.Center.ToTileCoordinates16(), target.Center.ToTileCoordinates16(),
				new Vector2(NPC.width / 16f - 2, NPC.height / 16f - 2f), null, new(0, -10));

			bool canPath = _pathfinder.HasPath && _pathfinder.Path.Count > 0;

			if (canPath)
			{
				List<Pathfinder.FoundPoint> checkPoints = _pathfinder.Path[^(Math.Min(_pathfinder.Path.Count, 6))..];
				Vector2 direction = -Vector2.Normalize(AveragePathDirection(checkPoints)) * (8 - PlateCount);
				NPC.velocity = direction;
			}
		}

		CheckIfVulnerable();
	}

	private void CheckIfVulnerable()
	{
		if (!NPC.dontTakeDamage)
		{
			return;
		}

		bool allDead = true;
		PlateCount = 0;

		for (int i = 0; i < _plates.Length; ++i)
		{
			NPC npc = Main.npc[_plates[i]];

			if (npc.active && npc.type == ModContent.NPCType<CoreModulePlate>())
			{
				allDead = false;
				PlateCount++;
			}
		}

		if (allDead)
		{
			NPC.dontTakeDamage = false;
		}
	}

	private static Vector2 AveragePathDirection(List<Pathfinder.FoundPoint> foundPoints)
	{
		Vector2 dir = Vector2.Zero;

		foreach (Pathfinder.FoundPoint point in foundPoints)
		{
			dir += Pathfinder.ToVector2(point.Direction);
		}

		return dir / foundPoints.Count;
	}

	//public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	//{
	//	Vector2 position = NPC.Center - screenPos + new Vector2(0, 6);
	//	spriteBatch.Draw(Glow.Value, position, NPC.frame, Color.White, NPC.rotation, NPC.frame.Size() / 2f, 1f, SpriteEffects.None, 0);
	//}
}
