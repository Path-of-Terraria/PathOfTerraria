using NPCUtils;
using PathOfTerraria.Common.NPCs.Worms;
using ReLogic.Content;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.Mapping.Desert.SunDevourer;

[AutoloadBanner]
internal class WormLightning : ModNPC
{
	internal class WormLightning_Body : WormSegment
	{
		public override void Defaults()
		{
			NPC.Size = new Vector2(22);
			NPC.damage = 30;
		}
	}

	internal class WormLightning_Tail : WormLightning_Body
	{
	}

	internal static Asset<Texture2D>[] Textures;

	private ref float State => ref NPC.ai[0];

	private Vector2 TargetGlass
	{
		get => new(NPC.ai[1], NPC.ai[2]);
		set => (NPC.ai[1], NPC.ai[2]) = (value.X, value.Y);
	}

	private ref float GlassBroken => ref NPC.ai[3];

	private bool _spawnedSegments = false;

	public override void SetStaticDefaults()
	{
		Textures = 
		[
			ModContent.Request<Texture2D>(Texture),
			ModContent.Request<Texture2D>(Texture + "_Body"),
			ModContent.Request<Texture2D>(Texture + "_Tail")
		];
	}

	public override void SetDefaults()
	{
		NPC.Size = new Vector2(22);
		NPC.lifeMax = 600;
		NPC.damage = 30;
		NPC.aiStyle = -1;
		NPC.noGravity = true;
		NPC.knockBackResist = 0;
		NPC.noTileCollide = true;
	}

	public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
	{
		bestiaryEntry.AddInfo(this, "Desert");
	}

	public override void AI()
	{
		if (!_spawnedSegments && Main.netMode != NetmodeID.MultiplayerClient)
		{
			WormSegment.SpawnWhole<WormLightning_Body, WormLightning_Tail>(NPC.GetSource_FromAI(), NPC, 24, 12);
			_spawnedSegments = true;
		}

		NPC.rotation = NPC.velocity.ToRotation();

		if (State == 0)
		{
			NPC.dontTakeDamage = false;
			NPC.TargetClosest();
			Player target = Main.player[NPC.target];

			NPC.velocity += NPC.DirectionTo(target.Center) * 0.4f;

			if (NPC.velocity.LengthSquared() > 12 * 12)
			{
				NPC.velocity = Vector2.Normalize(NPC.velocity) * 12;
			}
		}
		else if (State == 1)
		{
			NPC.dontTakeDamage = true;
			NPC.velocity += NPC.DirectionTo(TargetGlass).RotatedByRandom(0.2f) * 1f;

			if (NPC.velocity.LengthSquared() > 12 * 12)
			{
				NPC.velocity = Vector2.Normalize(NPC.velocity) * 12;
			}

			CheckResetGlassPosition();

			Point tilePos = (NPC.Center - new Vector2(12, 0)).ToTileCoordinates();
			Tile tile = Main.tile[tilePos];
			bool brokeGlass = false;

			if (tile.HasTile && tile.TileType == TileID.Glass)
			{
				WorldGen.KillTile(tilePos.X, tilePos.Y, false, false, true);
				GlassBroken++;
				brokeGlass = true;
			}
			
			tilePos = (NPC.Center + new Vector2(12, 0)).ToTileCoordinates();
			tile = Main.tile[tilePos];

			if (tile.HasTile && tile.TileType == TileID.Glass)
			{
				WorldGen.KillTile(tilePos.X, tilePos.Y, false, false, true);
				GlassBroken++;
				brokeGlass = true;
			}

			if (brokeGlass)
			{
				TargetGlass = SunDevourerNPC.FindGlass(NPC.Center, 120, 80);

				if (GlassBroken >= 4)
				{
					State = 0;
					NPC.velocity = Vector2.Zero;
				}
			}
		}
	}

	private void CheckResetGlassPosition()
	{
		Tile tile = Main.tile[TargetGlass.ToTileCoordinates16()];

		if (!tile.HasTile || tile.TileType != TileID.Glass)
		{
			TargetGlass = SunDevourerNPC.FindGlass(TargetGlass, 300, 120);
		}
	}

	public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		Texture2D value = TextureAssets.Npc[Type].Value;
		spriteBatch.Draw(value, NPC.Center - screenPos, null, Color.White, NPC.rotation, value.Size() * new Vector2(1f, 0.5f), 1f, SpriteEffects.None, 0);
		return false;
	}
}