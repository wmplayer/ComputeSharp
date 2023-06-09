using ComputeSharp.D2D1;
//using System.Runtime.InteropServices;

namespace ComputeSharp.SwapChain.Shaders.D2D1;

[D2DInputCount(2)]
[D2DInputSimple(0)]
[D2DInputSimple(1)]
[D2DRequiresScenePosition]
[D2DShaderProfile(D2D1ShaderProfile.PixelShader50)]
[AutoConstructor]
internal readonly partial struct JoshEffects : ID2D1PixelShader
{
    /// <summary>
    /// The current time since the start of the application.
    /// </summary>
    private readonly float time;

    /// <summary>
    /// The dispatch size for the current output.
    /// </summary>
    private readonly int2 dispatchSize;

    private readonly float3 gyro;

    /// <inheritdoc/>
    public float4 Execute()
    {
        int2 xy = (int2)D2D.GetScenePosition().XY;
        // Normalized screen space UV coordinates from 0.0 to 1.0
        float2 uv = xy / (float2)this.dispatchSize;
        float4 inpixlDpth = D2D.SampleInputAtPosition(1, uv);
        float var = Hlsl.Sin(this.time);
        float2 offsetPosition = uv + (inpixlDpth.R * var);

        float4 inpixlSrc = D2D.SampleInputAtPosition(0, uv + offsetPosition);

        // Output to screen
        return inpixlSrc;
    }
}