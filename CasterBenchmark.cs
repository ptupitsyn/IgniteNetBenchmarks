using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Emit;
using System.Text;
using BenchmarkDotNet.Attributes;

namespace IgniteNetBenchmarks
{
    public class CasterBenchmark
    {
        private static readonly long Val = long.MaxValue/2;

        [Benchmark]
        public void TypeCaster()
        {
            var x = TypeCaster<long>.Cast(Val);

            if (x < 3)
                throw new Exception();
        }

        [Benchmark]
        public void ObjectCast()
        {
            var x = TypeCaster2<long>.Cast(Val);

            if (x < 3)
                throw new Exception();
        }

        [Benchmark]
        public void UnsafeCast()
        {
            var x = TypeCaster3<long>.Cast(Val);

            if (x < 3)
                throw new Exception();
        }
    }

    public static class TypeCaster2<T>
    {
        /// <summary>
        /// Efficiently casts an object from TFrom to T.
        /// Does not cause boxing for value types.
        /// </summary>
        /// <typeparam name="TFrom">Source type to cast from.</typeparam>
        /// <param name="obj">The object to cast.</param>
        /// <returns>Casted object.</returns>
        public static T Cast<TFrom>(TFrom obj)
        {
            return (T) (object) obj;
        }
    }

    public static class TypeCaster<T>
    {
        /// <summary>
        /// Efficiently casts an object from TFrom to T.
        /// Does not cause boxing for value types.
        /// </summary>
        /// <typeparam name="TFrom">Source type to cast from.</typeparam>
        /// <param name="obj">The object to cast.</param>
        /// <returns>Casted object.</returns>
        public static T Cast<TFrom>(TFrom obj)
        {
            return Casters<TFrom>.Caster(obj);
        }

        /// <summary>
        /// Inner class serving as a cache.
        /// </summary>
        private static class Casters<TFrom>
        {
            /// <summary>
            /// Compiled caster delegate.
            /// </summary>
            internal static readonly Func<TFrom, T> Caster = Compile();

            /// <summary>
            /// Compiles caster delegate.
            /// </summary>
            private static Func<TFrom, T> Compile()
            {
                if (typeof(T) == typeof(TFrom))
                {
                    // Just return what we have
                    var pExpr = Expression.Parameter(typeof(TFrom));

                    return Expression.Lambda<Func<TFrom, T>>(pExpr, pExpr).Compile();
                }

                var paramExpr = Expression.Parameter(typeof(TFrom));
                var convertExpr = Expression.Convert(paramExpr, typeof(T));

                return Expression.Lambda<Func<TFrom, T>>(convertExpr, paramExpr).Compile();
            }
        }
    }

    public static class TypeCaster3<T>
    {
        /// <summary>
        /// Efficiently casts an object from TFrom to T.
        /// Does not cause boxing for value types.
        /// </summary>
        /// <typeparam name="TFrom">Source type to cast from.</typeparam>
        /// <param name="obj">The object to cast.</param>
        /// <returns>Casted object.</returns>
        public static T Cast<TFrom>(TFrom obj)
        {
            return Casters<TFrom>.Caster(obj);
        }

        /// <summary>
        /// Inner class serving as a cache.
        /// </summary>
        private static class Casters<TFrom>
        {
            /// <summary>
            /// Compiled caster delegate.
            /// </summary>
            internal static readonly Func<TFrom, T> Caster = Compile();

            /// <summary>
            /// Compiles caster delegate.
            /// </summary>
            private static Func<TFrom, T> Compile()
            {
                var method = new DynamicMethod(string.Empty, typeof(T), new[] { typeof(TFrom) }, typeof(Casters<TFrom>), true);

                var il = method.GetILGenerator();

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ret);

                return (Func<TFrom, T>) method.CreateDelegate(typeof (Func<TFrom, T>));
            }
        }
    }
}
