using RaikesSimplexService.Joel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using RaikesSimplexService.DataModel;

namespace UnitTests
{
    
    
    /// <summary>
    ///This is a test class for SolverTest and is intended
    ///to contain all SolverTest Unit Tests
    ///</summary>
    [TestClass()]
    public class SolverTest
    {
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

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


        /// <summary>
        ///A test for Solve
        ///</summary>
        [TestMethod()]
        public void ExampleSolveTest()
        {
            #region Arrange
            //var target = new Solver();            

            //var lc1 = new LinearConstraint()
            //{
            //    Coefficients = new double[2] { 8, 12 },
            //    Relationship = Relationship.GreaterThanOrEquals,
            //    //Relationship = Relationship.LessThanOrEquals,
            //    Value = 24
            //};

            //var lc2 = new LinearConstraint()
            //{
            //    Coefficients = new double[2] { 12, 12 },
            //    Relationship = Relationship.GreaterThanOrEquals,
            //    //Relationship = Relationship.LessThanOrEquals,
            //    Value = 36
            //};

            //var lc3 = new LinearConstraint()
            //{
            //    Coefficients = new double[2] { 2, 1 },
            //    Relationship = Relationship.GreaterThanOrEquals,
            //    //Relationship = Relationship.LessThanOrEquals,
            //    Value = 4
            //};

            //var lc4 = new LinearConstraint()
            //{
            //    Coefficients = new double[2] { 1, 1 },
            //    Relationship = Relationship.LessThanOrEquals,
            //    //Relationship = Relationship.Equals,
            //    Value = 5
            //};

            //var constraints = new List<LinearConstraint>() {lc1, lc2, lc3, lc4};

            //var goal = new Goal() 
            //{ 
            //    Coefficients = new double[2] { 0.2, 0.3 },
            //    ConstantTerm = 0
            //};           

            //var model = new Model()
            //{
            //    Constraints = constraints,
            //    Goal = goal,
            //    GoalKind = GoalKind.Minimize
            //};
            
            //var expected = new Solution()
            //{
            //    Decisions = new double[2] { 3, 0 },
            //    Quality = SolutionQuality.Optimal,
            //    AlternateSolutionsExist = false,
            //    OptimalValue = 0.6
            //};
            #endregion

            #region Test2
            var target = new Solver();

            var lc1 = new LinearConstraint()
            {
                Coefficients = new double[2] { 2, 1 },
                Relationship = Relationship.LessThanOrEquals,
                Value = 32
            };

            var lc2 = new LinearConstraint()
            {
                Coefficients = new double[2] { 1, 1 },
                Relationship = Relationship.LessThanOrEquals,
                Value = 18
            };

            var lc3 = new LinearConstraint()
            {
                Coefficients = new double[2] { 1, 3 },
                Relationship = Relationship.LessThanOrEquals,
                Value = 36
            };

            var constraints = new List<LinearConstraint>() { lc1, lc2, lc3 };

            var goal = new Goal()
            {
                Coefficients = new double[2] { 80, 70 },
                ConstantTerm = 0
            };

            var model = new Model()
            {
                Constraints = constraints,
                Goal = goal,
                GoalKind = GoalKind.Minimize
            };

            var expected = new Solution()
            {
                Decisions = new double[2] { 14, 4 },
                Quality = SolutionQuality.Optimal,
                AlternateSolutionsExist = false,
                OptimalValue = 1400
            };
            #endregion

            //Act
            var actual = target.Solve(model);
            
            //Assert
            CollectionAssert.AreEqual(expected.Decisions, actual.Decisions);
            Assert.AreEqual(expected.Quality, actual.Quality);
            Assert.AreEqual(expected.AlternateSolutionsExist, actual.AlternateSolutionsExist);
        }


        /// <summary>
        ///A test for Solve
        ///</summary>
        [TestMethod()]
        public void ExampleSolveTest2()
        {
            #region Test3
            var target = new Solver();

            var lc1 = new LinearConstraint()
            {
                Coefficients = new double[2] { 1, 4 },
                Relationship = Relationship.LessThanOrEquals,
                Value = 24
            };

            var lc2 = new LinearConstraint()
            {
                Coefficients = new double[2] { 1, 2 },
                Relationship = Relationship.LessThanOrEquals,
                Value = 16
            };

            var constraints = new List<LinearConstraint>() { lc1, lc2 };

            var goal = new Goal()
            {
                Coefficients = new double[2] { 3 , 9 },
                ConstantTerm = 0
            };

            var model = new Model()
            {
                Constraints = constraints,
                Goal = goal,
                GoalKind = GoalKind.Minimize
            };

            var expected = new Solution()
            {
                Decisions = new double[2] { 8 , 4 },
                Quality = SolutionQuality.Optimal,
                AlternateSolutionsExist = false,
                OptimalValue = 60
            };
            #endregion

            //Act
            var actual = target.Solve(model);
            
            //Assert
            CollectionAssert.AreEqual(expected.Decisions, actual.Decisions);
            Assert.AreEqual(expected.Quality, actual.Quality);
            Assert.AreEqual(expected.AlternateSolutionsExist, actual.AlternateSolutionsExist);
        }
    }
}
