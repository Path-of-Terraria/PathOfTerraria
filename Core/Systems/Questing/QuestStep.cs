using PathOfTerraria.Core.Systems.ModPlayers;
using PathOfTerraria.Core.Systems.TreeSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.Items;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Core.Systems.Questing;
internal abstract class QuestStep
{
	public virtual void Track(Player player, Action onCompletion) { }
	public virtual void UnTrack() { }
	public virtual string QuestString() { return ""; }
	public virtual string QuestCompleteString() { return "Step completed"; }
	public virtual void Save(TagCompound tag) { }
	public virtual void Load(TagCompound tag) { }
}
