// -----------------------------------------------------------------------
// CSE445 Project 2
// -----------------------------------------------------------------------

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Project2.Tests
{
    ///<summary>
    ///  This is a test class for ChickenFarmTest and is intended to contain all ChickenFarmTest Unit Tests
    ///</summary>
    [TestClass]
    public class ChickenFarmTest
    {
        ///<summary>
        ///  Gets or sets the test context which provides information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes

        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //

        #endregion

        ///<summary>
        ///  A test for FarmSomeChickens
        ///</summary>
        [TestMethod]
        public void FarmSomeChickensTest()
        {
            var target = new ChickenFarm(); // TODO: Initialize to an appropriate value
            target.FarmSomeChickens();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        ///<summary>
        ///  A test for UpdatePrice
        ///</summary>
        [TestMethod]
        [DeploymentItem("Project2.exe")]
        public void UpdatePriceTest()
        {
            var target = new ChickenFarm_Accessor(); // TODO: Initialize to an appropriate value
            target.UpdatePrice();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }
    }
}