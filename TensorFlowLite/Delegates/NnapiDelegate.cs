#if UNITY_ANDROID && !UNITY_EDITOR
using System.Runtime.InteropServices;
using TensorFlowLite.Delegates.Native;
using TensorFlowLite.Native;
using static TensorFlowLite.Native.CApi;
using static TensorFlowLite.Delegates.Native.NnapiDelegateCApi;
namespace TensorFlowLite.Delegates
{
    public unsafe struct NnapiDelegate : IDelegate
    {
        public static NnapiDelegate Default { get; } = new NnapiDelegate(Options.Default);

        TfLiteDelegate* tfLiteDelegate;
        TfLiteDelegate* IDelegate.TfLiteDelegate => tfLiteDelegate;
        public NnapiDelegate(in Options options)
        {
            fixed (TfLiteNnapiDelegateOptions* ptr = &options.tfLiteNnapiDelegateOptions)
            {
                tfLiteDelegate = TfLiteNnapiDelegateCreate(ptr);
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
                get => tfLiteNnapiDelegateOptions.execution_preference;
                set => tfLiteNnapiDelegateOptions.execution_preference = value;
            }
            public bool DisallowNnapiCpu
            {
                get => tfLiteNnapiDelegateOptions.disallow_nnapi_cpu != 0;
                set => tfLiteNnapiDelegateOptions.disallow_nnapi_cpu = value ? 1 : 0;
            }
            public bool AllowFp16
            {
                get => tfLiteNnapiDelegateOptions.allow_fp16 != 0;
                set => tfLiteNnapiDelegateOptions.allow_fp16 = value ? 1 : 0;
            }
        }
        public void Dispose()
        {
            if (tfLiteDelegate == null)
            {
                return;
            }
            TfLiteNnapiDelegateDelete(tfLiteDelegate);
            tfLiteDelegate = null;
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
#endif