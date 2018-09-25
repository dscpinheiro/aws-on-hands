using System;
using Xunit;

namespace DebuggingExample.Tests
{
    public class TimeProcessorTest
    {
        [Fact]
        public void TestCurrentTimeUTC()
        {
            var processor = new TimeProcessor();
            var preTestTimeUtc = DateTime.UtcNow;

            var result = processor.CurrentTimeUTC();

            var postTestTimeUtc = DateTime.UtcNow;
            Assert.True(result >= preTestTimeUtc);
            Assert.True(result <= postTestTimeUtc);
        }
    }
}