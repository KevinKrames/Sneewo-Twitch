using Moq;
using NUnit.Framework;
using SneetoApplication;
using SneetoApplication.Data_Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.Tests
{
    [TestFixture]
    public class TestTokenMemoryManager
    {
        Token token;
        Guid firstGuid;
        Guid secondGuid;
        Guid thirdGuid;
        Guid fourthGuid;
        Guid fifthGuid;
        Mock<TokenMemoryManager> mockTokenMemoryManager;

        [SetUp]
        public void Setup()
        {
            token = new Token();
            firstGuid = Guid.NewGuid();
            secondGuid = Guid.NewGuid();
            thirdGuid = Guid.NewGuid();
            fourthGuid = Guid.NewGuid();
            fifthGuid = Guid.NewGuid();
            token.ChildrenTokens = new List<Guid>
            {
                firstGuid,
                secondGuid,
                thirdGuid,
                fourthGuid,
                fifthGuid
            };

            var tempToken = new Token();
            tempToken.ID = firstGuid;
            tempToken.WordText = "alpha";
            TokenManager.SetTokenForID(firstGuid, tempToken);

            tempToken = new Token();
            tempToken.ID = secondGuid;
            tempToken.WordText = "cat";
            TokenManager.SetTokenForID(secondGuid, tempToken);

            tempToken = new Token();
            tempToken.ID = thirdGuid;
            tempToken.WordText = "fart";
            TokenManager.SetTokenForID(thirdGuid, tempToken);

            tempToken = new Token();
            tempToken.ID = fourthGuid;
            tempToken.WordText = "poop";
            TokenManager.SetTokenForID(fourthGuid, tempToken);

            tempToken = new Token();
            tempToken.ID = fifthGuid;
            tempToken.WordText = "zeta";
            TokenManager.SetTokenForID(fifthGuid, tempToken);

            mockTokenMemoryManager = new Mock<TokenMemoryManager> { CallBase = true };
            mockTokenMemoryManager.Setup(mm => mm.SetupManager());
        }

        [Test, Sequential]
        
        public void Test_DoesTokenExist_TestExistingToken(
            [Values("alpha", "cat", "fart", "poop", "zeta")] string searchText,
            [Values(0, 1, 2, 3, 4)]int index
            )
        {
            var result = mockTokenMemoryManager.Object.DoesTokenExist(searchText, token, out var outIndex);

            Assert.IsTrue(result);
            Assert.AreEqual(index, outIndex);
        }

        [Test, Sequential]

        public void Test_DoesTokenExist_TestNotExistingToken(
            [Values("abraham", "butts", "everything", "ostrich", "wow", "zit")] string searchText,
            [Values(0, 1, 2, 3, 4, 5)]int index
            )
        {
            var result = mockTokenMemoryManager.Object.DoesTokenExist(searchText, token, out var outIndex);

            Assert.IsFalse(result);
            Assert.AreEqual(index, outIndex);
        }
    }
}
