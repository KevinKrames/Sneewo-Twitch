using Moq;
using NUnit.Framework;
using SneetoApplication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.Tests
{
    [TestFixture]
    class TestChannelMemory
    {
        private Mock<ChannelMemory> testMemory;
        [SetUp]
        public void Setup()
        {
            testMemory = new Mock<ChannelMemory> { CallBase = true };
            testMemory.Setup(m => m.DecayMemory(It.IsAny<decimal>()));
        }

        [Test]
        public void Test_Sentence_Check_For_Ten_Minutes()
        {
            var check1 = "FirstSentence";
            testMemory.Setup(m => m.GetTimeMilliseconds()).Returns(() => { return 0; });

            testMemory.Object.MessageSent(check1);
            testMemory.Object.Update();

            var result = testMemory.Object.HasMessageSent(check1);

            Assert.IsFalse(result, "Time has not moved. Should not be able to speak.");

            testMemory.Setup(m => m.GetTimeMilliseconds()).Returns(() => { return 600; });
            testMemory.Object.Update();

            result = testMemory.Object.HasMessageSent(check1);

            Assert.IsFalse(result, "Time has not moved enough. Should not be able to speak.");

            testMemory.Setup(m => m.GetTimeMilliseconds()).Returns(() => { return 599999; });
            testMemory.Object.Update();

            result = testMemory.Object.HasMessageSent(check1);

            Assert.IsFalse(result, "Time has not moved enough. Should not be able to speak.");

            testMemory.Setup(m => m.GetTimeMilliseconds()).Returns(() => { return 600001; });
            testMemory.Object.Update();

            result = testMemory.Object.HasMessageSent(check1);

            Assert.IsTrue(result, "Time is enough, you can now speak this sentence.");
        }
    }
}
