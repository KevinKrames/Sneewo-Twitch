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

        Token mainToken;
        Token backwardsMainToken;
        Token firstToken;
        Token secondToken;
        Token thirdToken;
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

            firstToken = new Token
            {
                ID = Guid.NewGuid(),
                ChildrenTokens = new List<Guid>(),
                WordText = "A"
            };

            secondToken = new Token
            {
                ID = Guid.NewGuid(),
                ChildrenTokens = new List<Guid>(),
                WordText = "big"
            };

            thirdToken = new Token
            {
                ID = Guid.NewGuid(),
                ChildrenTokens = new List<Guid>(),
                WordText = "cat"
            };

            mainToken = new Token
            {
                ID = Guid.NewGuid(),
                ChildrenTokens = new List<Guid>(),
                WordText = "root"
            };

            backwardsMainToken = new Token
            {
                ID = Guid.NewGuid(),
                ChildrenTokens = new List<Guid>(),
                WordText = "backwardsRoot"
            };

            TokenManager.SetTokenForID(mainToken.ID, mainToken);
            TokenManager.SetTokenForID(backwardsMainToken.ID, backwardsMainToken);

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

            Brain.configuration = new Dictionary<string, string>();
            Brain.configuration[Brain.USE_DATABASE] = bool.FalseString;

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

        [Test]
        public void Test_TrainTokenList_NoExistingTokens_NoLinks()
        {
            var inputText = new TokenList("A big cat");

            mockTokenMemoryManager.Object.TrainTokenList(inputText, mainToken, new List<Token>());

            Assert.AreEqual(1, mainToken.ChildrenTokens.Count);
            var nextToken = TokenManager.GetTokenForID(mainToken.ChildrenTokens[0]);
            Assert.AreEqual(1, nextToken.ChildrenTokens.Count);
            Assert.AreEqual("A", nextToken.WordText);

            nextToken = TokenManager.GetTokenForID(nextToken.ChildrenTokens[0]);
            Assert.AreEqual(1, nextToken.ChildrenTokens.Count);
            Assert.AreEqual("big", nextToken.WordText);

            nextToken = TokenManager.GetTokenForID(nextToken.ChildrenTokens[0]);
            Assert.AreEqual(null, nextToken.ChildrenTokens);
            Assert.AreEqual("cat", nextToken.WordText);
        }

        [Test]
        public void Test_TrainTokenList_NoExistingTokens_WithLinks()
        {
            var inputText = new TokenList("A big cat");

            var linkedTokens = new List<Token>();
            linkedTokens.Add(firstToken);
            linkedTokens.Add(secondToken);
            linkedTokens.Add(thirdToken);

            TokenManager.SetTokenForID(firstToken.ID, firstToken);
            TokenManager.SetTokenForID(secondToken.ID, secondToken);
            TokenManager.SetTokenForID(thirdToken.ID, thirdToken);

            mockTokenMemoryManager.Object.TrainTokenList(inputText, mainToken, new List<Token>(), linkedTokens);

            Assert.AreEqual(1, mainToken.ChildrenTokens.Count);
            var nextToken = TokenManager.GetTokenForID(mainToken.ChildrenTokens[0]);
            Assert.AreEqual(1, nextToken.ChildrenTokens.Count);
            Assert.AreEqual("A", nextToken.WordText);
            Assert.AreEqual(firstToken.ID, nextToken.PartnerID);
            Assert.AreEqual(nextToken.ID, firstToken.PartnerID);

            nextToken = TokenManager.GetTokenForID(nextToken.ChildrenTokens[0]);
            Assert.AreEqual(1, nextToken.ChildrenTokens.Count);
            Assert.AreEqual("big", nextToken.WordText);
            Assert.AreEqual(secondToken.ID, nextToken.PartnerID);
            Assert.AreEqual(nextToken.ID, secondToken.PartnerID);

            nextToken = TokenManager.GetTokenForID(nextToken.ChildrenTokens[0]);
            Assert.AreEqual(null, nextToken.ChildrenTokens);
            Assert.AreEqual("cat", nextToken.WordText);
            Assert.AreEqual(thirdToken.ID, nextToken.PartnerID);
            Assert.AreEqual(nextToken.ID, thirdToken.PartnerID);
        }

        [Test]
        public void Test_TrainTokenList_ExistingTokens_NoLinks()
        {
            firstToken.WordText = "big";
            secondToken.WordText = "old";
            thirdToken.WordText = "big";

            var existingToken = new Token
            {
                ID = Guid.NewGuid(),
                WordText = "big"
            };

            var reverseExistingToken = new Token
            {
                ID = Guid.NewGuid(),
                WordText = "big"
            };
            existingToken.PartnerID = reverseExistingToken.ID;
            reverseExistingToken.PartnerID = existingToken.ID;

            TokenManager.SetTokenForID(existingToken.ID, existingToken);
            TokenManager.SetTokenForID(reverseExistingToken.ID, reverseExistingToken);

            var existingTokens = new List<Token> { existingToken };
            var inputText = new TokenList("big old big");

            mockTokenMemoryManager.Object.TrainTokenList(inputText, mainToken, existingTokens);

            Assert.AreEqual(1, mainToken.ChildrenTokens.Count);
            var nextToken = TokenManager.GetTokenForID(mainToken.ChildrenTokens[0]);
            Assert.AreEqual(1, nextToken.ChildrenTokens.Count);
            Assert.AreEqual("big", nextToken.WordText);

            nextToken = TokenManager.GetTokenForID(nextToken.ChildrenTokens[0]);
            Assert.AreEqual(1, nextToken.ChildrenTokens.Count);
            Assert.AreEqual("old", nextToken.WordText);

            nextToken = TokenManager.GetTokenForID(nextToken.ChildrenTokens[0]);
            Assert.AreEqual(null, nextToken.ChildrenTokens);
            Assert.AreEqual("big", nextToken.WordText);
            Assert.AreEqual(existingToken.ID, nextToken.ID);
            Assert.AreEqual(existingToken.PartnerID, nextToken.PartnerID);
        }

        [Test]
        public void Test_TrainTokenList_AllExistingTokens_NoLinks()
        {
            firstToken.WordText = "large";
            secondToken.WordText = "old";
            thirdToken.WordText = "big";

            var existingTokens = new List<Token> { firstToken, secondToken, thirdToken };
            var inputText = new TokenList("large old big");

            TokenManager.SetTokenForID(firstToken.ID, firstToken);
            TokenManager.SetTokenForID(secondToken.ID, secondToken);
            TokenManager.SetTokenForID(thirdToken.ID, thirdToken);

            mockTokenMemoryManager.Object.TrainTokenList(inputText, mainToken, existingTokens);

            Assert.AreEqual(1, mainToken.ChildrenTokens.Count);
            var nextToken = TokenManager.GetTokenForID(mainToken.ChildrenTokens[0]);
            Assert.AreEqual(1, nextToken.ChildrenTokens.Count);
            Assert.AreEqual("large", nextToken.WordText);
            Assert.AreEqual(firstToken.ID, nextToken.ID);
            Assert.AreEqual(firstToken.PartnerID, nextToken.PartnerID);

            nextToken = TokenManager.GetTokenForID(nextToken.ChildrenTokens[0]);
            Assert.AreEqual(1, nextToken.ChildrenTokens.Count);
            Assert.AreEqual("old", nextToken.WordText);
            Assert.AreEqual(secondToken.ID, nextToken.ID);
            Assert.AreEqual(secondToken.PartnerID, nextToken.PartnerID);

            nextToken = TokenManager.GetTokenForID(nextToken.ChildrenTokens[0]);
            Assert.AreEqual(0, nextToken.ChildrenTokens.Count);
            Assert.AreEqual("big", nextToken.WordText);
            Assert.AreEqual(thirdToken.ID, nextToken.ID);
            Assert.AreEqual(thirdToken.PartnerID, nextToken.PartnerID);
        }

        [Test]
        public void Test_TrainTokenList_AllExistingTokens_WithLinks()
        {
            firstToken.WordText = "large";
            secondToken.WordText = "old";
            thirdToken.WordText = "big";

            var firstLink = new Token
            {
                ID = Guid.NewGuid(),
                ChildrenTokens = new List<Guid>(),
                WordText = "large"
            };

            var secondLink = new Token
            {
                ID = Guid.NewGuid(),
                ChildrenTokens = new List<Guid>(),
                WordText = "old"
            };

            var thirdLink = new Token
            {
                ID = Guid.NewGuid(),
                ChildrenTokens = new List<Guid>(),
                WordText = "big"
            };

            var linkedTokens = new List<Token>();
            linkedTokens.Add(firstLink);
            linkedTokens.Add(secondLink);
            linkedTokens.Add(thirdLink);

            TokenManager.SetTokenForID(firstLink.ID, firstLink);
            TokenManager.SetTokenForID(secondLink.ID, secondLink);
            TokenManager.SetTokenForID(thirdLink.ID, thirdLink);

            var existingTokens = new List<Token> { firstToken, secondToken, thirdToken };
            var inputText = new TokenList("large old big");

            TokenManager.SetTokenForID(firstToken.ID, firstToken);
            TokenManager.SetTokenForID(secondToken.ID, secondToken);
            TokenManager.SetTokenForID(thirdToken.ID, thirdToken);

            mockTokenMemoryManager.Object.TrainTokenList(inputText, mainToken, existingTokens, linkedTokens);

            Assert.AreEqual(1, mainToken.ChildrenTokens.Count);
            var nextToken = TokenManager.GetTokenForID(mainToken.ChildrenTokens[0]);
            Assert.AreEqual(1, nextToken.ChildrenTokens.Count);
            Assert.AreEqual("large", nextToken.WordText);
            Assert.AreEqual(firstToken.ID, nextToken.ID);
            Assert.AreEqual(firstToken.PartnerID, nextToken.PartnerID);
            Assert.AreEqual(firstToken.PartnerID, firstLink.ID);
            Assert.AreEqual(firstLink.PartnerID, firstToken.ID);

            nextToken = TokenManager.GetTokenForID(nextToken.ChildrenTokens[0]);
            Assert.AreEqual(1, nextToken.ChildrenTokens.Count);
            Assert.AreEqual("old", nextToken.WordText);
            Assert.AreEqual(secondToken.ID, nextToken.ID);
            Assert.AreEqual(secondToken.PartnerID, nextToken.PartnerID);
            Assert.AreEqual(secondToken.PartnerID, secondLink.ID);
            Assert.AreEqual(secondLink.PartnerID, secondToken.ID);

            nextToken = TokenManager.GetTokenForID(nextToken.ChildrenTokens[0]);
            Assert.AreEqual(0, nextToken.ChildrenTokens.Count);
            Assert.AreEqual("big", nextToken.WordText);
            Assert.AreEqual(thirdToken.ID, nextToken.ID);
            Assert.AreEqual(thirdToken.PartnerID, nextToken.PartnerID);
            Assert.AreEqual(thirdToken.PartnerID, thirdLink.ID);
            Assert.AreEqual(thirdLink.PartnerID, thirdToken.ID);
        }

        [Test]
        public void Test_TrainTokenList_TrainsPartnersCorrectly()
        {
            var inputText = new TokenList("An interesting new sentence.");
            mockTokenMemoryManager.Object.TrainTokenList(inputText, mainToken, null);
            var existingTokens = mockTokenMemoryManager.Object.GetExisitingTokens(inputText, mainToken);
            var linkedTokens = mockTokenMemoryManager.Object.GetExisitingTokens(inputText, mainToken);

            inputText.Invert();
            mockTokenMemoryManager.Object.TrainTokenList(inputText, backwardsMainToken, null, linkedTokens);
            var existingBackwardsTokens = mockTokenMemoryManager.Object.GetExisitingTokens(inputText, backwardsMainToken);

            var inputText2 = new TokenList("An interesting old sentence.");
            mockTokenMemoryManager.Object.TrainTokenList(inputText2, mainToken, null);
            linkedTokens = mockTokenMemoryManager.Object.GetExisitingTokens(inputText, mainToken);
            inputText.Invert();
            mockTokenMemoryManager.Object.TrainTokenList(inputText2, backwardsMainToken, null, linkedTokens);

            existingBackwardsTokens.Reverse();
            for (var i = 0; i < existingTokens.Count; i++)
            {
                Assert.AreEqual(existingTokens[i].PartnerID, existingBackwardsTokens[i].ID);
                Assert.AreEqual(existingBackwardsTokens[i].PartnerID, existingTokens[i].ID);
            }
        }
    }
}
