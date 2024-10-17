using PathOfTerraria.Common.Systems.DrawLayers;
using Terraria.DataStructures;

namespace PathOfTerraria.Content.Buffs;

public sealed class MoltenShieldBuff : ModBuff
{
	public override void Load()
	{
		MiscDrawLayer.OnDraw += DrawMoltenShell;
	}

	public override void Unload()
	{
		MiscDrawLayer.OnDraw -= DrawMoltenShell;
	}

	private void DrawMoltenShell(ref PlayerDrawSet drawInfo)
	{
		Player player = drawInfo.drawPlayer;

		if (!player.HasBuff<MoltenShieldBuff>() || Main.gameMenu)
		{
			return;
		}

		Texture2D texture = ModContent.Request<Texture2D>("PathOfTerraria/Assets/Skills/MoltenShieldAnimation").Value;
		Rectangle frame = texture.Frame(1, 6, 0, (int)(player.GetModPlayer<MoltenShieldPlayer>().DrawTimer / 4f % 6));
		Vector2 position = drawInfo.Center - Main.screenPosition;
		drawInfo.DrawDataCache.Add(new DrawData(texture, position.Floor(), frame, Color.White, drawInfo.rotation, frame.Size() / 2f, 1f, SpriteEffects.None));
	}

	public override void Update(Player player, ref int buffIndex)
	{
		MoltenShieldPlayer shieldPlayer = player.GetModPlayer<MoltenShieldPlayer>();
		player.statDefense += shieldPlayer.ShieldLevel * 2;

		if (shieldPlayer.HealthBuffer <= 0) // Remove buff if buffer has run out
		{
			player.DelBuff(buffIndex);
			buffIndex--;
		}
	}

	public class MoltenShieldPlayer : ModPlayer
	{
		public int ShieldLevel = 0;
		public int HealthBuffer = 0;
		public bool SetOnce = true;

		internal int DrawTimer = 0;

		public override void ResetEffects()
		{
			if (!Player.HasBuff<MoltenShieldBuff>())
			{
				SetOnce = true;
				HealthBuffer = 0;
			}
		}

		public void SetBuff(int level, int duration)
		{
			Player.AddBuff(ModContent.BuffType<MoltenShieldBuff>(), duration);
			ShieldLevel = level;
			HealthBuffer = level * 15 + 20;
		}

		public override void ModifyHurt(ref Player.HurtModifiers modifiers)
		{
			modifiers.ModifyHurtInfo += ModifyHurtToUseBuffer;
		}

		private void ModifyHurtToUseBuffer(ref Player.HurtInfo info)
		{
			if (HealthBuffer <= 0)
			{
				return;
			}

			int damageToBeDone = (int)(info.Damage * 0.75f);
			int damageSaved = Math.Min(damageToBeDone, HealthBuffer);

			if (damageSaved > 0)
			{
				info.Damage -= damageSaved;
				HealthBuffer -= damageSaved;

				Rectangle offsetRect = Player.Hitbox;
				offsetRect.Offset(0, -54);
				CombatText.NewText(offsetRect, Color.LightYellow, damageSaved);
			}
		}

		public override void PreUpdate()
		{
			DrawTimer++;
		}
	}
}