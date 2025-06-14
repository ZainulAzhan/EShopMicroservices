﻿namespace Basket.API.Basket.GetBasket;

public record GetBasketResponse(ShoppingCart Cart);
public class GetBasketEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/basket/{userName}", async (string userName, ISender sender) =>
        {
            var result = await sender.Send(new GetBasketQuery(userName));
            var response = result.Adapt<GetBasketResponse>();
            return Results.Ok(response);
        })
            .WithName("GetShoppingCart")
            .WithSummary("Get a user's shopping cart")
            .Produces<GetBasketResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Retrieves the shopping cart for the user with username");
    }
}
