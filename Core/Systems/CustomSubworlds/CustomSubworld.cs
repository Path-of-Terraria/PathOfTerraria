using SubworldLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Core.Systems.CustomSubworlds;
public class CustomSubworld(string name, int width, int height, List<GenPass> tasks) : Subworld
{
	protected new void Register() { }

	public override string Name => name;

	public override int Width => width;

	public override int Height => height;

	public override List<GenPass> Tasks => tasks;

	public override int ReturnDestination => int.MinValue; // back to main screen

	public override bool ShouldSave => true; // we want to edit it and we will be replacing buildings / fixing them
}
