using System.Collections.Generic;
using System.IO;
using System.Linq;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.UI.Hotbar;
using PathOfTerraria.Content.SkillPassives.RainOfArrowsTree;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Content.Skills.Ranged;

public class RainOfArrows : Skill
{
	private static readonly HashSet<int> WeaponBlacklist = [ItemID.Harpoon, ItemID.Sandgun];

	public override int MaxLevel => 3;

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
		base.UseSkill(player);

		player.PickAmmo(player.HeldItem, out int projToShoot, out float speed, out int damage, out float knockBack, out int ammo, true);

		if (player.HeldItem.ModItem is not null)
		{
			Vector2 throwaway = Vector2.Zero;
			ItemLoader.ModifyShootStats(player.HeldItem, player, ref throwaway, ref throwaway, ref projToShoot, ref damage, ref knockBack);
		}

		damage = GetTotalDamage(damage * (1 + Level * 0.15f));

		if (projToShoot <= 0)
		{
			return;
		}

		for (int i = 0; i < 6 + Level * 2; ++i)
		{
			Vector2 pos = player.Center + new Vector2(Main.rand.NextFloat(-16, 16), Main.rand.NextFloat(-10, 10));
			Vector2 velocity = Vector2.UnitY.RotatedByRandom(0.6f) * -10 * Main.rand.NextFloat(0.9f, 1.1f);
			int proj = Projectile.NewProjectile(new EntitySource_ItemUse_WithAmmo(player, player.HeldItem, ammo), pos, velocity, projToShoot, damage, 2, player.whoAmI);

			Main.projectile[proj].GetGlobalProjectile<RainProjectile>().SetRainProjectile(Main.projectile[proj], this);
		}
	}

	public override bool CanUseSkill(Player player)
	{
		return base.CanUseSkill(player) && player.HeldItem.CountsAsClass(DamageClass.Ranged) && Main.myPlayer == player.whoAmI && 
			!WeaponBlacklist.Contains(player.HeldItem.type) && !player.HeldItem.consumable;
	}

	public override void ModifyTooltips(List<NewHotbar.SkillTooltip> tooltips)
	{
		tooltips.Remove(tooltips.FirstOrDefault(x => x.Name == "WeaponType"));
		NewHotbar.SkillTooltip tooltip = tooltips.First(x => x.Name == "Description");
		string text = Language.GetText($"Mods.{PoTMod.ModName}.Skills.NeedsWeapon").Format(WeaponType.LocalizeText().ToLower());

		if (Main.LocalPlayer.HeldItem.CountsAsClass(DamageClass.Ranged))
		{
			text = Language.GetText($"Mods.{PoTMod.ModName}.Skills.BaseDamage").Format(Main.LocalPlayer.HeldItem.damage);
		}

		tooltips.Add(new NewHotbar.SkillTooltip("ExtraDamage", text, tooltip.Slot + 0.1f));
	}

	private class RainProjectile : GlobalProjectile
	{
		public override bool InstancePerEntity => true;

		internal bool IsRainProjectile = false;
		internal Vector2 RainTarget = Vector2.Zero;

		private short _rainTimer = 0;
		private float _originalMagnitude = 0f;
		private bool _poison = false;

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

		internal void SetRainProjectile(Projectile projectile, RainOfArrows skill)
		{
			IsRainProjectile = true;
			RainTarget = Main.MouseWorld;

			_originalMagnitude = projectile.velocity.Length();
			_rainTimer = 0;

			if (skill.Tree.Specialization is NaturesBarrage)
			{
				_poison = true;
			}

			projectile.tileCollide = false;

			if (Main.netMode != NetmodeID.SinglePlayer)
			{
				NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, projectile.whoAmI);
			}
		}

		public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (_poison)
			{
				target.AddBuff(BuffID.Poisoned, 60 * 4);
			}
		}

		public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter)
		{
			bitWriter.WriteBit(IsRainProjectile);

			if (IsRainProjectile)
			{
				binaryWriter.WriteVector2(RainTarget);
				binaryWriter.Write((Half)_originalMagnitude);
				bitWriter.WriteBit(_poison);
			}
		}

		public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader)
		{
			IsRainProjectile = bitReader.ReadBit();

			if (IsRainProjectile)
			{
				RainTarget = binaryReader.ReadVector2();
				_originalMagnitude = (float)binaryReader.ReadHalf();
				_poison = bitReader.ReadBit();
			}
		}
	}
}