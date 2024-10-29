using PathOfTerraria.Common.Systems;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.VanillaModifications;
using PathOfTerraria.Core.Items;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Content.Items.Gear.Weapons.WarShields;

internal abstract class WarShield : Gear, IParryItem, GetItemLevel.IItem
{
	/// <summary>
	/// Stores shield data, namely how long the dash is, how long the cooldown is, and the speed of the dash.
	/// Times are in frames and speeds are in pixels/second.
	/// </summary>
	/// <param name="dashTime"></param>
	/// <param name="cooldown"></param>
	/// <param name="dashMagnitude"></param>
	public readonly struct ShieldData(int dashTime, int cooldown, float dashMagnitude, int bashDust)
	{
		public readonly int DashTime = dashTime;
		public readonly int Cooldown = cooldown;
		public readonly float DashMagnitude = dashMagnitude;
		public readonly int BashDust = bashDust;
	}

	int GetItemLevel.IItem.GetItemLevel(int realLevel)
	{
		return 1;
	}

	public virtual ShieldData Data => new(15, 100, 12, DustID.WoodFurniture);

	protected virtual int BoomerangCount => 1;
	protected override string GearLocalizationCategory => "WarShield";

	public override void Load()
	{
		string texture = $"{PoTMod.ModName}/Assets/Items/Gear/Weapons/WarShields/{GetType().Name}_Shield";
		EquipLoader.AddEquipTexture(Mod, texture, EquipType.Shield, this, $"{GetType().Name}_Shield");
	}

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;
		staticData.AltUseDescription = Language.GetText("Mods.PathOfTerraria.Gear.WarShield.AltUse");

		AddValidShieldParryItems.AddParryItem(Type);
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.CloneDefaults(ItemID.AdamantiteSword);
		Item.width = 16;
		Item.height = 28;
		Item.useTime = Data.DashTime;
		Item.useAnimation = Data.DashTime;
		Item.useStyle = ItemUseStyleID.Shoot;
		Item.damage = 5;
		Item.channel = true;
		Item.noMelee = true;
		Item.noUseGraphic = true;
		Item.crit = 6;
		Item.knockBack = 8;

		PoTInstanceItemData data = this.GetInstanceData();
		data.ItemType = Common.Enums.ItemType.WarShield;
	}

	public override bool CanUseItem(Player player)
	{
		if (player.altFunctionUse == 2)
		{
			return false;
		}
		
		return player.GetModPlayer<WarShieldPlayer>().CanBash;
	}

	public override bool AltFunctionUse(Player player)
	{
		return true;
	}

	public override bool? UseItem(Player player)
	{
		if (player.altFunctionUse != 2)
		{
			player.GetModPlayer<WarShieldPlayer>().StartBash(Data.DashTime, Data.Cooldown, Data.DashMagnitude);
		}

		return true;
	}

	public virtual bool CanRaiseShield(Player player)
	{
		AltUsePlayer altUsePlayer = player.GetModPlayer<AltUsePlayer>();
		return altUsePlayer.AltFunctionAvailable || altUsePlayer.AltFunctionActive;
	}

	public virtual void OnRaiseShield(Player player)
	{
		AltUsePlayer altUsePlayer = player.GetModPlayer<AltUsePlayer>();

		if (!altUsePlayer.AltFunctionActive)
		{
			altUsePlayer.SetAltCooldown(9 * 60, 60);
		}
	}

	public bool GetParryCondition(Player player)
	{
		AltUsePlayer altUsePlayer = player.GetModPlayer<AltUsePlayer>();
		return altUsePlayer.AltFunctionActive;
	}

	public virtual bool ParryProjectile(Player player, Projectile projectile)
	{
		projectile.velocity *= -1;
		projectile.friendly = true;
		projectile.owner = player.whoAmI;
		projectile.hostile = false;
		return true;
	}
}
