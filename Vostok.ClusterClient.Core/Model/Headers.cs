using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using JetBrains.Annotations;
using Vostok.Commons.Collections;

namespace Vostok.Clusterclient.Core.Model
{
    /// <summary>
    /// <para>Represents a collection of HTTP headers (string key-value pairs).</para>
    /// <para>Every <see cref="Headers"/> object is effectively immutable. Any modifications made via <see cref="Set"/> method produce a new object.</para>
    /// </summary>
    [PublicAPI]
    public class Headers : IEnumerable<Header>
    {
        /// <summary>
        /// Represents an empty <see cref="Headers"/> object. Useful to start building headers from scratch.
        /// </summary>
        public static readonly Headers Empty = new Headers(0);

        [NotNull]
        private readonly IReadOnlyDictionary<string, string> headers;

        /// <param name="capacity">Initial capacity of headers collection.</param>
        public Headers(int capacity)
            : this(new ImmutableArrayDictionary<string, string>(capacity, StringComparer.OrdinalIgnoreCase))
        {
        }

        public Headers([CanBeNull] IReadOnlyDictionary<string, string> headers)
            => this.headers = headers ?? ImmutableArrayDictionary<string, string>.Empty;

        /// <summary>
        /// Returns the count of headers in this <see cref="Headers"/> object.
        /// </summary>
        public int Count => headers.Count;

        /// <summary>
        /// Returns the names of all headers contained in this <see cref="Headers"/> object.
        /// </summary>
        [NotNull]
        public IEnumerable<string> Names => headers.Select(pair => pair.Key);

        /// <summary>
        /// Returns the values of all headers contained in this <see cref="Headers"/> object.
        /// </summary>
        [NotNull]
        public IEnumerable<string> Values => headers.Select(header => header.Value);

        /// <summary>
        /// <para>Produces a new <see cref="Headers"/> instance where the header with given name will have given value.</para>
        /// <para>If the header does not exist in current instance, it will be added to new one.</para>
        /// <para>If the header exists in current instance, it will be overwritten in new one.</para>
        /// <para>Current instance is not modified in any case.</para>
        /// </summary>
        /// <param name="name">Header name.</param>
        /// <param name="value">Header value. ToString() is used to obtain string value.</param>
        /// <returns>A new <see cref="Headers"/> object with updated header value.</returns>
        [Pure]
        [NotNull]
        public Headers Set<T>([NotNull] string name, [NotNull] T value)
        {
            return Set(name, value.ToString());
        }

        /// <summary>
        /// <para>Produces a new <see cref="Headers"/> instance where the header with given name will have given value.</para>
        /// <para>If the header does not exist in current instance, it will be added to new one.</para>
        /// <para>If the header exists in current instance, it will be overwritten in new one.</para>
        /// <para>Current instance is not modified in any case.</para>
        /// </summary>
        /// <param name="name">Header name.</param>
        /// <param name="value">Header value.</param>
        /// <returns>A new <see cref="Headers"/> object with updated header value.</returns>
        [Pure]
        [NotNull]
        public Headers Set([NotNull] string name, [NotNull] string value)
        {
            var newHeaders = PrepareForMutation().Set(name, value);

            return ReferenceEquals(headers, newHeaders)
                ? this
                : new Headers(newHeaders);
        }

        /// <summary>
        /// <para>Produces a new <see cref="Headers"/> instance where the header with given name will be removed.</para>
        /// <para>If the header does not exist in current instance, the same <see cref="Headers"/> object will be returned instead.</para>
        /// <para>Current instance is not modified in any case.</para>
        /// </summary>
        /// <param name="name">Header name.</param>
        /// <returns>A new <see cref="Headers"/> object without a header with given name.</returns>
        [Pure]
        [NotNull]
        public Headers Remove([NotNull] string name)
        {
            var newHeaders = PrepareForMutation().Remove(name);

            return ReferenceEquals(headers, newHeaders)
                ? this
                : new Headers(newHeaders);
        }

        /// <returns>
        /// <para>Headers string representation in the following format:</para>
        /// <para>Name1: Value1</para>
        /// <para>Name2: Value2</para>
        /// <para>...</para>
        /// </returns>
        public override string ToString()
        {
            if (headers.Count == 0)
                return string.Empty;

            var builder = new StringBuilder();

            var firstIteration = true;

            foreach (var header in this)
            {
                if (firstIteration)
                    firstIteration = false;
                else
                    builder.AppendLine();

                builder.Append(header.Name);
                builder.Append(": ");
                builder.Append(header.Value);
            }

            return builder.ToString();
        }

        /// <inheritdoc />
        public IEnumerator<Header> GetEnumerator()
        {
            foreach (var pair in headers)
            {
                yield return new Header(pair.Key, pair.Value);
            }
        }

        /// <summary>
        /// Attempts to fetch the value of header with given name.
        /// </summary>
        /// <param name="name">Header name.</param>
        /// <returns>Header value if found, <c>null</c> otherwise.</returns>
        [CanBeNull]
        public string this[string name] => headers.TryGetValue(name, out var v) ? v : null;

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ImmutableArrayDictionary<string, string> PrepareForMutation()
            => headers as ImmutableArrayDictionary<string, string> ?? new ImmutableArrayDictionary<string, string>(headers);

        #region Specific header getters

        /// <summary>
        /// Returns the value of <see cref="HeaderNames.Accept"/> header or <c>null</c> if it's not specified.
        /// </summary>
        [CanBeNull]
        public string Accept => this[HeaderNames.Accept];

        /// <summary>
        /// Returns the value of <see cref="HeaderNames.Age"/> header or <c>null</c> if it's not specified.
        /// </summary>
        [CanBeNull]
        public string Age => this[HeaderNames.Age];

        /// <summary>
        /// Returns the value of <see cref="HeaderNames.Authorization"/> header or <c>null</c> if it's not specified.
        /// </summary>
        [CanBeNull]
        public string Authorization => this[HeaderNames.Authorization];

        /// <summary>
        /// Returns the value of <see cref="HeaderNames.ContentEncoding"/> header or <c>null</c> if it's not specified.
        /// </summary>
        [CanBeNull]
        public string ContentEncoding => this[HeaderNames.ContentEncoding];

        /// <summary>
        /// Returns the value of <see cref="HeaderNames.ContentLength"/> header or <c>null</c> if it's not specified.
        /// </summary>
        [CanBeNull]
        public string ContentLength => this[HeaderNames.ContentLength];

        /// <summary>
        /// Returns the value of <see cref="HeaderNames.ContentType"/> header or <c>null</c> if it's not specified.
        /// </summary>
        [CanBeNull]
        public string ContentType => this[HeaderNames.ContentType];

        /// <summary>
        /// Returns the value of <see cref="HeaderNames.ContentRange"/> header or <c>null</c> if it's not specified.
        /// </summary>
        [CanBeNull]
        public string ContentRange => this[HeaderNames.ContentRange];

        /// <summary>
        /// Returns the value of <see cref="HeaderNames.Date"/> header or <c>null</c> if it's not specified.
        /// </summary>
        [CanBeNull]
        public string Date => this[HeaderNames.Date];

        /// <summary>
        /// Returns the value of <see cref="HeaderNames.ETag"/> header or <c>null</c> if it's not specified.
        /// </summary>
        [CanBeNull]
        public string ETag => this[HeaderNames.ETag];

        /// <summary>
        /// Returns the value of <see cref="HeaderNames.Host"/> header or <c>null</c> if it's not specified.
        /// </summary>
        [CanBeNull]
        public string Host => this[HeaderNames.Host];

        /// <summary>
        /// Returns the value of <see cref="HeaderNames.LastModified"/> header or <c>null</c> if it's not specified.
        /// </summary>
        [CanBeNull]
        public string LastModified => this[HeaderNames.LastModified];

        /// <summary>
        /// Returns the value of <see cref="HeaderNames.Location"/> header or <c>null</c> if it's not specified.
        /// </summary>
        [CanBeNull]
        public string Location => this[HeaderNames.Location];

        /// <summary>
        /// Returns the value of <see cref="HeaderNames.Range"/> header or <c>null</c> if it's not specified.
        /// </summary>
        [CanBeNull]
        public string Range => this[HeaderNames.Range];

        /// <summary>
        /// Returns the value of <see cref="HeaderNames.Referer"/> header or <c>null</c> if it's not specified.
        /// </summary>
        [CanBeNull]
        public string Referer => this[HeaderNames.Referer];

        /// <summary>
        /// Returns the value of <see cref="HeaderNames.RetryAfter"/> header or <c>null</c> if it's not specified.
        /// </summary>
        [CanBeNull]
        public string RetryAfter => this[HeaderNames.RetryAfter];

        /// <summary>
        /// Returns the value of <see cref="HeaderNames.Server"/> header or <c>null</c> if it's not specified.
        /// </summary>
        [CanBeNull]
        public string Server => this[HeaderNames.Server];

        /// <summary>
        /// Returns the value of <see cref="HeaderNames.TransferEncoding"/> header or <c>null</c> if it's not specified.
        /// </summary>
        [CanBeNull]
        public string TransferEncoding => this[HeaderNames.TransferEncoding];

        /// <summary>
        /// Returns the value of <see cref="HeaderNames.UserAgent"/> header or <c>null</c> if it's not specified.
        /// </summary>
        [CanBeNull]
        public string UserAgent => this[HeaderNames.UserAgent];

        /// <summary>
        /// Returns the value of <see cref="HeaderNames.WWWAuthenticate"/> header or <c>null</c> if it's not specified.
        /// </summary>
        [CanBeNull]
        // ReSharper disable once InconsistentNaming
        public string WWWAuthenticate => this[HeaderNames.WWWAuthenticate];

        #endregion
    }
}
