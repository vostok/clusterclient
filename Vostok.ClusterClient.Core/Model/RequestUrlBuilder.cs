using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using JetBrains.Annotations;
using Vostok.Commons.Collections;

namespace Vostok.Clusterclient.Core.Model
{
    /// <summary>
    /// <para>Represents an efficient builder of request urls.</para>
    /// <para>Supports collection initializer syntax:</para>
    /// <list type="bullet">
    /// <item><description>You can add string or object which are treated as path segments.</description></item>
    /// <item><description>You can add key-value pairs which are treated as query parameters.</description></item>
    /// </list>
    /// </summary>
    /// <example>
    /// <code>
    /// var url = new RequestUrlBuilder
    /// {
    ///     "foo", "bar", "baz",
    ///     { "key", "value" }
    /// }
    /// .Build();
    /// </code>
    /// This creates following url: <c>foo/bar/baz?key=value</c>
    /// </example>
    [PublicAPI]
    public class RequestUrlBuilder : IDisposable, IEnumerable
    {
        private static readonly Func<string, string> Escape = Uri.EscapeDataString;
        private static readonly UnboundedObjectPool<StringBuilder> Builders;

        private StringBuilder builder;
        private bool hasQueryParameters;
        private Uri result;

        static RequestUrlBuilder()
        {
            Builders = new UnboundedObjectPool<StringBuilder>(() => new StringBuilder(128));
        }

        /// <param name="initialUrl">The initial Url.</param>
        /// <exception cref="ArgumentNullException"><paramref name="initialUrl"/> is <c>null</c>.</exception>
        public RequestUrlBuilder([NotNull] string initialUrl = "")
        {
            if (initialUrl == null)
                throw new ArgumentNullException(nameof(initialUrl));

            builder = Builders.Acquire();
            builder.Clear();
            builder.Append(initialUrl);

            hasQueryParameters = initialUrl.IndexOf("?", StringComparison.Ordinal) >= 0;
        }

        /// <summary>
        /// Check that builder instance is disposed.
        /// </summary>
        public bool IsDisposed => builder == null;

        /// <summary>
        /// Releases underlying buffer.
        /// </summary>
        public void Dispose()
        {
            var oldBuilder = Interlocked.Exchange(ref builder, null);
            if (oldBuilder == null)
                return;

            Builders.Return(oldBuilder);
        }

        /// <summary>
        /// Builds a final <see cref="Uri"/> and disposes this builder instance. No further actions are possible after <see cref="Build"/> call.
        /// </summary>
        [NotNull]
        public Uri Build()
        {
            if (result != null)
                return result;

            using (this)
            {
                var uriString = builder.ToString();
                var uriKind = uriString.StartsWith("/") ? UriKind.Relative : UriKind.RelativeOrAbsolute;
                return result = new Uri(uriString, uriKind);
            }
        }

        /// <summary>
        /// <para>Appends a new path <paramref name="segment"/>. <see cref="object.ToString"/> is called on <paramref name="segment"/>.</para>
        /// <para>Note that it's not possible to append to path when url already has some query parameters.</para>
        /// </summary>
        [NotNull]
        public RequestUrlBuilder AppendToPath<T>([CanBeNull] T segment)
        {
            return AppendToPath(FormatValue(segment));
        }

        /// <summary>
        /// <para>Appends a new path <paramref name="segment"/>.</para>
        /// <para>Note that it's not possible to append to path when url already has some query parameters.</para>
        /// </summary>
        [NotNull]
        public RequestUrlBuilder AppendToPath([CanBeNull] string segment)
        {
            EnsureNotDisposed();
            EnsureQueryNotStarted();

            if (string.IsNullOrEmpty(segment))
                return this;

            if (segment.StartsWith("/"))
            {
                if (builder.Length > 0 && builder[builder.Length - 1] == '/')
                    builder.Remove(builder.Length - 1, 1);
            }
            else
            {
                if (builder.Length > 0 && builder[builder.Length - 1] != '/')
                    builder.Append('/');
            }

            builder.Append(segment);

            return this;
        }

        /// <inheritdoc cref="AppendToQuery{T}(string,T,bool)"/>
        [NotNull]
        public RequestUrlBuilder AppendToQuery<T>([CanBeNull] string key, [CanBeNull] T value)
            => AppendToQuery(key, FormatValue(value), false);

        /// <summary>
        /// <para>Appends a new query parameter with given <paramref name="key"/> and <paramref name="value"/>. <see cref="object.ToString"/> is called on <paramref name="value"/>.</para>
        /// <para><paramref name="key"/> and <paramref name="value"/> are encoded using percent-encoding.</para>
        /// </summary>
        [NotNull]
        public RequestUrlBuilder AppendToQuery<T>([CanBeNull] string key, [CanBeNull] T value, bool allowEmptyValue)
            => AppendToQuery(key, FormatValue(value), allowEmptyValue);

        /// <inheritdoc cref="AppendToQuery(string,string,bool)"/>
        [NotNull]
        public RequestUrlBuilder AppendToQuery([CanBeNull] string key, [CanBeNull] string value)
            => AppendToQuery(key, value, false);

        /// <summary>
        /// <para>Appends a new query parameter with given <paramref name="key"/> and <paramref name="value"/>.</para>
        /// <para><paramref name="key"/> and <paramref name="value"/> are encoded using percent-encoding.</para>
        /// </summary>
        [NotNull]
        public RequestUrlBuilder AppendToQuery([CanBeNull] string key, [CanBeNull] string value, bool allowEmptyValue)
        {
            EnsureNotDisposed();

            if (string.IsNullOrEmpty(key) || value == null || !allowEmptyValue && string.IsNullOrEmpty(value))
                return this;

            if (hasQueryParameters)
            {
                builder.Append('&');
            }
            else
            {
                builder.Append('?');
                hasQueryParameters = true;
            }

            builder.Append(Escape(key));
            builder.Append('=');
            builder.Append(Escape(value));

            return this;
        }

        /// <summary>
        /// Same as <see cref="AppendToPath"/>. Needed for collection initializer syntax.
        /// </summary>
        public void Add([CanBeNull] string segment)
        {
            AppendToPath(segment);
        }

        /// <summary>
        /// Same as <see cref="AppendToPath{T}"/>. Needed for collection initializer syntax.
        /// </summary>
        public void Add<T>([CanBeNull] T segment)
        {
            AppendToPath(segment);
        }

        /// <summary>
        /// Same as <see cref="AppendToQuery(string,string)"/>. Needed for collection initializer syntax.
        /// </summary>
        public void Add([CanBeNull] string key, [CanBeNull] string value)
        {
            AppendToQuery(key, value);
        }

        /// <summary>
        /// Same as <see cref="AppendToQuery{T}(string,T)"/>. Needed for collection initializer syntax.
        /// </summary>
        public void Add<T>([CanBeNull] string key, [CanBeNull] T value)
        {
            AppendToQuery(key, value);
        }

        /// <summary>
        /// Throws if builder is disposed.
        /// </summary>
        protected void EnsureNotDisposed()
        {
            if (builder == null)
                throw new ObjectDisposedException(nameof(builder), "Can not reuse a builder which already built an url.");
        }

        /// <summary>
        /// Throws if request url already has query parameters
        /// </summary>
        protected void EnsureQueryNotStarted()
        {
            if (hasQueryParameters)
                throw new InvalidOperationException("Can not append to path after appending query parameters.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string FormatValue<T>(T value)
        {
            return value?.ToString();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotSupportedException();
        }
    }
}