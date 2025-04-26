using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PSInterpreter;
using PSInterpreter.Constants;

public class OperatorTests
{

    [SetUp]
    public void Setup()
    {
        Interpreter.Reset();
    }

    [TestCase("1", "1", "add", ExpectedResult = 2)]
    [TestCase("1", "2.5", "add", ExpectedResult = 3.5)]
    [TestCase("2.5", "1", "add", ExpectedResult = 3.5)]
    [TestCase("2.5", "2.5", "add", ExpectedResult = 5)]
    public object AddOperatorTest(string val1, string val2, string op)
    {
        Interpreter.ProcessInput(val1);
        Interpreter.ProcessInput(val2);
        Interpreter.ProcessInput(op);

        // Return the value of the stack after the operation
        return Interpreter.PeekStack();
    }
}