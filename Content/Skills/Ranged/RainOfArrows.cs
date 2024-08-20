using System.Collections.Generic;
using System.IO;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Mechanics;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Content.Skills.Ranged;

public class RainOfArrows : Skill
{
	private static readonly HashSet<int> WeaponBlacklist = [ItemID.Harpoon, ItemID.Sandgun];

	public override int MaxLevel => 3;
	public override List<SkillPassive> Passives => [];

	public override void LevelTo(byte level)
	{
		Level = level;
		Cooldown = MaxCooldown = 10 * 60;
		ManaCost = 20;
		Duration = 0;
		WeaponType = ItemType.Ranged;
	}

	public override void UseSkill(Player player)
	{
		player.statMana -= ManaCost;
		Timer = Cooldown;

		player.PickAmmo(player.HeldItem, out int projToShoot, out float speed, out int damage, out float knockBack, out int _, true);

		if (player.HeldItem.ModItem is not null)
		{
			Vector2 throwaway = Vector2.Zero;
			ItemLoader.ModifyShootStats(player.HeldItem, player, ref throwaway, ref throwaway, ref projToShoot, ref damage, ref knockBack);
		}

		damage = (int)(damage * (0.7f + Level * 0.15f));

		if (projToShoot <= 0)
		{
			return;
		}

		for (int i = 0; i < 6 + Level * 2; ++i)
		{
			Vector2 pos = player.Center + new Vector2(Main.rand.NextFloat(-16, 16), Main.rand.NextFloat(-10, 10));
			Vector2 velocity = Vector2.UnitY.RotatedByRandom(0.6f) * -10 * Main.rand.NextFloat(0.9f, 1.1f);
			int proj = Projectile.NewProjectile(new EntitySource_Misc("RainOfArrows"), pos, velocity, projToShoot, damage, 2);

			Main.projectile[proj].GetGlobalProjectile<RainProjectile>().SetRainProjectile(Main.projectile[proj]);
		}
	}

	public override bool CanUseSkill(Player player)
	{
		return base.CanUseSkill(player) && player.HeldItem.CountsAsClass(DamageClass.Ranged) && Main.myPlayer == player.whoAmI && 
			!WeaponBlacklist.Contains(player.HeldItem.type) && !player.HeldItem.consumable;
	}

	private class RainProjectile : GlobalProjectile
	{
		public override bool InstancePerEntity => true;

		internal bool IsRainProjectile = false;
		internal Vector2 RainTarget = Vector2.Zero;

		private short _rainTimer = 0;
		private float _originalMagnitude = 0f;

		public override bool PreAI(Projectile projectile)
		{
			if (IsRainProjectile)
			{
				_rainTimer++;

				if (_rainTimer < 60f)
				{
					projectile.Opacity = 1 - _rainTimer / 60f;
				}
				else if (_rainTimer > 60f)
				{
					projectile.Opacity = MathHelper.Lerp(projectile.Opacity, 1f, 0.1f);
				}
				else
				{
					Vector2 offset = -Vector2.UnitY.RotatedByRandom(0.6f);
					projectile.Center = RainTarget + offset * 400;
					projectile.velocity = -offset * _originalMagnitude;

					for (int i = 0; i < 3; ++i)
					{
						Vector2 velocity = -projectile.velocity.RotatedByRandom(0.2f) * 0.3f;
						Dust.NewDust(projectile.Center, 1, 1, DustID.AncientLight, velocity.X, velocity.Y);
					}

					projectile.netUpdate = true;
				}

				if (projectile.Center.Y > RainTarget.Y)
				{
					projectile.tileCollide = true;
				}
			}

			return true;
		}

		internal void SetRainProjectile(Projectile projectile)
		{
			IsRainProjectile = true;
			RainTarget = Main.MouseWorld;

			_originalMagnitude = projectile.velocity.Length();
			_rainTimer = 0;

			projectile.tileCollide = false;

			if (Main.netMode != NetmodeID.SinglePlayer)
			{
				NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, projectile.whoAmI);
			}
		}

		public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter)
		{
			bitWriter.WriteBit(IsRainProjectile);

			if (IsRainProjectile)
			{
				binaryWriter.WriteVector2(RainTarget);
				binaryWriter.Write((Half)_originalMagnitude);
			}
		}

		public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader)
		{
			IsRainProjectile = bitReader.ReadBit();

			if (IsRainProjectile)
			{
				RainTarget = binaryReader.ReadVector2();
				_originalMagnitude = (float)binaryReader.ReadHalf();
			}
		}
	}
}