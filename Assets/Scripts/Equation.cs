using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Equation
{
    private static readonly string[] digits = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
    private static readonly string[] ops = { "+", "−", "×", "÷" };

    // input a string equation and its intended solution
    // return true/false
    public static bool CheckProblem(string equation, int solution)
    {
        return CalculateSolution(equation) == solution;
    }

    // parses a string equation into an array of strings (multidigit numbers and operators)
    static string[] Parse(string equation)
    {
        equation = equation.Replace("+", " + ").Replace("−", " − ").Replace("×", " × ").Replace("÷", " ÷ ");
        return equation.Split(" ", StringSplitOptions.RemoveEmptyEntries);
    }

    // return the solution to the equation, or int.MaxValue if formatted improperly, /0 error, etc.
    public static int CalculateSolution(string equation)
    {
        List<string> equationList = Parse(equation).ToList();
        while (equationList.Count > 1)
        {
            int index;
            // index is the index of the operator
            // operations done in order ÷ -> × -> - -> +
            // this is the most efficient solution I could think of, though the compiler gives a warning for it
            if ((index = equationList.IndexOf("÷")) != -1) ;
            else if ((index = equationList.IndexOf("×")) != -1) ;
            else if ((index = equationList.IndexOf("−")) != -1) ;
            else if ((index = equationList.IndexOf("+")) != -1) ;
            else return int.MaxValue;
            // no values on left/right of index = badly formatted
            if (index == 0 || index == equationList.Count - 1) return int.MaxValue;
            int num1;
            int num2;
            // left/right values are not integers = badly formatted
            if (!int.TryParse(equationList[index - 1], out num1) || !int.TryParse(equationList[index + 1], out num2))
            {
                return int.MaxValue;
            }
            // special case for division
            // make sure no /0 errors
            // C# automatically rounds division to int, so check for attempted non-integer division here
            if (equationList[index] == "÷")
            {
                if (num2 == 0) return int.MaxValue;
                if (num1 % num2 != 0) return int.MaxValue;
            }
            // at this point, the operation must be valid, so do it
            int result = DoOperation(num1, num2, equationList[index]);
            // replace the 2 original numbers and 1 operator with the result
            equationList.RemoveAt(index - 1);
            equationList.RemoveAt(index - 1);
            equationList[index - 1] = result.ToString();
        }
        if (equationList.Count == 0) return int.MaxValue;
        int solution;
        return int.TryParse(equationList[0], out solution) ? solution : int.MaxValue;
    }

    // perform one of +, -, *, / on two numbers
    private static int DoOperation(int num1, int num2, string op)
    {
        if (op == "+") return num1 + num2;
        else if (op == "−") return num1 - num2;
        else if (op == "×") return num1 * num2;
        else if (op == "÷") return num1 / num2;
        else return int.MaxValue;
    }

    // EVERYTHING FROM THIS POINT BELOW IS UNUSED

    // returns a tuple
    // first element is an array of all digits/ops of the problem
    // second element is the problem's integer solution
    static (string[], int) MakeProblem(int difficulty)
    {
        // difficulty is the total number of ops
        // difficulty=1 -> a+b
        // difficulty=2 -> a+b*c
        // ...
        int length = difficulty * 2 + 1;
        string[] equation = new string[length];

        for (int x = 0; x < length; x++)
        {
            // at even position, insert a random number
            // at odd position, insert a random op
            equation[x] = (x % 2 == 0)
                ? digits[UnityEngine.Random.Range(0, digits.Length)]
                : ops[UnityEngine.Random.Range(0, ops.Length)];
        }
        // manually adjust any division operations to ensure the solution is always an integer
        // also correct for /0 errors
        // prevResult is used to ensure chained division (a÷b÷c÷d...) will always result in integers
        int prevResult = int.MaxValue;
        for (int x = 0; x < length; x++)
        {
            if (equation[x] == "÷")
            {
                prevResult = (prevResult == int.MaxValue) ? int.Parse(equation[x - 1]) : prevResult;
                // change the value of the divisor until the division results in an integer
                while (equation[x + 1] == "0" || prevResult % int.Parse(equation[x + 1]) != 0)
                {
                    equation[x + 1] = digits[UnityEngine.Random.Range(1, digits.Length)];
                }
                prevResult = DoOperation(prevResult, int.Parse(equation[x + 1]), "÷");
            }
            else if (x % 2 != 0)
            {
                prevResult = int.MaxValue;
            }
        }
        int solution = CalculateSolution(String.Join("", equation));
        // shuffle equation array in-place
        Shuffle(equation);
        return (equation, solution);
    }

    // UNUSED FUNCTION
    // Fisher-Yates shuffle in-place
    private static void Shuffle(string[] equation)
    {
        for (int i = equation.Length - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            string temp = equation[i];
            equation[i] = equation[j];
            equation[j] = temp;
        }
    }
}
