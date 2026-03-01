using System;
using System.Linq;
using System.Reflection;
using Flow.Launcher.Plugin;
using Flow.Launcher.Plugin.SharedModels;
using Moq;
using Xunit;

namespace Flow.Launcher.Plugin.MultiprofileBookmarks.Tests
{
    public class FlowLauncherFuzzyMatcherTests
    {
        [Fact]
        public void FuzzySearch_WithMatchingString_ReturnsPositiveScore()
        {
            var fuzzySearchMethod = FindFuzzySearchMethod();
            var args = BuildArguments(fuzzySearchMethod.GetParameters(), "flb", "Flow Launcher Bookmarks");

            var match = fuzzySearchMethod.Invoke(null, args) as MatchResult;

            Assert.NotNull(match);
            Assert.True(match!.Score > 0);
        }

        private static MethodInfo FindFuzzySearchMethod()
        {
            var assembly = typeof(PluginInitContext).Assembly;
            var method = assembly
                .GetTypes()
                .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Static))
                .Where(m => m.Name == "FuzzySearch" && m.ReturnType == typeof(MatchResult))
                .OrderByDescending(MethodPriority)
                .ThenBy(m => m.GetParameters().Length)
                .FirstOrDefault();

            Assert.True(method is not null, "Could not find public static FuzzySearch in Flow.Launcher.Plugin.");
            return method!;
        }

        private static int MethodPriority(MethodInfo method)
        {
            var parameters = method.GetParameters();
            if (parameters.Length >= 2 &&
                parameters[0].ParameterType == typeof(string) &&
                parameters[1].ParameterType == typeof(string))
            {
                return 2;
            }

            if (parameters.Length >= 3 &&
                parameters[0].ParameterType == typeof(IPublicAPI) &&
                parameters[1].ParameterType == typeof(string) &&
                parameters[2].ParameterType == typeof(string))
            {
                return 1;
            }

            return 0;
        }

        private static object?[] BuildArguments(ParameterInfo[] parameters, string query, string valueToMatch)
        {
            var apiMock = new Mock<IPublicAPI>();
            var args = new object?[parameters.Length];
            var stringIndex = 0;

            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];

                if (parameter.ParameterType == typeof(string))
                {
                    args[i] = stringIndex++ == 0 ? query : valueToMatch;
                    continue;
                }

                if (parameter.ParameterType == typeof(IPublicAPI))
                {
                    args[i] = apiMock.Object;
                    continue;
                }

                if (parameter.HasDefaultValue)
                {
                    args[i] = parameter.DefaultValue;
                    continue;
                }

                args[i] = parameter.ParameterType.IsValueType
                    ? Activator.CreateInstance(parameter.ParameterType)
                    : null;
            }

            return args;
        }
    }
}
