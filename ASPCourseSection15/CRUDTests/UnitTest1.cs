/*
    the xUnit project is added to the solution note that it contains two packages that are actually relevant
    xunit and xunit.runner.visualstudio under Dependencies/Packages

    hotkey for Test Explorer in VS2022
    CTRL+E, T
 */
namespace CRUDTests
{
    public class UnitTest1
    {
        //Fact is the attribute with which test methods, which may have several unit tests within,
        //are decorated to signal that this test should execute
        [Fact]
        public void Test1()
        {
            //Arrange - inputs 
            //instantiate needed classes that will be tested
            //identify your test inputs and EXPECTED RESULT
            MyMath myMath = new MyMath();
            int input1 = 10; 
            int input2 = 20;
            int expected = 30;

            //Act - calling method
            //activate what is being tested with test parameters,
            //store return result if possible
            int actual = myMath.Add(input1, input2);


            //Assert - compare expected value with actual value
            //the expected result from the operation compared to
            //the actual value returned
            Assert.Equal(expected, actual);
        }
    }
}