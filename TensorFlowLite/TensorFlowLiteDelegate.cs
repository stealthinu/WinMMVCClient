using System;
using TensorFlowLite.Native;

namespace TensorFlowLite
{
    public unsafe struct TensorFlowLiteDelegate : IDisposable
    {
        internal TfLiteDelegate* TfLiteDelegate;
        internal delegate*<TfLiteDelegate*, void> Delete;
        public TfLiteDelegateFlags Flags
        {
            readonly get
            {
                ThrowIfTfLiteDelegateIsNull();
                return TfLiteDelegate->flags;
            }

            set
            {
                ThrowIfTfLiteDelegateIsNull();
                TfLiteDelegate->flags = value;
            }
        }

        readonly void ThrowIfTfLiteDelegateIsNull()
        {
            if(TfLiteDelegate is null)
            {
                Throw();
                static void Throw()
                {
                    throw new InvalidOperationException("Delegate is not initialized, or already disposed.");
                }
            }
        }

        public void Dispose()
        {
            if(TfLiteDelegate is null)
            {
                return;
            }
            Delete(TfLiteDelegate);
            this = default;
        }
    }
}