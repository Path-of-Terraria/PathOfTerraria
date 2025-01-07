# Path of Terraria

## Contributing 

### Getting Started
1) Terraria is not included as a nuget package so we need to use the included tModLoader.targets file for pulling in the correct dependencies
    2) In the PathOfTerraria.csproj we see this line: `<Import Project="..\tModLoader.targets" />` - I have this setup so you can use this project outisde of the steam tModLoader folder. So the easiest way to get this to the place it's expecting (1 level up) simply copy the `tmodloader.targets` file from your steam tModLoader folder to the folder right above the root of this project.
3) We use SubworldLibrary for the subworlds. The .dll and .xml you need is located in the /Dependencies project. Subscribe to https://steamcommunity.com/sharedfiles/filedetails/?id=2785100219&searchtext=subworld
   4) **In Visual Studio** right click on "Dependencies" (NOT the same folder as where the .dll is, it's a non-existent folder that appears in the solution explorer) and click "Add Project Reference" and select the .dll file
   5) **In Rider** right click on "Dependencies"  (NOT the same folder as where the .dll is, it's a non-existent folder that appears in the solution explorer) and click "Reference..." and select the .dll file
4) You should now be ready to launch the project. Simply hit play in your IDE and it should launch Terraria with the mod loaded.


### Additional Tooling
There are several mods that can help us do local development. I recommend the following:
1) Dragonlens - https://steamcommunity.com/sharedfiles/filedetails/?id=2939737748
2) Structurehelper - https://steamcommunity.com/sharedfiles/filedetails/?id=2790924965

### Standards

#### Semantic Commits
This repository is setup to automatically tag and release new versions of the mod based on the branching structure.
Merges into `qa` -> Releases a prerelease tag
Merges into `main` -> Releases a production tag

Utilize semantic commits when commiting as these are used to generate the changelog. Commit messages should be treated as changelog ready messaging and should be conscise and direct.
More on semantic commmits here: https://www.conventionalcommits.org/en/v1.0.0/


### Project Notes
* All images are stored in the /Assets folder for consolidation. They are not in the same folder as the .cs files as tmodloader example mod has.
