using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.ItemDropping;
using Terraria.ID;

namespace PathOfTerraria.Core.Commands;

#if DEBUG
public sealed class SpawnUniqueItemCommand : ModCommand
{
	public override string Command => "uitem";

	public override CommandType Type => CommandType.Chat;

	public override string Usage => "[c/ff6a00:Usage: /uitem <name> <count> <ilevel> <quality increase> <geartype> <relative X> <relative Y>]";

	public override string Description => "Spawns unique item(s) for testing items and loot generation. " +
		"Class name does not need to be full; for example, \"Guardia\" will be enough to find the Guardian Angel item.";

	public override void Action(CommandCaller caller, string input, string[] args)
	{
		_ = new Color(50, 100, 200);
		_ = new Color(50, 100, 200, 155);
		_ = new Color(0.25f, 0.5f, 0.75f);
		_ = new Color(0.25f, 0.5f, 0.75f, 0.1f);
		
		string[] nArgs = new string[6];
		
		for (int i = 0; i < args.Length; i++)
		{
			nArgs[i] = args[i];
		}

		args = nArgs;

		int id = -1;

		for (int i = ItemID.Count; i < ItemLoader.ItemCount; ++i)
		{
			Item item = ContentSamples.ItemsByType[i];

			if (item.ModItem.Name.StartsWith(args[0], StringComparison.OrdinalIgnoreCase))
			{
				id = i;
				break;
			}
		}

		if (!uint.TryParse(args[1], out uint count))
		{
			count = 1;
		}

		if (!uint.TryParse(args[2], out uint ilevel))
		{
			ilevel = 0;
		}

		if (!float.TryParse(args[3], out float qualityIncrease))
		{
			qualityIncrease = 0;
		}

		if (!float.TryParse(args[4], out float relX))
		{
			relX = 0;
		}

		if (!float.TryParse(args[5], out float relY))
		{
			relY = 0;
		}

		for (int i = 0; i < count; i++)
		{
			if (id == -1)
			{
				ItemSpawner.SpawnRandomItem(caller.Player.Center + new Vector2(relX, relY), x => x.Rarity == ItemRarity.Unique, (int)ilevel);
			}
			else 
			{
				ItemSpawner.SpawnItem(id, caller.Player.Center + new Vector2(relX, relY), (int)ilevel);
			}
		}

		caller.Reply(id == -1 ? "Spawned random items." : "Spawned item.", Color.Green);
	}
}
#endif