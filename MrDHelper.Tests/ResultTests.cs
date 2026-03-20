using MrDHelper.Models;

namespace MrDHelper.Tests;

[TestFixture]
public class ResultTests
{
    [Test]
    public void Ok_ShouldTrimMessage_AndRemainSuccessful()
    {
        var result = Result.Ok("  done  ");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.IsFailure, Is.False);
            Assert.That(result.Message, Is.EqualTo("done"));
            Assert.That(result.FirstError, Is.EqualTo(Error.None));
            Assert.That(result.ToString(), Is.EqualTo("Success: done"));
        });
    }

    [Test]
    public void Failure_WithMessage_ShouldNormalizeMessageAndFirstError()
    {
        var result = Result.Failure("  failed to save  ");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Message, Is.EqualTo("failed to save"));
            Assert.That(result.FirstError.Code, Is.EqualTo("UNDEFINED-CODE"));
            Assert.That(result.FirstError.Description, Is.EqualTo("failed to save"));
        });
    }

    [Test]
    public void Failure_WithError_ShouldAlignMessageToDescription()
    {
        var result = Result.Failure(new Error("E-01", "  invalid state  "));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Message, Is.EqualTo("invalid state"));
            Assert.That(result.FirstError.Code, Is.EqualTo("E-01"));
            Assert.That(result.FirstError.Description, Is.EqualTo("invalid state"));
        });
    }

    [Test]
    public void ThrowIfFailure_ShouldThrowUsingFirstErrorMessage()
    {
        var result = Result.Failure("E-02", "broken");

        var ex = Assert.Throws<InvalidOperationException>(() => result.ThrowIfFailure());

        Assert.That(ex!.Message, Is.EqualTo(result.FirstError.ToString()));
    }

    [Test]
    public void GenericOk_WithNullReferenceValue_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Result<string>.Ok(null!));
    }

    [Test]
    public void GenericMatch_OnFailure_ShouldUseFailureBranch()
    {
        var result = Result<int>.Failure(new Error("E-03", "no value"));

        var matched = result.Match(
            onSuccess: value => $"value:{value}",
            onFailure: error => $"{error.Code}:{error.Description}");

        Assert.That(matched, Is.EqualTo("E-03:no value"));
    }

    [Test]
    public void Map_OnSuccess_ShouldProjectValue_AndPreserveMessage()
    {
        var result = Result<int>.Ok(7, "  ready  ");

        var mapped = result.Map(value => $"#{value * 2}");

        Assert.Multiple(() =>
        {
            Assert.That(mapped.IsSuccess, Is.True);
            Assert.That(mapped.Value, Is.EqualTo("#14"));
            Assert.That(mapped.Message, Is.EqualTo("ready"));
        });
    }

    [Test]
    public void Map_OnFailure_ShouldNotInvokeMapper_AndShouldPreserveFailure()
    {
        var invoked = false;
        var result = Result<int>.Failure("E-04", "missing");

        var mapped = result.Map<string>(_ =>
        {
            invoked = true;
            return "unexpected";
        });

        Assert.Multiple(() =>
        {
            Assert.That(invoked, Is.False);
            Assert.That(mapped.IsFailure, Is.True);
            Assert.That(mapped.Message, Is.EqualTo("missing"));
            Assert.That(mapped.FirstError.Code, Is.EqualTo("E-04"));
        });
    }

    [Test]
    public void Bind_OnFailure_ShouldNotInvokeBinder()
    {
        var invoked = false;
        var result = Result<int>.Failure("E-05", "stopped");

        var bound = result.Bind(_ =>
        {
            invoked = true;
            return Result<string>.Ok("unexpected");
        });

        Assert.Multiple(() =>
        {
            Assert.That(invoked, Is.False);
            Assert.That(bound.IsFailure, Is.True);
            Assert.That(bound.Message, Is.EqualTo("stopped"));
            Assert.That(bound.FirstError.Code, Is.EqualTo("E-05"));
        });
    }
}
