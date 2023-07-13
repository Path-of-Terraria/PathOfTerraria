using FunnyExperience.Core.Systems.TreeSystem;
using System.Collections.Generic;

namespace FunnyExperience.Content.Passives
{
	internal class LifePassive : Passive
	{
		public LifePassive() : base()
		{
			name = "Empowered Flesh";
			tooltip = "Increases your maximum life by 20 per level";
			maxLevel = 5;
			treePos = new Vector2(400, 400);
		}

		public override void BuffPlayer(Player player)
		{
			player.statLifeMax2 += 20 * level;
		}

		public override void Connect(List<Passive> all, Player player)
		{
			Connect<MeleePassive>(all, player);
			Connect<RangedPassive>(all, player);
			Connect<MagicPassive>(all, player);
			Connect<SummonPassive>(all, player);
		}
	}
}
