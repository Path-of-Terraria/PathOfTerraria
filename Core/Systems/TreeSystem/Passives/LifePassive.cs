using System.Collections.Generic;

namespace FunnyExperience.Core.Systems.TreeSystem.Passives
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

		public override void Connect(List<Passive> all)
		{
			Connect<MeleePassive>(all);
			Connect<RangedPassive>(all);
			Connect<MagicPassive>(all);
			Connect<SummonPassive>(all);
		}
	}
}
