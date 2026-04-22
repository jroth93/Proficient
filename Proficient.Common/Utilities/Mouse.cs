// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.ComponentModel;
using System.Security;
using System.Security.Permissions;
using Point = System.Windows.Point;

namespace Proficient.Utilities;

/// <summary>
/// Exposes a simple interface to common mouse operations, allowing the user to simulate mouse input.
/// </summary>
/// <example>The following code moves to screen coordinate 100,100 and left clicks.
/// <code>
/// Mouse.MoveTo(new Point(100, 100));
/// Mouse.Click(MouseButton.Left);
/// </code>
/// </example>
public static class Mouse
{
    #region Public Methods

    /// <summary>
    /// Clicks a mouse button.
    /// </summary>
    /// <param name="mouseButton">The mouse button to click.</param>
    public static void Click(MouseButton mouseButton)
    {
        Down(mouseButton);
        Up(mouseButton);
    }

    /// <summary>
    /// Double-clicks a mouse button.
    /// </summary>
    /// <param name="mouseButton">The mouse button to click.</param>
    public static void DoubleClick(MouseButton mouseButton)
    {
        Click(mouseButton);
        Click(mouseButton);
    }

    /// <summary>
    /// Performs a mouse-down operation for a specified mouse button.
    /// </summary>
    /// <param name="mouseButton">The mouse button to use.</param>
    public static void Down(MouseButton mouseButton)
    {
        var inputFlags = GetInputFlags(mouseButton, false, out int additionalData);
        SendMouseInput(0, 0, additionalData, inputFlags);
    }

    /// <summary>
    /// Drags the mouse pointer to the specified screen coordinates, using the specified mouse button.
    /// Dragging involves pressing and holding the mouse button, moving to the specified location, and 
    /// releasing the mouse button.
    /// </summary>
    /// <param name="mouseButton">The mouse button to use.</param>
    /// <param name="point">The screen coordinates to move to.</param>
    public static void DragTo(MouseButton mouseButton, System.Drawing.Point point)
    {
        Down(mouseButton);
        MoveTo(point);
        Up(mouseButton);
    }

    /// <summary>
    /// Moves the mouse pointer to the specified screen coordinates.
    /// </summary>
    /// <param name="point">The screen coordinates to move to.</param>
    public static void MoveTo(System.Drawing.Point point)
    {
        SendMouseInput(point.X, point.Y, 0, SendMouseInputFlags.Move | SendMouseInputFlags.Absolute);
    }

    /// <summary>
    /// Resets the system mouse to a clean state.
    /// </summary>
    public static void Reset()
    {
        MoveTo(new System.Drawing.Point(0, 0));

        if (GetButtonState(MouseButton.Left) == MouseButtonState.Pressed)
        {
            SendMouseInput(0, 0, 0, SendMouseInputFlags.LeftUp);
        }

        if (GetButtonState(MouseButton.Middle) == MouseButtonState.Pressed)
        {
            SendMouseInput(0, 0, 0, SendMouseInputFlags.MiddleUp);
        }

        if (GetButtonState(MouseButton.Right) == MouseButtonState.Pressed)
        {
            SendMouseInput(0, 0, 0, SendMouseInputFlags.RightUp);
        }

        if (GetButtonState(MouseButton.XButton1) == MouseButtonState.Pressed)
        {
            SendMouseInput(0, 0, (int)NativeMethods.XBUTTON1, SendMouseInputFlags.XUp);
        }

        if (GetButtonState(MouseButton.XButton2) == MouseButtonState.Pressed)
        {
            SendMouseInput(0, 0, (int)NativeMethods.XBUTTON2, SendMouseInputFlags.XUp);
        }
    }

    /// <summary>
    /// Simulates scrolling of the mouse wheel up or down.
    /// </summary>
    /// <param name="lines">
    /// The number of lines to scroll. Use positive numbers to 
    /// scroll up and negative numbers to scroll down.
    /// </param>
    public static void Scroll(double lines)
    {
        int amount = (int)(NativeMethods.WheelDelta * lines);
        SendMouseInput(0, 0, amount, SendMouseInputFlags.Wheel);
    }

    /// <summary>
    /// Performs a mouse-up operation for a specified mouse button.
    /// </summary>
    /// <param name="mouseButton">The mouse button to use.</param>
    public static void Up(MouseButton mouseButton)
    {
        var inputFlags = GetInputFlags(mouseButton, true, out int additionalData);
        SendMouseInput(0, 0, additionalData, inputFlags);
    }

    /// <summary>
    /// Struct representing a point.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;

        public static implicit operator Point(POINT point)
        {
            return new Point(point.X, point.Y);
        }
    }

    /// <summary>
    /// Retrieves the cursor's position, in screen coordinates.
    /// </summary>
    /// <see>See MSDN documentation for further information.</see>
    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

    public static Point GetCursorPosition()
    {
        GetCursorPos(out var lpPoint);
        // NOTE: If you need error handling
        // bool success = GetCursorPos(out lpPoint);
        // if (!success)

        return lpPoint;
    }
    #endregion Public Methods

    #region Private Methods

    /// <summary>
    /// Sends mouse input.
    /// </summary>
    /// <param name="x">x coordinate</param>
    /// <param name="y">y coordinate</param>
    /// <param name="data">scroll wheel amount</param>
    /// <param name="flags">SendMouseInputFlags flags</param>
#if PRE25
    [PermissionSet(SecurityAction.Assert, Name = "FullTrust")]
#endif
    private static void SendMouseInput(int x, int y, int data, SendMouseInputFlags flags)
    {
#if PRE25
        PermissionSet permissions = new (PermissionState.Unrestricted);
        permissions.Demand();
#endif

        var intflags = (uint)flags;

        if ((intflags & (int)SendMouseInputFlags.Absolute) != 0)
        {
            // Absolute position requires normalized coordinates.
            NormalizeCoordinates(ref x, ref y);
            intflags |= NativeMethods.MouseeventfVirtualdesk;
        }

        var mi = new NativeMethods.INPUT
        {
            Type = NativeMethods.INPUT_MOUSE,
            Data = new NativeMethods.MOUSEKEYBDHARDWAREINPUT
            {
                Mouse = new NativeMethods.MOUSEINPUT
                {
                    dx = x,
                    dy = y,
                    mouseData = data,
                    dwFlags = intflags,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                }
            }
        };

        if (NativeMethods.SendInput(1, [mi], Marshal.SizeOf(mi)) == 0)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }

    private static SendMouseInputFlags GetInputFlags(MouseButton mouseButton, bool isUp, out int additionalData)
    {
        SendMouseInputFlags flags;
        additionalData = 0;

        if (mouseButton == MouseButton.Left && isUp)
        {
            flags = SendMouseInputFlags.LeftUp;
        }
        else if (mouseButton == MouseButton.Left && !isUp)
        {
            flags = SendMouseInputFlags.LeftDown;
        }
        else if (mouseButton == MouseButton.Right && isUp)
        {
            flags = SendMouseInputFlags.RightUp;
        }
        else if (mouseButton == MouseButton.Right && !isUp)
        {
            flags = SendMouseInputFlags.RightDown;
        }
        else if (mouseButton == MouseButton.Middle && isUp)
        {
            flags = SendMouseInputFlags.MiddleUp;
        }
        else if (mouseButton == MouseButton.Middle && !isUp)
        {
            flags = SendMouseInputFlags.MiddleDown;
        }
        else if (mouseButton == MouseButton.XButton1 && isUp)
        {
            flags = SendMouseInputFlags.XUp;
            additionalData = (int)NativeMethods.XBUTTON1;
        }
        else if (mouseButton == MouseButton.XButton1 && !isUp)
        {
            flags = SendMouseInputFlags.XDown;
            additionalData = (int)NativeMethods.XBUTTON1;
        }
        else if (mouseButton == MouseButton.XButton2 && isUp)
        {
            flags = SendMouseInputFlags.XUp;
            additionalData = (int)NativeMethods.XBUTTON2;
        }
        else if (mouseButton == MouseButton.XButton2 && !isUp)
        {
            flags = SendMouseInputFlags.XDown;
            additionalData = (int)NativeMethods.XBUTTON2;
        }
        else
        {
            throw new InvalidOperationException();
        }

        return flags;
    }

    private static void NormalizeCoordinates(ref int x, ref int y)
    {
        int vScreenWidth = NativeMethods.GetSystemMetrics(NativeMethods.SMCxvirtualscreen);
        int vScreenHeight = NativeMethods.GetSystemMetrics(NativeMethods.SMCyvirtualscreen);
        int vScreenLeft = NativeMethods.GetSystemMetrics(NativeMethods.SMXvirtualscreen);
        int vScreenTop = NativeMethods.GetSystemMetrics(NativeMethods.SMYvirtualscreen);

        // Absolute input requires that input is in 'normalized' coords - with the entire
        // desktop being (0,0)...(65536,65536). Need to convert input x,y coords to this
        // first.
        //
        // In this normalized world, any pixel on the screen corresponds to a block of values
        // of normalized coords - eg. on a 1024x768 screen,
        // y pixel 0 corresponds to range 0 to 85.333,
        // y pixel 1 corresponds to range 85.333 to 170.666,
        // y pixel 2 correpsonds to range 170.666 to 256 - and so on.
        // Doing basic scaling math - (x-top)*65536/Width - gets us the start of the range.
        // However, because int math is used, this can end up being rounded into the wrong
        // pixel. For example, if we wanted pixel 1, we'd get 85.333, but that comes out as
        // 85 as an int, which falls into pixel 0's range - and that's where the pointer goes.
        // To avoid this, we add on half-a-"screen pixel"'s worth of normalized coords - to
        // push us into the middle of any given pixel's range - that's the 65536/(Width*2)
        // part of the formula. So now pixel 1 maps to 85+42 = 127 - which is comfortably
        // in the middle of that pixel's block.
        // The key ting here is that unlike points in coordinate geometry, pixels take up
        // space, so are often better treated like rectangles - and if you want to target
        // a particular pixel, target its rectangle's midpoint, not its edge.
        if (vScreenWidth == 0) vScreenWidth = 1920;
        if (vScreenHeight == 0) vScreenHeight = 1080;

        x = ((x - vScreenLeft) * 65536) / vScreenWidth + 65536 / (vScreenWidth * 2);
        y = ((y - vScreenTop) * 65536) / vScreenHeight + 65536 / (vScreenHeight * 2);
    }

    private static MouseButtonState GetButtonState(MouseButton mouseButton)
    {
        var mouseButtonState = MouseButtonState.Released;

        int virtualKeyCode = 0;
        switch (mouseButton)
        {
            case MouseButton.Left:
                virtualKeyCode = NativeMethods.VK_LBUTTON;
                break;
            case MouseButton.Right:
                virtualKeyCode = NativeMethods.VK_RBUTTON;
                break;
            case MouseButton.Middle:
                virtualKeyCode = NativeMethods.VK_MBUTTON;
                break;
            case MouseButton.XButton1:
                virtualKeyCode = NativeMethods.VK_XBUTTON1;
                break;
            case MouseButton.XButton2:
                virtualKeyCode = NativeMethods.VK_XBUTTON2;
                break;
        }

        mouseButtonState = (NativeMethods.GetKeyState(virtualKeyCode) & 0x8000) != 0 ? MouseButtonState.Pressed : MouseButtonState.Released;
        return mouseButtonState;
    }

#endregion Private Methods
}

internal enum MouseButtonState
{
    Released = 0,
    Pressed = 1,
}

public enum MouseButton
{
    /// <summary>
    /// The left mouse button.
    /// </summary>
    Left = 0,

    /// <summary>
    /// The middle mouse button.
    /// </summary>
    Middle = 1,

    /// <summary>
    /// The right mouse button.
    /// </summary>
    Right = 2,

    /// <summary>
    /// The first extended mouse button.
    /// </summary>
    XButton1 = 3,

    /// <summary>
    /// The second extended mouse button
    /// </summary>
    XButton2 = 4,
}

[Flags]
internal enum SendMouseInputFlags
{
    Move = 0x0001,
    LeftDown = 0x0002,
    LeftUp = 0x0004,
    RightDown = 0x0008,
    RightUp = 0x0010,
    MiddleDown = 0x0020,
    MiddleUp = 0x0040,
    XDown = 0x0080,
    XUp = 0x0100,
    Wheel = 0x0800,
    Absolute = 0x8000,
}