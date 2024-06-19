using PathOfTerraria.Content.Items.Gear;
using PathOfTerraria.Core.Systems.Affixes.ItemTypes.WeaponAffixes;

namespace PathOfTerraria.Core.Systems.ModPlayers;
internal class UniversalBuffingPlayer : ModPlayer
{
	public EntityModifier UniversalModifier;
	public float OnFireChance = 0;

	public override void PostUpdateEquips()
	{
		(Player.inventory[0].ModItem as Gear)?.ApplyAffixes(UniversalModifier);

		UniversalModifier.ApplyTo(Player);

		Player.statLifeMax = Math.Min(400, Player.statLifeMax2);
	}

	public override void ResetEffects()
	{
		UniversalModifier = new EntityModifier();
	}
	
	/// <summary>
	/// Used to apply on hit effects for affixes that have them
	/// </summary>
	/// <param name="target"></param>
	/// <param name="hit"></param>
	/// <param name="damageDone"></param>
	public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
	{
		if (OnFireChance > 0)
		{
			var affix = new ModifyHitAffixes.ChanceToApplyOnFireGearAffix();
			affix.TryApplyDebuff(target);
		}

		base.OnHitNPC(target, hit, damageDone);
	}
}
