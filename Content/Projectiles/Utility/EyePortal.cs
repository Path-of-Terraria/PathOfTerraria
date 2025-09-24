using PathOfTerraria.Common.Projectiles;
using PathOfTerraria.Common.Subworlds.BossDomains.Prehardmode;
using PathOfTerraria.Content.Items.Consumables.Maps;
using SubworldLibrary;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Content.Projectiles.Utility;

internal class EyePortal : ModProjectile, ISaveProjectile, IRightClickableProjectile
{
	private ref float Timer => ref Projectile.ai[0];
	private ref float Uses => ref Projectile.ai[1];
	private ref float MaxUses => ref Projectile.ai[2];

			});
		});
	}

			});
		});
	}

	public override void SetDefaults()
	{
		Projectile.friendly = false;
		Projectile.hostile = false;
		Projectile.timeLeft = 2;
		Projectile.tileCollide = false;
		Projectile.Size = new Vector2(80, 80);
		Projectile.Opacity = 0.5f;
		Projectile.netImportant = true;
	}

	public override bool? CanDamage()
	{
		return false;
	}

	public override void AI()
	{
		if (NPC.downedBoss1)
		{
			Projectile.Kill();
			SpawnDust();
			return;
		}

		Projectile.timeLeft++;
		Projectile.rotation += 0.15f;
		Projectile.Opacity = MathHelper.Lerp(Projectile.Opacity, 1f, 0.05f);
		Projectile.velocity *= 0.96f;

		if (MaxUses == 0)
		{
			MaxUses = Map.GetBossUseCount();
		}
		
		if (Timer++ == 48)
		{
			SpawnDust();
		}

		if (Main.rand.NextBool(14))
		{
			Dust.NewDust(Projectile.position + new Vector2(8), Projectile.width - 16, Projectile.height - 16, DustID.Firework_Red);
		}

		Lighting.AddLight(Projectile.Center, TorchID.Red);
	}

	private void SpawnDust()
	{
		for (int i = 0; i < 20; ++i)
		{
			Dust.NewDust(Projectile.position + new Vector2(8), Projectile.width - 16, Projectile.height - 16, DustID.Firework_Red);
		}
	}

	public override bool PreDraw(ref Color lightColor)
	{
		Texture2D tex = TextureAssets.Projectile[Type].Value;

		for (int i = 0; i < 3; ++i)
		{
			float rotation = Projectile.rotation * (i % 2 == 0 ? -1 : 1);
			Vector2 position = Projectile.Center - Main.screenPosition;
			Color color = lightColor * ((3 - i) * 0.2f) * Projectile.Opacity;
			Main.spriteBatch.Draw(tex, position, null, color, rotation, tex.Size() / 2f, 1f - i * 0.2f, SpriteEffects.None, 0);
		}

		int highlightTextureDrawMode = TryInteracting();

		if (highlightTextureDrawMode == 0)
		{
			// If not in range, or if smart cursor is off, we don't draw the highlight texture at all.
			return false;
		}

		return false;
	}

	private int TryInteracting()
	{
		if (Main.gamePaused || Main.gameMenu)
		{
			return 0;
		}

		bool cursorHighlights = Main.SmartCursorIsUsed || PlayerInput.UsingGamepad;
		Player localPlayer = Main.LocalPlayer;
		Vector2 compareSpot = localPlayer.Center;

		if (!localPlayer.IsProjectileInteractibleAndInInteractionRange(Projectile, ref compareSpot))
		{
			return 0;
		}

		// Due to a quirk in how projectiles drawn using behindProjectiles are implemented,
		// we need to do some math to calculate the correct world position of the mouse instead of using Main.MouseWorld directly.
		Matrix matrix = Matrix.Invert(Main.GameViewMatrix.ZoomMatrix);
		Vector2 position = Main.ReverseGravitySupport(Main.MouseScreen);
		Vector2.Transform(Main.screenPosition, matrix);
		Vector2 realMouseWorld = Main.MouseWorld;// Vector2.Transform(position, matrix) + Main.screenPosition;

		bool mouseDirectlyOver = Projectile.Hitbox.Contains(realMouseWorld.ToPoint());
		bool interactingWithThisProjectile = mouseDirectlyOver || Main.SmartInteractProj == Projectile.whoAmI;

		if (!interactingWithThisProjectile || localPlayer.lastMouseInterface)
		{
			// 0 == Don't draw highlight texture, 1 == Draw faded highlight
			return cursorHighlights ? 1 : 0;
		}

		if (cursorHighlights)
		{
			return 2; // Draw highlight texture
		}
		else
		{
			return 0;
		}
	}

	public void SaveData(TagCompound tag)
	{
		tag.Add("uses", Uses);
	}

	public void LoadData(TagCompound tag, Projectile projectile)
	{
		projectile.ai[0] = 49;
		projectile.ai[1] = tag.GetFloat("uses");
	}

	bool IRightClickableProjectile.RightClick(Projectile self, Player player)
	{
		if (Main.mouseRight && Main.mouseRightRelease)
		{
			SubworldSystem.Enter<EyeDomain>();

			self.ai[1]++;
			self.netUpdate = true;

			if (self.ai[1] > self.ai[2])
			{
				self.Kill();
			}

			return true;
		}

		Tooltip.Create(new TooltipDescription
		{
			Identifier = "Portal",
			SimpleTitle = Language.GetTextValue($"Mods.{PoTMod.ModName}.Misc.Enter"),
		});
		return false;
	}
}
