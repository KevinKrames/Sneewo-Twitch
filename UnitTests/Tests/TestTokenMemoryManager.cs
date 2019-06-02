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
        Token bigToken;
        Guid firstGuid;
        Guid secondGuid;
        Guid thirdGuid;
        Guid fourthGuid;
        Guid fifthGuid;
        Mock<TokenMemoryManager> mockTokenMemoryManager;

        [SetUp]
        public void Setup()
        {
            TokenManager.ClearTokens();
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

            bigToken = new Token();
            bigToken.ChildrenTokens = new List<Guid>();

            for(char first = 'a'; first <= 'z'; first++)
            {
                for (char second = 'a'; second <= 'z'; second++)
                {
                    for (char third = 'a'; third <= 'z'; third++)
                    {
                        tempToken = new Token();
                        tempToken.ID = Guid.NewGuid();
                        tempToken.WordText = $"{first}{second}{third}";
                        TokenManager.SetTokenForID(tempToken.ID, tempToken);
                        bigToken.ChildrenTokens.Add(tempToken.ID);
                    }
                }
            }

            mockTokenMemoryManager = new Mock<TokenMemoryManager> { CallBase = true };
            mockTokenMemoryManager.Setup(mm => mm.SetupManager());
        }

        [Test, Sequential]
        
        public void Test_DoesTokenExist_TestExistingToken(
            [Values("alpha", "cat", "fart", "poop", "zeta")] string searchText,
            [Values(0, 1, 2, 3, 4)] int index
            )
        {
            var result = TokenManager.DoesTokenExist(searchText, token.ChildrenTokens, out var outIndex);

            Assert.IsTrue(result);
            Assert.AreEqual(index, outIndex);
        }

        [Test, Sequential]

        public void Test_DoesTokenExist_TestNotExistingToken(
            [Values("abraham", "butts", "everything", "ostrich", "wow", "zit")] string searchText,
            [Values(0, 1, 2, 3, 4, 5)] int index
            )
        {
            var result = TokenManager.DoesTokenExist(searchText, token.ChildrenTokens, out var outIndex);

            Assert.IsFalse(result);
            Assert.AreEqual(index, outIndex);
        }

        [Test, Sequential]

        public void Test_DoesTokenExist_TestExistingTokenLargeRoot(
            [Values("ale", "cat", "fat", "pop", "zit")] string searchText,
            [Values(290, 1371, 3399, 10519, 17127)] int index
            )
        {
            var moqObject = mockTokenMemoryManager.Object;
            var start = DateTime.Now.Ticks;
            var result = TokenManager.DoesTokenExist(searchText, bigToken.ChildrenTokens, out var outIndex);

            var timeInMS = (DateTime.Now.Ticks - start) / (TimeSpan.TicksPerMillisecond * 1.0m);

            //Assert.Warn($"{searchText} completed, took: {timeInMS}ms");
            Assert.IsTrue(result);
            Assert.AreEqual(index, outIndex);
        }

        [Test, Sequential]
        public void Test_DoesTokenExist_TestNotExistingTokenLargeRoot(
            [Values("alex", "cats", "fate", "pope", "zapper")] string searchText,
            [Values(291, 1372, 3400, 10520, 16916)] int index
            )
        {
            var moqObject = mockTokenMemoryManager.Object;
            var start = DateTime.Now.Ticks;
            var result = TokenManager.DoesTokenExist(searchText, bigToken.ChildrenTokens, out var outIndex);

            var timeInMS = (DateTime.Now.Ticks - start) / (TimeSpan.TicksPerMillisecond * 1.0m);

            //Assert.Warn($"{searchText} completed, took: {timeInMS}ms");
            Assert.IsFalse(result);
            Assert.AreEqual(index, outIndex);
        }

        [Test]
        public void Test_GetExisitingTokens_PartialList()
        {
            var tokenList = new TokenList("A list of existing tokens");
            var childGuid = Guid.NewGuid();

            var rootToken = new Token
            {
                ID = Guid.NewGuid(),
                ChildrenTokens = new List<Guid>(),
                WordText = "root"
            };
            rootToken.ChildrenTokens.Add(childGuid);
            TokenManager.SetTokenForID(rootToken.ID, rootToken);

            var firstToken = new Token
            {
                ID = childGuid,
                ChildrenTokens = new List<Guid>(),
                WordText = "A"
            };
            childGuid = Guid.NewGuid();
            firstToken.ChildrenTokens.Add(childGuid);
            TokenManager.SetTokenForID(firstToken.ID, firstToken);

            var secondToken = new Token
            {
                ID = childGuid,
                ChildrenTokens = new List<Guid>(),
                WordText = "list"
            };
            childGuid = Guid.NewGuid();
            secondToken.ChildrenTokens.Add(childGuid);
            TokenManager.SetTokenForID(secondToken.ID, secondToken);

            var thirdToken = new Token
            {
                ID = childGuid,
                ChildrenTokens = new List<Guid>(),
                WordText = "of"
            };
            TokenManager.SetTokenForID(thirdToken.ID, thirdToken);

            var computedList = mockTokenMemoryManager.Object.GetExisitingTokens(tokenList, rootToken);
            Assert.AreEqual(new List<Token> { thirdToken, secondToken, firstToken }, computedList);
        }

        [Test]
        public void Test_GetExisitingTokens_SingleItem()
        {
            var tokenList = new TokenList("Kappa");
            var childGuid = Guid.NewGuid();

            var rootToken = new Token
            {
                ID = Guid.NewGuid(),
                ChildrenTokens = new List<Guid>(),
                WordText = "root"
            };
            rootToken.ChildrenTokens.Add(childGuid);
            TokenManager.SetTokenForID(rootToken.ID, rootToken);

            var firstToken = new Token
            {
                ID = childGuid,
                ChildrenTokens = new List<Guid>(),
                WordText = "Kappa"
            };
            TokenManager.SetTokenForID(firstToken.ID, firstToken);

            var computedList = mockTokenMemoryManager.Object.GetExisitingTokens(tokenList, rootToken);
            Assert.AreEqual(new List<Token> { firstToken }, computedList);
        }

        [Test]
        public void Test_GetExisitingTokens_NoItems()
        {
            var tokenList = new TokenList("Kappa 123");

            var rootToken = new Token
            {
                ID = Guid.NewGuid(),
                ChildrenTokens = new List<Guid>(),
                WordText = "root"
            };
            TokenManager.SetTokenForID(rootToken.ID, rootToken);

            var computedList = mockTokenMemoryManager.Object.GetExisitingTokens(tokenList, rootToken);
            Assert.AreEqual(new List<Token>(), computedList);
        }
    }
}
