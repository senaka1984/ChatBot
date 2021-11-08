using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ChatBot.Common.Domain.Base
{
    public abstract class Enumeration : IComparable
    {
        public int Id { get; }
        public string Name { get; }
        public string DisplayName { get; }

        protected Enumeration() { }

        protected Enumeration(int id, string name, string displayName)
        {
            Id = id;
            Name = name;
            DisplayName = displayName;
        }

        public override string ToString() => DisplayName;

        public static IEnumerable<T> GetAll<T>() where T : Enumeration, new()
        {
            var type = typeof(T);
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

            foreach (var info in fields)
            {
                var instance = new T();
                if (info.GetValue(instance) is T locatedValue)
                    yield return locatedValue;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is Enumeration otherValue)
            {
                var typeMatches = GetType().Equals(obj.GetType());
                var valueMatches = Id.Equals(otherValue.Id);

                return typeMatches && valueMatches;
            }
            return false;
        }

        public override int GetHashCode() => Id.GetHashCode();

        public static int AbsoluteDifference(Enumeration firstValue, Enumeration secondValue)
        {
            var absoluteDifference = Math.Abs(firstValue.Id - secondValue.Id);
            return absoluteDifference;
        }

        public static T FromValue<T>(int value) where T : Enumeration, new()
        {
            var matchingItem = Parse<T, int>(value, "value", item => item.Id == value);
            return matchingItem;
        }

        public static T FromDisplayName<T>(string displayName) where T : Enumeration, new()
        {
            var matchingItem = Parse<T, string>(displayName, "display name", item => item.DisplayName == displayName);
            return matchingItem;
        }

        public static T FromName<T>(string name) where T : Enumeration, new()
        {
            var matchingItem = Parse<T, string>(name, "name", item => item.Name == name);
            return matchingItem;
        }

        private static T Parse<T, K>(K value, string description, Func<T, bool> predicate) where T : Enumeration, new()
        {
            var matchingItem = GetAll<T>().FirstOrDefault(predicate);

            if (matchingItem == null)
                throw new InvalidOperationException($"'{value}' is not a valid {description} in {typeof(T)}");

            return matchingItem;
        }

        public int CompareTo(object other) => Id.CompareTo(((Enumeration)other).Id);

        public static string ToCsv<T>(IEnumerable<T> enumerables, string separator)
        {
            if (!enumerables.Any()) return "";
            StringBuilder sb = new();
            foreach (var o in enumerables)
            {
                sb.Append(o.ToString());
                sb.Append(separator);
            }
            sb.Remove(sb.Length - 1, 1);
            return sb.ToString();
        }

        public static List<TEnum> CSVToEnumerable<TEnum>(string s) where TEnum : struct
        {
            if (string.IsNullOrEmpty(s))
                return default;
            return s.Split(',').Select(x => x.Trim()).Select(x => (TEnum)Enum.Parse(typeof(TEnum), x)).ToList();
        }
    }
}