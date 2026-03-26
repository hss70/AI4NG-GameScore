using Microsoft.AspNetCore.Mvc.Filters;
using AI4NGGameScoresLambda.Helpers;

namespace AI4NGGameScoresLambda.Filters;

public sealed class ApiExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        context.Result = ApiExceptionMapper.Map(context.Exception);
        context.ExceptionHandled = true;
    }
}