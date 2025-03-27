namespace CRUDTests
{
    internal class MyMath
    {
        //simple example method to execute the example test with.
        //this class could be anything that needs to be unit tested,
        //a Controller, a Service etc
        public int Add(int a, int b)
        {
            int c = a + b;

            //given the inputs of 10 an 20, the expected test result is 30
            //if anything else is returned here, the test will fail
            return c;
        }
    }
}
