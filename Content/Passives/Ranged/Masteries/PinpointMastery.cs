using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using Terraria.DataStructures;

namespace PathOfTerraria.Content.Passives;

internal class PinpointMastery : Passive
{
	public override string TexturePath => $"{PoTMod.ModName}/Assets/Passives/MasteryPassive";

	internal class PinpointProjectile : GlobalProjectile
	{
		public override void OnSpawn(Projectile projectile, IEntitySource source)
		{
			if (!projectile.friendly || projectile.penetrate < 0 || !projectile.TryGetOwner(out Player player)
				|| !player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<PinpointMastery>(out float value))
			{
				return;
			}

			projectile.penetrate += (int)value;
		}
	}
}
