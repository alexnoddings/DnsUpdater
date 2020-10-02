﻿using Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DnsUpdater.Google
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGoogleDnsUpdaterService(this IServiceCollection services)
        {
            services.AddScoped<IDnsRecordUpdater, GoogleDnsUpdaterService>();

            return services;
        }
    }
}