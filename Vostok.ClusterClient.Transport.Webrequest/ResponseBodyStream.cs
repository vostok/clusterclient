using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Vostok.ClusterClient.Transport.Webrequest
{
    internal class ResponseBodyStream : Stream
    {
        private readonly WebRequestState state;

        public ResponseBodyStream(WebRequestState state)
        {
            this.state = state;
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override bool CanTimeout => false;

        public override int Read(byte[] buffer, int offset, int count) =>
            state.ResponseStream.Read(buffer, offset, count);

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
            state.ResponseStream.ReadAsync(buffer, offset, count, cancellationToken);

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state) =>
            this.state.ResponseStream.BeginRead(buffer, offset, count, callback, state);

        public override int EndRead(IAsyncResult asyncResult) =>
            state.ResponseStream.EndRead(asyncResult);

        public override int ReadByte() => state.ResponseStream.ReadByte();

        protected override void Dispose(bool disposing) => state.Dispose();

        #region Not supported

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin) =>
            throw new NotSupportedException();

        public override void SetLength(long value) =>
            throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) =>
            throw new NotSupportedException();

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state) =>
            throw new NotSupportedException();

        public override void EndWrite(IAsyncResult asyncResult) =>
            throw new NotSupportedException();

        public override void WriteByte(byte value) =>
            throw new NotSupportedException();

        public override void Flush() =>
            throw new NotSupportedException();

        public override Task FlushAsync(CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        #endregion
    }
}