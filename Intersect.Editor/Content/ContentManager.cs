using Intersect.Editor.Core;
using Intersect.Editor.Localization;
using Intersect.Editor.Networking;
using Intersect.Framework.Core.GameObjects.Mapping.Tilesets;
using Intersect.GameObjects;
using Intersect.IO.Files;
using Intersect.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Intersect.Editor.Content;


public static partial class GameContentManager
{

    public enum TextureType
    {

        Tileset = 0,

        Item,

        Entity,

        Spell,

        Animation,

        Face,

        Image,

        Fog,

        Resource,

        Paperdoll,

        Gui,

        Misc,

    }

    //Game Content
    public static List<Texture> AllTextures { get; } = new List<Texture>();

    public static List<Texture> FogTextures = new List<Texture>();

    static IDictionary<string, Texture> sAnimationDict = new Dictionary<string, Texture>();

    //MonoGame Content Manager
    private static ContentManager sContentManger;

    static IDictionary<string, Texture> sEntityDict = new Dictionary<string, Texture>();

    private static string sErrorString = string.Empty;

    static IDictionary<string, Texture> sFaceDict = new Dictionary<string, Texture>();

    static IDictionary<string, Texture> sFogDict = new Dictionary<string, Texture>();

    static IDictionary<string, Texture> sGuiDict = new Dictionary<string, Texture>();

    static IDictionary<string, Texture> sImageDict = new Dictionary<string, Texture>();

    static IDictionary<string, Texture> sItemDict = new Dictionary<string, Texture>();

    static IDictionary<string, Texture> sMiscDict = new Dictionary<string, Texture>();

    static IDictionary<string, object> sMusicDict = new Dictionary<string, object>();

    static IDictionary<string, Texture> sPaperdollDict = new Dictionary<string, Texture>();

    static IDictionary<string, Texture> sResourceDict = new Dictionary<string, Texture>();

    static IDictionary<string, Effect> sShaderDict = new Dictionary<string, Effect>();

    static IDictionary<string, object> sSoundDict = new Dictionary<string, object>();

    static IDictionary<string, Texture> sSpellDict = new Dictionary<string, Texture>();

    static IDictionary<string, Texture> sTilesetDict = new Dictionary<string, Texture>();

    public static List<Texture> TilesetTextures = new List<Texture>();

    public static string[] MusicNames => sMusicDict?.Keys.ToArray();

    public static string[] SmartSortedMusicNames => SmartSort(MusicNames);

    public static string[] SoundNames => sSoundDict?.Keys.ToArray();

    public static string[] SmartSortedSoundNames => SmartSort(SoundNames);

    //Resource Downloader
    public static void CheckForResources()
    {
        if (!Directory.Exists("resources"))
        {
            MessageBox.Show(
                Strings.Errors.resourcesnotfound, Strings.Errors.resourcesnotfoundtitle, MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );

            Environment.Exit(1);
        }
    }

    public static void LoadEditorContent()
    {
        //Start by creating a MonoGame Content Manager
        //We create a dummy game service so we can load up a content manager.
        var container = new GameServiceContainer();
        container.AddService(
            typeof(IGraphicsDeviceService), new DummyGraphicsDeviceManager(Core.Graphics.GetGraphicsDevice())
        );

        sContentManger = new ContentManager(container, "");
        LoadEntities();
        LoadSpells();
        LoadAnimations();
        LoadImages();
        LoadFogs();
        LoadResources();
        LoadPaperdolls();
        LoadGui();
        LoadFaces();
        LoadItems();
        LoadMisc();
        LoadShaders();
        LoadSounds();
        LoadMusic();
    }

    public static void Update()
    {
        for (var i = 0; i < AllTextures.Count; i++)
        {
            AllTextures[i].Update();
        }
    }

    //Loading Game Resources
    public static void LoadTextureGroup(string directory, IDictionary<string, Texture> dict)
    {
        dict.Clear();
        if (!Directory.Exists("resources/" + directory))
        {
            Directory.CreateDirectory("resources/" + directory);
        }

        var items = Directory.GetFiles("resources/" + directory, "*.png");
        for (var i = 0; i < items.Length; i++)
        {
            var filename = items[i].Replace("resources/" + directory + "\\", "").ToLower();
            dict.Add(filename, new Texture("resources/" + directory + "/" + filename));
        }
    }

    public static void LoadTilesets()
    {
        if (!Directory.Exists("resources/tilesets"))
        {
            Directory.CreateDirectory("resources/tilesets");
        }

        var tilesets = Directory.GetFiles("resources/tilesets", "*.png");
        var tilesetWarning = false;
        var suppressTilesetWarning = Preferences.LoadPreference("SuppressTextureWarning");
        if (suppressTilesetWarning != "" && Convert.ToBoolean(suppressTilesetWarning))
        {
            tilesetWarning = true;
        }

        var newTilesets = new List<string>();
        Array.Sort(tilesets, new AlphanumComparatorFast());
        if (tilesets.Length > 0)
        {
            var tilesetBaseList = TilesetDescriptor.Names;
            for (var i = 0; i < tilesets.Length; i++)
            {
                tilesets[i] = tilesets[i].Replace("resources/tilesets\\", "");
                if (tilesetBaseList.Length > 0)
                {
                    for (var x = 0; x < tilesetBaseList.Length; x++)
                    {
                        if (tilesetBaseList[x].ToLower() == tilesets[i].ToLower())
                        {
                            break;
                        }

                        if (x != tilesetBaseList.Length - 1)
                        {
                            continue;
                        }

                        newTilesets.Add(tilesets[i]);
                    }
                }
                else
                {
                    newTilesets.Add(tilesets[i]);
                }
            }
        }

        sTilesetDict.Clear();
        TilesetTextures.Clear();
        var badTilesets = new List<string>();
        for (var i = 0; i < TilesetDescriptor.Lookup.Count; i++)
        {
            var tileset = TilesetDescriptor.Get(TilesetDescriptor.IdFromList(i));
            if (File.Exists("resources/tilesets/" + tileset.Name))
            {
                try
                {
                    sTilesetDict[tileset.Name.ToLower()] = new Texture("resources/tilesets/" + tileset.Name);
                    TilesetTextures.Add(sTilesetDict[tileset.Name.ToLower()]);
                }
                catch (Exception exception)
                {
                    Intersect.Core.ApplicationContext.Context.Value?.Logger.LogError(
                        exception,
                        $"Fake methods! ({tileset.Name})"
                    );
                    
                    if (exception.InnerException != null)
                    {
                        Intersect.Core.ApplicationContext.Context.Value?.Logger.LogError(
                            exception.InnerException,
                            "Caused by"
                        );
                    }

                    throw;
                }

                if (!tilesetWarning)
                {
                    using (var img = Image.FromFile("resources/tilesets/" + tileset.Name))
                    {
                        if (img.Width > 2048 || img.Height > 2048)
                        {
                            badTilesets.Add(tileset.Name);
                        }
                    }
                }
            }
        }

        if (badTilesets.Count > 0)
        {
            MessageBox.Show(
                "One or more tilesets is too large and likely won't load for your players on older machines! We recommmend that no graphic is larger than 2048 pixels in width or height.\n\nFaulting tileset(s): " +
                string.Join(",", badTilesets.ToArray()), "Large Tileset Warning!", MessageBoxButtons.OK,
                MessageBoxIcon.Exclamation
            );
        }

        if (newTilesets.Count > 0)
        {
            PacketSender.SendNewTilesets(newTilesets.ToArray());
        }
    }

    private static void LoadItems()
    {
        LoadTextureGroup("items", sItemDict);
    }

    private static void LoadEntities()
    {
        LoadTextureGroup("entities", sEntityDict);
    }

    private static void LoadSpells()
    {
        LoadTextureGroup("spells", sSpellDict);
    }

    private static void LoadAnimations()
    {
        LoadTextureGroup("animations", sAnimationDict);
    }

    private static void LoadFaces()
    {
        LoadTextureGroup("faces", sFaceDict);
    }

    private static void LoadImages()
    {
        LoadTextureGroup("images", sImageDict);
    }

    private static void LoadGui()
    {
        LoadTextureGroup("gui", sGuiDict);
    }

    private static void LoadFogs()
    {
        LoadTextureGroup("fogs", sFogDict);
        FogTextures.Clear();
        FogTextures.AddRange(sFogDict.Values.ToArray());
    }

    private static void LoadResources()
    {
        LoadTextureGroup("resources", sResourceDict);
    }

    private static void LoadPaperdolls()
    {
        LoadTextureGroup("paperdolls", sPaperdollDict);
    }

    private static void LoadMisc()
    {
        LoadTextureGroup("misc", sMiscDict);
    }

    public static void LoadShaders()
    {
        sShaderDict.Clear();

        const string shaderPrefix = "Intersect.Editor.Resources.Shaders.";
        var availableShaders = typeof(GameContentManager).Assembly
            .GetManifestResourceNames()
            .Where(resourceName =>
                resourceName.StartsWith(shaderPrefix)
                && resourceName.EndsWith(".xnb")
            ).ToArray();

        for (var i = 0; i < availableShaders.Length; i++)
        {
            var resourceFullName = availableShaders[i];
            var shaderName = resourceFullName.Substring(shaderPrefix.Length);

            using (var resourceStream = typeof(GameContentManager).Assembly.GetManifestResourceStream(resourceFullName))
            {
                var extractedPath = FileSystemHelper.WriteToTemporaryFolder(resourceFullName, resourceStream);
                var shader = sContentManger.Load<Effect>(Path.ChangeExtension(extractedPath, null));
                sShaderDict.Add(shaderName, shader);
            }
        }
    }

    public static void LoadSounds()
    {
        sSoundDict.Clear();
        if (!Directory.Exists("resources/" + "sounds"))
        {
            Directory.CreateDirectory("resources/" + "sounds");
        }

        var items = Directory.GetFiles("resources/" + "sounds", "*.wav");
        for (var i = 0; i < items.Length; i++)
        {
            var filename = items[i].Replace("resources/" + "sounds" + "\\", "").ToLower();
            sSoundDict.Add(filename, null); //TODO Sound Playback
        }
    }

    public static void LoadMusic()
    {
        sMusicDict.Clear();
        if (!Directory.Exists("resources/" + "music"))
        {
            Directory.CreateDirectory("resources/" + "music");
        }

        var items = Directory.GetFiles("resources/" + "music", "*.ogg");
        for (var i = 0; i < items.Length; i++)
        {
            var filename = items[i].Replace("resources/" + "music" + "\\", "").ToLower();
            sMusicDict.Add(filename, null); //TODO Music Playback
        }
    }

    public static string RemoveExtension(string fileName)
    {
        var fileExtPos = fileName.LastIndexOf(".");
        if (fileExtPos >= 0)
        {
            fileName = fileName.Substring(0, fileExtPos);
        }

        return fileName;
    }

    //Retreiving Game Resources
    //Content Getters
    public static Texture2D GetTexture(TextureType type, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        IDictionary<string, Texture> textureDict = null;
        switch (type)
        {
            case TextureType.Tileset:
                textureDict = sTilesetDict;

                break;
            case TextureType.Item:
                textureDict = sItemDict;

                break;
            case TextureType.Entity:
                textureDict = sEntityDict;

                break;
            case TextureType.Spell:
                textureDict = sSpellDict;

                break;
            case TextureType.Animation:
                textureDict = sAnimationDict;

                break;
            case TextureType.Face:
                textureDict = sFaceDict;

                break;
            case TextureType.Image:
                textureDict = sImageDict;

                break;
            case TextureType.Fog:
                textureDict = sFogDict;

                break;
            case TextureType.Resource:
                textureDict = sResourceDict;

                break;
            case TextureType.Paperdoll:
                textureDict = sPaperdollDict;

                break;
            case TextureType.Gui:
                textureDict = sGuiDict;

                break;
            case TextureType.Misc:
                textureDict = sMiscDict;

                break;
            default:
                return null;
        }

        if (textureDict == null)
        {
            return null;
        }

        if (textureDict == sTilesetDict
        ) //When assigning name in tilebase base we force it to be lowercase.. so lets save some processing time here..
        {
            return textureDict.TryGetValue(name, out var texture1) ? texture1.GetTexture() : null;
        }

        return textureDict.TryGetValue(name.ToLower(), out var texture) ? texture.GetTexture() : null;
    }

    public static Effect GetShader(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            Intersect.Core.ApplicationContext.Context.Value?.Logger.LogError("Tried to load shader with null name.");

            return null;
        }

        if (sShaderDict == null)
        {
            return null;
        }

        return sShaderDict.TryGetValue(name.ToLower(), out var effect) ? effect : null;
    }

    public static object GetMusic(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            Intersect.Core.ApplicationContext.Context.Value?.Logger.LogError("Tried to load music with null name.");

            return null;
        }

        if (sMusicDict == null)
        {
            return null;
        }

        return sMusicDict.TryGetValue(name.ToLower(), out var music) ? music : null;
    }

    public static object GetSound(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            Intersect.Core.ApplicationContext.Context.Value?.Logger.LogError("Tried to load sound with null name.");

            return null;
        }

        if (sSoundDict == null)
        {
            return null;
        }

        return sSoundDict.TryGetValue(name.ToLower(), out var sound) ? sound : null;
    }

    public static string[] GetSmartSortedTextureNames(TextureType type)
    {
        return SmartSort(GetTextureNames(type));
    }

    //Getting Filenames
    public static string[] GetTextureNames(TextureType type)
    {
        IDictionary<string, Texture> textureDict = null;
        switch (type)
        {
            case TextureType.Tileset:
                textureDict = sTilesetDict;

                break;
            case TextureType.Item:
                textureDict = sItemDict;

                break;
            case TextureType.Entity:
                textureDict = sEntityDict;

                break;
            case TextureType.Spell:
                textureDict = sSpellDict;

                break;
            case TextureType.Animation:
                textureDict = sAnimationDict;

                break;
            case TextureType.Face:
                textureDict = sFaceDict;

                break;
            case TextureType.Image:
                textureDict = sImageDict;

                break;
            case TextureType.Fog:
                textureDict = sFogDict;

                break;
            case TextureType.Resource:
                textureDict = sResourceDict;

                break;
            case TextureType.Paperdoll:
                textureDict = sPaperdollDict;

                break;
            case TextureType.Gui:
                textureDict = sGuiDict;

                break;
            case TextureType.Misc:
                textureDict = sMiscDict;

                break;
            default:
                return null;
        }

        var keys = textureDict?.Keys.ToArray();
        if (keys != null)
        {
            Array.Sort(keys, new AlphanumComparatorFast());
        }

        return textureDict?.Keys.ToArray();
    }

    private static readonly char[] OverridesDelimiter = new[] { '_' };

    public static IEnumerable<string> GetOverridesFor(TextureType textureType, string filterState, string filterBase = default)
    {
        return GetTextureNames(textureType)
            /* By using SelectMany() instead of Select() we do not have to then filter out null */
            .SelectMany(sprite =>
            {
                // Trim .png
                var baseName = Path.GetFileNameWithoutExtension(sprite);

                var searchState = $"_{filterState}";
                var indexOfState = baseName.IndexOf(searchState);
                if (indexOfState < 1 || baseName.Length <= indexOfState + searchState.Length)
                {
                    return Array.Empty<string>();
                }

                var customPart = baseName[(indexOfState + searchState.Length + 1)..].Trim();
                if (string.IsNullOrWhiteSpace(customPart))
                {
                    return Array.Empty<string>();
                }

                if (string.IsNullOrWhiteSpace(filterBase))
                {
                    return new[] { customPart };
                }

                var basePart = baseName[0..indexOfState].Trim();
                if (string.Equals(filterBase.Trim(), basePart, StringComparison.Ordinal))
                {
                    return new[] { customPart };
                }

                return Array.Empty<string>();
            })
            /* Find only distinct values, don't show duplicates */
            .Distinct();
    }

    private static string[] SmartSort(string[] strings)
    {
        var sortedStrings = strings ?? new string[] { };
        Array.Sort(sortedStrings, new AlphanumComparator());

        return sortedStrings;
    }

}

internal partial class DummyGraphicsDeviceManager : IGraphicsDeviceService
{

    public DummyGraphicsDeviceManager(GraphicsDevice graphicsDevice)
    {
        GraphicsDevice = graphicsDevice;
    }

    public GraphicsDevice GraphicsDevice { get; private set; }

    // Not used but required that these be here:
    public event EventHandler<EventArgs> DeviceCreated;

    public event EventHandler<EventArgs> DeviceDisposing;

    public event EventHandler<EventArgs> DeviceReset;

    public event EventHandler<EventArgs> DeviceResetting;

}
