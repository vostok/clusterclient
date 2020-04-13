using JetBrains.Annotations;

namespace Vostok.Clusterclient.Core.Model
{
    /// <summary>
    /// A set of query parameters-related extensions for <see cref="Request"/>.
    /// </summary>
    [PublicAPI]
    public static class RequestQueryExtensions
    {
        /// <summary>
        /// <para>Produces a new <see cref="Request"/> instance with a new query parameter in url.</para>
        /// <para>If a query parameter with same name already exists in url, it's not replaced. Two parameters will be present instead.</para>
        /// <para>See <see cref="Request.WithUrl(System.Uri)"/> method documentation for more details.</para>
        /// </summary>
        /// <param name="request">Original request.</param>
        /// <param name="key">Parameter name.</param>
        /// <param name="value">Parameter value.</param>
        /// <param name="allowEmptyValue">Whether to allow empty values or not.</param>
        /// <returns>A new <see cref="Request"/> object with updated url.</returns>
        [Pure]
        [NotNull]
        public static Request WithAdditionalQueryParameter([NotNull] this Request request, [CanBeNull] string key, [CanBeNull] string value, bool allowEmptyValue)
        {
            var newUrl = new RequestUrlBuilder(request.Url.ToString())
                .AppendToQuery(key, value, allowEmptyValue)
                .Build();

            return request.WithUrl(newUrl);
        }

        /// <inheritdoc cref="WithAdditionalQueryParameter(Request,string,string,bool)"/>
        [Pure]
        [NotNull]
        public static Request WithAdditionalQueryParameter([NotNull] this Request request, [CanBeNull] string key, [CanBeNull] string value)
            => WithAdditionalQueryParameter(request, key, value, false);

        /// <summary>
        /// <para>Produces a new <see cref="Request"/> instance with a new query parameter in url.</para>
        /// <para>If a query parameter with same name already exists in url, it's not replaced. Two parameters will be present instead.</para>
        /// <para>See <see cref="Request.WithUrl(System.Uri)"/> method documentation for more details.</para>
        /// </summary>
        /// <param name="request">Original request.</param>
        /// <param name="key">Parameter name.</param>
        /// <param name="value">Parameter value. ToString() is used to obtain string value.</param>
        /// <param name="allowEmptyValue">Whether to allow empty values or not.</param>
        /// <returns>A new <see cref="Request"/> object with updated url.</returns>
        [Pure]
        [NotNull]
        public static Request WithAdditionalQueryParameter<T>([NotNull] this Request request, [CanBeNull] string key, [CanBeNull] T value, bool allowEmptyValue)
        {
            var newUrl = new RequestUrlBuilder(request.Url.ToString())
                .AppendToQuery(key, value, allowEmptyValue)
                .Build();

            return request.WithUrl(newUrl);
        }

        /// <inheritdoc cref="WithAdditionalQueryParameter{T}(Request,string,T,bool)"/>
        [Pure]
        [NotNull]
        public static Request WithAdditionalQueryParameter<T>([NotNull] this Request request, [CanBeNull] string key, [CanBeNull] T value)
            => WithAdditionalQueryParameter(request, key, value, false);

    }
}