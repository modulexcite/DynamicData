﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace DynamicData.Kernel
{

	/// <summary>
	/// Extensions for optional
	/// </summary>
	public static class OptionExtensions
    {
        /// <summary>
        /// Returns the value if the optional has a value, otherwise returns the result of the value selector
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="valueSelector">The value selector.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">valueSelector</exception>
        public static T ValueOr<T>(this Optional<T> source, Func<T> valueSelector )
        {
            if (valueSelector == null) throw new ArgumentNullException("valueSelector");
            return source.HasValue ? source.Value : valueSelector();
        }

        /// <summary>
        /// Returns the value if the optional has a value, otherwise returns the default value of T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static T ValueOrDefault<T>(this Optional<T> source)
        {
            return source.HasValue ? source.Value : default(T);
        }

        /// <summary>
        /// Returns the value if the optional has a value, otherwise throws an exception as specified by the exception generator
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="exceptionGenerator">The exception generator.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">exceptionGenerator</exception>
        public static T ValueOrThrow<T>(this Optional<T> source, Func<Exception> exceptionGenerator)
        {
            if (exceptionGenerator == null) throw new ArgumentNullException("exceptionGenerator");
            if (source.HasValue)
                return source.Value;

            throw  exceptionGenerator();
        }




        /// <summary>
        /// Converts the option value if it has a value, otherwise returns the result of the fallback converter
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TDestination">The type of the destination.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="converter">The converter.</param>
        /// <param name="fallbackConverter">The fallback converter.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">
        /// converter
        /// or
        /// fallbackConverter
        /// </exception>
        public static TDestination ConvertOr<TSource, TDestination>(this Optional<TSource> source, Func<TSource, TDestination> converter, Func<TDestination> fallbackConverter)
        {
            if (converter == null) throw new ArgumentNullException("converter");
            if (fallbackConverter == null) throw new ArgumentNullException("fallbackConverter");

            return source.HasValue ? converter(source.Value) : fallbackConverter();
        }

        /// <summary>
        /// Converts the specified source.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TDestination">The type of the destination.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="converter">The converter.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">converter</exception>
        public static Optional<TDestination> Convert<TSource, TDestination>(this Optional<TSource> source, Func<TSource, TDestination> converter)
        {
            if (converter == null) throw new ArgumentNullException("converter");
            return source.HasValue ? converter(source.Value) : Optional.None<TDestination>();
        }


		/// <summary>
		/// Filters where Optional<typeparam name="T"></typeparam> has a value
		/// and return the values only
		/// </summary>
		/// <param name="source">The source.</param>
		/// <returns></returns>
		public static IEnumerable<T> SelectValues<T>(this IEnumerable<Optional<T>> source)
        {
            return source.Where(t => t.HasValue).Select(t => t.Value);
        }


		/// <summary>
		/// Overloads a TryGetValue of the dictionary wrapping the result as an Optional<typeparam>
		///         <name>&amp;gt;TValue</name>
		///     </typeparam>
		/// </summary>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <param name="source">The source.</param>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		public static Optional<TValue> Lookup<TValue, TKey>(this IDictionary<TKey, TValue> source, TKey key)
        {
            TValue contained;
            bool result = source.TryGetValue(key, out contained);
            return result ? contained : Optional.None<TValue>();
        }


        /// <summary>
        /// Removes item if contained in the cache
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static bool RemoveIfContained<TValue, TKey>(this IDictionary<TKey, TValue> source, TKey key)
        {
            if (source.ContainsKey(key))
              return  source.Remove(key);

            return false;
        }

		/// <summary>
		/// Overloads Enumerable.FirstOrDefault() and wraps the result in a Optional<typeparam>
		///         <name>&amp;gt;T</name>
		///     </typeparam> container
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source">The source.</param>
		/// <param name="selector">The selector.</param>
		/// <returns></returns>
		public static Optional<T> FirstOrOptional<T>(this IEnumerable<T> source, Func<T, bool> selector)
        {
            var result = source.FirstOrDefault(selector);
            return !Equals(result, null) ? result : Optional.None<T>();
        }


        /// <summary>
        /// Invokes the specified action when 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        public static OptionElse IfHasValue<T>(this Optional<T> source,  Action<T> action)
        {
            if (!source.HasValue) return new OptionElse();
            action(source.Value);
            return OptionElse.NoAction;
        }
    }
}