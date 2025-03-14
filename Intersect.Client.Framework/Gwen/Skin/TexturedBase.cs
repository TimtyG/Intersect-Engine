using System.Diagnostics;
using Intersect.Client.Framework.Content;
using Intersect.Client.Framework.File_Management;
using Intersect.Client.Framework.GenericClasses;
using Intersect.Client.Framework.Graphics;
using Intersect.Client.Framework.Gwen.Control;
using Intersect.Client.Framework.Gwen.ControlInternal;
using Intersect.Client.Framework.Gwen.Skin.Texturing;
using Intersect.Core;
using Intersect.Framework;
using Intersect.Framework.Reflection;
using Microsoft.Extensions.Logging;
using Single = Intersect.Client.Framework.Gwen.Skin.Texturing.Single;

namespace Intersect.Client.Framework.Gwen.Skin;


#region UI element textures

public partial struct SkinTextures
{

    public Bordered StatusBar;

    public Bordered Selection;

    public Bordered Shadow;

    public Bordered Tooltip;

    public partial struct _Panel
    {
        public Bordered Control;

        public Bordered Normal;

        public Bordered Bright;

        public Bordered Dark;

        public Bordered Highlight;

    }

    public partial struct _Window
    {

        public Bordered Normal;

        public Bordered Inactive;

        public Bordered ActiveTitleBar;

        public Bordered InactiveTitleBar;

        public _Input._Button CloseButton;

    }

    public partial struct _FillableButton
    {
        public Single Box;

        public Single Fill;
    }

    public partial struct _CheckBox
    {

        public partial struct _Baked
        {
            public Single Normal;

            public Single Checked;
        }

        public _Baked Active_Baked;

        public _Baked Disabled_Baked;

        public _FillableButton Default;

        public _FillableButton Active;

        public _FillableButton Hovered;

        public _FillableButton Disabled;
    }

    public partial struct _RadioButton
    {
        public partial struct _Baked
        {
            public Single Normal;

            public Single Checked;
        }

        public _Baked Active_Baked;

        public _Baked Disabled_Baked;

        public _FillableButton Default;

        public _FillableButton Active;

        public _FillableButton Hovered;

        public _FillableButton Disabled;
    }

    public partial struct _TextBox
    {

        public Bordered Normal;

        public Bordered Focus;

        public Bordered Disabled;

    }

    public partial struct _Tree
    {

        public Bordered Background;

        public Single Minus;

        public Single Plus;

    }

    public partial struct _ProgressBar
    {

        public Bordered Back;

        public Bordered Front;

    }

    public partial struct _Scroller
    {
        public Bordered TrackH;
        public Bordered TrackV;
        public _Input._Button BarH;
        public _Input._Button BarV;

        public partial struct _Button
        {

            public Bordered[] Normal;

            public Bordered[] Hovered;

            public Bordered[] Active;

            public Bordered[] Disabled;

        }

        public _Button Button;

    }

    public partial struct _Menu
    {

        public Single RightArrow;

        public Single Check;

        public Bordered Strip;

        public Bordered Background;

        public Bordered BackgroundWithMargin;

        public Bordered Hovered;

    }

    public partial struct _Input
    {
        public partial struct _Button
        {
            public IAtlasDrawable Normal;
            public IAtlasDrawable Disabled;
            public IAtlasDrawable Hovered;
            public IAtlasDrawable Active;
        }

        public partial struct _ComboBox
        {

            public Bordered Normal;

            public Bordered Hover;

            public Bordered Down;

            public Bordered Disabled;

            public partial struct _Button
            {

                public Single Normal;

                public Single Hover;

                public Single Down;

                public Single Disabled;

            }

            public _Button Button;

        }

        public partial struct _Slider
        {
            public partial struct _SliderSource
            {
                public Bordered Normal;
                public Bordered Hover;
                public Bordered Active;
                public Bordered Disabled;
            }

            public _SliderSource H;

            public _SliderSource V;

        }

        public partial struct _ListBox
        {

            public Bordered Background;

            public Bordered Hovered;

            public Bordered EvenLine;

            public Bordered OddLine;

            public Bordered EvenLineSelected;

            public Bordered OddLineSelected;

        }

        public partial struct _UpDown
        {

            public partial struct _Up
            {

                public Single Normal;

                public Single Hover;

                public Single Down;

                public Single Disabled;

            }

            public partial struct _Down
            {

                public Single Normal;

                public Single Hover;

                public Single Down;

                public Single Disabled;

            }

            public _Up Up;

            public _Down Down;

        }

        public _Button Button;

        public _ComboBox ComboBox;

        public _Slider Slider;

        public _ListBox ListBox;

        public _UpDown UpDown;

    }

    public partial struct _Tab
    {

        public partial struct _Bottom
        {

            public Bordered Inactive;

            public Bordered Active;

        }

        public partial struct _Top
        {

            public Bordered Inactive;

            public Bordered Active;

        }

        public partial struct _Left
        {

            public Bordered Inactive;

            public Bordered Active;

        }

        public partial struct _Right
        {

            public Bordered Inactive;

            public Bordered Active;

        }

        public _Bottom Bottom;

        public _Top Top;

        public _Left Left;

        public _Right Right;

        public Bordered Control;

        public Bordered HeaderBar;

    }

    public partial struct _CategoryList
    {

        public Bordered Outer;

        public Bordered Inner;

        public Bordered Header;

    }

    public _Panel Panel;

    public _Window Window;

    public _CheckBox CheckBox;

    public _RadioButton RadioButton;

    public _TextBox TextBox;

    public _Tree Tree;

    public _ProgressBar ProgressBar;

    public _Scroller Scroller;

    public _Menu Menu;

    public _Input Input;

    public _Tab Tab;

    public _CategoryList CategoryList;

}

#endregion

/// <summary>
///     Base textured skin.
/// </summary>
public partial class TexturedBase : Skin.Base
{
    public static TexturedBase FindSkin(Renderer.Base renderer, GameContentManager contentManager, string skinName)
    {
        if (string.Equals("Intersect2021", skinName, StringComparison.InvariantCultureIgnoreCase))
        {
            return new Intersect2021(renderer, contentManager);
        }

        var skinNameParts = skinName.Split(':');
        var skinClassName = skinNameParts.FirstOrDefault() ?? nameof(IntersectSkin);
        var skinTextureName = skinNameParts.Skip(1).FirstOrDefault();

        if (string.Equals(nameof(IntersectSkin), skinClassName, StringComparison.InvariantCultureIgnoreCase))
        {
            return string.IsNullOrWhiteSpace(skinTextureName)
                ? new IntersectSkin(renderer, contentManager)
                : new IntersectSkin(renderer, contentManager, skinTextureName);
        }

        var skinMap = typeof(TexturedBase).Assembly.GetTypes()
            .Where(type => !type.IsAbstract && typeof(TexturedBase).IsAssignableFrom(type))
            .ToDictionary(type => type.Name.ToLowerInvariant(), type => type);

        if (!skinMap.TryGetValue(skinClassName.ToLowerInvariant(), out var skinType))
        {
            return new TexturedBase(renderer, contentManager, skinName);
        }

        try
        {
            var skin = Activator.CreateInstance(skinType, renderer, contentManager, skinTextureName) as TexturedBase;
            if (skin != null)
            {
                return skin;
            }

            ApplicationContext.Context.Value?.Logger.LogError(
                "Failed to create instance of '{SkinTypeName}'",
                skinType.GetName(qualified: true)
            );
        }
        catch
        {
            // ignore
        }

        return new TexturedBase(renderer, contentManager, skinName);
    }

    protected readonly IGameTexture _texture;

    protected SkinTextures mTextures;

    public string TextureName { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="TexturedBase" /> class.
    /// </summary>
    /// <param name="renderer">Renderer to use.</param>
    /// <param name="texture"></param>
    public TexturedBase(Renderer.Base renderer, IGameTexture texture) : base(renderer)
    {
        _texture = texture ?? throw new ArgumentNullException(nameof(texture));
        texture.Loaded += OnTextureLoaded;
        texture.Reload();
    }

    private void OnTextureLoaded(IAsset _)
    {
        InitializeColors();
        InitializeTextures();
    }

    public TexturedBase(Renderer.Base renderer, GameContentManager contentManager)
        : this(renderer, contentManager, "defaultskin.png")
    {
    }

    protected TexturedBase(Renderer.Base renderer, GameContentManager contentManager, string textureName)
        : this(renderer, contentManager?.GetTexture(Content.TextureType.Gui, textureName)!)
    {
    }

    /// <summary>
    ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public override void Dispose()
    {
        _texture.Loaded -= OnTextureLoaded;
        base.Dispose();
    }

    #region Initialization

    protected virtual void InitializeColors()
    {
        Colors.Window.TitleActive = Renderer.PixelColor(_texture, 4 + 8 * 0, 508, Color.Red);
        Colors.Window.TitleInactive = Renderer.PixelColor(_texture, 4 + 8 * 1, 508, Color.Yellow);

        Colors.Button.Normal = Renderer.PixelColor(_texture, 4 + 8 * 2, 508, Color.Yellow);
        Colors.Button.Hover = Renderer.PixelColor(_texture, 4 + 8 * 3, 508, Color.Yellow);
        Colors.Button.Active = Renderer.PixelColor(_texture, 4 + 8 * 2, 500, Color.Yellow);
        Colors.Button.Disabled = Renderer.PixelColor(_texture, 4 + 8 * 3, 500, Color.Yellow);

        Colors.Tab.Active.Normal = Renderer.PixelColor(_texture, 4 + 8 * 2, 508, Color.Yellow);
        Colors.Tab.Active.Hover = Renderer.PixelColor(_texture, 4 + 8 * 3, 508, Color.Yellow);
        Colors.Tab.Active.Active = Renderer.PixelColor(_texture, 4 + 8 * 2, 500, Color.Yellow);
        Colors.Tab.Active.Disabled = Renderer.PixelColor(_texture, 4 + 8 * 3, 500, Color.Yellow);
        Colors.Tab.Inactive.Normal = Renderer.PixelColor(_texture, 4 + 8 * 2, 508, Color.Yellow);
        Colors.Tab.Inactive.Hover = Renderer.PixelColor(_texture, 4 + 8 * 3, 508, Color.Yellow);
        Colors.Tab.Inactive.Active = Renderer.PixelColor(_texture, 4 + 8 * 2, 500, Color.Yellow);
        Colors.Tab.Inactive.Disabled = Renderer.PixelColor(_texture, 4 + 8 * 3, 500, Color.Yellow);

        Colors.Label.Normal = Renderer.PixelColor(_texture, 4 + 8 * 8, 508, Color.Yellow);
        Colors.Label.Hover = Renderer.PixelColor(_texture, 4 + 8 * 9, 508, Color.Yellow);
        Colors.Label.Disabled = Renderer.PixelColor(_texture, 4 + 8 * 8, 500, Color.Yellow);
        Colors.Label.Active = Renderer.PixelColor(_texture, 4 + 8 * 9, 500, Color.Yellow);

        Colors.Tree.Lines = Renderer.PixelColor(_texture, 4 + 8 * 10, 508, Color.Yellow);
        Colors.Tree.Normal = Renderer.PixelColor(_texture, 4 + 8 * 11, 508, Color.Yellow);
        Colors.Tree.Hover = Renderer.PixelColor(_texture, 4 + 8 * 10, 500, Color.Yellow);
        Colors.Tree.Selected = Renderer.PixelColor(_texture, 4 + 8 * 11, 500, Color.Yellow);

        Colors.Properties.LineNormal = Renderer.PixelColor(_texture, 4 + 8 * 12, 508, Color.Yellow);
        Colors.Properties.LineSelected = Renderer.PixelColor(_texture, 4 + 8 * 13, 508, Color.Yellow);
        Colors.Properties.LineHover = Renderer.PixelColor(_texture, 4 + 8 * 12, 500, Color.Yellow);
        Colors.Properties.Title = Renderer.PixelColor(_texture, 4 + 8 * 13, 500, Color.Yellow);
        Colors.Properties.ColumnNormal = Renderer.PixelColor(_texture, 4 + 8 * 14, 508, Color.Yellow);
        Colors.Properties.ColumnSelected = Renderer.PixelColor(_texture, 4 + 8 * 15, 508, Color.Yellow);
        Colors.Properties.ColumnHover = Renderer.PixelColor(_texture, 4 + 8 * 14, 500, Color.Yellow);
        Colors.Properties.Border = Renderer.PixelColor(_texture, 4 + 8 * 15, 500, Color.Yellow);
        Colors.Properties.LabelNormal = Renderer.PixelColor(_texture, 4 + 8 * 16, 508, Color.Yellow);
        Colors.Properties.LabelSelected = Renderer.PixelColor(_texture, 4 + 8 * 17, 508, Color.Yellow);
        Colors.Properties.LabelHover = Renderer.PixelColor(_texture, 4 + 8 * 16, 500, Color.Yellow);

        Colors.ModalBackground = Renderer.PixelColor(_texture, 4 + 8 * 18, 508, Color.Yellow);

        Colors.TooltipText = Renderer.PixelColor(_texture, 4 + 8 * 9, 508, Color.Yellow);

        Colors.Category.Header = Renderer.PixelColor(_texture, 4 + 8 * 18, 500, Color.Yellow);
        Colors.Category.HeaderClosed = Renderer.PixelColor(_texture, 4 + 8 * 19, 500, Color.Yellow);
        Colors.Category.Line.Text = Renderer.PixelColor(_texture, 4 + 8 * 20, 508, Color.Yellow);
        Colors.Category.Line.TextHover = Renderer.PixelColor(_texture, 4 + 8 * 21, 508, Color.Yellow);
        Colors.Category.Line.TextSelected = Renderer.PixelColor(_texture, 4 + 8 * 20, 500, Color.Yellow);
        Colors.Category.Line.Button = Renderer.PixelColor(_texture, 4 + 8 * 21, 500, Color.Yellow);
        Colors.Category.Line.ButtonHover = Renderer.PixelColor(_texture, 4 + 8 * 22, 508, Color.Yellow);
        Colors.Category.Line.ButtonSelected = Renderer.PixelColor(_texture, 4 + 8 * 23, 508, Color.Yellow);
        Colors.Category.LineAlt.Text = Renderer.PixelColor(_texture, 4 + 8 * 22, 500, Color.Yellow);
        Colors.Category.LineAlt.TextHover = Renderer.PixelColor(_texture, 4 + 8 * 23, 500, Color.Yellow);
        Colors.Category.LineAlt.TextSelected = Renderer.PixelColor(_texture, 4 + 8 * 24, 508, Color.Yellow);
        Colors.Category.LineAlt.Button = Renderer.PixelColor(_texture, 4 + 8 * 25, 508, Color.Yellow);
        Colors.Category.LineAlt.ButtonHover = Renderer.PixelColor(_texture, 4 + 8 * 24, 500, Color.Yellow);
        Colors.Category.LineAlt.ButtonSelected = Renderer.PixelColor(_texture, 4 + 8 * 25, 500, Color.Yellow);
    }

    protected virtual void InitializeTextures()
    {
        mTextures.Shadow = new Bordered(_texture, 448, 0, 31, 31, Margin.Eight);
        mTextures.Tooltip = new Bordered(_texture, 128, 320, 127, 31, Margin.Eight);
        mTextures.StatusBar = new Bordered(_texture, 128, 288, 127, 31, Margin.Eight);
        mTextures.Selection = new Bordered(_texture, 384, 32, 31, 31, Margin.Four);

        mTextures.Panel.Control = new Bordered(_texture, 256, 0, 63, 63, new Margin(16, 16, 16, 16));
        mTextures.Panel.Normal = new Bordered(_texture, 256, 0, 63, 63, new Margin(16, 16, 16, 16));
        mTextures.Panel.Bright = new Bordered(_texture, 256 + 64, 0, 63, 63, new Margin(16, 16, 16, 16));
        mTextures.Panel.Dark = new Bordered(_texture, 256, 64, 63, 63, new Margin(16, 16, 16, 16));
        mTextures.Panel.Highlight = new Bordered(_texture, 256 + 64, 64, 63, 63, new Margin(16, 16, 16, 16));

        mTextures.Window.Normal = new Bordered(_texture, 0, 0, 127, 127, new Margin(8, 32, 8, 8));
        mTextures.Window.Inactive = new Bordered(_texture, 128, 0, 127, 127, new Margin(8, 32, 8, 8));

        mTextures.CheckBox.Active_Baked.Checked = new Single(_texture, 448, 32, 15, 15);
        mTextures.CheckBox.Active_Baked.Normal = new Single(_texture, 464, 32, 15, 15);
        mTextures.CheckBox.Disabled_Baked.Normal = new Single(_texture, 448, 48, 15, 15);
        mTextures.CheckBox.Disabled_Baked.Normal = new Single(_texture, 464, 48, 15, 15);

        mTextures.RadioButton.Active_Baked.Checked = new Single(_texture, 448, 64, 15, 15);
        mTextures.RadioButton.Active_Baked.Normal = new Single(_texture, 464, 64, 15, 15);
        mTextures.RadioButton.Disabled_Baked.Normal = new Single(_texture, 448, 80, 15, 15);
        mTextures.RadioButton.Disabled_Baked.Normal = new Single(_texture, 464, 80, 15, 15);

        mTextures.TextBox.Normal = new Bordered(_texture, 0, 150, 127, 21, Margin.Four);
        mTextures.TextBox.Focus = new Bordered(_texture, 0, 172, 127, 21, Margin.Four);
        mTextures.TextBox.Disabled = new Bordered(_texture, 0, 193, 127, 21, Margin.Four);

        mTextures.Menu.Strip = new Bordered(_texture, 0, 128, 127, 21, Margin.One);
        mTextures.Menu.BackgroundWithMargin = new Bordered(_texture, 128, 128, 127, 63, new Margin(24, 8, 8, 8));
        mTextures.Menu.Background = new Bordered(_texture, 128, 192, 127, 63, Margin.Eight);
        mTextures.Menu.Hovered = new Bordered(_texture, 320, 320, 32, 32, Margin.Six);
        mTextures.Menu.RightArrow = new Single(_texture, 464, 112, 15, 15);
        mTextures.Menu.Check = new Single(_texture, 448, 112, 15, 15);

        mTextures.Tab.Control = new Bordered(_texture, 0, 256, 127, 127, Margin.Eight);
        mTextures.Tab.Bottom.Active = new Bordered(_texture, 0, 416, 63, 31, Margin.Eight);
        mTextures.Tab.Bottom.Inactive = new Bordered(_texture, 0 + 128, 416, 63, 31, Margin.Eight);
        mTextures.Tab.Top.Active = new Bordered(_texture, 0, 384, 63, 31, Margin.Eight);
        mTextures.Tab.Top.Inactive = new Bordered(_texture, 0 + 128, 384, 63, 31, Margin.Eight);
        mTextures.Tab.Left.Active = new Bordered(_texture, 64, 384, 31, 63, Margin.Eight);
        mTextures.Tab.Left.Inactive = new Bordered(_texture, 64 + 128, 384, 31, 63, Margin.Eight);
        mTextures.Tab.Right.Active = new Bordered(_texture, 96, 384, 31, 63, Margin.Eight);
        mTextures.Tab.Right.Inactive = new Bordered(_texture, 96 + 128, 384, 31, 63, Margin.Eight);
        mTextures.Tab.HeaderBar = new Bordered(_texture, 128, 352, 127, 31, Margin.Four);

        mTextures.Window.CloseButton = new SkinTextures._Input._Button
        {
            Normal = new Bordered(_texture, 0, 224, 24, 24, default),
            Disabled = new Bordered(_texture, 32, 224, 24, 24, default),
            Hovered = new Bordered(_texture, 64, 224, 24, 24, default),
            Active = new Bordered(_texture, 96, 224, 24, 24, default),
        };

        mTextures.Scroller.TrackV = new Bordered(_texture, 384, 208, 15, 127, Margin.Four);
        mTextures.Scroller.BarV.Normal = new Bordered(_texture, 384 + 16, 208, 15, 127, Margin.Four);
        mTextures.Scroller.BarV.Hovered = new Bordered(_texture, 384 + 32, 208, 15, 127, Margin.Four);
        mTextures.Scroller.BarV.Active = new Bordered(_texture, 384 + 48, 208, 15, 127, Margin.Four);
        mTextures.Scroller.BarV.Disabled = new Bordered(_texture, 384 + 64, 208, 15, 127, Margin.Four);
        mTextures.Scroller.TrackH = new Bordered(_texture, 384, 128, 127, 15, Margin.Four);
        mTextures.Scroller.BarH.Normal = new Bordered(_texture, 384, 128 + 16, 127, 15, Margin.Four);
        mTextures.Scroller.BarH.Hovered = new Bordered(_texture, 384, 128 + 32, 127, 15, Margin.Four);
        mTextures.Scroller.BarH.Active = new Bordered(_texture, 384, 128 + 48, 127, 15, Margin.Four);
        mTextures.Scroller.BarH.Disabled = new Bordered(_texture, 384, 128 + 64, 127, 15, Margin.Four);

        mTextures.Scroller.Button.Normal = new Bordered[4];
        mTextures.Scroller.Button.Disabled = new Bordered[4];
        mTextures.Scroller.Button.Hovered = new Bordered[4];
        mTextures.Scroller.Button.Active = new Bordered[4];

        mTextures.Tree.Background = new Bordered(_texture, 256, 128, 127, 127, new Margin(16, 16, 16, 16));
        mTextures.Tree.Plus = new Single(_texture, 448, 96, 15, 15);
        mTextures.Tree.Minus = new Single(_texture, 464, 96, 15, 15);

        mTextures.Input.Button.Normal = new Bordered(_texture, 480, 0, 31, 31, Margin.Eight);
        mTextures.Input.Button.Hovered = new Bordered(_texture, 480, 32, 31, 31, Margin.Eight);
        mTextures.Input.Button.Disabled = new Bordered(_texture, 480, 64, 31, 31, Margin.Eight);
        mTextures.Input.Button.Active = new Bordered(_texture, 480, 96, 31, 31, Margin.Eight);

        for (var i = 0; i < 4; i++)
        {
            mTextures.Scroller.Button.Normal[i] = new Bordered(_texture, 464 + 0, 208 + i * 16, 15, 15, Margin.Two);
            mTextures.Scroller.Button.Hovered[i] = new Bordered(_texture, 480, 208 + i * 16, 15, 15, Margin.Two);
            mTextures.Scroller.Button.Active[i] = new Bordered(_texture, 464, 272 + i * 16, 15, 15, Margin.Two);
            mTextures.Scroller.Button.Disabled[i] = new Bordered(
                _texture, 480 + 48, 272 + i * 16, 15, 15, Margin.Two
            );
        }

        mTextures.Input.ListBox.Background = new Bordered(_texture, 256, 256, 63, 127, Margin.Six);
        mTextures.Input.ListBox.Hovered = new Bordered(_texture, 320, 320, 32, 32, Margin.Six);
        mTextures.Input.ListBox.EvenLine = new Bordered(_texture, 352, 256, 32, 32, Margin.Six);
        mTextures.Input.ListBox.EvenLineSelected = new Bordered(_texture, 320, 256, 32, 32, Margin.Six);
        mTextures.Input.ListBox.OddLine = new Bordered(_texture, 352, 288, 32, 32, Margin.Six);
        mTextures.Input.ListBox.OddLineSelected = new Bordered(_texture, 320, 288, 32, 32, Margin.Six);

        mTextures.Input.ComboBox.Normal = new Bordered(_texture, 385, 336, 127, 31, new Margin(8, 8, 32, 8));
        mTextures.Input.ComboBox.Hover = new Bordered(_texture, 385, 336 + 32, 127, 31, new Margin(8, 8, 32, 8));
        mTextures.Input.ComboBox.Down = new Bordered(_texture, 385, 336 + 64, 127, 31, new Margin(8, 8, 32, 8));
        mTextures.Input.ComboBox.Disabled = new Bordered(_texture, 385, 336 + 96, 127, 31, new Margin(8, 8, 32, 8));

        mTextures.Input.ComboBox.Button.Normal = new Single(_texture, 496, 272, 15, 15);
        mTextures.Input.ComboBox.Button.Hover = new Single(_texture, 496, 272 + 16, 15, 15);
        mTextures.Input.ComboBox.Button.Down = new Single(_texture, 496, 272 + 32, 15, 15);
        mTextures.Input.ComboBox.Button.Disabled = new Single(_texture, 496, 272 + 48, 15, 15);

        mTextures.Input.UpDown.Up.Normal = new Single(_texture, 384, 112, 7, 7);
        mTextures.Input.UpDown.Up.Hover = new Single(_texture, 384 + 8, 112, 7, 7);
        mTextures.Input.UpDown.Up.Down = new Single(_texture, 384 + 16, 112, 7, 7);
        mTextures.Input.UpDown.Up.Disabled = new Single(_texture, 384 + 24, 112, 7, 7);
        mTextures.Input.UpDown.Down.Normal = new Single(_texture, 384, 120, 7, 7);
        mTextures.Input.UpDown.Down.Hover = new Single(_texture, 384 + 8, 120, 7, 7);
        mTextures.Input.UpDown.Down.Down = new Single(_texture, 384 + 16, 120, 7, 7);
        mTextures.Input.UpDown.Down.Disabled = new Single(_texture, 384 + 24, 120, 7, 7);

        mTextures.ProgressBar.Back = new Bordered(_texture, 384, 0, 31, 31, Margin.Eight);
        mTextures.ProgressBar.Front = new Bordered(_texture, 384 + 32, 0, 31, 31, Margin.Eight);

        mTextures.Input.Slider.H.Normal = new Bordered(_texture, 416, 32, 15, 15, new Margin(7));
        mTextures.Input.Slider.H.Hover = new Bordered(_texture, 416, 32 + 16, 15, 15, new Margin(7));
        mTextures.Input.Slider.H.Active = new Bordered(_texture, 416, 32 + 32, 15, 15, new Margin(7));
        mTextures.Input.Slider.H.Disabled = new Bordered(_texture, 416, 32 + 48, 15, 15, new Margin(7));

        mTextures.Input.Slider.V.Normal = new Bordered(_texture, 416 + 16, 32, 15, 15, new Margin(7));
        mTextures.Input.Slider.V.Hover = new Bordered(_texture, 416 + 16, 32 + 16, 15, 15, new Margin(7));
        mTextures.Input.Slider.V.Active = new Bordered(_texture, 416 + 16, 32 + 32, 15, 15, new Margin(7));
        mTextures.Input.Slider.V.Disabled = new Bordered(_texture, 416 + 16, 32 + 48, 15, 15, new Margin(7));

        mTextures.CategoryList.Outer = new Bordered(_texture, 256, 384, 63, 63, Margin.Eight);
        mTextures.CategoryList.Inner = new Bordered(_texture, 256 + 64, 384, 63, 63, new Margin(8, 21, 8, 8));
        mTextures.CategoryList.Header = new Bordered(_texture, 320, 352, 63, 31, Margin.Eight);
    }

    #endregion

    #region UI elements

    public override void DrawSplitter(SplitterBar splitterBar)
    {
        throw new NotImplementedException();
    }

    protected virtual void DrawButton(
        Button button,
        SkinTextures._Input._Button textureGroup
    )
    {
        var componentState = button.ComponentState;
        var stateTexture = button.GetStateTexture(componentState);
        if (stateTexture == null)
        {
            var drawable = componentState switch
            {
                ComponentState.Normal => textureGroup.Normal,
                ComponentState.Hovered => textureGroup.Hovered,
                ComponentState.Active => textureGroup.Active,
                ComponentState.Disabled => textureGroup.Disabled,
                _ => throw new UnreachableException(),
            };
            drawable.Draw(Renderer, button.RenderBounds, button.RenderColor);
        }
        else
        {
            Renderer.DrawColor = button.RenderColor;
            Renderer.DrawTexturedRect(stateTexture, button.RenderBounds, button.RenderColor);
        }

        // ReSharper disable once InvertIf
        if (button.HasFocus)
        {
            Renderer.PushDrawColor(Color.White);
            Renderer.PushLineThickness(1);
            Renderer.DrawLinedRect(button.RenderBounds);
            Renderer.PopLineThickness();
            Renderer.PopDrawColor();
        }
    }

    public override void DrawButton(Button button)
    {
        DrawButton(button, mTextures.Input.Button);
    }

    public override void DrawWindowCloseButton(CloseButton closeButton)
    {
        DrawButton(closeButton, mTextures.Window.CloseButton);
    }

    public override void DrawMenuRightArrow(Control.Base control)
    {
        mTextures.Menu.RightArrow.Draw(Renderer, control.RenderBounds, control.RenderColor);
    }

    public override void DrawMenuItem(Control.Base control, bool submenuOpen, bool isChecked)
    {
        if (!control.ShouldDrawBackground)
        {
            return;
        }

        if (submenuOpen || control.IsHovered)
        {
            mTextures.Menu.Hovered.Draw(Renderer, control.RenderBounds, control.RenderColor);
            return;
        }

        if (isChecked)
        {
            mTextures.Menu.Check.Draw(
                Renderer, new Rectangle(control.RenderBounds.X + 4, control.RenderBounds.Y + 3, 15, 15),
                control.RenderColor
            );
        }
    }

    public override void DrawMenuStrip(Control.Base control)
    {
        mTextures.Menu.Strip.Draw(Renderer, control.RenderBounds, control.RenderColor);
    }

    public override void DrawMenu(Control.Base control, bool paddingDisabled)
    {
        if (!control.ShouldDrawBackground)
        {
            return;
        }

        if (control is not Menu menu)
        {
            return;
        }

        if (menu.GetTemplate() is not { } menuTemplate)
        {
            if (paddingDisabled)
            {
                mTextures.Menu.Background.Draw(Renderer, control.RenderBounds, control.RenderColor);
            }
            else
            {
                mTextures.Menu.BackgroundWithMargin.Draw(Renderer, control.RenderBounds, control.RenderColor);
            }


            return;
        }

        //Draw Top Left Corner
        Renderer.DrawTexturedRect(
            menuTemplate,
            new Rectangle(control.RenderBounds.X, control.RenderBounds.Y, 2, 2),
            control.RenderColor,
            0,
            0,
            2f / menuTemplate.Width,
            2f / menuTemplate.Height
        );

        //Draw Top
        Renderer.DrawTexturedRect(
            menuTemplate,
            new Rectangle(control.RenderBounds.X + 2, control.RenderBounds.Y, control.RenderBounds.Width - 4, 2),
            control.RenderColor,
            2f / menuTemplate.Width,
            0,
            (menuTemplate.Width - 2f) / menuTemplate.Width,
            2f / menuTemplate.Height
        );

        //Draw Top Right Corner
        Renderer.DrawTexturedRect(
            menuTemplate,
            new Rectangle(control.RenderBounds.Right - 2, control.RenderBounds.Y, 2, 2),
            control.RenderColor,
            (menuTemplate.Width - 2f) / menuTemplate.Width,
            0,
            1f,
            2f / menuTemplate.Height
        );

        //Draw Left
        Renderer.DrawTexturedRect(
            menuTemplate,
            new Rectangle(control.RenderBounds.X, control.RenderBounds.Y + 2, 2, control.RenderBounds.Height - 4),
            control.RenderColor,
            0,
            2f / menuTemplate.Height,
            2f / menuTemplate.Width,
            (menuTemplate.Height - 2f) / menuTemplate.Height
        );

        //Draw Middle
        Renderer.DrawTexturedRect(
            menuTemplate,
            new Rectangle(
                control.RenderBounds.X + 2,
                control.RenderBounds.Y + 2,
                control.RenderBounds.Width - 4,
                control.RenderBounds.Height - 4
            ),
            control.RenderColor,
            2f / menuTemplate.Width,
            2f / menuTemplate.Height,
            (menuTemplate.Width - 2f) / menuTemplate.Width,
            (menuTemplate.Height - 2f) / menuTemplate.Height
        );

        //Draw Right
        Renderer.DrawTexturedRect(
            menuTemplate,
            new Rectangle(
                control.RenderBounds.Width - 2,
                control.RenderBounds.Y + 2,
                2,
                control.RenderBounds.Height - 4
            ),
            control.RenderColor,
            (menuTemplate.Width - 2f) / menuTemplate.Width,
            2f / menuTemplate.Height,
            1,
            (menuTemplate.Height - 2f) / menuTemplate.Height
        );

        // Draw Bottom Left Corner
        Renderer.DrawTexturedRect(
            menuTemplate,
            new Rectangle(control.RenderBounds.X, control.RenderBounds.Bottom - 2, 2, 2),
            control.RenderColor,
            0,
            (menuTemplate.Height - 2f) / menuTemplate.Height,
            2f / menuTemplate.Width,
            1f
        );

        //Draw Bottom
        Renderer.DrawTexturedRect(
            menuTemplate,
            new Rectangle(
                control.RenderBounds.X + 2,
                control.RenderBounds.Bottom - 2,
                control.RenderBounds.Width - 4,
                2
            ),
            control.RenderColor,
            2f / menuTemplate.Width,
            (menuTemplate.Height - 2f) / menuTemplate.Height,
            (menuTemplate.Width - 2f) / menuTemplate.Width,
            1f
        );

        //Draw Bottom Right Corner
        Renderer.DrawTexturedRect(
            menuTemplate,
            new Rectangle(control.RenderBounds.Right - 2, control.RenderBounds.Bottom - 2, 2, 2),
            control.RenderColor,
            (menuTemplate.Width - 2f) / menuTemplate.Width,
            (menuTemplate.Height - 2f) / menuTemplate.Height,
            1f,
            1f
        );
    }

    public override void DrawShadow(Control.Base control)
    {
        var r = control.RenderBounds;
        r.X -= 4;
        r.Y -= 4;
        r.Width += 10;
        r.Height += 10;
        mTextures.Shadow.Draw(Renderer, r, control.RenderColor);
    }

    public override void DrawRadioButton(Control.Base control, bool selected, bool depressed)
    {
        if (TryGetOverrideTexture(control as Checkbox, selected, depressed, out var overrideTexture))
        {
            Renderer.DrawColor = control.RenderColor;
            Renderer.DrawTexturedRect(overrideTexture, control.RenderBounds, control.RenderColor, 0, 0);
            return;
        }

        if (selected)
        {
            if (control.IsDisabledByTree)
            {
                mTextures.RadioButton.Disabled_Baked.Checked.Draw(Renderer, control.RenderBounds, control.RenderColor);
            }
            else
            {
                mTextures.RadioButton.Active_Baked.Checked.Draw(Renderer, control.RenderBounds, control.RenderColor);
            }
        }
        else
        {
            if (control.IsDisabledByTree)
            {
                mTextures.RadioButton.Disabled_Baked.Normal.Draw(Renderer, control.RenderBounds, control.RenderColor);
            }
            else
            {
                mTextures.RadioButton.Active_Baked.Normal.Draw(Renderer, control.RenderBounds, control.RenderColor);
            }
        }
    }

    protected bool TryGetOverrideTexture(Checkbox control, bool selected, bool pressed, out IGameTexture overrideTexture)
    {
        Checkbox.ControlState controlState = Checkbox.ControlState.Normal;
        if (selected)
        {
            controlState = control.IsDisabledByTree ? Checkbox.ControlState.CheckedDisabled : Checkbox.ControlState.CheckedNormal;
        }
        else if (control.IsDisabledByTree)
        {
            controlState = Checkbox.ControlState.Disabled;
        }

        overrideTexture = control.GetImage(controlState);
        return overrideTexture != default;
    }

    public override void DrawCheckBox(Control.Base control, bool selected, bool depressed)
    {
        if (TryGetOverrideTexture(control as Checkbox, selected, depressed, out var overrideTexture))
        {
            Renderer.DrawColor = control.RenderColor;
            Renderer.DrawTexturedRect(overrideTexture, control.RenderBounds, control.RenderColor, 0, 0);
            return;
        }

        if (selected)
        {
            if (control.IsDisabledByTree)
            {
                mTextures.CheckBox.Disabled_Baked.Checked.Draw(Renderer, control.RenderBounds, control.RenderColor);
            }
            else
            {
                mTextures.CheckBox.Active_Baked.Checked.Draw(Renderer, control.RenderBounds, control.RenderColor);
            }
        }
        else
        {
            if (control.IsDisabledByTree)
            {
                mTextures.CheckBox.Disabled_Baked.Normal.Draw(Renderer, control.RenderBounds, control.RenderColor);
            }
            else
            {
                mTextures.CheckBox.Active_Baked.Normal.Draw(Renderer, control.RenderBounds, control.RenderColor);
            }
        }
    }

    public override void DrawGroupBox(Control.Base control, int textStart, int textHeight, int textWidth)
    {
        var rect = control.RenderBounds;

        rect.Y += (int) (textHeight * 0.5f);
        rect.Height -= (int) (textHeight * 0.5f);

        var colDarker = Color.FromArgb(50, 0, 50, 60);
        var colLighter = Color.FromArgb(150, 255, 255, 255);

        Renderer.DrawColor = colLighter;

        Renderer.DrawFilledRect(new Rectangle(rect.X + 1, rect.Y + 1, textStart - 3, 1));
        Renderer.DrawFilledRect(
            new Rectangle(rect.X + 1 + textStart + textWidth, rect.Y + 1, rect.Width - textStart + textWidth - 2, 1)
        );

        Renderer.DrawFilledRect(new Rectangle(rect.X + 1, rect.Y + rect.Height - 1, rect.X + rect.Width - 2, 1));

        Renderer.DrawFilledRect(new Rectangle(rect.X + 1, rect.Y + 1, 1, rect.Height));
        Renderer.DrawFilledRect(new Rectangle(rect.X + rect.Width - 2, rect.Y + 1, 1, rect.Height - 1));

        Renderer.DrawColor = colDarker;

        Renderer.DrawFilledRect(new Rectangle(rect.X + 1, rect.Y, textStart - 3, 1));
        Renderer.DrawFilledRect(
            new Rectangle(rect.X + 1 + textStart + textWidth, rect.Y, rect.Width - textStart - textWidth - 2, 1)
        );

        Renderer.DrawFilledRect(new Rectangle(rect.X + 1, rect.Y + rect.Height - 1, rect.X + rect.Width - 2, 1));

        Renderer.DrawFilledRect(new Rectangle(rect.X, rect.Y + 1, 1, rect.Height - 1));
        Renderer.DrawFilledRect(new Rectangle(rect.X + rect.Width - 1, rect.Y + 1, 1, rect.Height - 1));
    }

    public override void DrawTextBox(Control.Base control)
    {
        if (control.IsDisabledByTree)
        {
            mTextures.TextBox.Disabled.Draw(Renderer, control.RenderBounds, control.RenderColor);

            return;
        }

        if (control.HasFocus)
        {
            mTextures.TextBox.Focus.Draw(Renderer, control.RenderBounds, control.RenderColor);
        }
        else
        {
            mTextures.TextBox.Normal.Draw(Renderer, control.RenderBounds, control.RenderColor);
        }
    }

    public override void DrawTabButton(Control.Base control, bool active, Pos dir)
    {
        if (active)
        {
            DrawActiveTabButton(control, dir);

            return;
        }

        if (dir == Pos.Top)
        {
            mTextures.Tab.Top.Inactive.Draw(Renderer, control.RenderBounds, control.RenderColor);

            return;
        }

        if (dir == Pos.Left)
        {
            mTextures.Tab.Left.Inactive.Draw(Renderer, control.RenderBounds, control.RenderColor);

            return;
        }

        if (dir == Pos.Bottom)
        {
            mTextures.Tab.Bottom.Inactive.Draw(Renderer, control.RenderBounds, control.RenderColor);

            return;
        }

        if (dir == Pos.Right)
        {
            mTextures.Tab.Right.Inactive.Draw(Renderer, control.RenderBounds, control.RenderColor);

            return;
        }
    }

    private void DrawActiveTabButton(Control.Base control, Pos dir)
    {
        switch (dir)
        {
            case Pos.Top:
                mTextures.Tab.Top.Active.Draw(
                    Renderer,
                    control.RenderBounds.Add(new Rectangle(0, 0, 0, 0)),
                    control.RenderColor
                );
                return;
            case Pos.Left:
                mTextures.Tab.Left.Active.Draw(
                    Renderer,
                    control.RenderBounds.Add(new Rectangle(0, 0, 0, 0)),
                    control.RenderColor
                );
                return;
            case Pos.Bottom:
                mTextures.Tab.Bottom.Active.Draw(
                    Renderer,
                    control.RenderBounds.Add(new Rectangle(0, 0, 0, 0)),
                    control.RenderColor
                );
                return;
            case Pos.Right:
                mTextures.Tab.Right.Active.Draw(
                    Renderer,
                    control.RenderBounds.Add(new Rectangle(0, 0, 0, 0)),
                    control.RenderColor
                );
                return;
        }
    }

    public override void DrawPanel(Panel panel)
    {
        mTextures.Panel.Control.Draw(Renderer, panel.RenderBounds, panel.RenderColor);
    }

    public override void DrawTabControl(Control.Base control)
    {
        mTextures.Tab.Control.Draw(Renderer, control.RenderBounds, control.RenderColor);
    }

    public override void DrawTabTitleBar(Control.Base control)
    {
        mTextures.Tab.HeaderBar.Draw(Renderer, control.RenderBounds, control.RenderColor);
    }

    public override void DrawWindow(Control.Base control, int topHeight, bool inFocus)
    {
        IGameTexture renderImg = null;
        if (((WindowControl) control).GetImage(Control.WindowControl.ControlState.Active) != null)
        {
            renderImg = ((WindowControl) control).GetImage(Control.WindowControl.ControlState.Active);
        }

        if (((WindowControl) control).GetImage(Control.WindowControl.ControlState.Inactive) != null)
        {
            renderImg = ((WindowControl) control).GetImage(Control.WindowControl.ControlState.Inactive);
        }

        if (renderImg != null)
        {
            Renderer.DrawColor = control.RenderColor;
            Renderer.DrawTexturedRect(renderImg, control.RenderBounds, control.RenderColor);

            return;
        }

        if (inFocus)
        {
            mTextures.Window.Normal.Draw(Renderer, control.RenderBounds, control.RenderColor);
        }
        else
        {
            mTextures.Window.Inactive.Draw(Renderer, control.RenderBounds, control.RenderColor);
        }
    }

    public override void DrawHighlight(Control.Base control)
    {
        var rect = control.RenderBounds;
        Renderer.DrawColor = Color.FromArgb(255, 255, 100, 255);
        Renderer.DrawFilledRect(rect);
    }

    public override void DrawScrollBar(Control.Base control, bool horizontal, bool depressed)
    {
        if (!control.ShouldDrawBackground)
        {
            return;
        }

        if (control is not ScrollBar scrollbar)
        {
            return;
        }

        if (scrollbar.GetTemplate() is not { } renderImg)
        {
            if (horizontal)
            {
                mTextures.Scroller.TrackH.Draw(Renderer, control.RenderBounds, control.RenderColor);
            }
            else
            {
                mTextures.Scroller.TrackV.Draw(Renderer, control.RenderBounds, control.RenderColor);
            }

            return;
        }

        Renderer.DrawColor = control.RenderColor;

        //Draw Top Left Corner
        Renderer.DrawTexturedRect(
            renderImg,
            new Rectangle(control.RenderBounds.X, control.RenderBounds.Y, 2, 2),
            control.RenderColor,
            0,
            0,
            2f / renderImg.Width,
            2f / renderImg.Height
        );

        //Draw Top
        Renderer.DrawTexturedRect(
            renderImg,
            new Rectangle(control.RenderBounds.X + 2, control.RenderBounds.Y, control.RenderBounds.Width - 4, 2),
            control.RenderColor,
            2f / renderImg.Width,
            0,
            (renderImg.Width - 2f) / renderImg.Width,
            2f / renderImg.Height
        );

        //Draw Top Right Corner
        Renderer.DrawTexturedRect(
            renderImg,
            new Rectangle(control.RenderBounds.Right - 2, control.RenderBounds.Y, 2, 2),
            control.RenderColor,
            (renderImg.Width - 2f) / renderImg.Width,
            0,
            1f,
            2f / renderImg.Height
        );

        //Draw Left
        Renderer.DrawTexturedRect(
            renderImg,
            new Rectangle(control.RenderBounds.X, control.RenderBounds.Y + 2, 2, control.RenderBounds.Height - 4),
            control.RenderColor,
            0,
            2f / renderImg.Height,
            2f / renderImg.Width,
            (renderImg.Height - 2f) / renderImg.Height
        );

        //Draw Middle
        Renderer.DrawTexturedRect(
            renderImg,
            new Rectangle(
                control.RenderBounds.X + 2,
                control.RenderBounds.Y + 2,
                control.RenderBounds.Width - 4,
                control.RenderBounds.Height - 4
            ),
            control.RenderColor,
            2f / renderImg.Width,
            2f / renderImg.Height,
            (renderImg.Width - 2f) / renderImg.Width,
            (renderImg.Height - 2f) / renderImg.Height
        );

        //Draw Right
        Renderer.DrawTexturedRect(
            renderImg,
            new Rectangle(
                control.RenderBounds.Width - 2,
                control.RenderBounds.Y + 2,
                2,
                control.RenderBounds.Height - 4
            ),
            control.RenderColor,
            (renderImg.Width - 2f) / renderImg.Width,
            2f / renderImg.Height,
            1,
            (renderImg.Height - 2f) / renderImg.Height
        );

        // Draw Bottom Left Corner
        Renderer.DrawTexturedRect(
            renderImg,
            new Rectangle(control.RenderBounds.X, control.RenderBounds.Bottom - 2, 2, 2),
            control.RenderColor,
            0,
            (renderImg.Height - 2f) / renderImg.Height,
            2f / renderImg.Width,
            1f
        );

        //Draw Bottom
        Renderer.DrawTexturedRect(
            renderImg,
            new Rectangle(
                control.RenderBounds.X + 2,
                control.RenderBounds.Bottom - 2,
                control.RenderBounds.Width - 4,
                2
            ),
            control.RenderColor,
            2f / renderImg.Width,
            (renderImg.Height - 2f) / renderImg.Height,
            (renderImg.Width - 2f) / renderImg.Width,
            1f
        );

        //Draw Bottom Right Corner
        Renderer.DrawTexturedRect(
            renderImg,
            new Rectangle(control.RenderBounds.Right - 2, control.RenderBounds.Bottom - 2, 2, 2),
            control.RenderColor,
            (renderImg.Width - 2f) / renderImg.Width,
            (renderImg.Height - 2f) / renderImg.Height,
            1f,
            1f
        );
    }

    public override void DrawScrollBarBar(ScrollBarBar scrollBarBar)
    {
        IGameTexture? renderImg = scrollBarBar.GetImage(ComponentState.Normal);
        if (scrollBarBar.IsDisabledByTree)
        {
            renderImg = scrollBarBar.GetImage(ComponentState.Disabled);
        }
        else if (scrollBarBar.IsActive)
        {
            renderImg = scrollBarBar.GetImage(ComponentState.Active);
        }
        else if (scrollBarBar.IsHovered)
        {
            renderImg = scrollBarBar.GetImage(ComponentState.Hovered);
        }

        if (renderImg == null)
        {
            var buttonSource = scrollBarBar.IsHorizontal ? mTextures.Scroller.BarH : mTextures.Scroller.BarV;
            var stateSource = buttonSource.Normal;
            if (scrollBarBar.IsDisabledByTree)
            {
                stateSource = buttonSource.Disabled;
            }
            else if (scrollBarBar.IsActive)
            {
                stateSource = buttonSource.Active;
            }
            else if (scrollBarBar.IsHovered)
            {
                stateSource = buttonSource.Hovered;
            }

            stateSource.Draw(Renderer, scrollBarBar.RenderBounds, scrollBarBar.RenderColor);
            return;
        }

        Renderer.DrawColor = scrollBarBar.RenderColor;



        if (scrollBarBar.IsVertical)
        {
            //Draw Top Left Corner
            Renderer.DrawTexturedRect(
                renderImg,
                new Rectangle(scrollBarBar.RenderBounds.X, scrollBarBar.RenderBounds.Y, 2, 2),
                scrollBarBar.RenderColor,
                0,
                0,
                2f / renderImg.Width,
                2f / renderImg.Height
            );

            //Draw Top
            Renderer.DrawTexturedRect(
                renderImg,
                new Rectangle(scrollBarBar.RenderBounds.X + 2, scrollBarBar.RenderBounds.Y, scrollBarBar.RenderBounds.Width - 4, 2),
                scrollBarBar.RenderColor,
                2f / renderImg.Width,
                0,
                (renderImg.Width - 2f) / renderImg.Width,
                2f / renderImg.Height
            );

            //Draw Top Right Corner
            Renderer.DrawTexturedRect(
                renderImg,
                new Rectangle(scrollBarBar.RenderBounds.Right - 2, scrollBarBar.RenderBounds.Y, 2, 2),
                scrollBarBar.RenderColor,
                (renderImg.Width - 2f) / renderImg.Width,
                0,
                1f,
                2f / renderImg.Height
            );

            //Draw Left
            Renderer.DrawTexturedRect(
                renderImg,
                new Rectangle(scrollBarBar.RenderBounds.X, scrollBarBar.RenderBounds.Y + 2, 2, scrollBarBar.RenderBounds.Height - 4),
                scrollBarBar.RenderColor,
                0,
                2f / renderImg.Height,
                2f / renderImg.Width,
                (renderImg.Height - 2f) / renderImg.Height
            );

            //Draw Middle
            Renderer.DrawTexturedRect(
                renderImg,
                new Rectangle(
                    scrollBarBar.RenderBounds.X + 2,
                    scrollBarBar.RenderBounds.Y + 2,
                    scrollBarBar.RenderBounds.Width - 4,
                    scrollBarBar.RenderBounds.Height - 4
                ),
                scrollBarBar.RenderColor,
                2f / renderImg.Width,
                2f / renderImg.Height,
                (renderImg.Width - 2f) / renderImg.Width,
                (renderImg.Height - 2f) / renderImg.Height
            );

            //Draw Right
            Renderer.DrawTexturedRect(
                renderImg,
                new Rectangle(
                    scrollBarBar.RenderBounds.Width - 2,
                    scrollBarBar.RenderBounds.Y + 2,
                    2,
                    scrollBarBar.RenderBounds.Height - 4
                ),
                scrollBarBar.RenderColor,
                (renderImg.Width - 2f) / renderImg.Width,
                2f / renderImg.Height,
                1,
                (renderImg.Height - 2f) / renderImg.Height
            );

            // Draw Bottom Left Corner
            Renderer.DrawTexturedRect(
                renderImg,
                new Rectangle(scrollBarBar.RenderBounds.X, scrollBarBar.RenderBounds.Bottom - 2, 2, 2),
                scrollBarBar.RenderColor,
                0,
                (renderImg.Height - 2f) / renderImg.Height,
                2f / renderImg.Width,
                1f
            );

            //Draw Bottom
            Renderer.DrawTexturedRect(
                renderImg,
                new Rectangle(
                    scrollBarBar.RenderBounds.X + 2,
                    scrollBarBar.RenderBounds.Bottom - 2,
                    scrollBarBar.RenderBounds.Width - 4,
                    2
                ),
                scrollBarBar.RenderColor,
                2f / renderImg.Width,
                (renderImg.Height - 2f) / renderImg.Height,
                (renderImg.Width - 2f) / renderImg.Width,
                1f
            );

            //Draw Bottom Right Corner
            Renderer.DrawTexturedRect(
                renderImg,
                new Rectangle(scrollBarBar.RenderBounds.Right - 2, scrollBarBar.RenderBounds.Bottom - 2, 2, 2),
                scrollBarBar.RenderColor,
                (renderImg.Width - 2f) / renderImg.Width,
                (renderImg.Height - 2f) / renderImg.Height,
                1f,
                1f
            );

            return;
        }

        //Draw Top Left Corner
        Renderer.DrawTexturedRect(
            renderImg,
            new Rectangle(scrollBarBar.RenderBounds.X, scrollBarBar.RenderBounds.Y, 2, 2),
            scrollBarBar.RenderColor,
            0,
            0,
            2f / renderImg.Width,
            2f / renderImg.Height
        );

        //Draw Top
        Renderer.DrawTexturedRect(
            renderImg,
            new Rectangle(scrollBarBar.RenderBounds.X + 2, scrollBarBar.RenderBounds.Y, scrollBarBar.RenderBounds.Width - 4, 2),
            scrollBarBar.RenderColor,
            2f / renderImg.Width,
            0,
            (renderImg.Width - 2f) / renderImg.Width,
            2f / renderImg.Height
        );

        //Draw Top Right Corner
        Renderer.DrawTexturedRect(
            renderImg,
            new Rectangle(scrollBarBar.RenderBounds.Right - 2, scrollBarBar.RenderBounds.Y, 2, 2),
            scrollBarBar.RenderColor,
            (renderImg.Width - 2f) / renderImg.Width,
            0,
            1f,
            2f / renderImg.Height
        );

        //Draw Left
        Renderer.DrawTexturedRect(
            renderImg,
            new Rectangle(scrollBarBar.RenderBounds.X, scrollBarBar.RenderBounds.Y + 2, 2, scrollBarBar.RenderBounds.Height - 4),
            scrollBarBar.RenderColor,
            0,
            2f / renderImg.Height,
            2f / renderImg.Width,
            (renderImg.Height - 2f) / renderImg.Height
        );

        //Draw Middle
        Renderer.DrawTexturedRect(
            renderImg,
            new Rectangle(
                scrollBarBar.RenderBounds.X + 2,
                scrollBarBar.RenderBounds.Y + 2,
                scrollBarBar.RenderBounds.Width - 4,
                scrollBarBar.RenderBounds.Height - 4
            ),
            scrollBarBar.RenderColor,
            2f / renderImg.Width,
            2f / renderImg.Height,
            (renderImg.Width - 2f) / renderImg.Width,
            (renderImg.Height - 2f) / renderImg.Height
        );

        //Draw Right
        Renderer.DrawTexturedRect(
            renderImg,
            new Rectangle(
                scrollBarBar.RenderBounds.Width - 2,
                scrollBarBar.RenderBounds.Y + 2,
                2,
                scrollBarBar.RenderBounds.Height - 4
            ),
            scrollBarBar.RenderColor,
            (renderImg.Width - 2f) / renderImg.Width,
            2f / renderImg.Height,
            1,
            (renderImg.Height - 2f) / renderImg.Height
        );

        // Draw Bottom Left Corner
        Renderer.DrawTexturedRect(
            renderImg,
            new Rectangle(scrollBarBar.RenderBounds.X, scrollBarBar.RenderBounds.Bottom - 2, 2, 2),
            scrollBarBar.RenderColor,
            0,
            (renderImg.Height - 2f) / renderImg.Height,
            2f / renderImg.Width,
            1f
        );

        //Draw Bottom
        Renderer.DrawTexturedRect(
            renderImg,
            new Rectangle(
                scrollBarBar.RenderBounds.X + 2,
                scrollBarBar.RenderBounds.Bottom - 2,
                scrollBarBar.RenderBounds.Width - 4,
                2
            ),
            scrollBarBar.RenderColor,
            2f / renderImg.Width,
            (renderImg.Height - 2f) / renderImg.Height,
            (renderImg.Width - 2f) / renderImg.Width,
            1f
        );

        //Draw Bottom Right Corner
        Renderer.DrawTexturedRect(
            renderImg,
            new Rectangle(scrollBarBar.RenderBounds.Right - 2, scrollBarBar.RenderBounds.Bottom - 2, 2, 2),
            scrollBarBar.RenderColor,
            (renderImg.Width - 2f) / renderImg.Width,
            (renderImg.Height - 2f) / renderImg.Height,
            1f,
            1f
        );
    }

    public override void DrawProgressBar(Control.Base control, bool horizontal, float progress)
    {
        var rect = control.RenderBounds;

        if (horizontal)
        {
            mTextures.ProgressBar.Back.Draw(Renderer, rect, control.RenderColor);
            rect.Width = (int) (rect.Width * progress);
            mTextures.ProgressBar.Front.Draw(Renderer, rect, control.RenderColor);
        }
        else
        {
            mTextures.ProgressBar.Back.Draw(Renderer, rect, control.RenderColor);
            rect.Y = (int) (rect.Y + rect.Height * (1 - progress));
            rect.Height = (int) (rect.Height * progress);
            mTextures.ProgressBar.Front.Draw(Renderer, rect, control.RenderColor);
        }
    }

    public override void DrawListBox(Control.Base control)
    {
        if (!control.ShouldDrawBackground)
        {
            return;
        }

        mTextures.Input.ListBox.Background.Draw(Renderer, control.RenderBounds, control.RenderColor);
    }

    public override void DrawListBoxLine(Control.Base control, bool selected, bool even)
    {
        if (!control.ShouldDrawBackground)
        {
            return;
        }

        if (control.IsHovered)
        {
            mTextures.Input.ListBox.Hovered.Draw(Renderer, control.RenderBounds, control.RenderColor);
            return;
        }

        if (selected)
        {
            if (even)
            {
                mTextures.Input.ListBox.EvenLineSelected.Draw(Renderer, control.RenderBounds, control.RenderColor);
                return;
            }

            mTextures.Input.ListBox.OddLineSelected.Draw(Renderer, control.RenderBounds, control.RenderColor);
            return;
        }

        if (even)
        {
            mTextures.Input.ListBox.EvenLine.Draw(Renderer, control.RenderBounds, control.RenderColor);
            return;
        }

        mTextures.Input.ListBox.OddLine.Draw(Renderer, control.RenderBounds, control.RenderColor);
    }

    public void DrawSliderNotches(
        Rectangle rect,
        double[]? notches,
        int numNotches,
        float thickness,
        float notchLength,
        Orientation orientation
    )
    {
        if (numNotches == 0 && notches is not { Length: > 0 })
        {
            return;
        }

        var positionLimit = orientation switch
        {
            Orientation.LeftToRight => rect.Width,
            Orientation.RightToLeft => rect.Width,
            Orientation.TopToBottom => rect.Height,
            Orientation.BottomToTop => rect.Height,
            _ => throw Exceptions.UnreachableInvalidEnum(orientation),
        };

        Rectangle notchBaseRect = default;
        notchBaseRect.X = rect.X;
        notchBaseRect.Y = rect.Y;
        switch (orientation)
        {
            case Orientation.LeftToRight:
            case Orientation.RightToLeft:
                notchBaseRect.Width = (int)thickness;
                notchBaseRect.Height = (int)(notchLength + thickness);
                break;
            case Orientation.TopToBottom:
            case Orientation.BottomToTop:
                notchBaseRect.Width = (int)(notchLength + thickness);
                notchBaseRect.Height = (int)thickness;
                break;
            default:
                throw Exceptions.UnreachableInvalidEnum(orientation);
        }

        if (notches == null)
        {
            var spacing = positionLimit / (float) numNotches;

            for (var notchIndex = 0; notchIndex <= numNotches; ++notchIndex)
            {
                Renderer.DrawFilledRect(
                    orientation is Orientation.LeftToRight or Orientation.RightToLeft
                        ? notchBaseRect with
                        {
                            X = (int)(notchBaseRect.X + spacing * notchIndex),
                        }
                        : notchBaseRect with
                        {
                            Y = (int)(notchBaseRect.Y + spacing * notchIndex),
                        }
                );
            }

            return;
        }

        var notchMin = notches.Min();
        var notchMax = notches.Max();
        var notchRange = notchMax - notchMin;
        if (notchRange == 0)
        {
            notchRange = 1;
        }

        var notchPositions = notches.Select(notch => positionLimit * (notch - notchMin) / notchRange).ToArray();
        foreach (var notchPosition in notchPositions)
        {
            Rectangle notchRect = orientation switch
            {
                Orientation.LeftToRight => notchBaseRect with { X = (int)(notchBaseRect.X + notchPosition) },
                Orientation.RightToLeft => notchBaseRect with { X = (int)(notchBaseRect.X + positionLimit - notchPosition) },
                Orientation.TopToBottom => notchBaseRect with { Y = (int)(notchBaseRect.Y + notchPosition) },
                Orientation.BottomToTop => notchBaseRect with { Y = (int)(notchBaseRect.Y + positionLimit - notchPosition) },
                _ => throw Exceptions.UnreachableInvalidEnum(orientation),
            };
            Renderer.DrawFilledRect(notchRect);
        }
    }

    public void DrawSliderNotchesH(Rectangle rect, double[] notches, int numNotches, float thickness, float notchLength)
    {
        if (numNotches == 0)
        {
            return;
        }

        var notchMin = notches?.Min();
        var notchMax = notches?.Max();
        var notchRange = notchMax - notchMin;
        if (notchRange == 0)
        {
            notchRange = 1;
        }
        var notchPositions = notches?.Select(notch => (rect.Width * (notch - notchMin) / notchRange).Value).ToArray();

        if (notchPositions != null)
        {
            foreach (var notchPosition in notchPositions)
            {
                Renderer.DrawFilledRect(Util.FloatRect(rect.X + (float)notchPosition, rect.Y, thickness, notchLength + thickness));
            }
        }
        else
        {
            var iSpacing = rect.Width / (float) numNotches;
            for (var i = 0; i < numNotches + 1; i++)
            {
                Renderer.DrawFilledRect(Util.FloatRect(rect.X + iSpacing * i, rect.Y, thickness, notchLength + thickness));
            }
        }
    }

    public void DrawSliderNotchesV(Rectangle rect, int numNotches, float dist)
    {
        if (numNotches == 0)
        {
            return;
        }

        var iSpacing = rect.Height / (float) numNotches;
        for (var i = 0; i < numNotches + 1; i++)
        {
            Renderer.DrawFilledRect(Util.FloatRect(rect.X + dist - 2, rect.Y + iSpacing * i, 5, 1));
        }
    }

    public override void DrawSlider(Control.Base control, bool horizontal, double[]? notches, int numNotches, int barSize)
    {
        if (control is not Slider slider)
        {
            return;
        }

        var rect = control.RenderBounds;

        if (slider.BackgroundImage is {} backgroundTexture)
        {
            if (horizontal)
            {
                //rect.X += (int) (barSize * 0.5);
                //rect.Width -= barSize;
                //rect.Y += (int) (rect.Height * 0.5 - 1);
                //rect.Height = 1;
                //DrawSliderNotchesH(rect, numNotches, barSize * 0.5f);
                //Renderer.DrawFilledRect(rect);
            }
            else
            {
                //rect.Y += (int) (barSize * 0.5);
                //rect.Height -= barSize;
                //rect.X += (int) (rect.Width * 0.5 - 1);
                //rect.Width = 1;
                //DrawSliderNotchesV(rect, numNotches, barSize * 0.4f);
                //Renderer.DrawFilledRect(rect);
            }

            Renderer.DrawColor = control.RenderColor;
            Renderer.DrawTexturedRect(backgroundTexture, rect, control.RenderColor);

            if (control.DrawDebugOutlines)
            {
                DrawRectStroke(rect, Color.Yellow);
            }
        }
        else
        {
            Renderer.DrawColor = control.IsDisabled ? Colors.Button.Disabled : control.RenderColor;

            if (horizontal)
            {
                rect.X += (int) (barSize * 0.5);
                rect.Width -= barSize;
                rect.Y += (int) (rect.Height * 0.5 - 1);
                rect.Height = 1;
                DrawSliderNotchesH(rect, notches, numNotches, rect.Height, barSize * 0.5f);
            }
            else
            {
                rect.Y += (int) (barSize * 0.5);
                rect.Height -= barSize;
                rect.X += (int) (rect.Width * 0.5 - 1);
                rect.Width = 1;
                DrawSliderNotchesV(rect, numNotches, barSize * 0.4f);
            }

            Renderer.DrawFilledRect(rect);
        }
    }

    public override void DrawSlider(Slider slider, Orientation orientation, double[]? notches, int numNotches, int barSize)
    {
        var bounds = slider.RenderBounds;
        var renderColor = slider.RenderColor;

        if (slider.BackgroundImage is {} backgroundTexture)
        {
            Renderer.DrawColor = renderColor;

            // TODO: Orientation

            Renderer.DrawTexturedRect(backgroundTexture, bounds, renderColor);

            if (slider.DrawDebugOutlines)
            {
                DrawRectStroke(bounds, Color.Yellow);
            }
        }
        else
        {
            if (slider.IsDisabledByTree)
            {
                renderColor = Colors.Button.Disabled;
            }

            Renderer.DrawColor = renderColor;

            float thickness;
            switch (orientation)
            {
                case Orientation.LeftToRight:
                case Orientation.RightToLeft:
                    bounds.X += (int) (barSize * 0.5);
                    bounds.Width -= barSize;
                    bounds.Y += (int) (bounds.Height * 0.5 - 1);
                    bounds.Height = 1;
                    thickness = bounds.Height;
                    break;
                case Orientation.TopToBottom:
                case Orientation.BottomToTop:
                    bounds.Y += (int) (barSize * 0.5);
                    bounds.Height -= barSize;
                    bounds.X += (int) (bounds.Width * 0.5 - 1);
                    bounds.Width = 1;
                    thickness = bounds.Width;
                    break;
                default:
                    throw Exceptions.UnreachableInvalidEnum(orientation);
            }

            DrawSliderNotches(bounds, notches, numNotches, thickness, barSize * 0.5f, orientation);

            Renderer.DrawFilledRect(bounds);
        }
    }

    public override void DrawComboBox(Control.Base control, bool down, bool open)
    {
        if (!control.ShouldDrawBackground)
        {
            return;
        }

        if (control.IsDisabledByTree)
        {
            mTextures.Input.ComboBox.Disabled.Draw(Renderer, control.RenderBounds, control.RenderColor);

            return;
        }

        if (down || open)
        {
            mTextures.Input.ComboBox.Down.Draw(Renderer, control.RenderBounds, control.RenderColor);

            return;
        }

        if (control.IsHovered)
        {
            mTextures.Input.ComboBox.Down.Draw(Renderer, control.RenderBounds, control.RenderColor);

            return;
        }

        mTextures.Input.ComboBox.Normal.Draw(Renderer, control.RenderBounds, control.RenderColor);
    }

    public override void DrawKeyboardHighlight(Control.Base control, Rectangle r, int offset)
    {
        var rect = r;

        rect.X += offset;
        rect.Y += offset;
        rect.Width -= offset * 2;
        rect.Height -= offset * 2;

        //draw the top and bottom
        var skip = true;
        for (var i = 0; i < rect.Width * 0.5; i++)
        {
            mRenderer.DrawColor = Color.Black;
            if (!skip)
            {
                Renderer.DrawPixel(rect.X + i * 2, rect.Y);
                Renderer.DrawPixel(rect.X + i * 2, rect.Y + rect.Height - 1);
            }
            else
            {
                skip = false;
            }
        }

        for (var i = 0; i < rect.Height * 0.5; i++)
        {
            Renderer.DrawColor = Color.Black;
            Renderer.DrawPixel(rect.X, rect.Y + i * 2);
            Renderer.DrawPixel(rect.X + rect.Width - 1, rect.Y + i * 2);
        }
    }

    public override void DrawToolTip(Control.Base control)
    {
        if (control is Label tooltip)
        {
            if (tooltip.ToolTipBackground != null)
            {
                var renderImg = tooltip.ToolTipBackground;

                //Draw from custom bg
                //Draw Top Left Corner
                Renderer.DrawTexturedRect(
                    renderImg, new Rectangle(control.RenderBounds.X, control.RenderBounds.Y, 8, 8),
                    control.RenderColor, 0, 0, 8f / renderImg.Width, 8f / renderImg.Height
                );

                //Draw Top
                Renderer.DrawTexturedRect(
                    renderImg,
                    new Rectangle(
                        control.RenderBounds.X + 8, control.RenderBounds.Y, control.RenderBounds.Width - 16, 8
                    ), control.RenderColor, 8f / renderImg.Width, 0,
                    (renderImg.Width - 8f) / renderImg.Width, 8f / renderImg.Height
                );

                //Draw Top Right Corner
                Renderer.DrawTexturedRect(
                    renderImg, new Rectangle(control.RenderBounds.Right - 8, control.RenderBounds.Y, 8, 8),
                    control.RenderColor, (renderImg.Width - 8f) / renderImg.Width, 0, 1f,
                    8f / renderImg.Height
                );

                //Draw Left
                Renderer.DrawTexturedRect(
                    renderImg,
                    new Rectangle(
                        control.RenderBounds.X, control.RenderBounds.Y + 8, 8, control.RenderBounds.Height - 16
                    ), control.RenderColor, 0, 8f / renderImg.Height, 8f / renderImg.Width,
                    (renderImg.Height - 8f) / renderImg.Height
                );

                //Draw Middle
                Renderer.DrawTexturedRect(
                    renderImg,
                    new Rectangle(
                        control.RenderBounds.X + 8, control.RenderBounds.Y + 8, control.RenderBounds.Width - 16,
                        control.RenderBounds.Height - 16
                    ), control.RenderColor, 8f / renderImg.Width, 8f / renderImg.Height,
                    (renderImg.Width - 8f) / renderImg.Width,
                    (renderImg.Height - 8f) / renderImg.Height
                );

                //Draw Right
                Renderer.DrawTexturedRect(
                    renderImg,
                    new Rectangle(
                        control.RenderBounds.Width - 8, control.RenderBounds.Y + 8, 8,
                        control.RenderBounds.Height - 16
                    ), control.RenderColor, (renderImg.Width - 8f) / renderImg.Width,
                    8f / renderImg.Height, 1, (renderImg.Height - 8f) / renderImg.Height
                );

                // Draw Bottom Left Corner
                Renderer.DrawTexturedRect(
                    renderImg, new Rectangle(control.RenderBounds.X, control.RenderBounds.Bottom - 8, 8, 8),
                    control.RenderColor, 0, (renderImg.Height - 8f) / renderImg.Height,
                    8f / renderImg.Width, 1f
                );

                //Draw Bottom
                Renderer.DrawTexturedRect(
                    renderImg,
                    new Rectangle(
                        control.RenderBounds.X + 8, control.RenderBounds.Bottom - 8,
                        control.RenderBounds.Width - 16, 8
                    ), control.RenderColor, 8f / renderImg.Width,
                    (renderImg.Height - 8f) / renderImg.Height,
                    (renderImg.Width - 8f) / renderImg.Width, 1f
                );

                //Draw Bottom Right Corner
                Renderer.DrawTexturedRect(
                    renderImg, new Rectangle(control.RenderBounds.Right - 8, control.RenderBounds.Bottom - 8, 8, 8),
                    control.RenderColor, (renderImg.Width - 8f) / renderImg.Width,
                    (renderImg.Height - 8f) / renderImg.Height, 1f, 1f
                );

                return;
            }
        }

        mTextures.Tooltip.Draw(Renderer, control.RenderBounds, control.RenderColor);
    }

    public override void DrawScrollButton(
        Control.Base control,
        Pos direction,
        bool depressed,
        bool hovered,
        bool disabled
    )
    {
        if (!(control is Button button))
        {
            return;
        }

        var i = 0;
        if (direction == Pos.Top)
        {
            i = 1;
        }

        if (direction == Pos.Right)
        {
            i = 2;
        }

        if (direction == Pos.Bottom)
        {
            i = 3;
        }

        IGameTexture renderImg = null;

        if (disabled && button.GetStateTexture(ComponentState.Disabled) != null)
        {
            renderImg = button.GetStateTexture(ComponentState.Disabled);
        }
        else if (depressed && button.GetStateTexture(ComponentState.Active) != null)
        {
            renderImg = button.GetStateTexture(ComponentState.Active);
        }
        else if (hovered && button.GetStateTexture(ComponentState.Hovered) != null)
        {
            renderImg = button.GetStateTexture(ComponentState.Hovered);
        }
        else if (button.GetStateTexture(ComponentState.Normal) != null)
        {
            renderImg = button.GetStateTexture(ComponentState.Normal);
        }

        if (renderImg != null)
        {
            Renderer.DrawColor = button.RenderColor;
            Renderer.DrawTexturedRect(renderImg, button.RenderBounds, button.RenderColor);

            return;
        }

        if (disabled)
        {
            mTextures.Scroller.Button.Disabled[i].Draw(Renderer, button.RenderBounds, button.RenderColor);

            return;
        }

        if (depressed)
        {
            mTextures.Scroller.Button.Active[i].Draw(Renderer, button.RenderBounds, button.RenderColor);

            return;
        }

        if (hovered)
        {
            mTextures.Scroller.Button.Hovered[i].Draw(Renderer, button.RenderBounds, button.RenderColor);

            return;
        }

        mTextures.Scroller.Button.Normal[i].Draw(Renderer, button.RenderBounds, button.RenderColor);
    }

    public override void DrawComboBoxArrow(Control.Base control, bool hovered, bool down, bool open, bool disabled)
    {
        if (disabled)
        {
            mTextures.Input.ComboBox.Button.Disabled.Draw(Renderer, control.RenderBounds, control.RenderColor);

            return;
        }

        if (down || open)
        {
            mTextures.Input.ComboBox.Button.Down.Draw(Renderer, control.RenderBounds, control.RenderColor);

            return;
        }

        if (hovered)
        {
            mTextures.Input.ComboBox.Button.Hover.Draw(Renderer, control.RenderBounds, control.RenderColor);

            return;
        }

        mTextures.Input.ComboBox.Button.Normal.Draw(Renderer, control.RenderBounds);
    }

    public override void DrawNumericUpDownButton(Control.Base control, bool depressed, bool up)
    {
        if (up)
        {
            if (control.IsDisabledByTree)
            {
                mTextures.Input.UpDown.Up.Disabled.DrawCenter(Renderer, control.RenderBounds, control.RenderColor);

                return;
            }

            if (depressed)
            {
                mTextures.Input.UpDown.Up.Down.DrawCenter(Renderer, control.RenderBounds, control.RenderColor);

                return;
            }

            if (control.IsHovered)
            {
                mTextures.Input.UpDown.Up.Hover.DrawCenter(Renderer, control.RenderBounds, control.RenderColor);

                return;
            }

            mTextures.Input.UpDown.Up.Normal.DrawCenter(Renderer, control.RenderBounds, control.RenderColor);

            return;
        }

        if (control.IsDisabledByTree)
        {
            mTextures.Input.UpDown.Down.Disabled.DrawCenter(Renderer, control.RenderBounds, control.RenderColor);

            return;
        }

        if (depressed)
        {
            mTextures.Input.UpDown.Down.Down.DrawCenter(Renderer, control.RenderBounds, control.RenderColor);

            return;
        }

        if (control.IsHovered)
        {
            mTextures.Input.UpDown.Down.Hover.DrawCenter(Renderer, control.RenderBounds, control.RenderColor);

            return;
        }

        mTextures.Input.UpDown.Down.Normal.DrawCenter(Renderer, control.RenderBounds, control.RenderColor);
    }

    public override void DrawStatusBar(Control.Base control)
    {
        mTextures.StatusBar.Draw(Renderer, control.RenderBounds, control.RenderColor);
    }

    public override void DrawTreeButton(Control.Base control, bool open)
    {
        var rect = control.RenderBounds;

        if (open)
        {
            mTextures.Tree.Minus.Draw(Renderer, rect);
        }
        else
        {
            mTextures.Tree.Plus.Draw(Renderer, rect);
        }
    }

    public override void DrawTreeControl(Control.Base control)
    {
        mTextures.Tree.Background.Draw(Renderer, control.RenderBounds, control.RenderColor);
    }

    public override void DrawTreeNode(
        Control.Base ctrl,
        bool open,
        bool selected,
        int treeNodeHeight,
        int labelHeight,
        int labelWidth,
        int halfWay,
        int lastBranch,
        bool isRoot
    )
    {
        if (selected)
        {
            mTextures.Selection.Draw(Renderer, new Rectangle(16, 0, labelWidth + 4, labelHeight), ctrl.RenderColor);
        }

        base.DrawTreeNode(ctrl, open, selected, treeNodeHeight, labelHeight, labelWidth, halfWay, lastBranch, isRoot);
    }

    public override void DrawColorDisplay(Control.Base control, Color color)
    {
        var rect = control.RenderBounds;

        if (color.A != 255)
        {
            Renderer.DrawColor = Color.FromArgb(255, 255, 255, 255);
            Renderer.DrawFilledRect(rect);

            Renderer.DrawColor = Color.FromArgb(128, 128, 128, 128);

            Renderer.DrawFilledRect(Util.FloatRect(0, 0, rect.Width * 0.5f, rect.Height * 0.5f));
            Renderer.DrawFilledRect(
                Util.FloatRect(rect.Width * 0.5f, rect.Height * 0.5f, rect.Width * 0.5f, rect.Height * 0.5f)
            );
        }

        Renderer.DrawColor = color;
        Renderer.DrawFilledRect(rect);

        Renderer.DrawColor = Color.Black;
        Renderer.DrawLinedRect(rect);
    }

    public override void DrawModalControl(Control.Base control)
    {
        if (!control.ShouldDrawBackground)
        {
            return;
        }

        var rect = control.RenderBounds;
        Renderer.DrawColor = new Color(200, 20, 20, 20);
        Renderer.DrawFilledRect(rect);
    }

    public override void DrawMenuDivider(Control.Base control)
    {
        var rect = control.RenderBounds;
        Renderer.DrawColor = Color.FromArgb(100, 0, 0, 0);
        Renderer.DrawFilledRect(rect);
    }

    public override void DrawSliderButton(SliderBar sliderBar)
    {
        var overrideTexture = sliderBar.GetImage(ComponentState.Normal);
        if (sliderBar.IsDisabledByTree)
        {
            overrideTexture = sliderBar.GetImage(ComponentState.Disabled);
        }
        else if (sliderBar.IsActive)
        {
            overrideTexture = sliderBar.GetImage(ComponentState.Active);
        }
        else if (sliderBar.IsHovered)
        {
            overrideTexture = sliderBar.GetImage(ComponentState.Hovered);
        }

        if (overrideTexture != null)
        {
            Renderer.DrawColor = sliderBar.RenderColor;
            Renderer.DrawTexturedRect(overrideTexture, sliderBar.RenderBounds, sliderBar.RenderColor);
            return;
        }

        var textureSource = sliderBar.Orientation is Orientation.LeftToRight or Orientation.RightToLeft
            ? mTextures.Input.Slider.H
            : mTextures.Input.Slider.V;

        var stateSource = textureSource.Normal;
        if (sliderBar.IsDisabledByTree)
        {
            stateSource = textureSource.Disabled;
        }
        else if (sliderBar.IsActive)
        {
            stateSource = textureSource.Active;
        }
        else if (sliderBar.IsHovered)
        {
            stateSource = textureSource.Hover;
        }

        stateSource.Draw(Renderer, sliderBar.RenderBounds, sliderBar.RenderColor);
    }

    public override void DrawCategoryHolder(Control.Base control)
    {
        mTextures.CategoryList.Outer.Draw(Renderer, control.RenderBounds, control.RenderColor);
    }

    public override void DrawCategoryInner(Control.Base control, bool collapsed)
    {
        if (collapsed)
        {
            mTextures.CategoryList.Header.Draw(Renderer, control.RenderBounds, control.RenderColor);
        }
        else
        {
            mTextures.CategoryList.Inner.Draw(Renderer, control.RenderBounds, control.RenderColor);
        }
    }

    public override void DrawLabel(Control.Base control)
    {
        if (control is not Label label)
        {
            return;
        }

        if (label.GetTemplate() is not { } templateTexture)
        {
            return;
        }

        Renderer.DrawColor = control.RenderColor;

        //Draw Top Left Corner
        Renderer.DrawTexturedRect(
            templateTexture,
            new Rectangle(control.RenderBounds.X, control.RenderBounds.Y, 2, 2),
            control.RenderColor,
            0,
            0,
            2f / templateTexture.Width,
            2f / templateTexture.Height
        );

        //Draw Top
        Renderer.DrawTexturedRect(
            templateTexture,
            new Rectangle(control.RenderBounds.X + 2, control.RenderBounds.Y, control.RenderBounds.Width - 4, 2),
            control.RenderColor,
            2f / templateTexture.Width,
            0,
            (templateTexture.Width - 2f) / templateTexture.Width,
            2f / templateTexture.Height
        );

        //Draw Top Right Corner
        Renderer.DrawTexturedRect(
            templateTexture,
            new Rectangle(control.RenderBounds.Right - 2, control.RenderBounds.Y, 2, 2),
            control.RenderColor,
            (templateTexture.Width - 2f) / templateTexture.Width,
            0,
            1f,
            2f / templateTexture.Height
        );

        //Draw Left
        Renderer.DrawTexturedRect(
            templateTexture,
            new Rectangle(control.RenderBounds.X, control.RenderBounds.Y + 2, 2, control.RenderBounds.Height - 4),
            control.RenderColor,
            0,
            2f / templateTexture.Height,
            2f / templateTexture.Width,
            (templateTexture.Height - 2f) / templateTexture.Height
        );

        //Draw Middle
        Renderer.DrawTexturedRect(
            templateTexture,
            new Rectangle(
                control.RenderBounds.X + 2,
                control.RenderBounds.Y + 2,
                control.RenderBounds.Width - 4,
                control.RenderBounds.Height - 4
            ),
            control.RenderColor,
            2f / templateTexture.Width,
            2f / templateTexture.Height,
            (templateTexture.Width - 2f) / templateTexture.Width,
            (templateTexture.Height - 2f) / templateTexture.Height
        );

        //Draw Right
        Renderer.DrawTexturedRect(
            templateTexture,
            new Rectangle(
                control.RenderBounds.Width - 2,
                control.RenderBounds.Y + 2,
                2,
                control.RenderBounds.Height - 4
            ),
            control.RenderColor,
            (templateTexture.Width - 2f) / templateTexture.Width,
            2f / templateTexture.Height,
            1,
            (templateTexture.Height - 2f) / templateTexture.Height
        );

        // Draw Bottom Left Corner
        Renderer.DrawTexturedRect(
            templateTexture,
            new Rectangle(control.RenderBounds.X, control.RenderBounds.Bottom - 2, 2, 2),
            control.RenderColor,
            0,
            (templateTexture.Height - 2f) / templateTexture.Height,
            2f / templateTexture.Width,
            1f
        );

        //Draw Bottom
        Renderer.DrawTexturedRect(
            templateTexture,
            new Rectangle(
                control.RenderBounds.X + 2,
                control.RenderBounds.Bottom - 2,
                control.RenderBounds.Width - 4,
                2
            ),
            control.RenderColor,
            2f / templateTexture.Width,
            (templateTexture.Height - 2f) / templateTexture.Height,
            (templateTexture.Width - 2f) / templateTexture.Width,
            1f
        );

        //Draw Bottom Right Corner
        Renderer.DrawTexturedRect(
            templateTexture,
            new Rectangle(control.RenderBounds.Right - 2, control.RenderBounds.Bottom - 2, 2, 2),
            control.RenderColor,
            (templateTexture.Width - 2f) / templateTexture.Width,
            (templateTexture.Height - 2f) / templateTexture.Height,
            1f,
            1f
        );
    }

    #endregion

}
