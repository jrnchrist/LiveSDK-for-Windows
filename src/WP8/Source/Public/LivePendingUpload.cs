﻿namespace Microsoft.Live
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Live.Phone;
    using Microsoft.Phone.BackgroundTransfer;

    public class LivePendingUpload
    {
        #region Instance member variables

        private readonly IBackgroundTransferService backgroundTransferService;
        private readonly BackgroundTransferRequest request;

        #endregion

        #region Constructors

        internal LivePendingUpload(
            IBackgroundTransferService backgroundTransferService,
            BackgroundTransferRequest request)
        {
            Debug.Assert(backgroundTransferService != null);
            Debug.Assert(request != null);
            Debug.Assert(BackgroundTransferHelper.IsUploadRequest(request));

            this.backgroundTransferService = backgroundTransferService;
            this.request = request;
        }

        #endregion

        #region Public members

        /// <summary>
        /// Attaches to the upload operation and receive the result of the operation when it is finished.
        /// </summary>
        /// <returns>A Task object representing the asynchronous operation.</returns>
        public Task<LiveOperationResult> AttachAsync()
        {
            return this.AttachAsync(new CancellationToken(false), null);
        }

        /// <summary>
        /// Attaches to the upload operation and receive the result of the operation when it is finished.
        /// </summary>
        /// <param name="ct">a token that is used to cancel the background upload operation.</param>
        /// <param name="progress">an object that is called to report the background upload's progress.</param>
        /// <returns>A Task object representing the asynchronous operation.</returns>
        public Task<LiveOperationResult> AttachAsync(CancellationToken ct, IProgress<LiveOperationProgress> progress)
        {
            var tcs = new TaskCompletionSource<LiveOperationResult>();

            ct.Register(() =>
            {
                // Remove from the service to cancel.
                this.backgroundTransferService.Remove(this.request);
                tcs.TrySetCanceled();
            });

            var uploadEventHandler = new BackgroundUploadEventAdapter(this.backgroundTransferService, tcs);
            return uploadEventHandler.ConvertTransferStatusChangedToTask(this.request, progress);
        }

        #endregion
    }
}
