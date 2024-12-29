using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Vitorm
{
    public static partial class IDataReader_Extensions
    {

        static IValueReader BuildValueReader(Type type)
        {
            if (type == typeof(string)) return ValueReader_String.Instance;
            else if (TypeUtil.IsNullable(type)) return new ValueReader_Nullable(type);
            return new ValueReader_Struct(type);
        }


        #region #1 ReadValue
        public static IEnumerable<Value> ReadValue<Value>(this IDataReader reader, int index = 0)
        {
            var valueReader = BuildValueReader(typeof(Value));
            while (reader.Read())
                yield return (Value)valueReader.Read(reader, index);
        }

        public static IEnumerable<Value> ReadValue<Value>(this IDataReader reader, string columnName)
        {
            var names = Enumerable.Range(0, reader.FieldCount).Select(i => reader.GetName(i)).ToList();
            int index = names.FindIndex(name => name.Equals(columnName, StringComparison.OrdinalIgnoreCase));

            if (index < 0) throw new ArgumentException("can not find column " + columnName);

            return ReadValue<Value>(reader, index);
        }
        #endregion




        #region #2 ReadTuple

        public static IEnumerable<(Column0, Column1)> ReadTuple<Column0, Column1>(this IDataReader reader, int[] indexes = null)
        {
            var valueReaders = new[] { typeof(Column0), typeof(Column1) }.Select(type => BuildValueReader(type)).ToArray();
            indexes ??= new[] { 0, 1 };

            while (reader.Read())
                yield return ((Column0)valueReaders[0].Read(reader, indexes[0]), (Column1)valueReaders[1].Read(reader, indexes[1]));
        }
        public static IEnumerable<(Column0, Column1)> ReadTuple<Column0, Column1>(this IDataReader reader, string[] columnNames)
        {
            var names = Enumerable.Range(0, reader.FieldCount).Select(i => reader.GetName(i)).ToList();
            var indexes = columnNames.Select(
                columnName =>
                {
                    var index = names.FindIndex(name => name.Equals(columnName, StringComparison.OrdinalIgnoreCase));
                    if (index < 0) throw new ArgumentException("can not find column " + columnName);
                    return index;
                }
            ).ToArray();

            return ReadTuple<Column0, Column1>(reader, indexes);
        }


        public static IEnumerable<(Column0, Column1, Column2)> ReadTuple<Column0, Column1, Column2>(this IDataReader reader, int[] indexes = null)
        {
            var valueReaders = new[] { typeof(Column0), typeof(Column1), typeof(Column2) }.Select(type => BuildValueReader(type)).ToArray();
            indexes ??= new[] { 0, 1, 2 };

            while (reader.Read())
                yield return ((Column0)valueReaders[0].Read(reader, indexes[0]), (Column1)valueReaders[1].Read(reader, indexes[1]), (Column2)valueReaders[2].Read(reader, indexes[2]));
        }
        public static IEnumerable<(Column0, Column1, Column2)> ReadTuple<Column0, Column1, Column2>(this IDataReader reader, string[] columnNames)
        {
            var names = Enumerable.Range(0, reader.FieldCount).Select(i => reader.GetName(i)).ToList();
            var indexes = columnNames.Select(
                columnName =>
                {
                    var index = names.FindIndex(name => name.Equals(columnName, StringComparison.OrdinalIgnoreCase));
                    if (index < 0) throw new ArgumentException("can not find column " + columnName);
                    return index;
                }
            ).ToArray();

            return ReadTuple<Column0, Column1, Column2>(reader, indexes);
        }


        public static IEnumerable<(Column0, Column1, Column2, Column3)> ReadTuple<Column0, Column1, Column2, Column3>(this IDataReader reader, int[] indexes = null)
        {
            var valueReaders = new[] { typeof(Column0), typeof(Column1), typeof(Column2), typeof(Column3) }.Select(type => BuildValueReader(type)).ToArray();
            indexes ??= new[] { 0, 1, 2, 3 };

            while (reader.Read())
                yield return ((Column0)valueReaders[0].Read(reader, indexes[0]), (Column1)valueReaders[1].Read(reader, indexes[1]), (Column2)valueReaders[2].Read(reader, indexes[2]), (Column3)valueReaders[3].Read(reader, indexes[3]));
        }
        public static IEnumerable<(Column0, Column1, Column2, Column3)> ReadTuple<Column0, Column1, Column2, Column3>(this IDataReader reader, string[] columnNames)
        {
            var names = Enumerable.Range(0, reader.FieldCount).Select(i => reader.GetName(i)).ToList();
            var indexes = columnNames.Select(
                columnName =>
                {
                    var index = names.FindIndex(name => name.Equals(columnName, StringComparison.OrdinalIgnoreCase));
                    if (index < 0) throw new ArgumentException("can not find column " + columnName);
                    return index;
                }
            ).ToArray();

            return ReadTuple<Column0, Column1, Column2, Column3>(reader, indexes);
        }
        #endregion








        #region #3 ReadEntity
        public static IEnumerable<Entity> ReadEntity<Entity>(this IDataReader reader) where Entity : class, new()
        {
            var fieldNames = Enumerable.Range(0, reader.FieldCount).Select(i => reader.GetName(i)).ToList();

            var entityReader = new EntityReader(typeof(Entity), fieldNames);

            while (reader.Read())
            {
                yield return (Entity)entityReader.Read(reader);
            }
        }


        #region ReadEntity by Delegate

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="func"></param>
        /// <param name="splitIndexes">if splitIndex is valid, will try populate all column to all entity</param>
        /// <param name="types"></param>
        /// <param name="fieldNames"></param>
        /// <returns></returns>
        static IEnumerable<object> ReadEntity(this IDataReader reader, Delegate func, int[] splitIndexes, Type[] types = null, List<string> fieldNames = null)
        {
            types ??= func.Method.GetParameters().Select(p => p.ParameterType).ToArray();
            fieldNames ??= Enumerable.Range(0, reader.FieldCount).Select(i => reader.GetName(i)).ToList();

            // splitIndexes  :   1, 5 , -1, 10
            splitIndexes ??= Enumerable.Repeat(-1, types.Length - 1).ToArray();

            var splitRangeStart = new[] { 0 }.Concat(splitIndexes.Select(i => i >= 0 ? i : 0)).ToArray();
            var splitRangeEnd = splitIndexes.Select(i => i >= 0 ? i - 1 : int.MaxValue).Concat(new[] { int.MaxValue }).ToArray();

            var fieldIndexes = fieldNames.Select((fieldName, index) => (fieldName: fieldName, index: index)).ToList();
            var entityReaders = types.Select((type, index) => new EntityReader(type, fieldIndexes.Where(field => splitRangeStart[index] <= field.index && field.index <= splitRangeEnd[index]).ToList())).ToList();

            while (reader.Read())
            {
                yield return func.DynamicInvoke(entityReaders.Select(entityReader => entityReader.Read(reader)).ToArray());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="func"></param>
        /// <param name="splitOns">if can not find splitOn column, will try populate all column to all entity</param>
        /// <returns></returns>
        static IEnumerable<object> ReadEntity(this IDataReader reader, Delegate func, string[] splitOns)
        {
            var types = func.Method.GetParameters().Select(p => p.ParameterType).ToArray();
            var fieldNames = Enumerable.Range(0, reader.FieldCount).Select(i => reader.GetName(i)).ToList();

            splitOns ??= new string[types.Length - 1];

            // splitIndexes  :   1, 5 , -1, 10
            var splitIndexes = splitOns.Select(splitOn => fieldNames.FindIndex(name => name.Equals(splitOn, StringComparison.OrdinalIgnoreCase))).Concat(new[] { fieldNames.Count }).ToArray();

            return ReadEntity(reader, func, splitIndexes, types, fieldNames);
        }



        static IEnumerable<TReturn> ReadEntity<TReturn>(this IDataReader reader, Delegate func, string[] splitOns)
            => ReadEntity(reader, func, splitOns).Select(m => (TReturn)m);

        static IEnumerable<TReturn> ReadEntity<TReturn>(this IDataReader reader, Delegate func, int[] splitIndexes = null)
            => ReadEntity(reader, func, splitIndexes).Select(m => (TReturn)m);
        #endregion


        #region ReadEntity2
        public static IEnumerable<TReturn> ReadEntity<TFirst, TSecond, TReturn>(this IDataReader reader, Func<TFirst, TSecond, TReturn> func)
            => ReadEntity<TReturn>(reader, func);

        public static IEnumerable<TReturn> ReadEntity<TFirst, TSecond, TReturn>(this IDataReader reader, Func<TFirst, TSecond, TReturn> func, string splitOn)
            => ReadEntity<TReturn>(reader, func, new[] { splitOn });

        public static IEnumerable<TReturn> ReadEntity<TFirst, TSecond, TReturn>(this IDataReader reader, Func<TFirst, TSecond, TReturn> func, string[] splitOns)
            => ReadEntity<TReturn>(reader, func, splitOns);

        public static IEnumerable<TReturn> ReadEntity<TFirst, TSecond, TReturn>(this IDataReader reader, Func<TFirst, TSecond, TReturn> func, int splitIndex)
            => ReadEntity<TReturn>(reader, func, new[] { splitIndex });
        public static IEnumerable<TReturn> ReadEntity<TFirst, TSecond, TReturn>(this IDataReader reader, Func<TFirst, TSecond, TReturn> func, int[] splitIndexes)
            => ReadEntity<TReturn>(reader, func, splitIndexes);

        #endregion

        #region ReadEntity3
        public static IEnumerable<TReturn> ReadEntity<TFirst, TSecond, TThird, TReturn>(this IDataReader reader, Func<TFirst, TSecond, TThird, TReturn> func)
            => ReadEntity<TReturn>(reader, func);

        public static IEnumerable<TReturn> ReadEntity<TFirst, TSecond, TThird, TReturn>(this IDataReader reader, Func<TFirst, TSecond, TThird, TReturn> func, string splitOn0, string splitOn1)
            => ReadEntity<TReturn>(reader, func, new[] { splitOn0, splitOn1 });

        public static IEnumerable<TReturn> ReadEntity<TFirst, TSecond, TThird, TReturn>(this IDataReader reader, Func<TFirst, TSecond, TThird, TReturn> func, string[] splitOns)
            => ReadEntity<TReturn>(reader, func, splitOns);

        public static IEnumerable<TReturn> ReadEntity<TFirst, TSecond, TThird, TReturn>(this IDataReader reader, Func<TFirst, TSecond, TThird, TReturn> func, int splitIndex0, int splitIndex1)
            => ReadEntity<TReturn>(reader, func, new[] { splitIndex0, splitIndex1 });
        public static IEnumerable<TReturn> ReadEntity<TFirst, TSecond, TThird, TReturn>(this IDataReader reader, Func<TFirst, TSecond, TThird, TReturn> func, int[] splitIndexes)
            => ReadEntity<TReturn>(reader, func, splitIndexes);

        #endregion


        #endregion




    }
}
