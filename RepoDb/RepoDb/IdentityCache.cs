﻿using RepoDb.Extensions;
using System.Collections.Concurrent;
using System.Reflection;

namespace RepoDb
{
    /// <summary>
    /// A class used to cache the identity property of the entity.
    /// </summary>
    public static class IdentityCache
    {
        private static readonly ConcurrentDictionary<string, PropertyInfo> m_cache = new ConcurrentDictionary<string, PropertyInfo>();

        /// <summary>
        /// Gets the cached identity key property for the entity.
        /// </summary>
        /// <typeparam name="TEntity">The type of the target entity.</typeparam>
        /// <returns>The cached identity property.</returns>
        public static PropertyInfo Get<TEntity>()
            where TEntity : class
        {
            var type = typeof(TEntity);
            var key = type.FullName;
            var property = (PropertyInfo)null;
            if (m_cache.TryGetValue(key, out property) == false)
            {
                property = DataEntityExtension.GetIdentityProperty(type);
                m_cache.TryAdd(key, property);
            }
            return property;
        }
    }
}