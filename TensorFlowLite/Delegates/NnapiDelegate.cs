using System.Runtime.InteropServices;
using TensorFlowLite.Delegates.Native;
using TensorFlowLite.Native;
using static TensorFlowLite.Native.CApi;
using static TensorFlowLite.Delegates.Native.NnapiDelegateCApi;
namespace TensorFlowLite.Delegates
{
    public static class NnapiDelegate
    {
        public static bool IsSupported =>
#if UNITY_ANDROID && !UNITY_EDITOR
            true
#else
            false
#endif
            ;
        public static unsafe TensorFlowLiteDelegate Create(in Options options)
        {
            fixed (TfLiteNnapiDelegateOptions* ptr = &options.tfLiteNnapiDelegateOptions)
            {
                return new()
                {
                    TfLiteDelegate = TfLiteNnapiDelegateCreate(ptr),
                    Delete = &TfLiteNnapiDelegateDelete,
                };
            }
        }

        public enum ExecutionPreference
        {
            Undefined = -1,
            LowPower = 0,
            FastSingleAnswer = 1,
            SustainedSpeed = 2,
        };
        public struct Options
        {
            public static Options Default { get; } = new Options { tfLiteNnapiDelegateOptions = TfLiteNnapiDelegateOptionsDefault() };

            internal TfLiteNnapiDelegateOptions tfLiteNnapiDelegateOptions;

            public ExecutionPreference ExecutionPreference
            {
                readonly get => tfLiteNnapiDelegateOptions.execution_preference;
                set => tfLiteNnapiDelegateOptions.execution_preference = value;
            }
            public bool DisallowNnapiCpu
            {
                readonly get => tfLiteNnapiDelegateOptions.disallow_nnapi_cpu != 0;
                set => tfLiteNnapiDelegateOptions.disallow_nnapi_cpu = value ? 1 : 0;
            }
            public bool AllowFp16
            {
                readonly get => tfLiteNnapiDelegateOptions.allow_fp16 != 0;
                set => tfLiteNnapiDelegateOptions.allow_fp16 = value ? 1 : 0;
            }
        }
    }
    namespace Native
    {
        public static unsafe class NnapiDelegateCApi
        {
            [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
            public static extern TfLiteDelegate* TfLiteNnapiDelegateCreate(TfLiteNnapiDelegateOptions* options);
            [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
            public static extern TfLiteNnapiDelegateOptions TfLiteNnapiDelegateOptionsDefault();
            [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
            public static extern void TfLiteNnapiDelegateDelete(TfLiteDelegate* @delegate);
        }
        public unsafe struct TfLiteNnapiDelegateOptions
        {
            public NnapiDelegate.ExecutionPreference execution_preference;
            public byte* accelerator_name;
            public byte* cache_dir;
            public byte* model_token;
            public nint disallow_nnapi_cpu;
            public nint allow_fp16;
            public nint max_number_delegated_partitions;
            public void* nnapi_support_library_handle;
        }
    }
}