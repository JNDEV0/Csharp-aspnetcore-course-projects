internal class Program
{
    private static void Main(string[] args)
    {
        //building the webApp
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        WebApplication webApp = builder.Build();

        webApp.Run(async (HttpContext context) =>
        {
            string requestUrlPath = context.Request.Path;
            string requestMethodType = context.Request.Method;
            int intA;
            int intB;

            //updating this header so client detects the data format correctly of the response
            context.Response.Headers["Content-Type"] = "text/html";

            if (requestUrlPath == "/" && requestMethodType == "GET")
            {
                try
                {
                    // a good request: server.com/?firstNumber=5&secondNumber=2&operation=add
                    string firstNumber = context.Request.Query.ContainsKey("firstNumber") == true ? context.Request.Query["firstNumber"] : default;
                    string secondNumber = context.Request.Query.ContainsKey("secondNumber") == true ? context.Request.Query["secondNumber"] : default;
                    string operation = context.Request.Query.ContainsKey("operation") == true ? context.Request.Query["operation"] : default;
                    
                    //validation could be expanded to include regex validation that the numbers are in fact numbers before converting
                    //for now if any of the query keys are missing content, throw bad request
                    if (operation == string.Empty) throw new Exception("operation is empty.");
                    if (firstNumber == string.Empty) throw new Exception("firstNumber is empty.");
                    else intA = Convert.ToInt32(firstNumber);
                    if (secondNumber == string.Empty) throw new Exception("secondNumber is empty.");
                    else intB = Convert.ToInt32(secondNumber);

                    //DEBUG, shows on server console
                    Console.WriteLine($"query request: {firstNumber} {operation} {secondNumber}");

                    int result = -1;
                    switch(operation)
                    {
                        case "add": 
                            result = intA + intB; break;
                        case "subtract":
                            result = intA - intB; break;
                        case "multiply":
                            result = intA * intB; break;
                        case "divide":
                            result = intA / intB; break;
                        case "remainder":
                            result = intA % intB; break;
                        default: 
                            throw new Exception("invalid operation, only these are valid: add, subtract, multiply, divide, remainder");
                    }
                    
                    //no exceptions to this point, the operation must have passed.
                    context.Response.StatusCode = 200;
                    await context.Response.WriteAsync(result.ToString());
                } 
                catch (Exception exception)
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync($"<h1>invalid request query!</h1>");
                    await context.Response.WriteAsync($"<p>{exception.Message}</p>");
                }
            }
            else 
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync($"<h1>invalid request method.</h1>");
            }

        });

        //starting server
        webApp.Run();
    }
}