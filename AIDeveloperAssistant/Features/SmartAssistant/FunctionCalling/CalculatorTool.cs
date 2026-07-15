using AIDeveloperAssistant.Features.Tools;
using OpenAI.Responses;
using System.Text.Json;

namespace AIDeveloperAssistant.Features.SmartAssistant.FunctionCalling;

internal enum CalculatorOperation
{
    Add,
    Subtract,
    Multiply,
    Divide
}

internal static class CalculatorTool
{
    public static double Calculate(double number1, double number2, string operation)
    {
        return operation.ToLower() switch
        {
            "add" => number1 + number2,
            "subtract" => number1 - number2,
            "multiply" => number1 * number2,
            "divide" => number2 != 0
                ? number1 / number2
                : throw new DivideByZeroException(),

            _ => throw new ArgumentException(
                $"Unsupported operation '{operation}'.")
        };
    }

    internal static class CalculatorToolDefinition
    {
#pragma warning disable OPENAI001

        public static FunctionToolRegistration Create()
        {
            return
                new FunctionToolRegistration
                {

                    FunctionName = "calculate",

                    ToolDefinition =
                ResponseTool.CreateFunctionTool(
                functionName: "calculate",
                functionDescription:
                    "Performs a basic mathematical operation on two numbers.",

                functionParameters: BinaryData.FromBytes(
                    """
                {
                  "type": "object",
                  "properties": {
                    "number1": {
                      "type": "number",
                      "description": "The first number."
                    },
                    "number2": {
                      "type": "number",
                      "description": "The second number."
                    },
                    "operation": {
                      "type": "string",
                      "enum": [
                        "add",
                        "subtract",
                        "multiply",
                        "divide"
                      ],
                      "description": "The mathematical operation to perform."
                    }
                  },
                  "required": [
                    "number1",
                    "number2",
                    "operation"
                  ],
                  "additionalProperties": false
                }
                """u8.ToArray()),

                strictModeEnabled: true),
                    ExecuteAsync = ExecuteFunction

                };
        }
        private static Task<string> ExecuteFunction(
      FunctionCallResponseItem functionCall)
        {
            if (!functionCall.FunctionName.Equals(
                "calculate",
                StringComparison.OrdinalIgnoreCase))
            {
                throw new NotImplementedException(
                    $"Unknown function: {functionCall.FunctionName}");
            }

            using JsonDocument arguments =
                JsonDocument.Parse(functionCall.FunctionArguments);

            JsonElement root = arguments.RootElement;

            double number1 =
                root.GetProperty("number1").GetDouble();

            double number2 =
                root.GetProperty("number2").GetDouble();

            string operation =
                root.GetProperty("operation").GetString()
                ?? throw new InvalidOperationException(
                    "Operation is missing.");

            double result = CalculatorTool.Calculate(
                number1,
                number2,
                operation);

            Console.WriteLine();
            Console.WriteLine(
                $"[TOOL] calculate({number1}, {number2}, {operation})");

            Console.WriteLine($"[TOOL RESULT] {result}");

            return  Task.FromResult(result.ToString(
                System.Globalization.CultureInfo.InvariantCulture));
        }
    }
}
