using System;
using System.Runtime.InteropServices;
using TensorFlowLite.Native;
using static TensorFlowLite.Native.CApi;
using static TensorFlowLite.Native.CApiExperimental;
namespace TensorFlowLite
{
    /// <summary>
    /// Provides inference from a provided model.
    /// </summary>
    public unsafe struct Interpreter : IDisposable
    {
        internal TfLiteInterpreter* tfLiteInterpreter;
        public Interpreter(Model model, InterpreterOptions? options = null)
        {
            tfLiteInterpreter = TfLiteInterpreterCreate(model.tfLiteModel, options.GetValueOrDefault().tfLiteInterpreterOptions);
            if(tfLiteInterpreter == null)
            {
                Throw();
                static void Throw()
                {
                    throw new Exception("Failed to create interpreter.");
                }
            }

        }

        public readonly int InputTensorCount => TfLiteInterpreterGetInputTensorCount(tfLiteInterpreter);
        public readonly ReadOnlySpan<nint> InputTensorIndices => new ReadOnlySpan<nint>(TfLiteInterpreterInputTensorIndices(tfLiteInterpreter), InputTensorCount);

        public readonly Tensor GetInputTensor(int inputIndex) => new Tensor(TfLiteInterpreterGetInputTensor(tfLiteInterpreter, inputIndex));

        public readonly TfLiteStatus ResizeInputTensor(int inputIndex, ReadOnlySpan<nint> dimensions)
        {
            fixed (nint* input_dims = dimensions)
            {
                return TfLiteInterpreterResizeInputTensor(tfLiteInterpreter, inputIndex, input_dims, dimensions.Length);
            }
        }

        public readonly TfLiteStatus AllocateTensors() => TfLiteInterpreterAllocateTensors(tfLiteInterpreter);
        public readonly TfLiteStatus Invoke() => TfLiteInterpreterInvoke(tfLiteInterpreter);
        public readonly int OutputTensorCount => TfLiteInterpreterGetOutputTensorCount(tfLiteInterpreter);
        public readonly ReadOnlySpan<nint> OutputTensorIndices => new ReadOnlySpan<nint>(TfLiteInterpreterOutputTensorIndices(tfLiteInterpreter), OutputTensorCount);
        public readonly Tensor GetOutputTensor(int outputIndex) => new Tensor(TfLiteInterpreterGetOutputTensor(tfLiteInterpreter, outputIndex));
        public readonly Tensor GetTensor(nint index) => new Tensor(TfLiteInterpreterGetTensor(tfLiteInterpreter, index));
        public readonly TfLiteStatus Cancel() => TfLiteInterpreterCancel(tfLiteInterpreter);
        /// <summary>
        /// Resets all variable tensors to zero.
        /// </summary>
        public readonly TfLiteStatus ResetVariableTensors() => TfLiteInterpreterResetVariableTensors(tfLiteInterpreter);
        public readonly int GetInputTensorIndex(int inputIndex) => TfLiteInterpreterGetInputTensorIndex(tfLiteInterpreter, inputIndex);
        public readonly int GetOutputTensorIndex(int outputIndex) => TfLiteInterpreterGetOutputTensorIndex(tfLiteInterpreter, outputIndex);
        public readonly int SignatureCount => TfLiteInterpreterGetSignatureCount(tfLiteInterpreter);
        public readonly string GetSignatureKey(int signatureIndex)
        {
            var ptr = TfLiteInterpreterGetSignatureKey(tfLiteInterpreter, signatureIndex);
            return Marshal.PtrToStringAnsi((IntPtr)ptr);
        }
        public readonly SignatureRunner GetSignatureRunner(string signatureKey)
        {
            var ptr = Marshal.StringToHGlobalAnsi(signatureKey);
            var tfLiteSignatureRunner = TfLiteInterpreterGetSignatureRunner(tfLiteInterpreter, (byte*)ptr);
            Marshal.FreeHGlobal(ptr);
            return new SignatureRunner(tfLiteSignatureRunner);
        }
        public void Dispose()
        {
            if (tfLiteInterpreter == null)
            {
                return;
            }
            TfLiteInterpreterDelete(tfLiteInterpreter);
            tfLiteInterpreter = null;
        }
    }
}
