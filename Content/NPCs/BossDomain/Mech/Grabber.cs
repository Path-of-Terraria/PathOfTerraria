using NPCUtils;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using PathOfTerraria.Content.Scenes;
using ReLogic.Content;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.BossDomain.Mech;

internal class Grabber : ModNPC
{
	private static Asset<Texture2D> Glow = null;
	private static Asset<Texture2D> Cord = null;

	private Vector2 Anchor
	{
		get => new(NPC.ai[0], NPC.ai[1]);
		
		set
		{
			NPC.ai[0] = value.X;
			NPC.ai[1] = value.Y;
		}
	}

	private ref float ShakeTimer => ref NPC.ai[2];

	private int HoldingPlayer
	{
		get => (int)NPC.ai[3];
		set => NPC.ai[3] = value;
	}

	private ref float RegrabTimer => ref NPC.localAI[0];

	public override void SetStaticDefaults()
	{
		Glow = ModContent.Request<Texture2D>(Texture + "_Glow");
		Cord = ModContent.Request<Texture2D>(Texture + "_Cord");

		Main.npcFrameCount[Type] = 3;
	}

	public override void SetDefaults()
	{
		NPC.CloneDefaults(NPCID.Slimer);
		NPC.Opacity = 1;
		NPC.Size = new Vector2(36);
		NPC.aiStyle = -1;
		NPC.lifeMax = 800;
		NPC.defense = 90;
		NPC.damage = 10;
		NPC.HitSound = SoundID.NPCHit4;
		NPC.noGravity = true;
		NPC.knockBackResist = 0;
		NPC.scale = 1;
		NPC.dontTakeDamage = true;
		NPC.npcSlots = 0;
		NPC.value = Item.buyPrice(0, 1);

		SpawnModBiomes = [ModContent.GetInstance<MechBiome>().Type];

		NPC.TryEnableComponent<NPCHitEffects>(c =>
		{
			c.AddDust(new(DustID.MinecartSpark, 6, null, static x => x.scale = 10));
			c.AddDust(new(DustID.MinecartSpark, 20, NPCHitEffects.OnDeath, static x => x.scale = 10));

			c.AddGore(new NPCHitEffects.GoreSpawnParameters($"{PoTMod.ModName}/{Name}_0", 2, NPCHitEffects.OnDeath));
			c.AddGore(new NPCHitEffects.GoreSpawnParameters($"{PoTMod.ModName}/{Name}_1", 1, NPCHitEffects.OnDeath));
		});
	}

	public override bool CheckActive()
	{
		return false;
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
		RegrabTimer--;

		if (Anchor == Vector2.Zero)
		{
			Anchor = NPC.Center;
			HoldingPlayer = -1;

			NPC.netUpdate = true;
		}

		if (!Main.tile[Anchor.ToTileCoordinates()].HasTile)
		{
			NPC.StrikeInstantKill();
			NPC.netUpdate = true;
			NPC.active = false;
			return;
		}

		if (HoldingPlayer != -1)
		{
			Player captive = Main.player[HoldingPlayer];
			ShakeTimer += captive.velocity.Length() * 2;

			NPC.velocity *= 0.9f;

			if (ShakeTimer > 180 || captive.DeadOrGhost)
			{
				HoldingPlayer = -1;
				ShakeTimer = -1;
				RegrabTimer = 100;

				captive.GetModPlayer<GrabberPlayer>().BeingGrabbed = -1;

				NPC.netUpdate = true;
			}
		}
		else if (RegrabTimer <= 0)
		{
			NPC.TargetClosest();
			Player target = Main.player[NPC.target];
			
			NPC.rotation = Anchor.AngleTo(target.Center) + MathHelper.PiOver2;
			NPC.velocity += NPC.DirectionTo(target.Center) * 0.5f;
		}
		else
		{
			NPC.velocity *= 0.99f;
		}

		if (NPC.velocity.LengthSquared() > 6 * 6)
		{
			NPC.velocity = Vector2.Normalize(NPC.velocity) * 6;
		}

		if (NPC.Center.DistanceSQ(Anchor) > 400 * 400)
		{
			NPC.Center = Anchor + Anchor.DirectionTo(NPC.Center) * 400;
		}
	}

	public override bool CanHitPlayer(Player target, ref int cooldownSlot)
	{
		return RegrabTimer <= 0 && HoldingPlayer == -1;
	}

	public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
	{
		if (HoldingPlayer == -1)
		{
			HoldingPlayer = target.whoAmI;
			ShakeTimer = 0;

			target.GetModPlayer<GrabberPlayer>().BeingGrabbed = NPC.whoAmI;
		}
	}

	public override void FindFrame(int frameHeight)
	{
		if (HoldingPlayer == -1)
		{
			NPC.frameCounter += 0.1f;
			int frame = (int)(NPC.frameCounter % 4);

			NPC.frame.Y = frame switch
			{
				3 => 1,
				_ => frame
			} * frameHeight;
		}
		else
		{
			NPC.frame.Y = 0;
			NPC.frameCounter = 0;
		}
	}

	public override void HitEffect(NPC.HitInfo hit)
	{
		if (Main.dedServ)
		{
			return;
		}

		Vector2 center = NPC.Center;
		Vector2 offset = Anchor - center;

		if (Anchor.DistanceSQ(center) < 4 * 4 || NPC.Center.HasNaNs() || offset.HasNaNs())
		{
			return;
		}

		int count = 0;

		while (true)
		{
			if (DropCordGore(ref center, ref offset, count))
			{
				DropCordGore(ref center, ref offset, count);
				break;
			}
		}
	}

	private bool DropCordGore(ref Vector2 center, ref Vector2 offset, int count)
	{
		const int CordHeight = 12;

		if (offset.Length() < CordHeight + 1)
		{
			return true;
		}
		else
		{
			center += Vector2.Normalize(offset) * CordHeight;
			offset = Anchor - center;

			int gore = Gore.NewGore(NPC.GetSource_Death(), center, Vector2.Zero, ModContent.Find<ModGore>($"{PoTMod.ModName}/Grabber_2").Type);
			Main.gore[gore].Frame = new SpriteFrame(1, 3, 0, (byte)(count % 3));
		}

		return false;
	}

	public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		if (NPC.IsABestiaryIconDummy)
		{
			return true;
		}

		Vector2 center = NPC.Center;
		Vector2 offset = Anchor - center;

		if (Anchor.DistanceSQ(center) < 4 * 4 || NPC.Center.HasNaNs() || offset.HasNaNs())
		{
			return false;
		}

		int count = 0;

		while (true)
		{
			if (DrawCord(ref center, ref offset, count))
			{
				break;
			}
		}

		Vector2 npcOrigin = NPC.frame.Size() / new Vector2(2, 1);
		Vector2 npcDrawPos = NPC.Center - Main.screenPosition;
		Main.spriteBatch.Draw(TextureAssets.Npc[Type].Value, npcDrawPos, NPC.frame, drawColor, NPC.rotation, npcOrigin, 1f, SpriteEffects.None, 0f);
		return false;
	}

	private bool DrawCord(ref Vector2 center, ref Vector2 offset, int count)
	{
		const int CordHeight = 12;

		if (offset.Length() < CordHeight + 1)
		{
			return true;
		}
		else
		{
			center += Vector2.Normalize(offset) * CordHeight;
			offset = Anchor - center;

			Color color = NPC.GetAlpha(Lighting.GetColor((int)center.X / 16, (int)(center.Y / 16.0)));
			Rectangle source = new(0, count % 3 * (CordHeight + 2), 12, 12);
			Main.spriteBatch.Draw(Cord.Value, center - Main.screenPosition, source, color, offset.ToRotation() - 1.57f, Cord.Size() / 2, 1f, SpriteEffects.None, 0f);
		}

		return false;
	}

	public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		if (RegrabTimer > -1)
		{
			return;
		}

		Vector2 position = NPC.Center - screenPos;
		spriteBatch.Draw(Glow.Value, position, NPC.frame, Color.White, NPC.rotation, NPC.frame.Size() / new Vector2(2, 1), 1f, SpriteEffects.None, 0);
	}
}

public class GrabberPlayer : ModPlayer
{
	public int BeingGrabbed = -1;

	public override void Load()
	{
		On_Player.GrappleMovement += ResetThePlayerToGrabberAgain;
	}

	private void ResetThePlayerToGrabberAgain(On_Player.orig_GrappleMovement orig, Player self)
	{
		orig(self);

		self.GetModPlayer<GrabberPlayer>().UpdateGrabbed();
	}

	public void UpdateGrabbed()
	{
		if (Player.dead)
		{
			BeingGrabbed = -1;
		}

		if (BeingGrabbed >= 0)
		{
			NPC grabber = Main.npc[BeingGrabbed];

			if (!grabber.active || grabber.type != ModContent.NPCType<Grabber>())
			{
				BeingGrabbed = -1;
				return;
			}

			Player.Center = grabber.Center + (grabber.rotation - MathHelper.PiOver2).ToRotationVector2() * 20;

			float speed = Player.velocity.Length();

			if (Player.grappling[0] > -1)
			{
				speed /= 4;
			}

			grabber.ai[2] += speed;

			Player.velocity = Vector2.Zero;
		}
	}

	public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genDust, ref PlayerDeathReason damageSource)
	{
		BeingGrabbed = -1;
		return true;
	}
}