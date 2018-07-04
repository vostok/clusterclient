using System;
using System.IO;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.ClusterClient.Core.Model;
using Vostok.ClusterClient.Core.Tests.Helpers;

// ReSharper disable ObjectCreationAsStatement

namespace Vostok.ClusterClient.Core.Tests.Model
{
    [TestFixture]
    internal class StreamContent_Tests
    {
        [Test]
        public void Should_not_allow_null_streams()
        {
            Action action = () => new StreamContent(null);

            action.Should().ThrowExactly<ArgumentNullException>().Which.ShouldBePrinted();
        }

        [Test]
        public void Should_not_allow_unreadable_streams()
        {
            var stream = Substitute.For<Stream>();

            stream.CanRead.Returns(false);

            Action action = () => new StreamContent(stream);

            action.Should().ThrowExactly<ArgumentException>().Which.ShouldBePrinted();
        }

        [Test]
        public void Should_allow_streams_without_navigation()
        {
            var stream = Substitute.For<Stream>();

            stream.CanRead.Returns(true);
            stream.CanSeek.Returns(false);

            Action action = () => new StreamContent(stream);

            action.Should().NotThrow();
        }

        [Test]
        public void Should_allow_lengths_exceeding_size_of_int32()
        {
            var stream = Substitute.For<Stream>();

            stream.CanRead.Returns(true);

            var length = 54363453653654L;

            var content = new StreamContent(stream, length);

            content.Length.Should().Be(length);
        }
    }
}