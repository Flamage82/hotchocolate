using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using HotChocolate.Execution;
using Newtonsoft.Json;

#if ASPNETCLASSIC
using Microsoft.Owin;
using HttpContext = Microsoft.Owin.IOwinContext;
using RequestDelegate = Microsoft.Owin.OwinMiddleware;
#else
using Microsoft.AspNetCore.Http;
#endif

#if ASPNETCLASSIC
namespace HotChocolate.AspNetClassic
#else
namespace HotChocolate.AspNetCore
#endif
{
    public class PostQueryMiddleware
        : QueryMiddlewareBase
    {
        public PostQueryMiddleware(
            RequestDelegate next,
            IQueryExecutor queryExecutor,
            IQueryResultSerializer resultSerializer,
            QueryMiddlewareOptions options)
                : base(next, queryExecutor, resultSerializer, options)
        { }

        protected override bool CanHandleRequest(HttpContext context)
        {
            return string.Equals(
                context.Request.Method,
                HttpMethods.Post,
                StringComparison.Ordinal);
        }

        protected override async Task<IQueryRequestBuilder>
            CreateQueryRequestAsync(HttpContext context)
        {
            QueryRequestDto request = await ReadRequestAsync(context)
                .ConfigureAwait(false);
#if ASPNETCLASSIC
            IServiceProvider serviceProvider = context.CreateRequestServices(
                Executor.Schema.Services);
#else
            IServiceProvider serviceProvider = context.CreateRequestServices();
#endif
            return QueryRequestBuilder.New()
                .SetQuery(request.Query)
                .SetOperation(request.OperationName)
                .SetVariableValues(
                    QueryMiddlewareUtilities.ToDictionary(request.Variables))
                .SetServices(serviceProvider);
        }

        private static async Task<QueryRequestDto> ReadRequestAsync(
            HttpContext context)
        {
            using (var reader = new StreamReader(context.Request.Body,
                Encoding.UTF8))
            {
                string content = await reader.ReadToEndAsync()
                    .ConfigureAwait(false);

                switch (context.Request.ContentType.Split(';')[0])
                {
                    case ContentType.Json:
                        return JsonConvert.DeserializeObject<QueryRequestDto>(
                            content, QueryMiddlewareUtilities.JsonSettings);

                    case ContentType.GraphQL:
                        return new QueryRequestDto { Query = content };

                    default:
                        throw new NotSupportedException();
                }
            }
        }
    }
}
