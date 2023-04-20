using System;
using TensorFlowLite.Native;
namespace TensorFlowLite
{
    public unsafe interface IDelegate : IDisposable
    {
        internal TfLiteDelegate* TfLiteDelegate { get; }
    }
}
