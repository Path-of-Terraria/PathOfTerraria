using PathOfTerraria.Common.Projectiles;
using PathOfTerraria.Common.Systems.MapContent;
using PathOfTerraria.Common.Systems.Synchronization.Handlers;
using PathOfTerraria.Common.UI;
using PathOfTerraria.Content.Items.Pickups;
using ReLogic.Content;
using SubworldLibraryCommunityFork;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Content.Projectiles.Utility;

internal class ExitPortal : ModProjectile, IRightClickableProjectile, IMapIcon
{
	private static Asset<Texture2D> Highlight = null;

	private ref float ItemMagnetTimer => ref Projectile.ai[0];
	protected virtual bool IsPlayerCreatedReturnPortal => false;

	public override void SetStaticDefaults()
	{
		ProjectileID.Sets.IsInteractable[Type] = true;

		Highlight = ModContent.Request<Texture2D>(Texture + "_Highlight");
	}

	public override void SetDefaults()
	{
		Projectile.friendly = false;
		Projectile.hostile = false;
		Projectile.timeLeft = 2;
		Projectile.tileCollide = false;
		Projectile.Size = new Vector2(80, 80);
		Projectile.Opacity = 0f;
		Projectile.netImportant = true;
	}

	public override bool? CanDamage()
	{
		return false;
	}

	public override void AI()
	{
		Projectile.timeLeft++;
		Projectile.rotation += 0.2f;
		Projectile.Opacity = MathHelper.Lerp(Projectile.Opacity, 1f, 0.1f);

		Main.CurrentFrameFlags.HadAnActiveInteractibleProjectile = true;

		if (Main.rand.NextBool(14))
		{
			Dust.NewDust(Projectile.position + new Vector2(8), Projectile.width - 16, Projectile.height - 16, DustID.Firework_Red);
		}

		Lighting.AddLight(Projectile.Center, TorchID.Red);

		if (!IsPlayerCreatedReturnPortal && ItemMagnetTimer++ == 60)
		{
			MagnetizeItems();
		}

		return;
	}

	private void MagnetizeItems()
	{
		foreach (Item item in Main.ActiveItems)
		{
			// Invalid items to teleport (inactive or takes up a lot of space, might push items far away)
			if (!item.active || item.IsACoin || item.type == ModContent.ItemType<HealingPotionPickup>() || item.type == ModContent.ItemType<ManaPotionPickup>()
				|| item.type == ItemID.Heart || item.type == ItemID.Star || ItemID.Sets.NebulaPickup[item.type])
			{
				continue;
			}

			// Only update items if this is a server, or if this item is a client-only item
			if (Main.netMode == NetmodeID.MultiplayerClient && item.playerIndexTheItemIsReservedFor != Main.myPlayer)
			{
				continue;
			}

			Vector2 pos;

			do
			{
				pos = Projectile.Center + Main.rand.NextVector2Circular(80, 80);
			} while (Collision.SolidCollision(pos, item.width, item.height) || Collision.LavaCollision(pos, item.width, item.height));

			SpawnVFX(item.Center);

			item.Center = pos;
			item.shimmered = true;

			if (Main.netMode == NetmodeID.Server)
			{
				NetMessage.SendData(MessageID.SyncItemsWithShimmer, -1, -1, null, item.whoAmI, 1);
			}

			SpawnVFX(item.Center);
		}

		static void SpawnVFX(Vector2 position)
		{
			if (Main.netMode == NetmodeID.SinglePlayer)
			{
				SpawnShimmerTeleportVFX(position);
			}
			else if (Main.netMode == NetmodeID.Server)
			{
				SendSpawnVFXModule.Send(position, SendSpawnVFXModule.VFXType.ShimmerTeleport);
			}
		}
	}

	public static void SpawnShimmerTeleportVFX(Vector2 center)
	{
		for (int i = 0; i < 1; ++i)
		{
			var settings = new ParticleOrchestraSettings
			{
				PositionInWorld = center,
				MovementVector = new Vector2(0, -18).RotatedByRandom(0.2f) * Main.rand.NextFloat(0.6f, 1.2f),
			};

			ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.RainbowRodHit, settings);
		}
	}

	public override bool PreDraw(ref Color lightColor)
	{
		Texture2D tex = TextureAssets.Projectile[Type].Value;
		Vector2 position = Projectile.Center - Main.screenPosition;

		for (int i = 0; i < 3; ++i)
		{
			float rotation = Projectile.rotation * (i % 2 == 0 ? -1 : 1);
			Color color = lightColor * ((3 - i) * 0.2f) * Projectile.Opacity;
			Main.spriteBatch.Draw(tex, position, null, color, rotation, tex.Size() / 2f, 1f - i * 0.2f, SpriteEffects.None, 0);
		}

		this.DrawHighlightAndCheckRightClickInteraction(Highlight.Value, position, lightColor);
		return false;
	}

	internal static bool OpenFor(Player player)
	{
		int portalType = ModContent.ProjectileType<ReturnPortal>();

		foreach (Projectile projectile in Main.ActiveProjectiles)
		{
			if (projectile.type != portalType)
			{
				continue;
			}

			// Mapping worlds use one shared return portal. Keeping it world-owned makes it survive
			// the period between a subserver loading and its first player reconnecting, and lets the
			// server authoritatively synchronize later hotkey relocations.
			projectile.owner = Main.maxPlayers;
			projectile.Center = player.Center;
			projectile.velocity = Vector2.Zero;
			projectile.ai[0] = 0f;
			projectile.timeLeft = 2;
			projectile.netUpdate = true;

			if (Main.netMode == NetmodeID.Server)
			{
				NetMessage.SendData(MessageID.SyncProjectile, number: projectile.whoAmI);
			}

			return true;
		}

		int index = Projectile.NewProjectile(player.GetSource_Misc("OpenReturnPortal"), player.Center, Vector2.Zero,
			portalType, 0, 0f, Main.maxPlayers);

		if (index < 0 || index >= Main.maxProjectiles)
		{
			return false;
		}

		Projectile portal = Main.projectile[index];
		portal.netUpdate = true;

		if (Main.netMode == NetmodeID.Server)
		{
			NetMessage.SendData(MessageID.SyncProjectile, number: index);
		}

		return true;
	}

	bool IRightClickableProjectile.RightClick(Player player, bool mouseDirectlyOver)
	{
		if (Main.mouseRight && Main.mouseRightRelease)
		{
			SubworldSystem.Exit();
			return true;
		}

		if (mouseDirectlyOver)
		{
			Tooltip.Create(new TooltipDescription
			{
				Identifier = "Portal",
				SimpleTitle = Language.GetTextValue($"Mods.{PoTMod.ModName}.Misc."
					+ (IsPlayerCreatedReturnPortal ? "ReturnToOverworld" : "Enter")),
			});
		}

		return false;
	}
}
