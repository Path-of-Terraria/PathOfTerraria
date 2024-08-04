using PathOfTerraria.Content.Socketables;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Core.Items;

partial class GearGlobalItem
{
	public override void SaveData(Item item, TagCompound tag)
	{
		base.SaveData(item, tag);

		if (!IsGearItem(item))
		{
			return;
		}

		GearInstanceData gearData = item.GetGearData();

		tag["socketCount"] = gearData.Sockets.Length;

		for (int i = 0; i < gearData.Sockets.Length; i++)
		{
			if (gearData.Sockets[i] is not { } socket)
			{
				continue;
			}

			var newTag = new TagCompound();
			socket.Save(newTag);
			tag.Add("Socket" + i, newTag);
		}
	}

	public override void LoadData(Item item, TagCompound tag)
	{
		base.LoadData(item, tag);

		if (!IsGearItem(item))
		{
			return;
		}

		GearInstanceData gearData = item.GetGearData();

		int socketCount = tag.GetInt("socketCount");
		gearData.Sockets = new Socketable[socketCount];

		for (int i = 0; i < gearData.Sockets.Length; i++)
		{
			if (tag.TryGet("Socket" + i, out TagCompound newTag))
			{
				var g = Socketable.FromTag(newTag);
				if (g is not null)
				{
					gearData.Sockets[i] = g;
				}
			}
		}
	}
}
