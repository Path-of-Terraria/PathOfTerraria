using PathOfTerraria.Common.Systems;
using PathOfTerraria.Core.Items;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Sword;

internal class Cryobrand : Sword
{
	public enum FormType
	{
		Sword,
		Shield,
	}

	private FormType _form = FormType.Sword;
	private int _comboIndex = 0;
	private int _comboResetTimer = 0;
	private FormType[] _comboHistory = new FormType[3];

	protected override bool CloneNewInstances => true;

	public override ModItem Clone(Item newEntity)
	{
		ModItem clone = base.Clone(newEntity);
		var cryo = clone as Cryobrand;
		cryo._form = _form;
		cryo._comboIndex = _comboIndex;
		cryo._comboResetTimer = _comboResetTimer;
		cryo._comboHistory = (FormType[])_comboHistory.Clone();
		return clone;
	}

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 0f;
		staticData.IsUnique = true;
		staticData.Description = this.GetLocalization("Description");
		staticData.AltUseDescription = this.GetLocalization("AltUseDescription");
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.damage = 42;
		Item.width = 50;
		Item.height = 50;
		Item.useTime = 30;
		Item.useAnimation = 30;
		Item.knockBack = 6;
		Item.crit = 8;
		Item.UseSound = SoundID.Item1;
		Item.shoot = ProjectileID.None;
		Item.value = Item.buyPrice(0, 5, 0, 0);
	}

	public override void ModifyTooltips(List<TooltipLine> tooltips)
	{
		TooltipLine nameTip = tooltips.FirstOrDefault(x => x.Name == "ItemName");
		if (nameTip is not null)
		{
			nameTip.OverrideColor = Color.Red;
		}

		string formText = _form == FormType.Sword ? "Sword Form (Fire)" : "Shield Form (Cold)";
		tooltips.Add(new TooltipLine(Mod, "CryobrandForm", formText)
		{
			OverrideColor = _form == FormType.Sword ? Color.OrangeRed : Color.LightSkyBlue
		});
	}

	public override bool AltFunctionUse(Player player)
	{
		AltUsePlayer modPlayer = player.GetModPlayer<AltUsePlayer>();

		if (!modPlayer.AltFunctionAvailable)
		{
			return false;
		}

		_form = _form == FormType.Sword ? FormType.Shield : FormType.Sword;
		_comboIndex = 0;
		_comboResetTimer = 0;

		modPlayer.SetAltCooldown(30, 0);

		SoundEngine.PlaySound(_form == FormType.Sword ? SoundID.Item34 : SoundID.Item28, player.Center);

		for (int i = 0; i < 12; i++)
		{
			int dustId = _form == FormType.Sword ? DustID.Torch : DustID.Frost;
			Dust.NewDust(player.position, player.width, player.height, dustId,
				Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f));
		}

		return false;
	}

	public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		return false;
	}

	public override void HoldItem(Player player)
	{
		base.HoldItem(player);

		if (_comboResetTimer > 0)
		{
			_comboResetTimer--;
			if (_comboResetTimer == 0)
			{
				_comboIndex = 0;
			}
		}

		// Form-based passive buff
		// Sword Form: +15% damage done
		// Shield Form: -15% damage taken
		// TODO: move to EntityModifier/ApplyAffix when the form-specific affix exists.
		if (_form == FormType.Sword)
		{
			player.GetDamage(DamageClass.Melee) += 0.15f;
		}
		else
		{
			player.endurance += 0.15f;
		}
	}

	public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
	{
		// Shield form deals heavy cold damage; rough placeholder scaling.
		if (_form == FormType.Shield)
		{
			damage *= 1.2f;
		}
	}

	public override void UseAnimation(Player player)
	{
		base.UseAnimation(player);

		_comboHistory[_comboIndex % 3] = _form;
		_comboIndex++;
		_comboResetTimer = 60;

		if (_comboIndex >= 3)
		{
			ExecuteCombo(player);
			_comboIndex = 0;
		}
	}

	private void ExecuteCombo(Player player)
	{
		bool swordSwordShield =
			_comboHistory[0] == FormType.Sword &&
			_comboHistory[1] == FormType.Sword &&
			_comboHistory[2] == FormType.Shield;

		bool shieldShieldSword =
			_comboHistory[0] == FormType.Shield &&
			_comboHistory[1] == FormType.Shield &&
			_comboHistory[2] == FormType.Sword;

		if (swordSwordShield)
		{
			// TODO: fire slash, fire slash, throw and spin shield around player
			SoundEngine.PlaySound(SoundID.Item74, player.Center);
			for (int i = 0; i < 30; i++)
			{
				Dust.NewDust(player.Center, 1, 1, DustID.Torch,
					Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-6f, 6f));
			}
		}
		else if (shieldShieldSword)
		{
			// TODO: shield slam, shield slam, stab sword into ground for massive icy shockwave
			SoundEngine.PlaySound(SoundID.Item120, player.Center);
			for (int i = 0; i < 30; i++)
			{
				Dust.NewDust(player.Center, 1, 1, DustID.Frost,
					Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-6f, 6f));
			}
		}
		else if (_form == FormType.Sword)
		{
			// TODO: default sword-form third hit -> fiery whirlwind around player
			SoundEngine.PlaySound(SoundID.Item74, player.Center);
		}
		else
		{
			// TODO: default shield-form third hit -> ground slam icy shockwave
			SoundEngine.PlaySound(SoundID.Item120, player.Center);
		}
	}
}
