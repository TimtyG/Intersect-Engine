﻿using Intersect.Client.Framework.File_Management;
using Intersect.Client.Framework.GenericClasses;
using Intersect.Client.Framework.Graphics;
using Intersect.Client.Framework.Gwen.ControlInternal;
using Intersect.Core;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Intersect.Client.Framework.Gwen.Control;


/// <summary>
///     Base class for scrollbars.
/// </summary>
public partial class ScrollBar : Base
{

    protected readonly ScrollBarBar mBar;

    protected readonly ScrollBarButton[] mScrollButton;

    private string mBackgroundTemplateFilename;

    private IGameTexture mBackgroundTemplateTex;

    protected float mContentSize;

    protected bool mDepressed;

    protected float _nudgeAmount;

    protected float _scrollAmount;

    protected float mViewableContentSize;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ScrollBar" /> class.
    /// </summary>
    /// <param name="parent">Parent control.</param>
    protected ScrollBar(Base parent) : base(parent)
    {
        mScrollButton = new ScrollBarButton[2];
        mScrollButton[0] = new ScrollBarButton(this);
        mScrollButton[1] = new ScrollBarButton(this);

        mBar = new ScrollBarBar(this);

        MinimumSize = new Point(15, 15);
        Size = new Point(15, 15);

        mDepressed = false;

        _scrollAmount = 0;
        mContentSize = 0;
        mViewableContentSize = 0;

        NudgeAmount = 20;
    }

    /// <summary>
    ///     Bar size (in pixels).
    /// </summary>
    public virtual int BarSize { get; set; }

    /// <summary>
    ///     Bar position (in pixels).
    /// </summary>
    public virtual int BarPos => 0;

    /// <summary>
    ///     Button size (in pixels).
    /// </summary>
    public virtual int ButtonSize => 0;

    public float BaseNudgeAmount
    {
        get => _nudgeAmount;
        set => _nudgeAmount = value;
    }

    public virtual float NudgeAmount
    {
        get => _nudgeAmount / mContentSize;
        set => _nudgeAmount = value;
    }

    public float ScrollAmount
    {
        get => _scrollAmount;
        set => SetScrollAmount(value);
    }

    public float ContentSize
    {
        get => mContentSize;
        set
        {
            if (mContentSize != value)
            {
                Invalidate();
            }

            mContentSize = value;
        }
    }

    public float ViewableContentSize
    {
        get => mViewableContentSize;
        set
        {
            if (mViewableContentSize != value)
            {
                Invalidate();
            }

            mViewableContentSize = value;
        }
    }

    /// <summary>
    ///     Indicates whether the bar is horizontal.
    /// </summary>
    public virtual bool IsHorizontal => false;

    /// <summary>
    ///     Invoked when the bar is moved.
    /// </summary>
    public event GwenEventHandler<EventArgs> BarMoved;

    public override JObject? GetJson(bool isRoot = false, bool onlySerializeIfNotEmpty = false)
    {
        var serializedProperties = base.GetJson(isRoot, onlySerializeIfNotEmpty);
        if (serializedProperties is null)
        {
            return null;
        }

        serializedProperties.Add("BackgroundTemplate", mBackgroundTemplateFilename);
        serializedProperties.Add("UpOrLeftButton", mScrollButton[0].GetJson());
        serializedProperties.Add("Bar", mBar.GetJson());
        serializedProperties.Add("DownOrRightButton", mScrollButton[1].GetJson());

        return base.FixJson(serializedProperties);
    }

    public override void LoadJson(JToken obj, bool isRoot = default)
    {
        base.LoadJson(obj);
        if (obj["BackgroundTemplate"] != null)
        {
            SetBackgroundTemplate(
                GameContentManager.Current.GetTexture(
                    Content.TextureType.Gui, (string)obj["BackgroundTemplate"]
                ), (string)obj["BackgroundTemplate"]
            );
        }

        if (obj["UpOrLeftButton"] != null)
        {
            mScrollButton[0].LoadJson(obj["UpOrLeftButton"]);
        }

        if (obj["Bar"] != null)
        {
            mBar.LoadJson(obj["Bar"]);
        }

        if (obj["DownOrRightButton"] != null)
        {
            mScrollButton[1].LoadJson(obj["DownOrRightButton"]);
        }
    }

    public IGameTexture GetTemplate()
    {
        return mBackgroundTemplateTex;
    }

    public void SetBackgroundTemplate(IGameTexture texture, string fileName)
    {
        if (texture == null && !string.IsNullOrWhiteSpace(fileName))
        {
            texture = GameContentManager.Current?.GetTexture(Content.TextureType.Gui, fileName);
        }

        mBackgroundTemplateFilename = fileName;
        mBackgroundTemplateTex = texture;
    }

    /// <summary>
    ///     Sets the scroll amount (0-1).
    /// </summary>
    /// <param name="value">Scroll amount.</param>
    /// <param name="forceUpdate">Determines whether the control should be updated.</param>
    /// <returns>True if control state changed.</returns>
    public virtual bool SetScrollAmount(float value, bool forceUpdate = false)
    {
        if (_scrollAmount == value && !forceUpdate)
        {
            return false;
        }

        var oldValue = _scrollAmount;
        _scrollAmount = value;
        Invalidate();
        OnBarMoved(this, EventArgs.Empty);

        return true;
    }

    /// <summary>
    ///     Renders the control using specified skin.
    /// </summary>
    /// <param name="skin">Skin to use.</param>
    protected override void Render(Skin.Base skin)
    {
        skin.DrawScrollBar(this, IsHorizontal, mDepressed);
    }

    /// <summary>
    ///     Handler for the BarMoved event.
    /// </summary>
    /// <param name="control">The control.</param>
    protected virtual void OnBarMoved(Base control, EventArgs args)
    {
        BarMoved?.Invoke(this, args);
    }

    protected override void Layout(Skin.Base skin)
    {
        base.Layout(skin);

        var displayedScrollAmount = CalculateScrolledAmount();
        var scrollAmount = _scrollAmount;
        // ApplicationContext.CurrentContext.Logger.LogTrace(
        //     "Scrollbar '{ScrollbarName}' at {DisplayedScrollAmount} but should be at {ActualScrollAmount} Size={Size}",
        //     CanonicalName,
        //     displayedScrollAmount,
        //     scrollAmount,
        //     mBar.Size
        // );

        if (!scrollAmount.Equals(displayedScrollAmount))
        {
            SetScrollAmount(scrollAmount, forceUpdate: true);
        }
    }

    protected virtual float CalculateScrolledAmount()
    {
        return 0;
    }

    protected virtual int CalculateBarSize()
    {
        return 0;
    }

    public virtual void ScrollToLeft()
    {
    }

    public virtual void ScrollToRight()
    {
    }

    public virtual void ScrollToTop()
    {
    }

    public virtual void ScrollToBottom()
    {
    }

    public ScrollBarButton GetScrollBarButton(Pos direction)
    {
        for (var i = 0; i < mScrollButton.Length; i++)
        {
            if (mScrollButton[i].Direction == direction)
            {
                return mScrollButton[i];
            }
        }

        return null;
    }

    public void SetScrollBarImage(IGameTexture texture, string fileName, ComponentState state)
    {
        mBar.SetImage(texture, fileName, state);
    }

    public IGameTexture GetScrollBarImage(ComponentState state)
    {
        return mBar.GetImage(state);
    }

}
