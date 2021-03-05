# Update Night

Update Night is a simple-to-use fortnite leaking tool with a tons of features

## Features

- Grabs files from Manifest, so there is no need to download the update locally
- Grabs lastest AES and Mappings
- Extracts :
- - Cosmetics (and make a collage of them)
- - Challenges
- - Weapons
- - Current Map, both with and without POIs
- - Textures from `UI/`

## Installation

(requires any c# ide, such as visual studio, rider, etc)

- Download the code
- Open `UpdateNight.sln` in your ide
- Compile it (or publish it for a single exe)

## Requirements

- Device Auth (optional)

Update Night now requires a `deviceauth.json` for dynamic pak files
if it does not exists, Update Night wont support dynamic paks

The `deviceauth.json` should look like the following
```json
{
  "accountId": "...",
  "deviceId": "...",
  "secret": "..."
}
```

## TODO

- Optimize code
- Add :
- - Consumables
- - BattlePass
- Maybe Add (aka low priority) :
- - XP Coins in map
- - NPC Locations and quests

## Libraries

Update Night uses the following libraries

- [EpicManifestParser](https://github.com/NotOfficer/EpicManifestParser)
- [K4os.Compression.LZ4](https://github.com/MiloszKrajewski/K4os.Compression.LZ4)
- [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)
- [SkiaSharp](https://github.com/mono/SkiaSharp)
- [Usmap.NET](https://github.com/NotOfficer/Usmap.NET)
- [PakReader](https://github.com/iAmAsval/FModel/tree/master/FModel/PakReader)