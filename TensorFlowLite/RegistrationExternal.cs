#nullable enable
using System;
using System.Runtime.InteropServices;
using TensorFlowLite.Native;
using static TensorFlowLite.Native.CApi;
namespace TensorFlowLite
{
    public unsafe struct RegistrationExternal : IDisposable
    {
        TfLiteRegistrationExternal* tfLiteRegistrationExternal;
        public RegistrationExternal(TfLiteBuiltinOperator builtinCode, string customName, int version)
        {
            var ptr = Marshal.StringToHGlobalAnsi(customName);
            tfLiteRegistrationExternal = TfLiteRegistrationExternalCreate(builtinCode, (byte*)ptr, version);
            Marshal.FreeHGlobal(ptr);
            if(tfLiteRegistrationExternal == null)
            {
                Throw();
                static void Throw()
                {
                    throw new Exception("Failed to create registration external.");
                }
            }
        }

        public readonly TfLiteBuiltinOperator BuiltInCode => TfLiteRegistrationExternalGetBuiltInCode(tfLiteRegistrationExternal);

        public readonly int Version => (int)TfLiteRegistrationExternalGetVersion(tfLiteRegistrationExternal);
        /// <summary>
        /// Returns the custom name. The returned <see cref="string"/>
        /// will be non-null if the op is a custom op.
        /// </summary>
        public readonly string? CustomName
        {
            get
            {
                var ptr = TfLiteRegistrationExternalGetCustomName(tfLiteRegistrationExternal);
                return Marshal.PtrToStringAnsi((IntPtr)ptr);
            }
        }
        public readonly void SetInitializationCallback(delegate*<TfLiteOpaqueContext*, byte*, nuint, void*> init)
            => TfLiteRegistrationExternalSetInit(tfLiteRegistrationExternal, init);

        public readonly void SetDeallocationCallback(delegate*<TfLiteOpaqueContext*, void*, void> free)
            => TfLiteRegistrationExternalSetFree(tfLiteRegistrationExternal, free);

        public readonly void SetPreparationCallback(delegate*<TfLiteOpaqueContext*, TfLiteOpaqueNode*, TfLiteStatus> prepare)
            => TfLiteRegistrationExternalSetPrepare(tfLiteRegistrationExternal, prepare);
        public readonly void SetInvocationCallback(delegate*<TfLiteOpaqueContext*, TfLiteOpaqueNode*, TfLiteStatus> invoke)
            => TfLiteRegistrationExternalSetInvoke(tfLiteRegistrationExternal, invoke);
        public void Dispose()
        {
            if(tfLiteRegistrationExternal == null)
            {
                return;
            }

            TfLiteRegistrationExternalDelete(tfLiteRegistrationExternal);
            tfLiteRegistrationExternal = null;
        }

    }
}
