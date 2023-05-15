using System;
using System.Runtime.InteropServices;
using TensorFlowLite.Delegates.Native;
using TensorFlowLite.Native;
using static TensorFlowLite.Delegates.Native.XNNPackDelegateCApi;
using static TensorFlowLite.Delegates.XNNPackDelegate;
using static TensorFlowLite.Native.CApi;
namespace TensorFlowLite.Delegates
{
    public static class XNNPackDelegate
    {
        public static bool IsSupported => true;
        [Flags]
        public enum Flags : uint
        {
            /// <summary>
            /// Enable XNNPACK acceleration for signed quantized 8-bit inference.
            /// This includes operators with channel-wise quantized weights.
            /// </summary>
            Qs8 = 0x00000001,
            /// <summary>
            /// Enable XNNPACK acceleration for unsigned quantized 8-bit inference.
            /// </summary>
            Qu8 = 0x00000002,
            /// <summary>
            /// Force FP16 inference for FP32 operators.
            /// </summary>
            ForceFP16 = 0x00000004,
        }

        public struct Options
        {
            public static Options Default { get; } = new Options { Flags = Flags.Qs8 | Flags.Qu8 };
            internal TfLiteXNNPackDelegateOptions tfLiteXNNPackDelegateOptions;
            public int ThreadCount
            {
                readonly get => tfLiteXNNPackDelegateOptions.numThreads;
                set => tfLiteXNNPackDelegateOptions.numThreads = value;
            }
            public Flags Flags
            {
                readonly get => tfLiteXNNPackDelegateOptions.flags;
                set => tfLiteXNNPackDelegateOptions.flags = value;
            }
            public unsafe WeightsCache WeightsCache
            {
                readonly get => new WeightsCache { tfLiteXNNPackDelegateWeightsCache = tfLiteXNNPackDelegateOptions.weightsCache };
                set => tfLiteXNNPackDelegateOptions.weightsCache = value.tfLiteXNNPackDelegateWeightsCache;
            }
        }
        public unsafe struct WeightsCache : IDisposable
        {
            internal TfLiteXNNPackDelegateWeightsCache* tfLiteXNNPackDelegateWeightsCache;
            public static WeightsCache Create() => new WeightsCache { tfLiteXNNPackDelegateWeightsCache = TfLiteXNNPackDelegateWeightsCacheCreate() };

            public void Dispose()
            {
                if(tfLiteXNNPackDelegateWeightsCache == null)
                {
                    return;
                }
                TfLiteXNNPackDelegateWeightsCacheDelete(tfLiteXNNPackDelegateWeightsCache);
                tfLiteXNNPackDelegateWeightsCache = null;
            }
        }
        public static unsafe TensorFlowLiteDelegate Create(in Options options)
        {
            fixed(TfLiteXNNPackDelegateOptions* ptr = &options.tfLiteXNNPackDelegateOptions)
            {
                return new()
                {
                    TfLiteDelegate = TfLiteXNNPackDelegateCreate(ptr),
                    Delete = &TfLiteXNNPackDelegateDelete,

                };
            }
        }
    }
    namespace Native
    {
        public struct TfLiteXNNPackDelegateWeightsCache { }
        public unsafe struct TfLiteXNNPackDelegateOptions
        {
            public int numThreads;
            public Flags flags;
            public TfLiteXNNPackDelegateWeightsCache* weightsCache;
        }
        public static unsafe class XNNPackDelegateCApi
        {
            /// <summary>
            /// Returns a structure with the default XNNPack delegate options.
            /// </summary>
            /// <returns></returns>
            [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
            public static extern TfLiteXNNPackDelegateOptions TfLiteXNNPackDelegateOptionsDefault();

            /// <summary>
            /// Creates a new delegate instance that need to be destroyed with
            /// `TfLiteXNNPackDelegateDelete` when delegate is no longer used by TFLite.
            /// When `options` is set to `nullptr`, the following default values are used:
            /// </summary>
            [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
            public static extern TfLiteDelegate* TfLiteXNNPackDelegateCreate(TfLiteXNNPackDelegateOptions* options);

            /// <summary>
            /// Destroys a delegate created with `TfLiteXNNPackDelegateCreate` call.
            /// </summary>
            /// <param name="xnnPackDelegate"></param>
            [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
            public static extern void TfLiteXNNPackDelegateDelete(TfLiteDelegate* xnnPackDelegate);

            // Weights Cache is disable due to build error in iOS and Unity 2021 LTS.
            // https://github.com/asus4/tf-lite-unity-sample/issues/261

            /// <summary>
            /// Creates a new weights cache that can be shared with multiple delegate instances.
            /// </summary>
            /// <returns></returns>
            [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
            public static extern TfLiteXNNPackDelegateWeightsCache* TfLiteXNNPackDelegateWeightsCacheCreate();

            /// <summary>
            /// Destroys a weights cache created with `TfLiteXNNPackDelegateWeightsCacheCreate` call.
            /// </summary>
            /// <param name="cache"></param>
            [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
            public static extern void TfLiteXNNPackDelegateWeightsCacheDelete(TfLiteXNNPackDelegateWeightsCache* cache);
        }
    }
}
