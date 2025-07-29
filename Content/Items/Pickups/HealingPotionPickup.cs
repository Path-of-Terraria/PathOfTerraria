﻿using PathOfTerraria.Common.Systems;
using PathOfTerraria.Common.Systems.Networking.Handlers;
using Terraria.Audio;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Pickups;

internal class HealingPotionPickup : PickupItem
{
	public override void PostUpdate()
	{
		Lighting.AddLight(Item.Center, new Vector3(0.3f, 0.02f, 0.02f) * (0.5f + (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.25f));
	}

	public override bool CanPickup(Player player)
	{
		int healLeft = player.GetModPlayer<PotionPlayer>().HealingLeft;
		int maxHeal = player.GetModPlayer<PotionPlayer>().MaxHealing;
		bool potionSpace = healLeft < maxHeal;
		bool orAutoHeal = healLeft >= maxHeal && player.statLife < player.statLifeMax2 && !player.HasBuff(BuffID.PotionSickness);
		return potionSpace || orAutoHeal;
	}

	public override bool OnPickup(Player player)
	{
		PotionPlayer potionPlr = player.GetModPlayer<PotionPlayer>();
		ref int healingLeft = ref potionPlr.HealingLeft;
		
		if (healingLeft >= player.GetModPlayer<PotionPlayer>().MaxHealing && player.statLife < player.statLifeMax2 && !player.HasBuff(BuffID.PotionSickness))
		{
			healingLeft++;
			PotionPlayer.UseHealingPotion(player, true);
		}
		else if (healingLeft < player.GetModPlayer<PotionPlayer>().MaxHealing)
		{
			healingLeft++;
			CombatText.NewText(player.Hitbox, new Color(255, 150, 150), this.GetLocalization("Pickup").Value);
		}

		if (Main.netMode != NetmodeID.SinglePlayer)
		{
			ModContent.GetInstance<HotbarPotionHandler>().Send((byte)player.whoAmI, true, (byte)player.GetModPlayer<PotionPlayer>().HealingLeft);
		}

		for (int k = 0; k < 10; k++)
		{
			Dust.NewDustPerfect(Item.Center, DustID.PortalBoltTrail, Main.rand.NextVector2Circular(3, 3), 0, new Color(255, 150, 150), 0.5f);
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

		var glowColor = new Color(255, 50, 50)
		{
			A = 0
		};

		glowColor *= 0.6f + (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.1f;

		spriteBatch.Draw(glow, Item.Center - Main.screenPosition, null, Item.GetAlpha(glowColor), 0, glow.Size() / 2f, 0.6f, 0, 0);
		spriteBatch.Draw(tex, Item.Center - Main.screenPosition, null, Item.GetAlpha(Color.White), 0, tex.Size() / 2f, 1, 0, 0);

		return false;
	}
}