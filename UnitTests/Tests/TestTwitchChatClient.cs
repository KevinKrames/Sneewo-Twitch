using Moq;
using NUnit.Framework;
using SneetoApplication;
using SneetoApplication.Data_Structures;
using SneetoApplication.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Client;
using TwitchLib.Client.Interfaces;
using TwitchLib.Client.Models;

namespace UnitTests.Tests
{
    [TestFixture]
    public class TestTwitchChatClient
    {
        TwitchChatClient twitchChatClient;
        Mock<ITwitchClient> mockTwitchClient;
        Mock<TwitchCredentials> mockTwitchCredentialFileReader;

        [SetUp]
        public void Setup()
        {
            mockTwitchClient = new Mock<ITwitchClient>();

            mockTwitchClient.Setup(s => s.Initialize(It.IsAny<ConnectionCredentials>(), It.IsAny<string>(), It.IsAny<char>(), It.IsAny<char>(), It.IsAny<bool>())).Verifiable();
            mockTwitchClient.Setup(s => s.Connect()).Verifiable();
            mockTwitchCredentialFileReader = new Mock<TwitchCredentials>();
            TwitchCredentials twitchCredentials = new TwitchCredentials
            {
                twitchUsername = "TestName",
                twitchOAuth = "raw/erwakjrhak;ew"
            };
            //mockTwitchCredentialFileReader.Setup(s => s.readTwitchCredentials(It.IsAny<string>())).Returns(twitchCredentials);

            twitchChatClient = new TwitchChatClient(mockTwitchClient.Object);
        }

        [Test]
        public void Test_TwitchChatClient_HasSingletonInstance()
        {
            Assert.IsTrue(twitchChatClient.GetType().IsInstanceOfType(new TwitchChatClient()));
        }

        [Test]
        public void Test_TwitchChatClient_CanConnect()
        {
            Assert.DoesNotThrow(() => twitchChatClient.Connect());
            mockTwitchClient.VerifyAll();
            mockTwitchCredentialFileReader.VerifyAll();
        }
    }
}