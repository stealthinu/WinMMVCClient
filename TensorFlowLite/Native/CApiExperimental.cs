using System.Runtime.InteropServices;
using static TensorFlowLite.Native.CApi;
namespace TensorFlowLite.Native
{
    #region TypeDefs
    /// <summary>
    /// <see cref="TfLiteSignatureRunner"/> is used to run inference on a signature.
    /// </summary>
    /// <remarks>
    /// A signature is used to define a computation in a TF model. A model can
    /// have multiple signatures. Each signature contains three components:
    ///   * Signature Key: A unique string to identify a signature
    ///   * Inputs: A list of names, each mapped to an input tensor of a signature
    ///   * Outputs: A list of names, each mapped to an output tensor of a signature
    ///
    /// To learn more about signatures in TFLite, refer to:
    /// https://www.tensorflow.org/lite/guide/signatures
    ///
    /// Using the TfLiteSignatureRunner, for a particular signature, you can set its
    /// inputs, invoke (i.e. execute) the computation, and retrieve its outputs.
    /// </remarks>
    public struct TfLiteSignatureRunner { }
    #endregion
    public static unsafe class CApiExperimental
    {
        /// <summary>
        /// Resets all variable tensors to zero.
        /// </summary>
        [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern TfLiteStatus TfLiteInterpreterResetVariableTensors(TfLiteInterpreter* interpreter);
        /// <summary>
        /// Enable or disable the NN API delegate for the interpreter (true to enable).
        /// </summary>
        [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void TfLiteInterpreterOptionsSetUseNNAPI(TfLiteInterpreterOptions* options, bool enable);
        /// <summary>
        /// Enable or disable CPU fallback for the interpreter (true to enable).
        /// If enabled, TfLiteInterpreterInvoke will do automatic fallback from
        /// executing with delegate(s) to regular execution without delegates
        /// (i.e. on CPU).
        ///
        /// Allowing the fallback is suitable only if both of the following hold:
        /// - The caller is known not to cache pointers to tensor data across
        ///   TfLiteInterpreterInvoke calls.
        /// - The model is not stateful (no variables, no LSTMs) or the state isn't
        ///   needed between batches.
        ///
        /// When delegate fallback is enabled, TfLiteInterpreterInvoke will
        /// behave as follows:
        ///   If one or more delegates were set in the interpreter options
        ///   (see TfLiteInterpreterOptionsAddDelegate),
        ///   AND inference fails,
        ///   then the interpreter will fall back to not using any delegates.
        ///   In that case, the previously applied delegate(s) will be automatically
        ///   undone, and an attempt will be made to return the interpreter to an
        ///   invokable state, which may invalidate previous tensor addresses,
        ///   and the inference will be attempted again, using input tensors with
        ///   the same value as previously set.
        /// </summary>
        [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void TfLiteInterpreterOptionsSetEnableDelegateFallback(TfLiteInterpreterOptions* options, bool enable);
        [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int TfLiteInterpreterGetInputTensorIndex(TfLiteInterpreter* interpreter, int input_index);
        [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int TfLiteInterpreterGetOutputTensorIndex(TfLiteInterpreter* interpreter, int output_index);
        /// <summary>
        /// Returns the number of signatures defined in the model.
        /// </summary>
        [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int TfLiteInterpreterGetSignatureCount(TfLiteInterpreter* interpreter);
        [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte* TfLiteInterpreterGetSignatureKey(TfLiteInterpreter* interpreter, int signature_index);
        [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern TfLiteSignatureRunner* TfLiteInterpreterGetSignatureRunner(TfLiteInterpreter* interpreter, byte* signature_key);
        [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern nuint TfLiteSignatureRunnerGetInputCount(TfLiteSignatureRunner* signature_runner);
        [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]

        public static extern byte* TfLiteSignatureRunnerGetInputName(TfLiteSignatureRunner* signature_runner, int input_index);
        [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern TfLiteStatus TfLiteSignatureRunnerResizeInputTensor(TfLiteSignatureRunner* signature_runner, byte* input_name, nint* input_dims, int input_dims_size);
        [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern TfLiteStatus TfLiteSignatureRunnerAllocateTensors(TfLiteSignatureRunner* signature_runner);
        [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern TfLiteTensor* TfLiteSignatureRunnerGetInputTensor(TfLiteSignatureRunner* signature_runner, byte* input_name);
        [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern TfLiteStatus TfLiteSignatureRunnerInvoke(TfLiteSignatureRunner* signature_runner);
        [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern nuint TfLiteSignatureRunnerGetOutputCount(TfLiteSignatureRunner* signature_runner);
        [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte* TfLiteSignatureRunnerGetOutputName(TfLiteSignatureRunner* signature_runner, int output_index);
        [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern TfLiteTensor* TfLiteSignatureRunnerGetOutputTensor(TfLiteSignatureRunner* signature_runner, byte* output_name);
        [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern TfLiteStatus TfLiteSignatureRunnerCancel(TfLiteSignatureRunner* signature_runner);
        [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void TfLiteSignatureRunnerDelete(TfLiteSignatureRunner* signature_runner);
    }
}
