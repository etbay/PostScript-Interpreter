using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PSInterpreter;
using PSInterpreter.Constants;
using System;

public class ArithmeticOperatorTests
{

    [SetUp]
    public void Setup()
    {
        Interpreter.Reset();
    }

    #region ADDITION

    [Test]
    public void AdditionOperatorTestIntInt()
    {
        Interpreter.ProcessInput("1");
        Interpreter.ProcessInput("1");
        Interpreter.ProcessInput("add");
        Assert.That(Interpreter.PeekStack(), Is.TypeOf<IntegerConstant>());
        IntegerConstant fc = (IntegerConstant)Interpreter.PeekStack();
        Assert.That(fc.Value, Is.EqualTo(2));
    }

    [Test]
    public void AdditionOperatorTestIntFloat()
    {
        Interpreter.ProcessInput("1");
        Interpreter.ProcessInput("2.5");
        Interpreter.ProcessInput("add");
        Assert.That(Interpreter.PeekStack(), Is.TypeOf<FloatConstant>());
        FloatConstant fc = (FloatConstant)Interpreter.PeekStack();
        Assert.That(fc.Value, Is.EqualTo(3.5));
    }

    [Test]
    public void AdditionOperatorTestFloatInt()
    {
        Interpreter.ProcessInput("2.5");
        Interpreter.ProcessInput("1");
        Interpreter.ProcessInput("add");
        Assert.That(Interpreter.PeekStack(), Is.TypeOf<FloatConstant>());
        FloatConstant fc = (FloatConstant)Interpreter.PeekStack();
        Assert.That(fc.Value, Is.EqualTo(3.5));
    }

    [Test]
    public void AdditionOperatorTestFloatFloatToFloat()
    {
        Interpreter.ProcessInput("2.5");
        Interpreter.ProcessInput("2.4");
        Interpreter.ProcessInput("add");
        Assert.That(Interpreter.PeekStack(), Is.TypeOf<FloatConstant>());
        FloatConstant fc = (FloatConstant)Interpreter.PeekStack();
        Assert.That(fc.Value, Is.EqualTo(4.9f).Within(0.0001f));
    }

    [Test]
    public void AdditionOperatorTestFloatFloatToInt()
    {
        Interpreter.ProcessInput("2.5");
        Interpreter.ProcessInput("2.5");
        Interpreter.ProcessInput("add");
        Assert.That(Interpreter.PeekStack(), Is.TypeOf<IntegerConstant>());
        IntegerConstant fc = (IntegerConstant)Interpreter.PeekStack();
        Assert.That(fc.Value, Is.EqualTo(5));
    }

    #endregion

    #region SUBTRACTION

    [Test]
    public void SubtractionOperatorTestIntInt()
    {
        Interpreter.ProcessInput("7");
        Interpreter.ProcessInput("5");
        Interpreter.ProcessInput("sub");
        Assert.That(Interpreter.PeekStack(), Is.TypeOf<IntegerConstant>());
        IntegerConstant fc = (IntegerConstant)Interpreter.PeekStack();
        Assert.That(fc.Value, Is.EqualTo(2));
    }

    [Test]
    public void SubtractionOperatorTestIntIntReverseOrder()
    {
        Interpreter.ProcessInput("5");
        Interpreter.ProcessInput("7");
        Interpreter.ProcessInput("sub");
        Assert.That(Interpreter.PeekStack(), Is.TypeOf<IntegerConstant>());
        IntegerConstant fc = (IntegerConstant)Interpreter.PeekStack();
        Assert.That(fc.Value, Is.EqualTo(-2));
    }

    [Test]
    public void SubtractionOperatorTestFloatInt()
    {
        Interpreter.ProcessInput("2.5");
        Interpreter.ProcessInput("1");
        Interpreter.ProcessInput("sub");
        Assert.That(Interpreter.PeekStack(), Is.TypeOf<FloatConstant>());
        FloatConstant fc = (FloatConstant)Interpreter.PeekStack();
        Assert.That(fc.Value, Is.EqualTo(1.5));
    }

    [Test]
    public void SubtractionOperatorTestFloatFloatToFloat()
    {
        Interpreter.ProcessInput("2.5");
        Interpreter.ProcessInput("2.4");
        Interpreter.ProcessInput("sub");
        Assert.That(Interpreter.PeekStack(), Is.TypeOf<FloatConstant>());
        FloatConstant fc = (FloatConstant)Interpreter.PeekStack();
        Assert.That(fc.Value, Is.EqualTo(0.1f).Within(0.0001f));
    }

    [Test]
    public void SubtractionOperatorTestFloatFloatToInt()
    {
        Interpreter.ProcessInput("2.5");
        Interpreter.ProcessInput("1.5");
        Interpreter.ProcessInput("sub");
        Assert.That(Interpreter.PeekStack(), Is.TypeOf<IntegerConstant>());
        IntegerConstant fc = (IntegerConstant)Interpreter.PeekStack();
        Assert.That(fc.Value, Is.EqualTo(1));
    }

    #endregion

    #region MULTIPLICATION

    [Test]
    public void MultiplicationOperatorTestIntInt()
    {
        Interpreter.ProcessInput("7");
        Interpreter.ProcessInput("3");
        Interpreter.ProcessInput("mul");
        Assert.That(Interpreter.PeekStack(), Is.TypeOf<IntegerConstant>());
        IntegerConstant fc = (IntegerConstant)Interpreter.PeekStack();
        Assert.That(fc.Value, Is.EqualTo(21));
    }

    [Test]
    public void MultiplicationOperatorTestFloatIntToFloat()
    {
        Interpreter.ProcessInput("2.5");
        Interpreter.ProcessInput("3");
        Interpreter.ProcessInput("mul");
        Assert.That(Interpreter.PeekStack(), Is.TypeOf<FloatConstant>());
        FloatConstant fc = (FloatConstant)Interpreter.PeekStack();
        Assert.That(fc.Value, Is.EqualTo(7.5));
    }

    [Test]
    public void MultiplicationOperatorTestFloatIntToInt()
    {
        Interpreter.ProcessInput("2.5");
        Interpreter.ProcessInput("2");
        Interpreter.ProcessInput("mul");
        Assert.That(Interpreter.PeekStack(), Is.TypeOf<IntegerConstant>());
        IntegerConstant fc = (IntegerConstant)Interpreter.PeekStack();
        Assert.That(fc.Value, Is.EqualTo(5));
    }

    [Test]
    public void MultiplicationOperatorTestFloatFloatToFloat()
    {
        Interpreter.ProcessInput("2.6");
        Interpreter.ProcessInput("5.6");
        Interpreter.ProcessInput("mul");
        Assert.That(Interpreter.PeekStack(), Is.TypeOf<FloatConstant>());
        FloatConstant fc = (FloatConstant)Interpreter.PeekStack();
        Assert.That(fc.Value, Is.EqualTo(14.56f).Within(0.0001f));
    }

    [Test]
    public void MultiplicationOperatorTestFloatFloatToInt()
    {
        Interpreter.ProcessInput("2.5");
        Interpreter.ProcessInput("5.6");
        Interpreter.ProcessInput("mul");
        Assert.That(Interpreter.PeekStack(), Is.TypeOf<IntegerConstant>());
        IntegerConstant fc = (IntegerConstant)Interpreter.PeekStack();
        Assert.That(fc.Value, Is.EqualTo(14));
    }

    #endregion

    #region DIVISION

    [Test]
    public void DivisionOperatorTestIntIntToInt()
    {
        Interpreter.ProcessInput("8");
        Interpreter.ProcessInput("4");
        Interpreter.ProcessInput("div");
        Assert.That(Interpreter.PeekStack(), Is.TypeOf<IntegerConstant>());
        IntegerConstant fc = (IntegerConstant)Interpreter.PeekStack();
        Assert.That(fc.Value, Is.EqualTo(2));
    }

    [Test]
    public void DivisionOperatorTestIntIntToFloat()
    {
        Interpreter.ProcessInput("16");
        Interpreter.ProcessInput("5");
        Interpreter.ProcessInput("div");
        Assert.That(Interpreter.PeekStack(), Is.TypeOf<FloatConstant>());
        FloatConstant fc = (FloatConstant)Interpreter.PeekStack();
        Assert.That(fc.Value, Is.EqualTo(3.2f).Within(0.0001f));
    }

    [Test]
    public void DivisionOperatorTestFloatInt()
    {
        Interpreter.ProcessInput("8.6");
        Interpreter.ProcessInput("4");
        Interpreter.ProcessInput("div");
        Assert.That(Interpreter.PeekStack(), Is.TypeOf<FloatConstant>());
        FloatConstant fc = (FloatConstant)Interpreter.PeekStack();
        Assert.That(fc.Value, Is.EqualTo(2.15f).Within(0.0001f));
    }

    [Test]
    public void DivisionOperatorTestFloatFloatToFloat()
    {
        Interpreter.ProcessInput("8.6");
        Interpreter.ProcessInput("2.5");
        Interpreter.ProcessInput("div");
        Assert.That(Interpreter.PeekStack(), Is.TypeOf<FloatConstant>());
        FloatConstant fc = (FloatConstant)Interpreter.PeekStack();
        Assert.That(fc.Value, Is.EqualTo(3.44f).Within(0.0001f));
    }

    [Test]
    public void DivisionOperatorTestFloatFloatToInt()
    {
        Interpreter.ProcessInput("5.5");
        Interpreter.ProcessInput("0.5");
        Interpreter.ProcessInput("div");
        Assert.That(Interpreter.PeekStack(), Is.TypeOf<IntegerConstant>());
        IntegerConstant fc = (IntegerConstant)Interpreter.PeekStack();
        Assert.That(fc.Value, Is.EqualTo(11));
    }

    [Test]
    public void DivisionOperatorTestIntDivideByZero()
    {
        Interpreter.ProcessInput("5");
        Interpreter.ProcessInput("0");
        Interpreter.ProcessInput("div");
        Assert.That(Interpreter.PeekStack(), Is.TypeOf<IntegerConstant>());
        IntegerConstant fc = (IntegerConstant)Interpreter.PeekStack();
        Assert.That(fc.Value, Is.EqualTo(0));
    }

    [Test]
    public void DivisionOperatorTestFloatDivideByZero()
    {
        Interpreter.ProcessInput("5.3");
        Interpreter.ProcessInput("0");
        Interpreter.ProcessInput("div");
        Assert.That(Interpreter.PeekStack(), Is.TypeOf<IntegerConstant>());
        IntegerConstant fc = (IntegerConstant)Interpreter.PeekStack();
        Assert.That(fc.Value, Is.EqualTo(0));
    }

    #endregion

    #region MODULAR_DIVISION

    [Test]
    public void ModularDivisionOperatorTestNoRemainder()
    {
        Interpreter.ProcessInput("8");
        Interpreter.ProcessInput("4");
        Interpreter.ProcessInput("mod");
        Assert.That(Interpreter.PeekStack(), Is.TypeOf<IntegerConstant>());
        IntegerConstant fc = (IntegerConstant)Interpreter.PeekStack();
        Assert.That(fc.Value, Is.EqualTo(0));
    }

    [Test]
    public void ModularDivisionOperatorTestRemainder()
    {
        Interpreter.ProcessInput("16");
        Interpreter.ProcessInput("5");
        Interpreter.ProcessInput("mod");
        Assert.That(Interpreter.PeekStack(), Is.TypeOf<IntegerConstant>());
        IntegerConstant fc = (IntegerConstant)Interpreter.PeekStack();
        Assert.That(fc.Value, Is.EqualTo(1));
    }

    #endregion

    #region INTEGER_DIVISION

    [Test]
    public void IntegerDivisionOperatorTestEqualsZero()
    {
        Interpreter.ProcessInput("16");
        Interpreter.ProcessInput("17");
        Interpreter.ProcessInput("idiv");
        Assert.That(Interpreter.PeekStack(), Is.TypeOf<IntegerConstant>());
        IntegerConstant fc = (IntegerConstant)Interpreter.PeekStack();
        Assert.That(fc.Value, Is.EqualTo(0));
    }

    [Test]
    public void IntegerDivisionOperatorTest()
    {
        Interpreter.ProcessInput("16");
        Interpreter.ProcessInput("7");
        Interpreter.ProcessInput("mod");
        Assert.That(Interpreter.PeekStack(), Is.TypeOf<IntegerConstant>());
        IntegerConstant fc = (IntegerConstant)Interpreter.PeekStack();
        Assert.That(fc.Value, Is.EqualTo(2));
    }

    #endregion
}