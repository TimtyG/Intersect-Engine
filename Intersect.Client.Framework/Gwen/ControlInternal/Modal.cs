﻿using Intersect.Client.Framework.Gwen.Control;
using Intersect.Client.Framework.Gwen.Input;

namespace Intersect.Client.Framework.Gwen.ControlInternal;


/// <summary>
///     Modal control for windows.
/// </summary>
public partial class Modal : Base
{

    /// <summary>
    ///     Initializes a new instance of the <see cref="Modal" /> class.
    /// </summary>
    /// <param name="parent">Parent control.</param>
    public Modal(Base parent) : base(parent)
    {
        KeyboardInputEnabled = true;
        MouseInputEnabled = true;
        ShouldDrawBackground = true;
        if (Canvas is { } canvas)
        {
            Size = canvas.Size;
        }
    }

    /// <summary>
    ///     Lays out the control's interior according to alignment, padding, dock etc.
    /// </summary>
    /// <param name="skin">Skin to use.</param>
    protected override void Layout(Skin.Base skin)
    {
        if (Canvas is { } canvas)
        {
            Size = canvas.Size;
        }
    }

    /// <summary>
    ///     Renders the control using specified skin.
    /// </summary>
    /// <param name="skin">Skin to use.</param>
    protected override void Render(Skin.Base skin)
    {
        skin.DrawModalControl(this);
    }

}
