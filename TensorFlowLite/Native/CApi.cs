using System;
using System.Runtime.InteropServices;

namespace TensorFlowLite.Native
{
    #region TypeDefs
    /// <summary>
    /// <see cref="TfLiteModel"/> wraps a loaded TensorFlow Lite model.
    /// </summary>
    public struct TfLiteModel { }
    /// <summary>
    /// <see cref="TfLiteInterpreterOptions"/> allows customized interpreter configuration.
    /// </summary>
    public struct TfLiteInterpreterOptions { }
    /// <summary>
    /// <see cref="TfLiteInterpreter"/> provides inference from a provided model.
    /// </summary>
    public struct TfLiteInterpreter { }

    /// <summary>
    /// <see cref="TfLiteTensor"/> wraps data associated with a graph tensor.
    /// </summary>
    /// <remarks>
    /// While the <see cref="TfLiteTensor"/> struct is not currently opaque, and its
    /// fields can be accessed directly, these methods are still convenient for
    /// language bindings. In the future the tensor struct will likely be made opaque
    /// in the public API.
    /// </remarks>
    public struct TfLiteTensor { }
    /// <summary>
    /// TfLiteRegistrationExternal is an external version of TfLiteRegistration to
    /// use custom op registration API.
    /// </summary>
    /// <remarks>
    /// This is an experimental type and subject to change.
    /// </remarks>
    public struct TfLiteRegistrationExternal { }

    public struct TfLiteContext { }
    public struct TfLiteOpaqueContext { }

    public struct TfLiteOpaqueNode { }
    public struct TfLiteOpaqueTensor { }
    [Flags]
    public enum TfLiteDelegateFlags : long
    {
        None = 0,

        /// <summary>
        /// The flag is set if the delegate can handle dynamic sized tensors.
        /// For example, the output shape of a `Resize` op with non-constant shape
        /// can only be inferred when the op is invoked.
        /// In this case, the Delegate is responsible for calling
        /// `SetTensorToDynamic` to mark the tensor as a dynamic tensor, and calling
        /// `ResizeTensor` when invoking the op.
        ///
        /// If the delegate isn't capable to handle dynamic tensors, this flag need
        /// to be set to false.
        /// </summary>
        AllowDynamicTensors = 1,
        /// <summary>
        /// This flag can be used by delegates (that allow dynamic tensors) to ensure
        /// applicable tensor shapes are automatically propagated in the case of tensor
        /// resizing.
        /// This means that non-dynamic (allocation_type != kTfLiteDynamic) I/O tensors
        /// of a delegate kernel will have correct shapes before its Prepare() method
        /// is called. The runtime leverages TFLite builtin ops in the original
        /// execution plan to propagate shapes.
        /// </summary>
        /// <remarks>
        /// A few points to note:
        /// 1. This requires kTfLiteDelegateFlagsAllowDynamicTensors. If that flag is
        /// false, this one is redundant since the delegate kernels are re-initialized
        /// every time tensors are resized.
        /// 2. Enabling this flag adds some overhead to AllocateTensors(), since extra
        /// work is required to prepare the original execution plan.
        /// 3. This flag requires that the original execution plan only have ops with
        /// valid registrations (and not 'dummy' custom ops like with Flex).
        /// WARNING: This feature is experimental and subject to change.
        /// </remarks>
        RequirePropagatedShapes = 2,

        /// <summary>
        /// This flag can be used by delegates to request per-operator profiling. If a
        /// node is a delegate node, this flag will be checked before profiling. If
        /// set, then the node will not be profiled. The delegate will then add per
        /// operator information using Profiler::EventType::OPERATOR_INVOKE_EVENT and
        /// the results will appear in the operator-wise Profiling section and not in
        /// the Delegate internal section.
        /// </summary>
        PerOperatorProfiling = 4
    }
    public unsafe struct TfLiteDelegate 
    {
        public void* data_;
        public void* Prepare;
        public void* CopyFromBufferHandle;
        public void* CopyToBufferHandle;
        public void* FreeBufferHandle;
        public TfLiteDelegateFlags flags;
        public void* opaque_delegate_builder;
    }
    public struct TfLiteOpaqueDelegate { }
    #endregion
    public static unsafe class CApi
    {
#if !UNITY_2021_2_OR_NEWER || UNITY_STANDALONE || UNITY_EDITOR 
        internal const string TensorFlowLiteLibraryName = "libtensorflowlite_c";
#elif UNITY_ANDROID
        internal const string TensorFlowLiteLibraryName = "libtensorflowlite_jni";
#elif UNITY_IOS
        internal const string TensorFlowLiteLibraryName = "__Internal";
#endif
        /// <summary>
        /// The TensorFlow Lite Runtime version.
        ///
        /// Returns a pointer to a statically allocated string that is the version
        /// number of the (potentially dynamically loaded) TF Lite Runtime library.
        /// TensorFlow Lite uses semantic versioning, and the return value should be
        /// in semver 2 format http://semver.org, starting with MAJOR.MINOR.PATCH,
        /// e.g. "2.12.0" or "2.13.0-rc2".
        /// </summary>
        [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]

        public static extern byte* TfLiteVersion();
        /// <summary>
        /// The supported TensorFlow Lite model file Schema version.
        ///
        /// Returns the (major) version number of the Schema used for model
        /// files that is supported by the (potentially dynamically loaded)
        /// TensorFlow Lite Runtime.
        ///
        /// Model files using schema versions different to this may not be supported by
        /// the current version of the TF Lite Runtime.
        /// </summary>
        [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint TfLiteSchemaVersion();
        /// <summary>
        /// Returns a model from the provided buffer, or null on failure.
        /// </summary>
        /// <remarks>
        /// The caller retains ownership of the <paramref name="model_data"/> buffer and should
        /// ensure that the lifetime of the <paramref name="model_data"/> buffer must be at least as long
        /// as the lifetime of the <see cref="TfLiteModel"/> and of any <see cref="TfLiteInterpreter"/> objects
        /// created from that <see cref="TfLiteModel"/>, and furthermore the contents of the
        /// <paramref name="model_data"/> buffer must not be modified during that time."
        /// </remarks>
        [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern TfLiteModel* TfLiteModelCreate(void* model_data, nuint model_size);
        /// <summary>
        /// Returns a model from the provided file, or null on failure.
        /// </summary>
        /// <remarks>
        /// The file's contents must not be modified during the lifetime of the
        /// <see cref="TfLiteModel"/> or of any <see cref="TfLiteInterpreter"/> objects created from that
        /// <see cref="TfLiteModel"/>.
        /// </remarks>
        [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern TfLiteModel* TfLiteModelCreateFromFile(byte* model_path);
        /// <summary>
        /// Destroys the model instance.
        /// </summary>
        [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void TfLiteModelDelete(TfLiteModel* model);
        /// <summary>
        /// Returns a new interpreter options instances.
        /// </summary>
        [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern TfLiteInterpreterOptions* TfLiteInterpreterOptionsCreate();
        /// <summary>
        /// Creates and returns a shallow copy of an options object.
        ///
        /// The caller is responsible for calling <see cref="TfLiteInterpreterOptionsDelete(TfLiteInterpreterOptions*)"/> to
        /// deallocate the object pointed to by the returned pointer.
        /// </summary>
        [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern TfLiteInterpreterOptions* TfLiteInterpreterOptionsCopy(TfLiteInterpreterOptions* from);
        /// <summary>
        /// Destroys the interpreter options instance.
        /// </summary>
        [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void TfLiteInterpreterOptionsDelete(TfLiteInterpreterOptions* options);
        /// <summary>
        /// Sets the number of CPU threads to use for the interpreter.
        /// </summary>
        [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void TfLiteInterpreterOptionsSetNumThreads(TfLiteInterpreterOptions* options, int num_threads);
        /// <summary>
        /// Adds a delegate to be applied during <see cref="TfLiteInterpreter"/> creation.
        ///
        /// If delegate application fails, interpreter creation will also fail with an
        /// associated error logged.
        /// </summary>
        /// <remarks>
        /// The caller retains ownership of the delegate and should ensure that it
        /// remains valid for the duration of any created interpreter's lifetime.
        /// </remarks>
        [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void TfLiteInterpreterOptionsAddDelegate(TfLiteInterpreterOptions* options, TfLiteDelegate* @delegate);
        /// <summary>
        /// Adds an op registration to be applied during <see cref="TfLiteInterpreter"/> creation.
        ///
        /// The <see cref="TfLiteRegistrationExternal"/> object is needed to implement custom op of
        /// TFLite Interpreter via C API. Calling this function ensures that any
        /// <see cref="TfLiteInterpreter"/> created with the specified `options` can execute models
        /// that use the custom operator specified in `registration`.
        /// Please refer https://www.tensorflow.org/lite/guide/ops_custom for custom op
        /// support.
        /// </summary>
        /// <remarks>
        /// The caller retains ownership of the <see cref="TfLiteRegistrationExternal"/> object
        /// and should ensure that it remains valid for the duration of any created
        /// interpreter's lifetime.
        /// 
        /// This is an experimental API and subject to change.
        /// </remarks>
        [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void TfLiteInterpreterOptionsAddRegistrationExternal(TfLiteInterpreterOptions* options, TfLiteRegistrationExternal* registration);
        /// <summary>
        /// Enables users to cancel in-flight invocations with
        /// `TfLiteInterpreterCancel`.
        ///
        /// By default it is disabled and calling to `TfLiteInterpreterCancel` will
        /// return kTfLiteError. See `TfLiteInterpreterCancel`.
        /// </summary>
        /// <remarks>
        /// This is an experimental API and subject to change.
        /// </remarks>
        [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern TfLiteStatus TfLiteInterpreterOptionsEnableCancellation(TfLiteInterpreterOptions* options, bool enable);
        /// <summary>
        /// Returns a new interpreter using the provided model and options, or null on
        /// failure.
        ///
        /// * `model` must be a valid model instance. The caller retains ownership of
        ///   the object, and may destroy it (via TfLiteModelDelete) immediately after
        ///   creating the interpreter.  However, if the TfLiteModel was allocated with
        ///   TfLiteModelCreate, then the `model_data` buffer that was passed to
        ///   TfLiteModelCreate must outlive the lifetime of the TfLiteInterpreter
        ///   object that this function returns, and must not be modified during that
        ///   time; and if the TfLiteModel was allocated with TfLiteModelCreateFromFile,
        ///   then the contents of the model file must not be modified during the
        ///   lifetime of the TfLiteInterpreter object that this function returns.
        /// * `optional_options` may be null. The caller retains ownership of the
        ///   object, and can safely destroy it (via TfLiteInterpreterOptionsDelete)
        ///   immediately after creating the interpreter.
        /// </summary>
        /// <remarks>
        /// The client *must* explicitly allocate tensors before attempting to
        /// access input tensor data or invoke the interpreter.
        /// </remarks>
        [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern TfLiteInterpreter* TfLiteInterpreterCreate(TfLiteModel* model, TfLiteInterpreterOptions* optional_options = null);
        /// <summary>
        /// Destroys the interpreter.
        /// </summary>
        /// <param name="interpreter"></param>
        [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void TfLiteInterpreterDelete(TfLiteInterpreter* interpreter);
        /// <summary>
        /// Returns the number of input tensors associated with the model.
        /// </summary>
        [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int TfLiteInterpreterGetInputTensorCount(TfLiteInterpreter* interpreter);
        /// <summary>
        /// Returns a pointer to an array of input tensor indices.  The length of the
        /// array can be obtained via a call to `TfLiteInterpreterGetInputTensorCount`.
        ///
        /// Typically the input tensors associated with an `interpreter` would be set
        /// during the initialization of the `interpreter`, through a mechanism like the
        /// `InterpreterBuilder`, and remain unchanged throughout the lifetime of the
        /// interpreter.  However, there are some circumstances in which the pointer may
        /// not remain valid throughout the lifetime of the interpreter, because calls
        /// to `SetInputs` on the interpreter invalidate the returned pointer.
        ///
        /// The ownership of the array remains with the TFLite runtime.
        /// </summary>
        [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint* TfLiteInterpreterInputTensorIndices(TfLiteInterpreter* interpreter);
        /// <summary>
        /// Returns the tensor associated with the input index.
        /// REQUIRES: 0 &lt;= input_index &lt; TfLiteInterpreterGetInputTensorCount(tensor)
        /// </summary>
        [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern TfLiteTensor* TfLiteInterpreterGetInputTensor(TfLiteInterpreter* interpreter, int input_index);
        /// <summary>
        /// Resizes the specified input tensor.
        /// </summary>
        /// <remarks>
        /// After a resize, the client *must* explicitly allocate tensors before
        /// attempting to access the resized tensor data or invoke the interpreter.
        ///
        /// REQUIRES: 0 &lt;= input_index &lt; TfLiteInterpreterGetInputTensorCount(tensor)
        ///
        /// This function makes a copy of the input dimensions, so the client can safely
        /// deallocate `input_dims` immediately after this function returns.
        /// </remarks>
        [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern TfLiteStatus TfLiteInterpreterResizeInputTensor(TfLiteInterpreter* interpreter, int input_index, nint* input_dims, int input_dims_size);
        /// <summary>
        /// Updates allocations for all tensors, resizing dependent tensors using the
        /// specified input tensor dimensionality.
        ///
        /// This is a relatively expensive operation, and need only be called after
        /// creating the graph and/or resizing any inputs.
        /// </summary>
        [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern TfLiteStatus TfLiteInterpreterAllocateTensors(TfLiteInterpreter* interpreter);
        /// <summary>
        /// Runs inference for the loaded graph.
        ///
        /// Before calling this function, the caller should first invoke
        /// TfLiteInterpreterAllocateTensors() and should also set the values for the
        /// input tensors.  After successfully calling this function, the values for the
        /// output tensors will be set.
        /// </summary>
        /// <remarks>
        /// It is possible that the interpreter is not in a ready state to
        /// evaluate (e.g., if AllocateTensors() hasn't been called, or if a
        /// ResizeInputTensor() has been performed without a subsequent call to
        /// AllocateTensors()).
        ///
        ///   If the (experimental!) delegate fallback option was enabled in the
        ///   interpreter options, then the interpreter will automatically fall back to
        ///   not using any delegates if execution with delegates fails. For details,
        ///   see TfLiteInterpreterOptionsSetEnableDelegateFallback in
        ///   c_api_experimental.h.
        ///
        /// Returns one of the following status codes:
        ///  - kTfLiteOk: Success. Output is valid.
        ///  - kTfLiteDelegateError: Execution with delegates failed, due to a problem
        ///    with the delegate(s). If fallback was not enabled, output is invalid.
        ///    If fallback was enabled, this return value indicates that fallback
        ///    succeeded, the output is valid, and all delegates previously applied to
        ///    the interpreter have been undone.
        ///  - kTfLiteApplicationError: Same as for kTfLiteDelegateError, except that
        ///    the problem was not with the delegate itself, but rather was
        ///    due to an incompatibility between the delegate(s) and the
        ///    interpreter or model.
        ///  - kTfLiteError: Unexpected/runtime failure. Output is invalid.
        /// </remarks>
        [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern TfLiteStatus TfLiteInterpreterInvoke(TfLiteInterpreter* interpreter);
        /// <summary>
        /// Returns the number of output tensors associated with the model.
        /// </summary>
        [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int TfLiteInterpreterGetOutputTensorCount(TfLiteInterpreter* interpreter);
        /// <summary>
        /// Returns a pointer to an array of output tensor indices.  The length of the
        /// array can be obtained via a call to `TfLiteInterpreterGetOutputTensorCount`.
        ///
        /// Typically the output tensors associated with an `interpreter` would be set
        /// during the initialization of the `interpreter`, through a mechanism like the
        /// `InterpreterBuilder`, and remain unchanged throughout the lifetime of the
        /// interpreter.  However, there are some circumstances in which the pointer may
        /// not remain valid throughout the lifetime of the interpreter, because calls
        /// to `SetOutputs` on the interpreter invalidate the returned pointer.
        ///
        /// The ownership of the array remains with the TFLite runtime.
        /// </summary>
        [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint* TfLiteInterpreterOutputTensorIndices(TfLiteInterpreter* interpreter);
        /// <summary>
        /// Returns the tensor associated with the output index.
        /// REQUIRES: 0 &lt;= output_index &lt; TfLiteInterpreterGetOutputTensorCount(tensor)
        ///
        /// \note The shape and underlying data buffer for output tensors may be not
        /// be available until after the output tensor has been both sized and
        /// allocated.
        /// In general, best practice is to interact with the output tensor *after*
        /// calling TfLiteInterpreterInvoke().
        /// </summary>
        [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern TfLiteTensor* TfLiteInterpreterGetOutputTensor(TfLiteInterpreter* interpreter, int output_index);
        /// <summary>
        /// Returns modifiable access to the tensor that corresponds to the
        /// specified `index` and is associated with the provided `interpreter`.
        ///
        /// This requires the `index` to be between 0 and N - 1, where N is the
        /// number of tensors in the model.
        ///
        /// Typically the tensors associated with the `interpreter` would be set during
        /// the `interpreter` initialization, through a mechanism like the
        /// `InterpreterBuilder`, and remain unchanged throughout the lifetime of the
        /// interpreter.  However, there are some circumstances in which the pointer may
        /// not remain valid throughout the lifetime of the interpreter, because calls
        /// to `AddTensors` on the interpreter invalidate the returned pointer.
        ///
        /// Note the difference between this function and
        /// `TfLiteInterpreterGetInputTensor` (or `TfLiteInterpreterGetOutputTensor` for
        /// that matter): `TfLiteInterpreterGetTensor` takes an index into the array of
        /// all tensors associated with the `interpreter`'s model, whereas
        /// `TfLiteInterpreterGetInputTensor` takes an index into the array of input
        /// tensors.
        ///
        /// The ownership of the tensor remains with the TFLite runtime, meaning the
        /// caller should not deallocate the pointer.
        /// </summary>
        [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern TfLiteTensor* TfLiteInterpreterGetTensor(TfLiteInterpreter* interpreter, nint index);
        /// <summary>
        /// Tries to cancel any in-flight invocation.
        /// </summary>
        /// <remarks>
        /// This only cancels `TfLiteInterpreterInvoke` calls that happen before
        /// calling this and it does not cancel subsequent invocations.
        /// \note Calling this function will also cancel any in-flight invocations of
        /// SignatureRunners constructed from this interpreter.
        /// Non-blocking and thread safe.
        ///
        /// Returns kTfLiteError if cancellation is not enabled via
        /// `TfLiteInterpreterOptionsEnableCancellation`.
        ///
        /// This is an experimental API and subject to change.
        /// </remarks>
        [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern TfLiteStatus TfLiteInterpreterCancel(TfLiteInterpreter* interpreter);
        /// <summary>
        /// Returns the type of a tensor element.
        /// </summary>
        [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern TfLiteType TfLiteTensorType(TfLiteTensor* tensor);
        /// <summary>
        /// Returns the length of the tensor in the "dim_index" dimension.
        /// REQUIRES: 0 &lt;= dim_index &lt; TFLiteTensorNumDims(tensor)
        /// </summary>
        [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int TfLiteTensorNumDims(TfLiteTensor* tensor);
        /// <summary>
        /// Returns the size of the underlying data in bytes.
        /// </summary>
        [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern nuint TfLiteTensorByteSize(TfLiteTensor* tensor);
        /// <summary>
        /// Returns a pointer to the underlying data buffer.
        /// </summary>
        /// <remarks>
        /// The result may be null if tensors have not yet been allocated, e.g.,
        /// if the Tensor has just been created or resized and `TfLiteAllocateTensors()`
        /// has yet to be called, or if the output tensor is dynamically sized and the
        /// interpreter hasn't been invoked.
        /// </remarks>
        [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void* TfLiteTensorData(TfLiteTensor* tensor);
        /// <summary>
        /// Returns the (null-terminated) name of the tensor.
        /// </summary>
        [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte* TfLiteTensorName(TfLiteTensor* tensor);
        /// <summary>
        /// Copies from the provided input buffer into the tensor's buffer.
        /// REQUIRES: input_data_size == TfLiteTensorByteSize(tensor)
        /// </summary>
        [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern TfLiteStatus TfLiteTensorCopyFromBuffer(TfLiteTensor* tensor, void* input_data, nuint input_data_size);
        /// <summary>
        /// Copies to the provided output buffer from the tensor's buffer.
        /// REQUIRES: output_data_size == TfLiteTensorByteSize(tensor)
        /// </summary>
        [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern TfLiteStatus TfLiteTensorCopyToBuffer(TfLiteTensor* output_tensor, void* output_data, nuint output_data_size);
        /// <summary>
        /// Returns a new TfLiteRegistrationExternal instance.
        /// </summary>
        /// <remarks>
        /// The caller retains ownership and should ensure that
        /// the lifetime of the `TfLiteRegistrationExternal` must be at least as long as
        /// the lifetime of the `TfLiteInterpreter`.
        /// 
        /// This is an experimental API and subject to change.
        /// </remarks>
        [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern TfLiteRegistrationExternal* TfLiteRegistrationExternalCreate(TfLiteBuiltinOperator builtin_code, byte* custom_name, nint version);
        /// <summary>
        /// Return the builtin op code of the provided external 'registration'.
        /// </summary>
        /// <remarks>
        /// This is an experimental API and subject to change.
        /// </remarks>
        [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern TfLiteBuiltinOperator TfLiteRegistrationExternalGetBuiltInCode(TfLiteRegistrationExternal* registration);
        /// <summary>
        /// Return the OP version of the provided external 'registration'.  Return -1
        /// in case of error, or if the provided address is null.
        /// </summary>
        /// <remarks>
        /// This is an experimental API and subject to change.
        /// </remarks>
        [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern nint TfLiteRegistrationExternalGetVersion(TfLiteRegistrationExternal* registration);
        /// <summary>
        /// Returns the custom name of the provided 'registration'. The returned pointer
        /// will be non-null iff the op is a custom op.
        /// </summary>
        /// <remarks>
        /// This is an experimental API and subject to change.
        /// </remarks>
        [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte* TfLiteRegistrationExternalGetCustomName(TfLiteRegistrationExternal* registration);
        /// <summary>
        /// Destroys the TfLiteRegistrationExternal instance.
        /// </summary>
        /// <remarks>
        /// This is an experimental API and subject to change.
        /// </remarks>
        [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void TfLiteRegistrationExternalDelete(TfLiteRegistrationExternal* registration);
        /// <summary>
        /// Sets the initialization callback for the registration.
        ///
        /// The callback is called to initialize the op from serialized data.
        /// Please refer `init` of `TfLiteRegistration` for the detail.
        /// </summary>
        /// <remarks>
        /// This is an experimental API and subject to change.
        /// </remarks>
        [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void TfLiteRegistrationExternalSetInit(TfLiteRegistrationExternal* registration, delegate*<TfLiteOpaqueContext*, byte*, nuint, void*> init);
        /// <summary>
        /// Sets the deallocation callback for the registration.
        ///
        /// This callback is called to deallocate the data returned by the init
        /// callback. The value passed in the `data` parameter is the value that was
        /// returned by the `init` callback.
        /// Please refer `free` of `TfLiteRegistration` for the detail.
        /// </summary>
        /// <remarks>
        /// This is an experimental API and subject to change.
        /// </remarks>
        [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void TfLiteRegistrationExternalSetFree(TfLiteRegistrationExternal* registration, delegate*<TfLiteOpaqueContext*, void*, void> free);
        /// <summary>
        /// Sets the preparation callback for the registration.
        ///
        /// The callback is called when the inputs of operator have been resized.
        /// Please refer `prepare` of `TfLiteRegistration` for the detail.
        /// </summary>
        /// <remarks>
        /// This is an experimental API and subject to change.
        /// </remarks>
        [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void TfLiteRegistrationExternalSetPrepare(TfLiteRegistrationExternal* registration, delegate*<TfLiteOpaqueContext*, TfLiteOpaqueNode*, TfLiteStatus> prepare);
        /// <summary>
        /// Sets the invocation callback for the registration.
        ///
        /// The callback is called when the operator is executed.
        /// Please refer `invoke` of `TfLiteRegistration` for the detail.
        /// </summary>
        /// <remarks>
        /// This is an experimental API and subject to change.
        /// </remarks>
        [DllImport(TensorFlowLiteLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void TfLiteRegistrationExternalSetInvoke(TfLiteRegistrationExternal* registration, delegate*<TfLiteOpaqueContext*, TfLiteOpaqueNode*, TfLiteStatus> invoke);
    }
}