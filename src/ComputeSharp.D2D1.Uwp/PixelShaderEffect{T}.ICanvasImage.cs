using System.Numerics;
using System.Runtime.InteropServices;
using ABI.Microsoft.Graphics.Canvas;
using ComputeSharp.D2D1.Extensions;
using ComputeSharp.Interop;
using Microsoft.Graphics.Canvas;
using TerraFX.Interop.Windows;
using Windows.Foundation;
using ICanvasResourceCreator = Microsoft.Graphics.Canvas.ICanvasResourceCreator;

namespace ComputeSharp.D2D1.Uwp;

/// <inheritdoc/>
unsafe partial class PixelShaderEffect<T>
{
    /// <inheritdoc/>
    public Rect GetBounds(ICanvasResourceCreator resourceCreator)
    {
        return GetBounds(resourceCreator, null);
    }

    /// <inheritdoc/>
    public Rect GetBounds(ICanvasResourceCreator resourceCreator, Matrix3x2 transform)
    {
        return GetBounds(resourceCreator, &transform);
    }

    /// <inheritdoc cref="ICanvasImage.GetBounds(ICanvasResourceCreator, Matrix3x2)"/>
    private Rect GetBounds(ICanvasResourceCreator resourceCreator, Matrix3x2* transform)
    {
        using ReferenceTracker.Lease _0 = GetReferenceTracker().GetLease();

        using ComPtr<IUnknown> resourceCreatorUnknown = default;
        using ComPtr<ABI.Microsoft.Graphics.Canvas.ICanvasResourceCreator> resourceCreatorAbi = default;

        // Get the ABI.Microsoft.Graphics.Canvas.ICanvasResourceCreator object from the input interface
        resourceCreatorUnknown.Attach((IUnknown*)Marshal.GetIUnknownForObject(resourceCreator));
        resourceCreatorUnknown.CopyTo(resourceCreatorAbi.GetAddressOf()).Assert();

        using ComPtr<IUnknown> canvasImageInteropUnknown = default;
        using ComPtr<ICanvasImageInterop> canvasImageInterop = default;

        // Get the ICanvasImageInterop object from the current instance
        canvasImageInteropUnknown.Attach((IUnknown*)Marshal.GetIUnknownForObject(this));
        canvasImageInteropUnknown.CopyTo(canvasImageInterop.GetAddressOf()).Assert();

        Rect bounds;

        // Forward the actual logic to Win2D to compute the image bounds (it needs the internal context)
        Win2D.GetBoundsForICanvasImageInterop(
            resourceCreator: resourceCreatorAbi.Get(),
            image: canvasImageInterop.Get(),
            transform: transform,
            rect: &bounds).Assert();

        return bounds;
    }
}