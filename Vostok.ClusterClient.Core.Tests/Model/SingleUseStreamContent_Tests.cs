using System;
using System.IO;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Tests.Helpers;

// ReSharper disable ReturnValueOfPureMethodIsNotUsed

namespace Vostok.Clusterclient.Core.Tests.Model
{
    [TestFixture]
    internal class SingleUseStreamContent_Tests
    {
        private SingleUseStreamContent content;

        [SetUp]
        public void TestSetup()
        {
            content = new SingleUseStreamContent(Stream.Null, 123);
        }

        [Test]
        public void WasUsed_property_should_return_false_initially()
        {
            content.WasUsed.Should().BeFalse();
        }

        [Test]
        public void WasUsed_property_should_return_true_after_first_stream_access()
        {
            content.Stream.GetHashCode();

            content.WasUsed.Should().BeTrue();
        }

        [Test]
        public void Stream_property_should_return_underlying_stream_on_first_access()
        {
            content.Stream.Should().BeSameAs(Stream.Null);
        }

        [Test]
        public void Stream_property_should_throw_after_first_access()
        {
            Action action = () => content.Stream.GetHashCode();

            action();

            action.Should().ThrowExactly<StreamAlreadyUsedException>().Which.ShouldBePrinted();
        }
    }
}