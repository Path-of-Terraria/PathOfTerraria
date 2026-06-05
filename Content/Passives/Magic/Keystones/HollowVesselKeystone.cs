using PathOfTerraria.Common.Systems.ElementalDamage;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives.Magic;

internal class HollowVesselKeystone : Passive
{
	internal class HollowVesselPlayer : ModPlayer
	{
		public override void PostUpdateEquips()
		{
			if (Player.GetModPlayer<PassiveTreePlayer>().HasNode<HollowVesselKeystone>())
			{
				// Set maximum life to 1
				Player.statLifeMax2 = 1;
				
				if (Player.statLife > 1)
				{
					Player.statLife = 1;
				}
			}
			
			ElementInstance chaos = Player.GetModPlayer<ElementalPlayer>()
				.Container[ElementType.Chaos];

			chaos.playerIsImmune = true;
		}
	}
}

