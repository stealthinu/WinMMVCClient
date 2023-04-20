using System;
using System.Runtime.InteropServices;
using TensorFlowLite.Native;
using static TensorFlowLite.Native.CApiExperimental;
namespace TensorFlowLite
{
    public unsafe struct SignatureRunner
    {
        internal TfLiteSignatureRunner* tfLiteSignatureRunner;
        internal SignatureRunner(TfLiteSignatureRunner* tfLiteSignatureRunner)
        {
            this.tfLiteSignatureRunner = tfLiteSignatureRunner;
        }
        public readonly int InputCount => (int)TfLiteSignatureRunnerGetInputCount(tfLiteSignatureRunner);

        public readonly string GetInputName(int inputIndex)
        {
            var ptr = TfLiteSignatureRunnerGetInputName(tfLiteSignatureRunner, inputIndex);
            return Marshal.PtrToStringAnsi((IntPtr)ptr);
        }
        public readonly TfLiteStatus ResizeInputTensor(string inputName, ReadOnlySpan<nint> inputDimensions)
        {
            var input_name = Marshal.StringToHGlobalAnsi(inputName);
            TfLiteStatus status;
            fixed (nint* input_dims = inputDimensions)
            {
                status = TfLiteSignatureRunnerResizeInputTensor(tfLiteSignatureRunner, (byte*)input_name, input_dims, inputDimensions.Length);
            }
            Marshal.FreeHGlobal(input_name);
            return status;
        }
        public readonly TfLiteStatus AllocateTensors() => TfLiteSignatureRunnerAllocateTensors(tfLiteSignatureRunner);
        public readonly Tensor GetInputTensor(string inputName)
        {
            var input_name = Marshal.StringToHGlobalAnsi(inputName);
            var tfLiteTensor = TfLiteSignatureRunnerGetInputTensor(tfLiteSignatureRunner, (byte*)input_name);
            Marshal.FreeHGlobal(input_name);
            return new Tensor(tfLiteTensor);
        }
        public readonly TfLiteStatus Invoke() => TfLiteSignatureRunnerInvoke(tfLiteSignatureRunner);
        public readonly int OutputCount => (int)TfLiteSignatureRunnerGetOutputCount(tfLiteSignatureRunner);
        public readonly string GetOutputName(int outputIndex)
        {
            var ptr = TfLiteSignatureRunnerGetOutputName(tfLiteSignatureRunner, outputIndex);
            return Marshal.PtrToStringAnsi((IntPtr)ptr);
        }
        public readonly Tensor GetOutputTensor(string outputName)
        {
            var output_name = Marshal.StringToHGlobalAnsi(outputName);
            var tfLiteTensor = TfLiteSignatureRunnerGetOutputTensor(tfLiteSignatureRunner, (byte*)output_name);
            Marshal.FreeHGlobal(output_name);
            return new Tensor(tfLiteTensor);
        }
        public readonly TfLiteStatus Cancel() => TfLiteSignatureRunnerCancel(tfLiteSignatureRunner);
        public void Dispose()
        {
            if (tfLiteSignatureRunner == null)
            {
                return;
            }
            TfLiteSignatureRunnerDelete(tfLiteSignatureRunner);
            tfLiteSignatureRunner = null;
        }
    }
}
