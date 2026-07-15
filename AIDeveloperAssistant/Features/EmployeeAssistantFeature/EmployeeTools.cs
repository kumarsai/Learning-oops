using OpenAI.Responses;

namespace AIDeveloperAssistant.Features.EmployeeAssistantFeature;
#pragma warning disable OPENAI001

internal static class EmployeeTools
{

    public static IReadOnlyList<ResponseTool> Create()
    {
        ResponseTool getEmployeeTool =
            ResponseTool.CreateFunctionTool(
                functionName: "get_employee",
                functionDescription:
                    "Gets complete employee details using an employee ID.",
                functionParameters: BinaryData.FromString(
                    """
                    {
                      "type": "object",
                      "properties": {
                        "employeeId": {
                          "type": "integer",
                          "description": "The employee ID, for example 1001."
                        }
                      },
                      "required": ["employeeId"],
                      "additionalProperties": false
                    }
                    """),
                strictModeEnabled: true);

        ResponseTool searchEmployeesTool =
            ResponseTool.CreateFunctionTool(
                functionName: "search_employees",
                functionDescription:
                    "Searches for employees by full or partial employee name.",
                functionParameters: BinaryData.FromString(
                    """
                    {
                      "type": "object",
                      "properties": {
                        "name": {
                          "type": "string",
                          "description": "Full or partial employee name."
                        }
                      },
                      "required": ["name"],
                      "additionalProperties": false
                    }
                    """),
                strictModeEnabled: true);

        ResponseTool getLeaveBalanceTool =
            ResponseTool.CreateFunctionTool(
                functionName: "get_leave_balance",
                functionDescription:
                    "Gets the remaining leave balance for an employee ID.",
                functionParameters: BinaryData.FromString(
                    """
                    {
                      "type": "object",
                      "properties": {
                        "employeeId": {
                          "type": "integer",
                          "description": "The employee ID."
                        }
                      },
                      "required": ["employeeId"],
                      "additionalProperties": false
                    }
                    """),
                strictModeEnabled: true);

        ResponseTool getEmployeeProjectsTool =
            ResponseTool.CreateFunctionTool(
                functionName: "get_employee_projects",
                functionDescription:
                    "Gets the projects assigned to an employee ID.",
                functionParameters: BinaryData.FromString(
                    """
                    {
                      "type": "object",
                      "properties": {
                        "employeeId": {
                          "type": "integer",
                          "description": "The employee ID."
                        }
                      },
                      "required": ["employeeId"],
                      "additionalProperties": false
                    }
                    """),
                strictModeEnabled: true);

        ResponseTool getEmployeesByDepartmentTool =
            ResponseTool.CreateFunctionTool(
                functionName: "get_employees_by_department",
                functionDescription:
                    "Gets all employees working in a specified department.",
                functionParameters: BinaryData.FromString(
                    """
                    {
                      "type": "object",
                      "properties": {
                        "department": {
                          "type": "string",
                          "description": "Department name, such as Engineering or Finance."
                        }
                      },
                      "required": ["department"],
                      "additionalProperties": false
                    }
                    """),
                strictModeEnabled: true);

        return
        [
            getEmployeeTool,
            searchEmployeesTool,
            getLeaveBalanceTool,
            getEmployeeProjectsTool,
            getEmployeesByDepartmentTool
        ];
    }
}