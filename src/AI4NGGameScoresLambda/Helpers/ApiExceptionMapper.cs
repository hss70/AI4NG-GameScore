using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace AI4NGGameScoresLambda.Helpers;

public static class ApiExceptionMapper
{
    public static IActionResult Map(Exception ex)
    {
        return ex switch
        {
            // Client errors
            ArgumentException => new BadRequestObjectResult(new { error = ex.Message }),
            FormatException => new BadRequestObjectResult(new { error = ex.Message }),
            KeyNotFoundException => new NotFoundObjectResult(new { error = ex.Message }),

            // Business/conflict
            InvalidOperationException => new ConflictObjectResult(new { error = ex.Message }),
            ConditionalCheckFailedException => new ConflictObjectResult(new { error = ex.Message }),
            TransactionCanceledException txEx when IsConditionalTransactionFailure(txEx)
                => new ConflictObjectResult(new { error = "Request could not be completed because it conflicts with existing data." }),

            // Auth
            UnauthorizedAccessException => new UnauthorizedObjectResult(new { error = ex.Message }),
            //ForbiddenException => new ObjectResult(new { error = ex.Message }) { StatusCode = 403 }, // add for the researcher endpoints. Look at experiments api

            // Timeout
            TimeoutException => new ObjectResult(new { error = "Request timeout" })
            {
                StatusCode = 408
            },

            // Dynamo/AWS throttling
            ProvisionedThroughputExceededException => Throttle(),
            RequestLimitExceededException => Throttle(),
            LimitExceededException => Throttle(),
            AmazonServiceException awsEx when IsThrottle(awsEx)
                => Throttle(),

            // Any other AWS error
            AmazonServiceException => new ObjectResult(new { error = "AWS service temporarily unavailable" })
            {
                StatusCode = 503
            },

            // Fallback
            _ => new ObjectResult(new
            {
                error = "Internal server error",
                details = IncludeDetails() ? ex.Message : null
            })
            {
                StatusCode = 500
            }
        };
    }

    private static ObjectResult Throttle()
        => new(new { error = "Service throttling, please retry" })
        {
            StatusCode = 503
        };

    private static bool IsThrottle(AmazonServiceException ex)
    {
        var code = ex.ErrorCode ?? string.Empty;

        return code.Equals("Throttling", StringComparison.OrdinalIgnoreCase)
            || code.Equals("ThrottlingException", StringComparison.OrdinalIgnoreCase)
            || code.Equals("RequestLimitExceeded", StringComparison.OrdinalIgnoreCase)
            || code.Equals("TooManyRequestsException", StringComparison.OrdinalIgnoreCase)
            || ex.StatusCode == HttpStatusCode.TooManyRequests;
    }

    private static bool IsConditionalTransactionFailure(TransactionCanceledException ex)
    {
        return ex.CancellationReasons?.Any(r =>
            string.Equals(r.Code, "ConditionalCheckFailed", StringComparison.Ordinal)) == true;
    }

    private static bool IncludeDetails()
    {
        var env = Environment.GetEnvironmentVariable("Environment")
                  ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                  ?? "prod";

        return env.Equals("dev", StringComparison.OrdinalIgnoreCase)
            || env.Equals("development", StringComparison.OrdinalIgnoreCase);
    }
}