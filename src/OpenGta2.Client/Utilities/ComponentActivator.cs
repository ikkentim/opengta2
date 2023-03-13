using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Xna.Framework;

namespace OpenGta2.Client.Utilities;

public static class ComponentActivator<T> where T : IGameComponent
{
    private static readonly Func<GtaGame, T> _activator;

    static ComponentActivator()
    {
        _activator = CreateFactory();
    }

    private static Func<GtaGame, T> CreateFactory()
    {
        var constructors = typeof(T).GetConstructors();

        if (constructors.Length != 1)
            throw new InvalidOperationException("Only components with a single constructor can be activated.");

        var constructor = constructors.Single();
        var parameters = constructor.GetParameters();

        var gameArg = Expression.Parameter(typeof(GtaGame));
        var arguments = new Expression[parameters.Length];

        var serviceProvider = Expression.Property(gameArg, nameof(Game.Services));
        var getServiceMethod = typeof(GameServiceContainer).GetMethod(nameof(GameServiceContainer.GetService), Type.EmptyTypes)!;

        for (var i = 0; i < parameters.Length; i++)
        {
            var parameter = parameters[i];

            if (parameter.ParameterType == typeof(GtaGame))
                arguments[i] = gameArg;
            else if (parameter.ParameterType == typeof(Game))
            {
                arguments[i] = Expression.Convert(gameArg, typeof(Game));
            }
            else
            {
                var method = getServiceMethod.MakeGenericMethod(parameter.ParameterType);
                arguments[i] = Expression.Call(serviceProvider, method);
            }
        }


        var instance = Expression.New(constructor, arguments);

        return Expression.Lambda<Func<GtaGame, T>>(instance, gameArg).Compile();
    }

    public static T Activate(GtaGame game)
    {
        return _activator(game);
    }
}