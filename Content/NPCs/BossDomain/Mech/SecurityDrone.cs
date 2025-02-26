using NPCUtils;
using PathOfTerraria.Common.NPCs.Pathfinding;
using PathOfTerraria.Common.Subworlds.BossDomains;
using PathOfTerraria.Content.Scenes;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria.GameContent.Bestiary;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.BossDomain.Mech;

//[AutoloadBanner]
internal class SecurityDrone : ModNPC
{
	private static Asset<Texture2D> Glow = null;

	private Pathfinder _pathfinder = new(60);

	public override void SetStaticDefaults()
	{
		Glow = ModContent.Request<Texture2D>(Texture + "_Glow");
	}

	public override void SetDefaults()
	{
		NPC.CloneDefaults(NPCID.Slimer);
		NPC.Opacity = 1;
		NPC.Size = new Vector2(36);
		NPC.aiStyle = -1;
		NPC.lifeMax = 200;
		NPC.defense = 40;
		NPC.damage = 50;
		NPC.HitSound = SoundID.NPCHit4;
		NPC.noGravity = true;
		NPC.knockBackResist = 0;
		NPC.scale = 1;

		SpawnModBiomes = [ModContent.GetInstance<MechBiome>().Type];
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
		NPC.TargetClosest(false);
		Player target = Main.player[NPC.target];

		if (_pathfinder.CheckDrawPath(NPC.Center.ToTileCoordinates16(), target.Center.ToTileCoordinates16()))
		{
		}
	}

	public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		Vector2 position = NPC.Center - screenPos + new Vector2(0, 6);
		spriteBatch.Draw(Glow.Value, position, NPC.frame, Color.White, NPC.rotation, NPC.frame.Size() / 2f, 1f, SpriteEffects.None, 0);
	}
}
