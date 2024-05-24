using PathOfTerraria.Content.Items.Gear;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Core.Systems.MobSystem
{
	internal class MobRaritySpawnSystem : GlobalNPC
	{
		public override bool InstancePerEntity => true;
		public MobRarity Rarity = MobRarity.Magic;

		public override void OnSpawn(NPC npc, IEntitySource source)
		{
			if (npc.friendly || npc.boss) //We only want to trigger these changes on hostile non-boss mobs
				return;
			
			Rarity = Main.rand.Next(100) switch
			{
				<2 => MobRarity.Rare, //2% Rare
				<17 => MobRarity.Magic, //15% Magic 
				_ => MobRarity.Normal,
			};
			npc.GivenName = Rarity switch
			{
				MobRarity.Magic or MobRarity.Rare => $"{Enum.GetName(Rarity)} - {npc.GivenOrTypeName}",
				MobRarity.Unique => "UNIQUE MOB",
				_ => npc.GivenName
			};
			switch (Rarity)
			{
				case MobRarity.Normal:
					break;
				case MobRarity.Magic:
					npc.color = new Color(0, 0, 255);
					npc.lifeMax *= 2; //Magic mobs get 100% increased life
					npc.life = npc.lifeMax + 1; //This will trigger health bar to appear
					npc.damage = Convert.ToInt32(npc.damage * 1.1); //Magic mobs get 10% increase damage
					break;
				case MobRarity.Rare:
					npc.color = new Color(255, 255, 0);
					npc.lifeMax *= 3; //Rare mobs get 200% Increased Life
					npc.life = npc.lifeMax + 1; //This will trigger health bar to appear
					npc.damage = Convert.ToInt32(npc.damage * 1.2); //Magic mobs get 20% increase damage
					break;
				case MobRarity.Unique:
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}