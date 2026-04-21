using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using MythApi.Gods.Interfaces;
using MythApi.Common.Database.Models;
using MythApi.Gods.Models;

namespace MythApi.Endpoints.v1;
public static class Gods {
    public static void RegisterGodEndpoints(this IEndpointRouteBuilder endpoints) {
        
        var gods = endpoints.MapGroup("/api/v1/gods");


        gods.MapGet("", GetAlllGods);
        gods.MapGet("{id}", (int id, IGodRepository repository) => repository.GetGodAsync(new GodParameter(id)));
        gods.MapGet("search/{name:regex(^[\\w\\s\\-]{{1,200}}$)}", (string name, IGodRepository repository, [FromQuery] bool includeAliases = false) => repository.GetGodByNameAsync(new GodByNameParameter(name, includeAliases)));
        gods.MapPost("", AddOrUpdateGods);
    }

    public static async Task<IResult> AddOrUpdateGods(List<GodInput> gods, IGodRepository repository)
    {
        Dictionary<string, List<string>>? errors = null;

        for (var index = 0; index < gods.Count; index++)
        {
            var validationContext = new ValidationContext(gods[index]);
            var validationResults = new List<ValidationResult>();
            if (Validator.TryValidateObject(gods[index], validationContext, validationResults, validateAllProperties: true))
            {
                continue;
            }

            errors ??= new Dictionary<string, List<string>>();
            foreach (var result in validationResults)
            {
                foreach (var memberName in result.MemberNames.DefaultIfEmpty(nameof(GodInput)))
                {
                    var key = $"gods[{index}].{memberName}";
                    if (!errors.TryGetValue(key, out var memberErrors))
                    {
                        memberErrors = new List<string>();
                        errors[key] = memberErrors;
                    }

                    memberErrors.Add(result.ErrorMessage ?? "Validation failed.");
                }
            }
        }

        if (errors is not null)
        {
            return Results.ValidationProblem(errors.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToArray()));
        }

        var updatedGods = await repository.AddOrUpdateGods(gods);
        return Results.Ok(updatedGods);
    }

    public static Task<IList<God>> GetAlllGods(IGodRepository repository) => repository.GetAllGodsAsync();
}
