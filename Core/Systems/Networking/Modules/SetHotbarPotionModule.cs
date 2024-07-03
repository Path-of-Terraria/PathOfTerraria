using NetEasy;
using Terraria.ID;

namespace PathOfTerraria.Core.Systems.Networking.Modules;

[Serializable]
public class SetHotbarPotionModule(byte whoAmI, bool healingPotion, int value) : Module
{
	protected readonly byte WhoAmI = whoAmI;
	protected readonly bool HealingPotion = healingPotion;

	protected override void Receive()
	{
		PotionSystem mp = Main.player[WhoAmI].GetModPlayer<PotionSystem>();
		
		if (HealingPotion)
		{
			mp.HealingLeft = value;
		}
		else
		{
			mp.ManaLeft = value;
		}

		if (Main.netMode == NetmodeID.Server)
		{
			Send(ignoreClient: WhoAmI, runLocally: false);
		}
	}
}
