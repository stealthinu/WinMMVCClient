using System;
using System.Runtime.InteropServices;
using TensorFlowLite.Native;
using static TensorFlowLite.Native.CApi;
namespace TensorFlowLite
{
    /// <summary>
    /// Wraps a loaded TensorFlow Lite model.
    /// </summary>
    public unsafe struct Model : IDisposable
    {
        internal TfLiteModel* tfLiteModel;
        byte* modelData;
        /// <summary>
        /// Create a model from the provided buffer.
        /// </summary>
        /// <param name="modelData"></param>
        public Model(ReadOnlySpan<byte> modelData)
        {
            this.modelData = (byte*)Marshal.AllocHGlobal(modelData.Length);
            modelData.CopyTo(new Span<byte>(this.modelData, modelData.Length));
            tfLiteModel = TfLiteModelCreate(this.modelData, (nuint)modelData.Length);
            ThrowIfModelCreationFailed();
        }
        /// <summary>
        /// Create a model from the provided file.
        /// </summary>
        /// <remarks>
        /// The file's contents must not be modified during the lifetime of the
        /// <see cref="Model"/> or of any <see cref="Interpreter"/> objects created from that
        /// <see cref="Model"/>.
        /// </remarks>
        public Model(string modelPath)
        {
            var ptr = Marshal.StringToHGlobalAnsi(modelPath);
            tfLiteModel = TfLiteModelCreateFromFile((byte*)ptr);
            Marshal.FreeHGlobal(ptr);
            modelData = null;
            ThrowIfModelCreationFailed();
        }
        private readonly void ThrowIfModelCreationFailed()
        {
            if (tfLiteModel == null)
            {
                Throw();
                static void Throw()
                {
                    throw new Exception($"Failed to create model.");
                }
            }
        }
        public void Dispose()
        {
            if (tfLiteModel == null)
            {
                return;
            }
            TfLiteModelDelete(tfLiteModel);
            tfLiteModel = null;
            if (modelData != null)
            {
                Marshal.FreeHGlobal((IntPtr)modelData);
            }
        }
    }
}
