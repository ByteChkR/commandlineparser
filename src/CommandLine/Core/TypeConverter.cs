﻿// Copyright 2005-2015 Giacomo Stelluti Scala & Contributors. All rights reserved. See License.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

using CommandLine.Infrastructure;

using CSharpx;

using RailwaySharp.ErrorHandling;

namespace CommandLine.Core
{
    internal static class TypeConverter
    {
        public static Maybe<object> ChangeType(IEnumerable<string> values,
                                               Type conversionType,
                                               bool scalar,
                                               bool isFlag,
                                               CultureInfo conversionCulture,
                                               bool ignoreValueCase)
        {
            return isFlag ? ChangeTypeFlagCounter(values, conversionType, conversionCulture, ignoreValueCase) :
                   scalar ? ChangeTypeScalar(values.Last(), conversionType, conversionCulture, ignoreValueCase) :
                   ChangeTypeSequence(values, conversionType, conversionCulture, ignoreValueCase);
        }

        private static Maybe<object> ChangeTypeSequence(IEnumerable<string> values,
                                                        Type conversionType,
                                                        CultureInfo conversionCulture,
                                                        bool ignoreValueCase)
        {
            Type type =
                conversionType.GetTypeInfo()
                              .GetGenericArguments()
                              .SingleOrDefault()
                              .ToMaybe()
                              .FromJustOrFail(new
                                                  InvalidOperationException("Non scalar properties should be sequence of type IEnumerable<T>."
                                                                           )
                                             );

            IEnumerable<Maybe<object>> converted =
                values.Select(value => ChangeTypeScalar(value, type, conversionCulture, ignoreValueCase));

            return converted.Any(a => a.MatchNothing())
                       ? Maybe.Nothing<object>()
                       : Maybe.Just(converted.Select(c => ((Just<object>)c).Value)
                                             .ToUntypedArray(type)
                                   );
        }

        private static Maybe<object> ChangeTypeScalar(string value,
                                                      Type conversionType,
                                                      CultureInfo conversionCulture,
                                                      bool ignoreValueCase)
        {
            Result<object, Exception> result =
                ChangeTypeScalarImpl(value, conversionType, conversionCulture, ignoreValueCase);

            result.Match((_, __) => { },
                         e => e.First()
                               .RethrowWhenAbsentIn(new[]
                                                    {
                                                        typeof(InvalidCastException),
                                                        typeof(FormatException),
                                                        typeof(OverflowException),
                                                    }
                                                   )
                        );

            return result.ToMaybe();
        }

        private static Maybe<object> ChangeTypeFlagCounter(IEnumerable<string> values,
                                                           Type conversionType,
                                                           CultureInfo conversionCulture,
                                                           bool ignoreValueCase)
        {
            IEnumerable<Maybe<object>> converted =
                values.Select(value => ChangeTypeScalar(value, typeof(bool), conversionCulture, ignoreValueCase));

            return converted.Any(maybe => maybe.MatchNothing())
                       ? Maybe.Nothing<object>()
                       : Maybe.Just((object)converted.Count(value => value.IsJust()));
        }

        private static object ConvertString(string value, Type type, CultureInfo conversionCulture)
        {
            try
            {
                return Convert.ChangeType(value, type, conversionCulture);
            }
            catch (InvalidCastException)
            {
                // Required for converting from string to TimeSpan because Convert.ChangeType can't
                return System.ComponentModel.TypeDescriptor.GetConverter(type)
                             .ConvertFrom(null, conversionCulture, value);
            }
        }

        private static Result<object, Exception> ChangeTypeScalarImpl(string value,
                                                                      Type conversionType,
                                                                      CultureInfo conversionCulture,
                                                                      bool ignoreValueCase)
        {
            Func<object> changeType = () =>
            {
                Func<object> safeChangeType = () =>
                {
                    bool isFsOption = ReflectionHelper.IsFSharpOptionType(conversionType);

                    Func<Type> getUnderlyingType =
                        () =>
#if !SKIP_FSHARP
                            isFsOption
                                ? FSharpOptionHelper.GetUnderlyingType(conversionType) :
#endif
                            Nullable.GetUnderlyingType(conversionType);

                    Type type = getUnderlyingType() ?? conversionType;

                    Func<object> withValue =
                        () =>
#if !SKIP_FSHARP
                            isFsOption
                                ? FSharpOptionHelper.Some(type, ConvertString(value, type, conversionCulture)) :
#endif
                            ConvertString(value, type, conversionCulture);
#if !SKIP_FSHARP
                    Func<object> empty = () => isFsOption ? FSharpOptionHelper.None(type) : null;
#else
                    Func<object> empty = () => null;
#endif

                    return value == null ? empty() : withValue();
                };

                return value.IsBooleanString() && conversionType == typeof(bool) ? value.ToBoolean() :
                       conversionType.GetTypeInfo()
                                     .IsEnum ? value.ToEnum(conversionType, ignoreValueCase) : safeChangeType();
            };

            Func<object> makeType = () =>
            {
                try
                {
                    ConstructorInfo ctor = conversionType.GetTypeInfo()
                                                         .GetConstructor(new[] { typeof(string) });

                    return ctor.Invoke(new object[] { value });
                }
                catch (Exception)
                {
                    throw new
                        FormatException("Destination conversion type must have a constructor that accepts a string.");
                }
            };

            if (conversionType.IsCustomStruct())
            {
                return Result.Try(makeType);
            }

            return Result.Try(conversionType.IsPrimitiveEx() || ReflectionHelper.IsFSharpOptionType(conversionType)
                                  ? changeType
                                  : makeType
                             );
        }

        private static object ToEnum(this string value, Type conversionType, bool ignoreValueCase)
        {
            object parsedValue;

            try
            {
                parsedValue = Enum.Parse(conversionType, value, ignoreValueCase);
            }
            catch (ArgumentException)
            {
                throw new FormatException();
            }

            if (IsDefinedEx(parsedValue))
            {
                return parsedValue;
            }

            throw new FormatException();
        }

        private static bool IsDefinedEx(object enumValue)
        {
            char firstChar = enumValue.ToString()[0];

            if (char.IsDigit(firstChar) || firstChar == '-')
            {
                return false;
            }

            return true;
        }
    }
}
