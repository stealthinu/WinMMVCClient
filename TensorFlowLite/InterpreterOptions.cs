using System;
using TensorFlowLite.Delegates;
using TensorFlowLite.Native;
using static TensorFlowLite.Native.CApi;
using static TensorFlowLite.Native.CApiExperimental;
namespace TensorFlowLite
{
    /// <summary>
    /// <see cref="InterpreterOptions"/> allows customized interpreter configuration.
    /// </summary>
    public unsafe struct InterpreterOptions : IDisposable
    {
        static InterpreterOptions()
        {
            var options = Create();
#if UNITY_ANDROID && !UNITY_EDITOR
            options.AddDelegate(NnapiDelegate.Default);
#elif UNITY_2022_2_OR_NEWER // TODO: Somehow XNNPackDelegate doesn't work on Pure C# project
            options.AddDelegate(XNNPackDelegate.Default);
#endif
            Default = options;

            options = Create();
        }
        public static InterpreterOptions Default { get; }

        internal TfLiteInterpreterOptions* tfLiteInterpreterOptions;
        /// <summary>
        /// Returns a new interpreter options instances.
        /// </summary>
        /// <returns></returns>
        public static InterpreterOptions Create() =>
            new InterpreterOptions
            {
                tfLiteInterpreterOptions = TfLiteInterpreterOptionsCreate()
            };
        /// <summary>
        /// Creates and returns a shallow copy of an options object.
        /// </summary>
        /// <param name="interpreterOptions"></param>
        public InterpreterOptions(InterpreterOptions interpreterOptions) =>
            tfLiteInterpreterOptions = TfLiteInterpreterOptionsCopy(interpreterOptions.tfLiteInterpreterOptions);

        /// <summary>
        /// Sets the number of CPU threads to use for the interpreter.
        /// </summary>
        public readonly void SetThreadCount(int threadCount) => TfLiteInterpreterOptionsSetNumThreads(tfLiteInterpreterOptions, threadCount);
        public readonly void AddDelegate<T>(T @delegate) where T: IDelegate => TfLiteInterpreterOptionsAddDelegate(tfLiteInterpreterOptions, @delegate.TfLiteDelegate); 
        public readonly void SetUseNNAPI(bool enable) => TfLiteInterpreterOptionsSetUseNNAPI(tfLiteInterpreterOptions, enable);
        public readonly void SetEnableDelegateFallback(bool enable) => TfLiteInterpreterOptionsSetEnableDelegateFallback(tfLiteInterpreterOptions, enable);
        public void Dispose()
        {
            if (tfLiteInterpreterOptions == null)
            {
                return;
            }
            TfLiteInterpreterOptionsDelete(tfLiteInterpreterOptions);
            tfLiteInterpreterOptions = null;
        }

    }
}
