using NetEasy;
using Terraria.ID;

namespace PathOfTerraria.Core.Systems.Networking.Modules;

/// <summary>
/// Sets the corresponding potions remaining value to the given value on all clients but whoAmI and server.
/// Use the runLocally parameter on <see cref="Module.Receive"/> in order to set the value on all clients.
/// </summary>
/// <param name="whoAmI">The player who used the potion.</param>
/// <param name="healingPotion">If it's a healing or mana potion.</param>
/// <param name="value">What to set the new value to, after the action taken.</param>
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
