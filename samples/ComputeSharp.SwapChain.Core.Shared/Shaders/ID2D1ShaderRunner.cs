#if WINDOWS_UWP
using Microsoft.Graphics.Canvas.UI.Xaml;
#endif

namespace ComputeSharp.SwapChain.Core.Shaders;

/// <summary>
/// An interface for a shader runner to be used with a Win2D renderer.
/// </summary>
public interface ID2D1ShaderRunner
{
#if WINDOWS_UWP
    /// <summary>
    /// Draws a new frame on a target <see cref="ICanvasAnimatedControl"/> instance.
    /// </summary>
    /// <param name="sender">The target <see cref="ICanvasAnimatedControl"/> instance.</param>
    /// <param name="args">The <see cref="CanvasAnimatedDrawEventArgs"/> instance to use to draw the new frame.</param>
    void Execute(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args);
#endif
}