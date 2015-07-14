using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ExpressionActivator
{
    public delegate T ActivatorDelegate<out T>(params object[] args);

    public delegate object ActivatorDelegate(params object[] args);

    public static class ExpressionActivator
    {
        public static ActivatorDelegate<T> CreateActivator<T>()
        {
            return CreateActivator<T>(Type.EmptyTypes);
        }

        public static ActivatorDelegate<T> CreateActivator<T>(params Type[] parameters)
        {
            var constructor = typeof(T).GetConstructor(parameters);
            if (constructor == null)
            {
                throw new ConstructorNotFoundException(typeof(T), parameters);
            }

            return constructor.CreateActivator<T>();
        }

        public static ActivatorDelegate<T> CreateActivator<T>(this ConstructorInfo constructor)
        {
            if (!typeof(T).IsAssignableFrom(constructor.DeclaringType))
            {
                throw new TypeMismatchException(typeof(T), constructor.DeclaringType);
            }

            return constructor.CreateActivator().Cast<T>();
        }

        public static ActivatorDelegate CreateActivator(this Type type)
        {
            return type.CreateActivator(Type.EmptyTypes);
        }

        public static ActivatorDelegate CreateActivator(this Type type, params Type[] parameters)
        {
            var constructor = type.GetConstructor(parameters);
            if (constructor == null)
            {
                throw new ConstructorNotFoundException(type, parameters);
            }

            return constructor.CreateActivator();
        }

        public static ActivatorDelegate CreateActivator(this ConstructorInfo constructor)
        {
            if (constructor == null)
            {
                throw new ArgumentNullException("constructor");
            }

            var parameter = Expression.Parameter(typeof(object[]), "args");

            var argsExpressions = constructor.GetParameters()
                .Select(CreateArgumentExpression)
                .Select(x => x.Invoke(parameter))
                .Cast<Expression>()
                .ToArray();

            var newExpression = Expression.New(constructor, argsExpressions);

            var lambdaExpression = Expression.Lambda(typeof(ActivatorDelegate), newExpression, parameter);

            return (ActivatorDelegate) lambdaExpression.Compile();
        }

        public static ActivatorDelegate<T> Cast<T>(this ActivatorDelegate activator)
        {
            return args => (T) activator.Invoke(args);
        }

        private static Func<ParameterExpression, UnaryExpression> CreateArgumentExpression(ParameterInfo parameterInfo, int index)
        {
            return parameterExpression =>
            {
                var indexExpression = Expression.Constant(index);

                var arrayAccessorExpression = Expression.ArrayIndex(parameterExpression, indexExpression);

                return Expression.Convert(arrayAccessorExpression, parameterInfo.ParameterType);
            };
        }
    }

    public class ConstructorNotFoundException : Exception
    {
        private const string ErrorMessage = "Could not find a constructor for type '{0}' with the following argument types: {1}";

        public ConstructorNotFoundException(Type type, IEnumerable<Type> parameterTypes)
            : base(string.Format(ErrorMessage, type.Name, string.Join(", ", parameterTypes.Select(x => x.Name))))
        {
        }
    }

    public class TypeMismatchException : Exception
    {
        private const string ErrorMessage = "Type '{0}' must be assignable to type '{1}' in order to create a typed activator.";

        public TypeMismatchException(Type type, Type declaringType)
            : base(string.Format(ErrorMessage, declaringType.Name, type.Name))
        {
        }
    }
}