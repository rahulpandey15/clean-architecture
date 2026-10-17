using eCommerce.Application;
using eCommerce.Application.Contracts;
using eCommerce.Infrastructure;
using eCommerce.Infrastructure.Background;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;


namespace eCommerce.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();


            builder.Services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = "127.0.0.1:6379";
                options.InstanceName = "redis-instance";
            });

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!)),
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        ValidateAudience = true,
                        ValidateIssuer = true,
                        ClockSkew = TimeSpan.Zero
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = async context =>
                        {
                            var sidClaim = context.Principal?.FindFirst(ClaimTypes.Sid).Value; ;
                            if(sidClaim == null)
                            {
                                context.Fail("Invalid Token: Session Id is missing");
                            }


                            var revocationService
                                     = context.HttpContext.RequestServices.GetRequiredService<ITokenRevocationService>();

                            bool isRevoked = await revocationService.IsSessionRevokedAsync(sidClaim);

                            if (isRevoked)
                                context.Fail("Session is revoked");
                        }
                    };

                });


            builder.Services.AddApplication();
            builder.Services.AddInfrastructure(
                builder.Configuration.GetConnectionString("DatabaseConnection")!);

            // register background worker hosted service (from infrastructure)
            builder.Services.AddHostedService<OrderProcessingService>();

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            builder.Services.AddSwaggerGen();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
