using System;
using System.Runtime.InteropServices;
using TensorFlowLite.Native;
using static TensorFlowLite.Native.CApi;
namespace TensorFlowLite
{
    /// <summary>
    /// Wraps data associated with a graph tensor.
    /// </summary>
    public unsafe struct Tensor
    {
        internal TfLiteTensor* tfLiteTensor;
        internal Tensor(TfLiteTensor* tfLiteTensor)
        {
            this.tfLiteTensor = tfLiteTensor;
        }
        public readonly TfLiteType Type => TfLiteTensorType(tfLiteTensor);
        public readonly int Rank => TfLiteTensorNumDims(tfLiteTensor);
        public readonly int Size => (int)TfLiteTensorByteSize(tfLiteTensor);
        public Span<T> GetData<T>()
            where T : unmanaged
        {
            var ptr = TfLiteTensorData(tfLiteTensor);
            if (ptr == null)
            {
                return Span<T>.Empty;
            }
            return MemoryMarshal.Cast<byte, T>(new Span<byte>(ptr, Size));
        }
        public readonly string Name
        {
            get
            {
                var ptr = TfLiteTensorName(tfLiteTensor);
                return Marshal.PtrToStringAnsi((IntPtr)ptr);
            }
        }
        public readonly TfLiteStatus CopyFromBuffer<T>(ReadOnlySpan<T> inputData)
            where T : unmanaged
        {
            fixed (void* ptr = inputData)
            {
                return TfLiteTensorCopyFromBuffer(tfLiteTensor, ptr, (nuint)(inputData.Length * sizeof(T)));
            }
        }
        public readonly TfLiteStatus CopyFromBuffer<T>(Span<T> inputData) where T : unmanaged => CopyFromBuffer((ReadOnlySpan<T>)inputData);
        public readonly TfLiteStatus CopyToBuffer<T>(Span<T> outputData)
            where T : unmanaged
        {
            fixed (void* ptr = outputData)
            {
                return TfLiteTensorCopyToBuffer(tfLiteTensor, ptr, (nuint)(outputData.Length * sizeof(T)));
            }
        }

    }
}
