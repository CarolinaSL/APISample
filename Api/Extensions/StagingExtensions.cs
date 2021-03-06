﻿using Data.EFCore.Repositorios;
using Domain.Repositorios;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Extensions
{
    /// <summary>
    /// Classe de extensões específicas para ambiente de staging
    /// </summary>
    public static class StagingExtensions
    {
        /// <summary>
        /// Adiciona os repositórios na injeção de dependência
        /// </summary>
        /// <param name="services"></param>
        public static void AddStagingServices(this IServiceCollection services)
        {
            services.AddSingleton<IItemRepository, EFCoreItemRepository>();
        }
    }
}
