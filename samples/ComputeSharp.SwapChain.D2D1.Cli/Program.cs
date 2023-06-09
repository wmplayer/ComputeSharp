using System;
using ComputeSharp.SwapChain.D2D1.Backend;
using ComputeSharp.SwapChain.Shaders.D2D1;
using TerraFX.Interop.Windows;
using Windows.Devices.Sensors;
using Microsoft.Graphics.Canvas;
using System.IO;
using Windows.Storage;

Accelerometer g_orientation = Windows.Devices.Sensors.Accelerometer.GetDefault();
double g_steeringWheel = 0.0;
double g_laptopLid = 0.0;

double g_speedFactor = 2.0;
double g_driftyness = 0.0;
TimeSpan g_startTime = TimeSpan.Zero;
g_orientation.ReportInterval = 50;
g_orientation.ReadingChanged += G_orientation_ReadingChanged;
CanvasBitmap[] g_bmps = new CanvasBitmap[2];

void G_orientation_ReadingChanged(object sender, AccelerometerReadingChangedEventArgs args)
{
    double[] newValues = { args.Reading.AccelerationX, args.Reading.AccelerationY, args.Reading.AccelerationZ };
    double smoothingWeight = 0.8;
    double[] smoothedValues = { 0, 0, 0 };
    for (int idx = 0; idx < 3; idx++)
    {
        smoothedValues[idx] = (smoothedValues[idx] * (1.0 - smoothingWeight)) + (newValues[idx] * smoothingWeight);
    }

    double X = newValues[0];
    double Y = newValues[1];
    double Z = newValues[2];
    double M_PI = Math.PI;
    double sign = (Z > 0) ? 1 : -1;
    double miu = 0.001;

    double Pitch = Math.Atan2(-X, Math.Sqrt((Y * Y) + (Z * Z))) * 180 / M_PI;
    double Roll = Math.Atan2(Y, sign * Math.Sqrt((Z * Z) + (miu * X * X)));

    const double g_smweight = 0.9;
    const double g_1m_smweight = 1.0 - g_smweight;
    g_laptopLid = (g_1m_smweight * g_laptopLid) + (g_smweight * Roll);
    g_steeringWheel = (g_1m_smweight * g_steeringWheel) + (g_smweight * Pitch);// * 0.5;
    if (double.IsNaN(g_laptopLid))
    {
        g_laptopLid = 0;
    }

    if (double.IsNaN(g_steeringWheel)) { g_steeringWheel = 0; }
}

// Accepted inputs:
//   - []
//   - [<SHADER_NAME>]
if (args is [] or [_])
{
    // If there are no args, just run the default shader
    if (args is not [string shaderName])
    {
        shaderName = nameof(TwoTiledTruchet);
    }

    using CanvasDevice canvasDevice = new();
    Windows.Storage.Pickers.FileOpenPicker picker = new();
    g_bmps = new CanvasBitmap[2];
    StorageFile[] storageFiles = new StorageFile[2];
    for (int i = 0; i < storageFiles.Length; i++)
    {
        storageFiles[i] = await picker.PickSingleFileAsync();
        using Stream stream = await storageFiles[i].OpenStreamForReadAsync();
        g_bmps[i] = await CanvasBitmap.LoadAsync(canvasDevice, stream.AsRandomAccessStream());
    }

    PixelShaderEffect? effect =
        new PixelShaderEffect.For<HelloWorld>
        (static (time, width, height) => new((float)time.TotalSeconds, new int2(width, height)));

    effect.ConstantBuffer = new JoshEffects(0, new int2(0, 0), new float3(0, 0, 0));
    //effect.Sources[0] = g_bmps[0];
    //effect.Sources[1] = g_bmps[1];

    // If a shader is found, run it
    if (effect is not null)
    {
        //effectvpn
        Win32Application win32Application = new();
        g_startTime = TimeSpan.Zero;

        win32Application.Draw += (_, e) =>
        {
            if (g_startTime == TimeSpan.Zero)
            {
                g_startTime = e.TotalTime;
            }
            else
            {
                g_startTime += TimeSpan.FromSeconds(g_driftyness);
            }
            // Set the effect properties

            effect.ElapsedTime = g_startTime + TimeSpan.FromSeconds(g_laptopLid * g_speedFactor);
            effect.ScreenWidth = (int)e.ScreenWidth;
            effect.ScreenHeight = (int)e.ScreenHeight;

            // Draw the effect
            e.DrawingSession.DrawImage(effect);
        };

        return Win32ApplicationRunner.Run(win32Application, "ComputeSharp.D2D1", "ComputeSharp.D2D1");
    }

    // If no shader matches, check if help was requested
    if (shaderName is "-h")
    {
        Console.WriteLine($"""
            Available shaders:

            - {nameof(HelloWorld)}
            - {nameof(ColorfulInfinity)}
            - {nameof(FractalTiling)}
            - {nameof(TwoTiledTruchet)}
            - {nameof(MengerJourney)}
            - {nameof(Octagrams)}
            - {nameof(ProteanClouds)}
            - {nameof(PyramidPattern)}
            - {nameof(TriangleGridContouring)}
            - {nameof(TerracedHills)}
            """);

        return S.S_OK;
    }
}

return E.E_INVALIDARG;

//Get the requested shader, if possible
//    PixelShaderEffect? effect = shaderName.ToLowerInvariant() switch
//    {
//        "helloworld" => new PixelShaderEffect.For<HelloWorld>(static (time, width, height) => new((float)time.TotalSeconds, new int2(width, height))),
//        "colorfulinfinity" => new PixelShaderEffect.For<ColorfulInfinity>(static (time, width, height) => new((float)time.TotalSeconds, new int2(width, height))),
//        "fractaltiling" => new PixelShaderEffect.For<FractalTiling>(static (time, width, height) => new((float)time.TotalSeconds, new int2(width, height))),
//        "twotiledtruchet" => new PixelShaderEffect.For<TwoTiledTruchet>(static (time, width, height) => new((float)time.TotalSeconds, new int2(width, height))),
//        "mengerjourney" => new PixelShaderEffect.For<MengerJourney>(static (time, width, height) => new((float)time.TotalSeconds, new int2(width, height))),
//        "octagrams" => new PixelShaderEffect.For<Octagrams>(static (time, width, height) => new((float)time.TotalSeconds, new int2(width, height))),
//        "proteanclouds" => new PixelShaderEffect.For<ProteanClouds>(static (time, width, height) => new((float)time.TotalSeconds, new int2(width, height))),
//        "pyramidpattern" => new PixelShaderEffect.For<PyramidPattern>(static (time, width, height) => new((float)time.TotalSeconds, new int2(width, height))),
//        "trianglegridcontouring" => new PixelShaderEffect.For<TriangleGridContouring>(static (time, width, height) => new((float)time.TotalSeconds, new int2(width, height))),
//        "terracedhills" => new PixelShaderEffect.For<TerracedHills>(static (time, width, height) => new((float)time.TotalSeconds, new int2(width, height))),
//        _ => null
//    };

