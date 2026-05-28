
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Testing.Shared;

public static class ValidationProblemDetailsExtentions
{


    public static void IsValid(this ValidationProblemDetails problemDetails,
        string[] expectedKeys,
        string[] expectedMessages
    )
    {
        Assert.NotNull(problemDetails);
        Assert.NotNull(problemDetails.Errors);

        Assert.Equal(expectedKeys.Length, expectedMessages.Length);

        var actualKeys = problemDetails.Errors.Keys;
        var actualErrors = problemDetails.Errors;

        for (int i = 0; i < expectedKeys.Length; i++)
        {
            var expectedKey = expectedKeys[i];
            var expectedMessage = expectedMessages[i];
            Assert.Contains(expectedKey, actualKeys);
            Assert.Contains(expectedMessage, actualErrors[expectedKey]);
        }
    }
}
