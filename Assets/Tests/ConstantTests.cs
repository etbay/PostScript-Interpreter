using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PSInterpreter;
using PSInterpreter.Constants;

public class ConstantTests
{

    [SetUp]
    public void Setup()
    {
        Interpreter.Reset();
    }

    [Test]
    public void IntegerConstantTest()
    {
        Interpreter.ProcessInput("4");
        Assert.That(Interpreter.PeekStack(), Is.TypeOf<IntegerConstant>());
        IntegerConstant fc = (IntegerConstant)Interpreter.PeekStack();
        Assert.That(fc.Value, Is.EqualTo(4));
    }

    [Test]
    public void FloatConstantTest()
    {
        Interpreter.ProcessInput("4.0");
        Assert.That(Interpreter.PeekStack(), Is.TypeOf<FloatConstant>());
        FloatConstant fc = (FloatConstant)Interpreter.PeekStack();
        Assert.That(fc.Value, Is.EqualTo(4.0f));
    }

    [Test]
    public void BooleanConstantTest()
    {
        Interpreter.ProcessInput("true");
        Assert.That(Interpreter.PeekStack(), Is.TypeOf<BooleanConstant>());
        BooleanConstant fc = (BooleanConstant)Interpreter.PeekStack();
        Assert.That(fc.Value, Is.EqualTo(true));
    }
}
