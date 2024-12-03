using System.Collections.Generic;
using PathOfTerraria.Common.Systems;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using PathOfTerraria.Core.Items;
using ReLogic.Content;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Sword;

internal class BloodOath : Sword, GenerateName.IItem
{
	public int ItemLevel
	{
		get => 1;
		set => this.GetInstanceData().RealLevel = value; // Technically preserves previous behavior.
	}

	protected override bool CloneNewInstances => true;

	private readonly HashSet<int> _hitNpcs = [];

	private bool _specialOn = false;

	public override ModItem Clone(Item newEntity)
	{
		// Items with special fields require cloning for things like dropping in world, splitting stacks (when applicable), displaying tooltips,
		// and some swapping operations (such as moving from or to chests).
		// This and CloneNewInstances makes sure the special isn't turned off or on when not desired.
		ModItem clone = base.Clone(newEntity);
		(clone as BloodOath)._specialOn = _specialOn;
		return clone;
	}

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 5f;
		staticData.IsUnique = true;
		staticData.Description = this.GetLocalization("Description");
		staticData.AltUseDescription = this.GetLocalization("AltUseDescription");
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.damage = 8;
		Item.width = 58;
		Item.height = 58;
		Item.UseSound = SoundID.Item1;
		Item.shoot = ProjectileID.None;
	}

	string GenerateName.IItem.GenerateName(string defaultName)
	{
		return $"[c/FF0000:{Language.GetTextValue("Mods.PathOfTerraria.Items.BloodOath.DisplayName")}]";
	}

	public override List<ItemAffix> GenerateImplicits()
	{
		var sharpAffix = (ItemAffix)Affix.CreateAffix<AddedDamageAffix>(0, 2, 5);
		var lifeAffix = (ItemAffix)Affix.CreateAffix<AddedLifeAffix>(0, 10, 10); // Add 10% life
		return [sharpAffix, lifeAffix];
	}

	public override bool AltFunctionUse(Player player)
	{
		AltUsePlayer modPlayer = player.GetModPlayer<AltUsePlayer>();

		if (!modPlayer.AltFunctionAvailable)
		{
			return false;
		}

		_specialOn = !_specialOn;

		if (_specialOn)
		{
			return false;
		}

		foreach (int npcWho in _hitNpcs)
		{
			NPC npc = Main.npc[npcWho];

			if (npc.active && npc.TryGetGlobalNPC(out BloodOathNPC bloodNpc))
			{
				bloodNpc.PopStacks(npc);
			}
		}

		modPlayer.SetAltCooldown(1 * 60, 0);

		return false;
	}

	public override void HoldItem(Player player)
	{
		base.HoldItem(player);

		if (_specialOn && Main.rand.NextBool(12))
		{
			Dust.NewDust(player.GetFrontHandPosition(Player.CompositeArmStretchAmount.None, 4f) + new Vector2(0, 4), 1,
				1, DustID.Firework_Red);
		}
	}

	public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
	{
		if (!_specialOn)
		{
			return;
		}

		var deathReason =
			PlayerDeathReason.ByCustomReason(
				Language.GetTextValue("Mods.PathOfTerraria.Items.BloodOath.DeathReason"));
		player.Hurt(deathReason, 1, 0, false, false, ImmunityCooldownID.TileContactDamage, false);
		player.hurtCooldowns[ImmunityCooldownID.TileContactDamage] = 3;
		target.GetGlobalNPC<BloodOathNPC>().ApplyStack(player.whoAmI);

		_hitNpcs.Add(target.whoAmI);
	}

	public class BloodOathNPC : GlobalNPC
	{
		public override bool InstancePerEntity => true;

		private static Asset<Texture2D> daggerTexture = null;

		private readonly Dictionary<int, int> _stacksPerPlayer = [];

		public override void SetStaticDefaults()
		{
			daggerTexture = ModContent.Request<Texture2D>("PathOfTerraria/Assets/Misc/VFX/OathDagger");
		}

		public void ApplyStack(int fromPlayer)
		{
			const int MaxStacks = 3;

			if (_stacksPerPlayer.TryGetValue(fromPlayer, out int value))
			{
				_stacksPerPlayer[fromPlayer] = ++value;
			}
			else
			{
				_stacksPerPlayer.Add(fromPlayer, 1);
			}

			if (_stacksPerPlayer[fromPlayer] > MaxStacks)
			{
				_stacksPerPlayer[fromPlayer] = MaxStacks;
			}
		}

		public void PopStacks(NPC npc)
		{
			foreach (int player in _stacksPerPlayer.Keys)
			{
				Player plr = Main.player[player];
				int value = _stacksPerPlayer[player];
				plr.Heal(3 * value);

				for (int i = 0; i < value; ++i)
				{
					sbyte direction = (sbyte)(i % 2 == 0 ? -1 : 1);
					float rotation = (Main.GameUpdateCount * 0.02f + i * 0.71f) * direction;
					Vector2 pos = npc.Center + new Vector2(i * 10).RotatedBy(rotation) - new Vector2(0, 40);

					for (int j = 0; j < 5; ++j)
					{
						Vector2 velocity = Main.rand.NextVector2Circular(4, 4);
						Dust.NewDust(pos, 1, 1, DustID.FireworkFountain_Red, velocity.X, velocity.Y);
					}
				}
			}

			_stacksPerPlayer.Clear();
		}

		public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			if (!_stacksPerPlayer.TryGetValue(Main.myPlayer, out int value) || value <= 0)
			{
				return;
			}

			for (int i = 0; i < value; ++i)
			{
				sbyte direction = (sbyte)(i % 2 == 0 ? -1 : 1);
				float rotation = (Main.GameUpdateCount * 0.02f + i * 0.71f) * direction;
				Vector2 pos = npc.Center - screenPos + new Vector2(i * 10).RotatedBy(rotation) - new Vector2(0, 40);
				float spriteRotation = npc.velocity.X * 0.06f;
				spriteBatch.Draw(daggerTexture.Value, pos, null, Color.White * (0.5f - i * 0.05f), spriteRotation,
					daggerTexture.Value.Size() / 2f, 1, SpriteEffects.None, 0);
			}
		}
	}
}