using System.Collections.Generic;
using PathOfTerraria.Core.Systems;
using PathOfTerraria.Core.Systems.Affixes;
using PathOfTerraria.Core.Systems.Affixes.ItemTypes.WeaponAffixes;
using ReLogic.Content;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Battleaxe;

internal class GuardianAngel : SteelBattleaxe
{
	public override float DropChance => 1f;
	public override bool IsUnique => true;
	public override string AltUseDescription => "Throw the axe to deal damage to enemies";
	public override string Description => "Something feels right about this axe...";
	public override int MinDropItemLevel => 26;

	public override void Defaults()
	{
		base.Defaults();
		Item.width = 94;
		Item.height = 108;
	}
	
	public override bool AltFunctionUse(Player player)
	{
		AltUsePlayer modPlayer = player.GetModPlayer<AltUsePlayer>();

		if (!modPlayer.AltFunctionAvailable)
		{
			return false;
		}

		foreach (NPC npc in Main.ActiveNPCs)
		{
			if (npc.CanBeChasedBy() && npc.DistanceSQ(player.Center) < 400 * 400)
			{
				npc.GetGlobalNPC<AngelRingNPC>().ApplyRing();
			}
		}

		modPlayer.SetAltCooldown(300, 180);
		return true;
	}
	
	public override bool CanUseItem(Player player)
	{
		return true;
	}

	public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
	{
		target.GetGlobalNPC<AngelRingNPC>().ApplyRing();
	}

	public override List<ItemAffix> GenerateAffixes()
	{
		var addedDamageAffix = (ItemAffix)Affix.CreateAffix<PassiveAffixes.AddedDamageAffix>();
		addedDamageAffix.MinValue = 1;
		addedDamageAffix.MaxValue = 4;
		
		var increasedDamageAffix = (ItemAffix)Affix.CreateAffix<PassiveAffixes.IncreasedDamageAffix>();
		increasedDamageAffix.Value = 0.1f;
		
		var attackSpeedAffix = (ItemAffix)Affix.CreateAffix<PassiveAffixes.IncreasedAttackSpeedAffix>();
		attackSpeedAffix.Value = 0.1f;
		
		var armorShredAffix = (ItemAffix)Affix.CreateAffix<ModifyHitAffixes.ChanceToApplyArmorShredGearAffix>();
		armorShredAffix.Value = 1f;
		armorShredAffix.Duration = 120;
		return [increasedDamageAffix, increasedDamageAffix, attackSpeedAffix, armorShredAffix];
	}

	private class AngelRingNPC : GlobalNPC
	{
		private static Asset<Texture2D> _ring = null;

		public override bool InstancePerEntity => true;

		private readonly float[] _ringFadeIn = new float[3];

		private int _ringCount = 0;
		private int _ringTime = 0;

		public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
		{
			return !entity.friendly && !entity.townNPC;
		}

		public override void SetStaticDefaults()
		{
			_ring = ModContent.Request<Texture2D>("PathOfTerraria/Assets/Misc/VFX/AngelRing");
		}

		public override void PostAI(NPC npc)
		{
			if (_ringCount >= 3)
			{
				DoLightBeam();
			}

			if (_ringTime-- < 0)
			{
				_ringCount = 0;
				_ringTime = 0;
			}
		}

		public void ApplyRing()
		{
			_ringCount++;
			_ringTime = 5 * 60;
		}

		private void DoLightBeam()
		{
		}

		public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			for (int i = 0; i < 3; i++)
			{
				if (_ringCount > i)
				{
					_ringFadeIn[i] = MathHelper.Lerp(_ringFadeIn[i], 1f, 0.05f + i * 0.01f);
				}
				else
				{
					_ringFadeIn[i] = MathHelper.Lerp(_ringFadeIn[i], 0f, 0.1f + i * 0.02f);
				}

				if (_ringFadeIn[i] < 0.01f)
				{
					continue;
				}

				var color = Color.Lerp(Lighting.GetColor(npc.Center.ToTileCoordinates()), Color.White, 0.3f);
				float scale = (1f + i * 0.4f) * _ringFadeIn[i];
				float rotation = (Main.GameUpdateCount * 0.02f + i * MathHelper.PiOver2) * (i % 2 == 0 ? -1 : 1);

				spriteBatch.Draw(_ring.Value, npc.Center - screenPos, null, color * _ringFadeIn[i] * (0.4f + i * 0.3f), rotation, _ring.Size() / 2f, scale, SpriteEffects.None, 0);
			}
		}
	}
}
