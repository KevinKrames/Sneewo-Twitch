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
    public class TestDatabaseManager
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test_Initialize()
        {
            var databaseManager = new DatabaseManager();
            databaseManager.RetrieveQueryString("SELECT * FROM WordNode WHERE WordText = 'Forward' AND parentID IS NULL");
        }
    }
}
