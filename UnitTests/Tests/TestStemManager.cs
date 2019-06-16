using NUnit.Framework;
using SneetoApplication.Data_Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.Tests
{
    [TestFixture]
    public class TestStemManager
    {
        private Token testToken;
        [SetUp]
        public void Setup()
        {
            StemManager.ClearStems();
            TokenManager.ClearTokens();
            testToken = new Token
            {
                ID = Guid.NewGuid(),
                WordText = "Texting"
            };
            TokenManager.SetTokenForID(testToken.ID, testToken);
        }

        [Test]
        public void Test_AddToken_MultipleTimes()
        {
            var testToken2 = new Token
            {
                ID = Guid.NewGuid(),
                WordText = "Texts"
            };
            TokenManager.SetTokenForID(testToken2.ID, testToken2);

            StemManager.AddToken(testToken);
            StemManager.AddToken(testToken);
            StemManager.AddToken(testToken2);

            var tokenList = StemManager.GetTokensForUnstemmedWord(testToken.WordText);

            Assert.AreEqual(2, tokenList.Count);
            Assert.AreEqual(new List<Token>{testToken, testToken2}, tokenList);

            Assert.AreEqual("text", StemManager.GetStemForToken(testToken.WordText));
            Assert.AreEqual("text", StemManager.GetStemForToken(testToken2.WordText));
        }
    }
}
