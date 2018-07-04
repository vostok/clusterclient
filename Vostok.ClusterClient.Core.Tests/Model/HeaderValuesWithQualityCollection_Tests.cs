using System;
using System.Linq;
using NUnit.Framework;
using Vostok.ClusterClient.Core.Model;

namespace Vostok.ClusterClient.Core.Tests.Model
{
    [TestFixture]
    [SetCulture("ru-RU")]
    public class HeaderValuesWithQualityCollection_Tests
    {
        [Test]
        public void ParseTest()
        {
            var values = HeaderValuesWithQualityCollection.Parse("application/xhtml+xml;q=0.999,text/html,application/xml;q=0.9,*/*;q=0.3,application/json;q=0.9");
            Assert.That(values, Is.Not.Null);
            Assert.That(values.Count, Is.EqualTo(5));
            Assert.That(values.FirstOrDefault().Value, Is.EqualTo("text/html"));
            Assert.That(values.LastOrDefault().Value, Is.EqualTo("*/*"));

            values = HeaderValuesWithQualityCollection.Parse("");
            Assert.That(values, Is.Not.Null);
            Assert.That(values, Is.Empty);
        }

        [Test]
        public void ToStringTest()
        {
            var values = HeaderValuesWithQualityCollection.Parse("application/xhtml+xml;q=0.999,text/html;q=1,application/xml;q=0.9,*/*;q=0.3,application/json;q=0.9");
            var newValue = values.ToString();
            Assert.That(newValue, Is.EqualTo("text/html,application/xhtml+xml;q=0.999,application/xml;q=0.9,application/json;q=0.9,*/*;q=0.3"));
        }

        [Test]
        public void ClearTest()
        {
            var values = new HeaderValuesWithQualityCollection { {"value1", 0.1m } };
            Assert.That(values, Is.Not.Empty);

            values.Clear();
            Assert.That(values, Is.Empty);
        }

        [Test]
        public void AddTest()
        {
            var values = new HeaderValuesWithQualityCollection{ "value1", {"value1", 0.999m}, "value2", "value2"};
            Assert.That(values.Count, Is.EqualTo(4));
        }

        [Test]
        public void RemoveTest()
        {
            var values = new HeaderValuesWithQualityCollection { "value1", { "Value1", 0.999m }, "value2", "vAlue2", {"valUe1", 0.998m} };
            Assert.That(values.Count, Is.EqualTo(5));

            values.Remove("value2");
            Assert.That(values.Count, Is.EqualTo(3));

            values.Remove(new HeaderValueWithQuality("value1", 0.999m));
            Assert.That(values.Count, Is.EqualTo(2));

            values.Remove("value1", 0.998m);
            Assert.That(values.Count, Is.EqualTo(1));
            Assert.That(values.FirstOrDefault(), Is.EqualTo(new HeaderValueWithQuality("value1")));
        }

        [Test]
        public void IndexerTest()
        {
            var values = new HeaderValuesWithQualityCollection { "value1", { "value1", 0.999m }, "value2", "value2", { "value1", 0.998m } };
            Assert.That(values.Count, Is.EqualTo(5));
            Assert.That(values[0], Is.EqualTo(values.FirstOrDefault()));
            Assert.That(values[values.Count-1], Is.EqualTo(values.LastOrDefault()));
        }

        [Test]
        [TestCase("value 1")]
        [TestCase("value=0.1")]
        [TestCase("value;q=-0")]
        [TestCase("value;q=0.0000")]
        [TestCase("value;q==0.1")]
        public void FormatExceptionTest(string value)
        {
            Assert.That(() => HeaderValuesWithQualityCollection.Parse(value), Throws.InstanceOf<FormatException>());
        }

        [Test]
        [TestCase("value;q=1.1")]
        public void ArgumentExceptionTest(string value)
        {
            Assert.That(() => HeaderValuesWithQualityCollection.Parse(value), Throws.InstanceOf<ArgumentException>());
        }

        [Test, Explicit("Neat visualisation for research purposes")]
        public void DecimalTests()
        {
            // Define an array of Decimal values.
            Decimal[] values =
            {
                0.1m, 0.01m, 0.001m, 0.0001m, 0.00001m,
                0.9m, 0.99m, 0.999m, 0.9999m, 0.99999m,
                1m, 1.0m, 1.00m
            };

            Console.WriteLine("{0,31}  {1,10:X8}{2,10:X8}{3,10:X8}{4,10:X8}", "Argument", "Bits[3]", "Bits[2]", "Bits[1]", "Bits[0]");
            Console.WriteLine("{0,31}  {1,10:X8}{2,10:X8}{3,10:X8}{4,10:X8}", "--------", "-------", "-------", "-------", "-------");

            // Iterate each element and display its binary representation
            foreach (var value in values)
            {
                int[] bits = decimal.GetBits(value);
                Console.WriteLine("{0,31}  {1,10:X8}{2,10:X8}{3,10:X8}{4,10:X8}", value, bits[3], bits[2], bits[1], bits[0]);
            }
        }
    }
}
