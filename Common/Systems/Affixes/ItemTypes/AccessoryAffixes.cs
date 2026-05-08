using PathOfTerraria.Common.Systems.ModPlayers;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace PathOfTerraria.Common.Systems.Affixes.ItemTypes;

internal abstract class RoundedAccessoryAffix : ItemAffix
{
	protected RoundedAccessoryAffix()
	{
		Round = true;
	}
}

internal class MiningSpeedAffix : ItemAffix
{
	public MiningSpeedAffix()
	{
		Round = true;
	}

	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		player.pickSpeed *= MathHelper.Clamp(1f - Value / 100f, 0.1f, 1f);
	}
}

internal class FishingPowerAffix : RoundedAccessoryAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		player.fishingSkill += (int)Value;
	}
}

internal class BaitConservationAffix : RoundedAccessoryAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		player?.GetModPlayer<AccessoryAffixPlayer>().AddBaitSaveChance(Value);
	}
}

internal class TilePlacementSpeedAffix : RoundedAccessoryAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		player.tileSpeed += Value / 100f;
	}
}

internal class WallPlacementSpeedAffix : RoundedAccessoryAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		player.wallSpeed += Value / 100f;
	}
}

internal class ItemPickupRangeAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		player?.GetModPlayer<AccessoryAffixPlayer>().AddItemPickupRange(Value);
	}
}

internal class CoinPickupRangeAffix : RoundedAccessoryAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		player?.GetModPlayer<AccessoryAffixPlayer>().AddCoinPickupRange(Value);
	}
}

internal class PotionCooldownReductionAffix : RoundedAccessoryAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		float multiplier = MathHelper.Clamp(1f - Value / 100f, 0.1f, 1f);
		player?.GetModPlayer<AccessoryAffixPlayer>().AddPotionCooldownReduction(Value);
		if (player is null)
		{
			return;
		}

		PotionPlayer potionPlayer = player.GetModPlayer<PotionPlayer>();
		potionPlayer.HealDelay = (int)Math.Round(potionPlayer.HealDelay * multiplier);
		potionPlayer.ManaDelay = (int)Math.Round(potionPlayer.ManaDelay * multiplier);
	}
}

internal class BuffDurationAffix : RoundedAccessoryAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		player?.GetModPlayer<AccessoryAffixPlayer>().AddBuffDuration(Value);
		player.GetModPlayer<BuffModifierPlayer>().BuffBonus += Value / 100f;
	}
}

internal class BreathCapacityAffix : RoundedAccessoryAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		player?.GetModPlayer<AccessoryAffixPlayer>().AddBreathCapacity(Value);
		player.breathMax += (int)Math.Round(player.breathMax * Value / 100f);
	}
}

internal class SwimSpeedAffix : RoundedAccessoryAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		player?.GetModPlayer<AccessoryAffixPlayer>().AddSwimSpeed(Value);
	}
}

internal class SafeFallDistanceAffix : RoundedAccessoryAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		player?.GetModPlayer<AccessoryAffixPlayer>().AddSafeFallDistance(Value);
		player.extraFall += (int)Math.Round(Value);
	}
}

internal class JumpHeightAffix : RoundedAccessoryAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		player?.GetModPlayer<AccessoryAffixPlayer>().AddJumpHeight(Value);
	}
}

internal class JumpSpeedAffix : RoundedAccessoryAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		player?.GetModPlayer<AccessoryAffixPlayer>().AddJumpSpeed(Value);
		player.jumpSpeedBoost += Value / 100f;
	}
}

internal class WingFlightTimeAffix : RoundedAccessoryAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		player?.GetModPlayer<AccessoryAffixPlayer>().AddWingFlightTime(Value);
		player.wingTimeMax = (int)Math.Round(player.wingTimeMax * (1f + Value / 100f));
	}
}

internal class WingGlideControlAffix : RoundedAccessoryAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		player?.GetModPlayer<AccessoryAffixPlayer>().AddWingGlideFallSpeedReduction(Value);
	}
}

internal class AccessoryAffixPlayer : ModPlayer
{
	private float _baitSaveChance;
	private float _itemPickupRangeTiles;
	private float _coinPickupRangeTiles;
	private float _potionCooldownReduction;
	private float _buffDuration;
	private float _breathCapacity;
	private float _swimSpeed;
	private float _safeFallDistance;
	private float _jumpHeight;
	private float _jumpSpeed;
	private float _wingFlightTime;
	private float _wingGlideFallSpeedReduction;

	public float BaitSaveChance => _baitSaveChance;
	public float ItemPickupRangeTiles => _itemPickupRangeTiles;
	public float CoinPickupRangeTiles => _coinPickupRangeTiles;
	public float PotionCooldownReduction => _potionCooldownReduction;
	public float BuffDuration => _buffDuration;
	public float BreathCapacity => _breathCapacity;
	public float SwimSpeed => _swimSpeed;
	public float SafeFallDistance => _safeFallDistance;
	public float JumpHeight => _jumpHeight;
	public float JumpSpeed => _jumpSpeed;
	public float WingFlightTime => _wingFlightTime;
	public float WingGlideFallSpeedReduction => _wingGlideFallSpeedReduction;

	public override void ResetEffects()
	{
		_baitSaveChance = 0f;
		_itemPickupRangeTiles = 0f;
		_coinPickupRangeTiles = 0f;
		_potionCooldownReduction = 0f;
		_buffDuration = 0f;
		_breathCapacity = 0f;
		_swimSpeed = 0f;
		_safeFallDistance = 0f;
		_jumpHeight = 0f;
		_jumpSpeed = 0f;
		_wingFlightTime = 0f;
		_wingGlideFallSpeedReduction = 0f;
	}

	public void AddBaitSaveChance(float chance)
	{
		_baitSaveChance = MathHelper.Clamp(_baitSaveChance + chance, 0f, 75f);
	}

	public void AddItemPickupRange(float tiles)
	{
		_itemPickupRangeTiles += tiles;
	}

	public void AddCoinPickupRange(float tiles)
	{
		_coinPickupRangeTiles += tiles;
	}

	public void AddPotionCooldownReduction(float reduction)
	{
		_potionCooldownReduction = MathHelper.Clamp(_potionCooldownReduction + reduction, 0f, 90f);
	}

	public void AddBuffDuration(float duration)
	{
		_buffDuration += duration;
	}

	public void AddBreathCapacity(float capacity)
	{
		_breathCapacity += capacity;
	}

	public void AddSwimSpeed(float speed)
	{
		_swimSpeed += speed;
	}

	public void AddSafeFallDistance(float distance)
	{
		_safeFallDistance += distance;
	}

	public void AddJumpHeight(float height)
	{
		_jumpHeight = MathHelper.Clamp(_jumpHeight + height, 0f, 50f);
	}

	public void AddJumpSpeed(float speed)
	{
		_jumpSpeed += speed;
	}

	public void AddWingFlightTime(float flightTime)
	{
		_wingFlightTime += flightTime;
	}

	public void AddWingGlideFallSpeedReduction(float reduction)
	{
		_wingGlideFallSpeedReduction = MathHelper.Clamp(_wingGlideFallSpeedReduction + reduction, 0f, 50f);
	}

	public override void PostUpdateEquips()
	{
		if (Player.wet)
		{
			Player.moveSpeed += _swimSpeed / 100f;
		}

		if (_wingGlideFallSpeedReduction <= 0f || Player.wings <= 0 || Player.velocity.Y <= 0f || !Player.controlJump)
		{
			return;
		}

		float maxGlideFallSpeed = Player.maxFallSpeed * (1f - _wingGlideFallSpeedReduction / 100f);
		if (Player.velocity.Y > maxGlideFallSpeed)
		{
			Player.velocity.Y = maxGlideFallSpeed;
		}
	}

	public override bool? CanConsumeBait(Item bait)
	{
		if (_baitSaveChance <= 0f)
		{
			return null;
		}

		return Main.rand.NextFloat(100f) < _baitSaveChance ? false : null;
	}

	public float GetAdditionalPickupRange(Item item)
	{
		return item.IsACoin ? _coinPickupRangeTiles : _itemPickupRangeTiles;
	}

	public int GetAdditionalJumpTicks(int jumpTicks)
	{
		return (int)Math.Round(jumpTicks * _jumpHeight / 100f);
	}
}

internal class AccessoryAffixGlobalItem : GlobalItem
{
	public override void GrabRange(Item item, Player player, ref int grabRange)
	{
		float extraTiles = player.GetModPlayer<AccessoryAffixPlayer>().GetAdditionalPickupRange(item);

		if (extraTiles > 0f)
		{
			grabRange += (int)Math.Round(extraTiles * 16f);
		}
	}
}

internal class AccessoryAffixJumpSystem : ModSystem
{
	public override void Load()
	{
		IL_Player.JumpMovement += AddJumpHeightBonus;
	}

	public override void Unload()
	{
		IL_Player.JumpMovement -= AddJumpHeightBonus;
	}

	private static void AddJumpHeightBonus(ILContext il)
	{
		ILCursor cursor = new(il);

		if (!cursor.TryGotoNext(MoveType.After, x => x.MatchLdsfld<Player>(nameof(Player.jumpHeight)), x => x.MatchStfld<Player>(nameof(Player.jump))))
		{
			return;
		}

		cursor.Emit(OpCodes.Ldarg_0);
		cursor.EmitDelegate(ApplyJumpHeightBonus);
	}

	private static void ApplyJumpHeightBonus(Player player)
	{
		player.jump += player.GetModPlayer<AccessoryAffixPlayer>().GetAdditionalJumpTicks(player.jump);
	}
}
