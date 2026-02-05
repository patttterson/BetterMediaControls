# BetterMediaControls
A simple plugin for [On-Together: Virtual Co-Working](https://store.steampowered.com/app/3707400/OnTogether_Virtual_CoWorking/) that adds custom songs and shuffle functionality to the media controls.

## Custom Songs
You can add your own custom songs to the media controls by placing audio files in the `BepInEx/plugins/BetterMediaControls/music` directory.

Currently, due to a Unity limitation, only `.wav` files are supported. Any other file formats will be ignored.
> [!NOTE]
> Use https://vert.sh/ for quick and easy audio file conversion

### Identifying
By default, the songs will be identified by their file names. However, you can create a `playlist.json` file in the same directory to provide this metadata.
The `playlist.json` file should be structured as follows:

```json
{
  "playlist": [
    {
      "file": "eta.wav",
      "title": "ETA",
      "artist": "NewJeans",
      "volume": 0.5
    },
    {
      "file": "ditto.wav",
      "title": "Ditto",
      "artist": "NewJeans",
      "volume": 0.5
    }
  ]
}
```

In game, your custom music will appear above the default tracks in the same order they were put into the `playlist.json` file.

## Shuffle Functionality
The plugin adds a shuffle button to the media controls, located next to the loop button. It uses the Fisher-Yates algorithm (spotify shuffle) to shuffle the playlist.

When shuffle is enabled, the next track will be randomly selected from the playlist.

<img width="944" height="276" alt="image" src="https://github.com/user-attachments/assets/8be2ecad-5f74-459e-b3e0-891d16c81ee2" />

## Configuration
You can configure the plugin by editing the `BepInEx/config/BetterMediaControls.cfg` file.
The available configuration options are:
- `MusicDirectory`: The directory where custom music files are stored (default: `music` [starting from plugin directory]).
- `EnableShuffle`: Enable or disable shuffle functionality (default: true).

- `EnableVanillaMusic`: Enable or disable the default music tracks (default: true).


## Contributing
Copy `Assembly-CSharp.dll` and `UnityEngine.UI.dll` from the game's installation directory and put them in `lib/`

Example Commands (change based on your steam library location):
```bash
cp "C:\Program Files (x86)\Steam\steamapps\common\On-Together Virtual Co-Working\OnTogether_Data\Managed\Assembly-CSharp.dll" lib/
cp "C:\Program Files (x86)\Steam\steamapps\common\On-Together Virtual Co-Working\OnTogether_Data\Managed\UnityEngine.UI.dll" lib/
```