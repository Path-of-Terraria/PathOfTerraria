using FunnyExperience.Core.Systems;
using Terraria.Audio;
using Terraria.ID;

namespace FunnyExperience.Content.Items.Pickups
{
	internal class HealingPotionPickup : ModItem
	{
		public override string Texture => $"{FunnyExperience.ModName}/Assets/Items/HealingPotionPickup";

		public override void SetDefaults()
		{
			Item.width = 16;
			Item.height = 16;
		}

		public override void Update(ref float gravity, ref float maxFallSpeed)
		{
			Lighting.AddLight(Item.Center, new Vector3(0.3f, 0.02f, 0.02f) * (0.5f + (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.25f));
		}

		public override bool ItemSpace(Player player)
		{
			return true;
		}

		public override bool CanPickup(Player player)
		{
			return player.GetModPlayer<PotionSystem>().HealingLeft < player.GetModPlayer<PotionSystem>().MaxHealing;
		}

		public override bool OnPickup(Player player)
		{
			player.GetModPlayer<PotionSystem>().HealingLeft++;

			CombatText.NewText(player.Hitbox, new Color(255, 150, 150), "Healing Potion");

			for (int k = 0; k < 10; k++)
			{
				Dust.NewDustPerfect(Item.Center, DustID.PortalBoltTrail, Main.rand.NextVector2Circular(3, 3), 0, new Color(255, 150, 150), 0.5f);
			}

			var style = new SoundStyle($"{FunnyExperience.ModName}/Sounds/PickupPotion")
			{
				Pitch = -0.2f
			};

			SoundEngine.PlaySound(style);

			return false;
		}

		public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			Texture2D glow = ModContent.Request<Texture2D>($"{FunnyExperience.ModName}/Assets/GlowAlpha").Value;

			var glowColor = new Color(255, 50, 50)
			{
				A = 0
			};

			glowColor *= 0.6f + (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.1f;

			spriteBatch.Draw(glow, Item.Center - Main.screenPosition, null, glowColor, 0, glow.Size() / 2f, 0.6f, 0, 0);
			spriteBatch.Draw(tex, Item.Center - Main.screenPosition, null, Color.White, 0, tex.Size() / 2f, 1, 0, 0);

			return false;
		}
	}
}
