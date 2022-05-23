﻿using System;
using System.Runtime.InteropServices;
using ComputeSharp.D2D1.Extensions;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;
#if !NET6_0_OR_GREATER
using UnmanagedCallersOnlyAttribute = ComputeSharp.NetStandard.System.Runtime.InteropServices.UnmanagedCallersOnlyAttribute;
#endif

namespace ComputeSharp.D2D1.Interop.Effects;

/// <summary>
/// A simple <see cref="ID2D1EffectImpl"/> and <see cref="ID2D1DrawTransform"/> implementation for a given pixel shader.
/// </summary>
internal unsafe partial struct PixelShaderEffect
{
    /// <summary>
    /// The implementation for <see cref="ID2D1EffectImpl"/>.
    /// </summary>
    private static class ID2D1EffectImplMethods
    {
#if !NET6_0_OR_GREATER
        /// <inheritdoc cref="Initialize"/>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate int InitializeDelegate(PixelShaderEffect* @this, ID2D1EffectContext* effectContext, ID2D1TransformGraph* transformGraph);

        /// <inheritdoc cref="PrepareForRender"/>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate int PrepareForRenderDelegate(PixelShaderEffect* @this, D2D1_CHANGE_TYPE changeType);

        /// <inheritdoc cref="SetGraph"/>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate int SetGraphDelegate(PixelShaderEffect* @this, ID2D1TransformGraph* transformGraph);

        /// <summary>
        /// A cached <see cref="QueryInterfaceDelegate"/> instance wrapping <see cref="QueryInterface"/>.
        /// </summary>
        public static readonly QueryInterfaceDelegate QueryInterfaceWrapper = QueryInterface;

        /// <summary>
        /// A cached <see cref="AddRefDelegate"/> instance wrapping <see cref="AddRef"/>.
        /// </summary>
        public static readonly AddRefDelegate AddRefWrapper = AddRef;

        /// <summary>
        /// A cached <see cref="ReleaseDelegate"/> instance wrapping <see cref="Release"/>.
        /// </summary>
        public static readonly ReleaseDelegate ReleaseWrapper = Release;

        /// <summary>
        /// A cached <see cref="InitializeDelegate"/> instance wrapping <see cref="Initialize"/>.
        /// </summary>
        public static readonly InitializeDelegate InitializeWrapper = Initialize;

        /// <summary>
        /// A cached <see cref="PrepareForRenderDelegate"/> instance wrapping <see cref="PrepareForRender"/>.
        /// </summary>
        public static readonly PrepareForRenderDelegate PrepareForRenderWrapper = PrepareForRender;

        /// <summary>
        /// A cached <see cref="SetGraphDelegate"/> instance wrapping <see cref="SetGraph"/>.
        /// </summary>
        public static readonly SetGraphDelegate SetGraphWrapper = SetGraph;
#endif

        /// <inheritdoc cref="ID2D1EffectImpl.QueryInterface"/>
        [UnmanagedCallersOnly]
        public static int QueryInterface(PixelShaderEffect* @this, Guid* riid, void** ppvObject)
        {
            return @this->QueryInterface(riid, ppvObject);
        }

        /// <inheritdoc cref="ID2D1EffectImpl.AddRef"/>
        [UnmanagedCallersOnly]
        public static uint AddRef(PixelShaderEffect* @this)
        {
            return @this->AddRef();
        }

        /// <inheritdoc cref="ID2D1EffectImpl.Release"/>
        [UnmanagedCallersOnly]
        public static uint Release(PixelShaderEffect* @this)
        {
            return @this->Release();
        }

        /// <inheritdoc cref="ID2D1EffectImpl.Initialize"/>
        [UnmanagedCallersOnly]
        public static int Initialize(PixelShaderEffect* @this, ID2D1EffectContext* effectContext, ID2D1TransformGraph* transformGraph)
        {
            int hresult = effectContext->LoadPixelShader(
                shaderId: &@this->shaderId,
                shaderBuffer: @this->bytecode,
                shaderBufferCount: (uint)@this->bytecodeSize);

            // If E_INVALIDARG was returned, try to check whether double precision support was requested when not available. This
            // is only done to provide a more helpful error message to callers. If no error was returned, the behavior is the same.
            if (hresult == E.E_INVALIDARG)
            {
                D2D1_FEATURE_DATA_DOUBLES d2D1FeatureDataDoubles = default;

                // If the call failed, just do nothing and return the previous result
                if (!Windows.SUCCEEDED(effectContext->CheckFeatureSupport(D2D1_FEATURE.D2D1_FEATURE_DOUBLES, &d2D1FeatureDataDoubles, (uint)sizeof(D2D1_FEATURE_DATA_DOUBLES))))
                {
                    return E.E_INVALIDARG;
                }

                // If the context does not support double precision values, check whether the shader requested them
                if (d2D1FeatureDataDoubles.doublePrecisionFloatShaderOps == 0)
                {
                    using ComPtr<ID3D11ShaderReflection> d3D11ShaderReflection = default;

                    // Create the reflection instance, and in case of error just return the previous error like above
                    if (!Windows.SUCCEEDED(DirectX.D3DReflect(
                        pSrcData: @this->bytecode,
                        SrcDataSize: (uint)@this->bytecodeSize,
                        pInterface: Windows.__uuidof<ID3D11ShaderReflection>(),
                        ppReflector: d3D11ShaderReflection.GetVoidAddressOf())))
                    {
                        return E.E_INVALIDARG;
                    }

                    // If the shader requires double precision support, return a more descriptive error
                    if ((d3D11ShaderReflection.Get()->GetRequiresFlags() & (D3D.D3D_SHADER_REQUIRES_DOUBLES | D3D.D3D_SHADER_REQUIRES_11_1_DOUBLE_EXTENSIONS)) != 0)
                    {
                        return D2DERR.D2DERR_INSUFFICIENT_DEVICE_CAPABILITIES;
                    }
                }
            }

            if (Windows.SUCCEEDED(hresult))
            {
                hresult = transformGraph->SetSingleTransformNode((ID2D1TransformNode*)&@this->lpVtblForID2D1DrawTransform);
            }

            return hresult;
        }

        /// <inheritdoc cref="ID2D1EffectImpl.PrepareForRender"/>
        [UnmanagedCallersOnly]
        public static int PrepareForRender(PixelShaderEffect* @this, D2D1_CHANGE_TYPE changeType)
        {
            if (@this->constantBuffer is not null)
            {
                return @this->d2D1DrawInfo->SetPixelShaderConstantBuffer(
                    buffer: @this->constantBuffer,
                    bufferCount: (uint)@this->constantBufferSize);
            }

            return S.S_OK;
        }

        /// <inheritdoc cref="ID2D1EffectImpl.SetGraph"/>
        [UnmanagedCallersOnly]
        public static int SetGraph(PixelShaderEffect* @this, ID2D1TransformGraph* transformGraph)
        {
            return E.E_NOTIMPL;
        }
    }
}