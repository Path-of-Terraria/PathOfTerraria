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
			if (!projectile.friendly || !projectile.TryGetOwner(out Player player)
				|| !player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<PinpointMastery>(out float value))
			{
				return;
			}

			projectile.penetrate = projectile.penetrate == -1 ? -1 : projectile.penetrate + (int)value;
		}
	}
}
