using System;
using FluentAssertions;
using NUnit.Framework;
using Vostok.ClusterClient.Core.Model;

namespace Vostok.ClusterClient.Core.Tests.Model
{
    [TestFixture]
    internal class Header_Tests
    {
        [Test]
        public void Should_throw_an_error_when_supplied_with_null_name()
        {
            Action action = () => new Header(null, "value");

            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Should_throw_an_error_when_supplied_with_null_value()
        {
            Action action = () => new Header("name", null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void ToString_should_return_correct_representation()
        {
            var header = new Header("X-Kontur-External-Url", "http://foo.kontur.ru/bar");

            header.ToString().Should().Be("X-Kontur-External-Url: http://foo.kontur.ru/bar");
        }

        [Test]
        public void Should_be_equal_if_key_and_value_equals()
        {
            var header1 = new Header("name", "value");
            var header2 = new Header("name", "value");

            header1.Should().Be(header2);
        }
        
        [Test]
        public void Should_have_case_insensitive_equality_for_key()
        {
            var header1 = new Header("name", "value");
            var header2 = new Header("Name", "value");

            header1.Should().Be(header2);
        }
        
        [Test]
        public void Should_have_case_sensitive_equality_for_value()
        {
            var header1 = new Header("name", "value");
            var header2 = new Header("name", "Value");

            header1.Should().NotBe(header2);
        }
    }
}
