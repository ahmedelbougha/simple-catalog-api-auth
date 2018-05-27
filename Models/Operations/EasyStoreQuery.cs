using System;
using System.Collections.Generic;
using System.Security.Claims;
using aspnetcoregraphql.Auth;
using aspnetcoregraphql.Data.Repositories;
using aspnetcoregraphql.Models.Entities;
using aspnetcoregraphql.Models.Types;
using GraphQL;
using GraphQL.Authorization;
using GraphQL.Types;

namespace aspnetcoregraphql.Models.Operations
{
    // [GraphQLAuthorize(Policy = "MyPolicy")]
    public class EasyStoreQuery : ObjectGraphType
    {
        public EasyStoreQuery(ICategoryRepository categoryRepository, IProductRepository productRepository, ICustomerRepository customerRepository, IOrderRepository orderRepository)
        {
            this.AuthorizeWith("UserPolicy");
            
            Name = "StoreQuery";

            Field<CategoryType>(
                name: "category",
                description: "specific category data",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IntGraphType>> {Name = "id", Description = "Category id"}
                ),
                resolve: context => categoryRepository.GetCategoryAsync(context.GetArgument<int>("id")).Result
            );

            Field<ListGraphType<CategoryType>>(
                name: "categories",
                description: "list all categories",
                resolve: context => categoryRepository.CategoriesAsync().Result
            );

            Field<ProductType>(
                name: "product",
                description: "specific product data",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IntGraphType>> {Name = "id", Description = "Product id"}
                ),
                resolve: context => productRepository.GetProductAsync(context.GetArgument<int>("id")).Result
            );

            Field<ListGraphType<ProductType>>(
                name: "products",
                description: "list all products",
                resolve: context => productRepository.GetProductsAsync().Result
            );
            
            Field<CustomerType>(
                name: "customer",
                description: "specific customer data",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IntGraphType>> {Name = "id", Description = "Customer id"}
                ),
                resolve: context => customerRepository.GetCustomerAsync(context.GetArgument<int>("id")).Result
            ).AuthorizeWith("ManagerPolicy");

            Field<ListGraphType<CustomerType>>(
                name: "customers",
                description: "list all customers",
                resolve: context => customerRepository.CustomersAsync().Result
            ).AuthorizeWith("ManagerPolicy");

            Field<OrderType>(
                name: "order",
                description: "specific order data",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IntGraphType>> {Name = "id", Description = "Order id"}
                ),
                resolve: context => orderRepository.GetOrderAsync(context.GetArgument<int>("id")).Result
            ).AuthorizeWith("AdminPolicy");

            Field<ListGraphType<OrderType>>(
                name: "orders",
                description: "list all orders",
                resolve: context => orderRepository.OrdersAsync().Result
            ).AuthorizeWith("AdminPolicy");

            Field<ListGraphType<OrderType>>(
                name: "myorders",
                description: "list user orders",
                resolve: context => {
                    var userContext = context.UserContext.As<GraphQLUserContext>();
                    int id = Convert.ToInt32(userContext.User.FindFirstValue("userid"));
                    return orderRepository.GetOrdersWithByCustomerIdAsync(id).Result;
                }
            );                              
        }
    }
}