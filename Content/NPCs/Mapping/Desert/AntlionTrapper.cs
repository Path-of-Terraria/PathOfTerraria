using NPCUtils;
using PathOfTerraria.Common.NPCs;
using PathOfTerraria.Common.NPCs.BestiaryInfoProviders;
using PathOfTerraria.Common.NPCs.Components;
using PathOfTerraria.Common.NPCs.Effects;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.Mapping.Desert;

[AutoloadBanner]
internal class AntlionTrapper : ModNPC
{
	private ref float JawAngle => ref NPC.ai[0];
	private ref float JawAngleTarget => ref NPC.ai[1];
	private ref float SnapCooldown => ref NPC.ai[2];
	private ref float SnapWait => ref NPC.ai[3];

	private bool _spawnItem = true;

	public override void SetDefaults()
	{
		NPC.Size = new Vector2(34, 50);
		NPC.aiStyle = -1;
		NPC.lifeMax = 60;
		NPC.defense = 0;
		NPC.damage = 50;
		NPC.HitSound = SoundID.NPCHit23;
		NPC.DeathSound = SoundID.NPCDeath16;
		NPC.noGravity = true;
		NPC.noTileCollide = true;
		NPC.color = Color.White;
		NPC.value = 0;
		NPC.npcSlots = 0.5f;
		NPC.hide = true;
		NPC.knockBackResist = 0f;
		NPC.ShowNameOnHover = false;
		NPC.dontTakeDamage = true;
		
		NPC.TryEnableComponent<NPCHitEffects>(
			c =>
			{
				c.AddGore(new NPCHitEffects.GoreSpawnParameters($"{PoTMod.ModName}/Trapper_0", 2, NPCHitEffects.OnDeath));
				c.AddGore(new NPCHitEffects.GoreSpawnParameters($"{PoTMod.ModName}/Trapper_1", 1, NPCHitEffects.OnDeath));

				c.AddDust(new NPCHitEffects.DustSpawnParameters(DustID.Gold, 3));
				c.AddDust(new NPCHitEffects.DustSpawnParameters(DustID.Gold, 12, NPCHitEffects.OnDeath));
			}
		);
	}

	public override bool CheckActive()
	{
		return false;
	}

	public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
	{
		bestiaryEntry.AddInfo(this, "Desert");
		bestiaryEntry.UIInfoProvider = new CustomInfoProvider(ContentSamples.NpcBestiaryCreditIdsByNpcNetIds[Type], false, 10);
	}

	public override void DrawBehind(int index)
	{
		Main.instance.DrawCacheNPCsBehindNonSolidTiles.Add(index);
	}

	public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
	{
		NPC.lifeMax = ModeUtils.ByMode(60, 90, 140);
		NPC.damage = ModeUtils.ByMode(70, 80, 110);
		NPC.defense = ModeUtils.ByMode(0, 35, 50, 80);
	}

	public override bool CanHitPlayer(Player target, ref int cooldownSlot)
	{
		return false;
	}

	public override void AI()
	{
		if (Main.netMode != NetmodeID.MultiplayerClient && _spawnItem)
		{
			_spawnItem = false;

			if (Main.rand.NextBool(3))
			{
				int item = Item.NewItem(NPC.GetSource_FromAI(), NPC.Center, ItemID.GoldCoin, Main.rand.Next(5, 9));
				Main.item[item].velocity = Vector2.Zero;
			}
		}

		bool stopFall = Collision.SolidCollision(NPC.position, NPC.width, NPC.height);

		if (Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
		{
			NPC.velocity.Y *= 0.6f;

			if (!Collision.SolidCollision(NPC.position + new Vector2(0, 16), NPC.width, 6))
			{
				NPC.velocity.Y += 0.1f;
			}
		}
		else
		{
			NPC.velocity.Y += 0.2f;
		}

		if (JawAngle < 0.01f && SnapCooldown <= 0)
		{
			Vector2 topLeft = NPC.position - new Vector2(18, 24);
			Rectangle snapBounds = new((int)topLeft.X, (int)topLeft.Y, 74, 32);

			foreach (Player player in Main.ActivePlayers)
			{
				if (player.Hitbox.Intersects(snapBounds))
				{
					SnapWait += 2;
				}
			}

			if (SnapWait >= 30)
			{
				Snap(snapBounds);
			}
		}
		else if (SnapCooldown <= 0)
		{
			JawAngleTarget = 0f;
			NPC.dontTakeDamage = true;
		}

		SnapCooldown--;
		SnapWait = MathHelper.Clamp(SnapWait > 15 ? SnapWait + 1 : SnapWait - 1, 0, float.PositiveInfinity);

		if (JawAngleTarget > JawAngle)
		{
			JawAngle += 0.3f;
		}
		else if (SnapCooldown <= 0)
		{
			JawAngle *= 0.98f;
		}
	}

	private void Snap(Rectangle snapBounds)
	{
		JawAngleTarget = MathHelper.PiOver2 * 0.78f;
		SnapCooldown = 200;
		SnapWait = 0;
		NPC.dontTakeDamage = false;

		foreach (Player player in Main.ActivePlayers)
		{
			if (player.Hitbox.Intersects(snapBounds))
			{
				var deathReason = PlayerDeathReason.ByCustomReason(this.GetLocalization("Death." + Main.rand.Next(3)).Format(player.name));
				player.Hurt(deathReason, ModeUtils.ByMode(300, 350, 480), 0, scalingArmorPenetration: 0.2f);
			}
		}

		SoundEngine.PlaySound(SoundID.Item70, NPC.Center);
	}

	public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		Texture2D texture = TextureAssets.Npc[Type].Value;
		Vector2 hitboxOffset = new Vector2(0, 20);
		var frame = new Rectangle(0, 0, 34, 50);
		Color bodyColor = NPC.IsABestiaryIconDummy ? Color.White : Lighting.GetColor(NPC.Top.ToTileCoordinates());
		Main.EntitySpriteDraw(texture, NPC.Center - screenPos + hitboxOffset, frame, bodyColor, 0f, frame.Size() / 2f, 1f, SpriteEffects.None, 0);

		frame = new Rectangle(0, 52, 40, 26);
		Vector2 lightOff = new(0, 10);
		Vector2 baseJawPos = NPC.position - screenPos + hitboxOffset;
		Vector2 leftJaw = baseJawPos + new Vector2(4, 14);
		Color leftJawColor = NPC.IsABestiaryIconDummy ? Color.White : Lighting.GetColor((leftJaw + screenPos - lightOff).ToTileCoordinates());
		Main.EntitySpriteDraw(texture, leftJaw, frame, leftJawColor, JawAngle, new Vector2(38, 24), 1f, SpriteEffects.None, 0);

		Vector2 rightJaw = baseJawPos + new Vector2(28, 14);
		Color rightJawColor = NPC.IsABestiaryIconDummy ? Color.White : Lighting.GetColor((rightJaw + screenPos - lightOff).ToTileCoordinates());
		Main.EntitySpriteDraw(texture, rightJaw, frame, rightJawColor, -JawAngle, new Vector2(0, 24), 1f, SpriteEffects.FlipHorizontally, 0);

		return false;
	}
}
