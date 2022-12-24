using System.Drawing;
using System.Runtime.InteropServices;

namespace ImageChat.Core.Windows;

// 枚举、结构体、API定义
enum DWMWINDOWATTRIBUTE : uint
{
    NCRenderingEnabled = 1,
    NCRenderingPolicy,
    TransitionsForceDisabled,
    AllowNCPaint,
    CaptionButtonBounds,
    NonClientRtlLayout,
    ForceIconicRepresentation,
    Flip3DPolicy,
    ExtendedFrameBounds,
    HasIconicBitmap,
    DisallowPeek,
    ExcludedFromPeek,
    Cloak,
    Cloaked,
    FreezeRepresentation
}

[StructLayout(LayoutKind.Sequential)]
public struct RECT
{
    public int Left, Top, Right, Bottom;

    public RECT(int left, int top, int right, int bottom)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }

    public RECT(Rectangle r) : this(r.Left, r.Top, r.Right, r.Bottom) { }

    public int X
    {
        get => Left;
        set
        {
            Right -= Left - value;
            Left = value;
        }
    }

    public int Y
    {
        get => Top;
        set
        {
            Bottom -= Top - value;
            Top = value;
        }
    }

    public int Height
    {
        get => Bottom - Top;
        set => Bottom = value + Top;
    }

    public int Width
    {
        get => Right - Left;
        set => Right = value + Left;
    }

    public Point Location
    {
        get => new(Left, Top);
        set
        {
            X = value.X;
            Y = value.Y;
        }
    }

    public Size Size
    {
        get => new(Width, Height);
        set
        {
            Width = value.Width;
            Height = value.Height;
        }
    }

    public static implicit operator Rectangle(RECT r)
    {
        return new Rectangle(r.Left, r.Top, r.Width, r.Height);
    }

    public static implicit operator RECT(Rectangle r)
    {
        return new RECT(r);
    }

    public static bool operator ==(RECT r1, RECT r2)
    {
        return r1.Equals(r2);
    }

    public static bool operator !=(RECT r1, RECT r2)
    {
        return !r1.Equals(r2);
    }

    public bool Equals(RECT r)
    {
        return r.Left == Left && r.Top == Top && r.Right == Right && r.Bottom == Bottom;
    }

    public override bool Equals(object obj)
    {
        return obj switch
        {
            RECT rect => Equals(rect),
            Rectangle rectangle => Equals(new RECT(rectangle)),
            _ => false
        };
    }

    public override int GetHashCode()
    {
        return ((Rectangle)this).GetHashCode();
    }

    public override string ToString()
    {
        return string.Format(System.Globalization.CultureInfo.CurrentCulture,
            "{{Left={0},Top={1},Right={2},Bottom={3}}}", Left, Top, Right, Bottom);
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct POINT
{
    public int X;
    public int Y;

    public POINT(int x, int y)
    {
        X = x;
        Y = y;
    }

    public static explicit operator Point(POINT p)
    {
        return new Point(p.X, p.Y);
    }

    public static explicit operator POINT(Point p)
    {
        return new POINT(p.X, p.Y);
    }
}