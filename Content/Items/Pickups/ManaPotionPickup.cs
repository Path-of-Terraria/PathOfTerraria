﻿using PathOfTerraria.Common.Systems;
using PathOfTerraria.Common.Systems.Networking.Handlers;
using Terraria.Audio;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Pickups;

internal class ManaPotionPickup : PickupItem
{
	public override void PostUpdate()
	{
		Lighting.AddLight(Item.Center, new Vector3(0.02f, 0.05f, 0.3f) * (0.5f + (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.25f));
	}

	public override bool CanPickup(Player player)
	{
		return player.GetModPlayer<PotionSystem>().ManaLeft < player.GetModPlayer<PotionSystem>().MaxMana;
	}

	public override bool OnPickup(Player player)
	{
		player.GetModPlayer<PotionSystem>().ManaLeft++;

		if (Main.netMode != NetmodeID.SinglePlayer)
		{
			HotbarPotionHandler.SendHotbarPotionUse((byte)player.whoAmI, false, (byte)player.GetModPlayer<PotionSystem>().ManaLeft);
		}

		CombatText.NewText(player.Hitbox, new Color(150, 190, 255), "Mana Potion");

		for (int k = 0; k < 10; k++)
		{
			Dust.NewDustPerfect(Item.Center, DustID.PortalBoltTrail, Main.rand.NextVector2Circular(3, 3), 0, new Color(150, 190, 255), 0.5f);
		}

		var style = new SoundStyle($"{PoTMod.ModName}/Assets/Sounds/PickupPotion")
		{
			Pitch = -0.2f
		};

		SoundEngine.PlaySound(style);

		return false;
	}

	public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
	{
		Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
		Texture2D glow = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/GlowAlpha").Value;

		var glowColor = new Color(50, 90, 255)
		{
			A = 0
		};

		glowColor *= 0.6f + (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.1f;

		spriteBatch.Draw(glow, Item.Center - Main.screenPosition, null, Item.GetAlpha(glowColor), 0, glow.Size() / 2f, 0.6f, 0, 0);
		spriteBatch.Draw(tex, Item.Center - Main.screenPosition, null, Item.GetAlpha(Color.White), 0, tex.Size() / 2f, 1, 0, 0);

		return false;
	}
}