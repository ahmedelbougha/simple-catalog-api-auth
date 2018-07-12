using System;
using System.Threading.Tasks;
using aspnetcoregraphql.Data;
using aspnetcoregraphql.Auth;
using aspnetcoregraphql.Models.Operations;
using GraphQL;
using GraphQL.Types;
using GraphQL.Validation;
using GraphQL.Validation.Complexity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using aspnetcoregraphql.Models.Entities;
using aspnetcoregraphql.Data.Repositories;
using System.Security.Claims;
using System.Collections.Generic;
using System.Web.Http.Filters;

namespace aspnetcoregraphql.Controllers
{
    [Route("graphql")]
    public class GraphQLController : Controller
    {
        private readonly IDocumentExecuter _documentExecuter;
        private readonly ISchema _schema;
        private readonly IEnumerable<IValidationRule> _validationRules;

        public GraphQLController(IDocumentExecuter documentExecuter,ISchema schema, IEnumerable<IValidationRule> validationRules)
        {
            _documentExecuter = documentExecuter;
            _schema = schema;
            _validationRules = validationRules;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] GraphQLQuery query, [FromHeader] string Authorization)
        {    
            if (query == null) { throw new ArgumentNullException(nameof(query)); }
            var validUsername = AuhtenticationManager.ValidateToken(Authorization);

            if (Authorization == null || validUsername == null)
            {
                return Unauthorized();
            }
            var userContext = new GraphQLUserContext();
            userContext.User = AuhtenticationManager.GetPrincipal(Authorization);

            var complexityConfiguration = new ComplexityConfiguration {
                // MaxDepth = 1,
                MaxComplexity = 85,
                FieldImpact = 5.0             
            };

            var executionOptions = new ExecutionOptions { 
                Schema = _schema, 
                Query = query.Query,
                OperationName = query.OperationName,
                Inputs = query.Variables.ToInputs(),
                // ComplexityConfiguration = complexityConfiguration,
                UserContext = userContext,
                ValidationRules = _validationRules,
                ExposeExceptions = true
            };

            try
            {
                var result = await _documentExecuter.ExecuteAsync(executionOptions).ConfigureAwait(false);

                if (result.Errors?.Count > 0)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch(Exception ex)
            {
                return BadRequest(ex);
            }
        }
    }
}