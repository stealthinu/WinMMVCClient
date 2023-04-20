using System;

namespace TensorFlowLite
{
    /// <remarks>
    /// Note that new error status values may be added in future in order to
    /// indicate more fine-grained internal states, therefore, applications should
    /// not rely on status values being members of the enum.
    /// </remarks>
    public enum TfLiteStatus
    {
        Ok = 0,
        /// <summary>
        /// Generally referring to an error in the runtime (i.e. interpreter)
        /// </summary>
        Error = 1,
        /// <summary>
        /// Generally referring to an error from a TfLiteDelegate itself.
        /// </summary>
        DelegateError = 2,
        /// <summary>
        /// Generally referring to an error in applying a delegate due to
        /// incompatibility between runtime and delegate, e.g., this error is returned
        /// when trying to apply a TF Lite delegate onto a model graph that's already
        /// immutable.
        /// </summary>
        ApplicationError = 3,
        /// <summary>
        /// Generally referring to serialized delegate data not being found.
        /// See tflite::delegates::Serialization.
        /// </summary>
		DelegateDataNotFound = 4,
        /// <summary>
        /// Generally referring to data-writing issues in delegate serialization.
        /// See tflite::delegates::Serialization.
        /// </summary>
		DelegateErrorWriteError = 5,
        /// <summary>
        /// Generally referring to data-reading issues in delegate serialization.
        /// See tflite::delegates::Serialization.
        /// </summary>
		DelegateDataReadError = 6,
        /// <summary>
        /// Generally referring to issues when the TF Lite model has ops that cannot be
        /// resolved at runtime. This could happen when the specific op is not
        /// registered or built with the TF Lite framework.
        /// </summary>
        UnresolvedOps = 7,
        /// <summary>
        /// Generally referring to invocation cancelled by the user.
        /// See `interpreter::Cancel`.
        /// TODO(b/194915839): Implement `interpreter::Cancel`.
        /// TODO(b/250636993): Cancellation triggered by `SetCancellationFunction`
        /// should also return this status code.
        /// </summary>
        Cancelled = 8,
    }

    public static class TfLiteStatusExtensions
    {
        public static void ThrowExceptionForStatus(this TfLiteStatus status)
        {
            switch (status)
            {
                case TfLiteStatus.Ok:
                    return;
                case TfLiteStatus.Cancelled:
                    throw new OperationCanceledException();
                default:
                    throw new Exception(status.ToString());
            }
        }
    }
}
