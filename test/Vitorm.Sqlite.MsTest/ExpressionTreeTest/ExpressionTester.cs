using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Vit.Linq;

namespace Vit.Linq.ExpressionTree.ExpressionTreeTest
{

    public abstract partial class ExpressionTester
    {

        public static List<User> Test(IQueryable<User> query, Expression<Func<User, bool>> predicate)
        {
            var expected = GetSourceData().AsQueryable().Where(predicate).ToList();
            if (expected.Count == 0) throw new Exception("result of predicate must not be empty");

            {
                var actual = query.Where(predicate).ToList();
                Check(expected, actual);
                return actual;
            }

            void Check(List<User> expected, List<User> actual)
            {
                Assert.AreEqual(expected.Count, actual.Count);
                for (var t = 0; t < expected.Count; t++)
                {
                    Assert.AreEqual(expected[t].id, actual[t].id);
                }
            }
        }



        public static void TestQueryable(IQueryable<User> query)
        {
            Expression<Func<User, bool>> predicate;

            #region #0 Add, An addition operation, such as a + b, without overflow checking, for numeric operands.
            {
                predicate = u => u.id + 1 == 6;
                var rows = Test(query, predicate);
                Assert.AreEqual(5, rows[0].id);
            }
            #endregion

            #region #3 AndAlso, A conditional AND operation that evaluates the second operand only if the first operand evaluates to true. It corresponds to (a && b)
            {
                predicate = u => u.id > 5 && u.id < 7;
                var rows = Test(query, predicate);
            }
            #endregion

            #region #7 Coalesce, A node that represents a null coalescing operation, such as (a ?? b)
            {
                predicate = u => (u.fatherId ?? u.id) == 5;
                var rows = Test(query, predicate);
            }
            #endregion

            #region #8 Conditional, A conditional operation, such as a > b ? a : b 
            {
                predicate = u => (u.id == 2 ? 1 : 0) == 1;
                var rows = Test(query, predicate);
            }
            #endregion

            #region #9 Constant, A constant value.
            {
                predicate = u => u.id == 2;
                var rows = Test(query, predicate);
            }
            #endregion

            #region #10 Convert, A cast or conversion operation, such as (SampleType)obj
            {
                predicate = u => ((double)u.id) <= 2.0;
                var rows = Test(query, predicate);
            }
            #endregion

            #region #12 Divide, A division operation, such as (a / b), for numeric operands.
            {
                predicate = u => u.id / 10.0 == 10.0;
                var rows = Test(query, predicate);
            }
            #endregion

            #region #13 Equal, A node that represents an equality comparison, such as (a == b) 
            {
                predicate = u => u.id == 2;
                var rows = Test(query, predicate);
            }
            {
                predicate = u => u.fatherId == null;
                var rows = Test(query, predicate);
            }
            #endregion

            #region #15 GreaterThan, A "greater than" comparison, such as (a > b).
            {
                predicate = u => u.id > 5;
                var rows = Test(query, predicate);
            }
            #endregion

            #region #16 GreaterThanOrEqual, A "greater than or equal to" comparison, such as (a >= b).
            {
                predicate = u => u.id >= 5;
                var rows = Test(query, predicate);
            }
            #endregion

            #region #20 LessThan, A "less than" comparison, such as (a < b).
            {
                predicate = u => u.id < 5;
                var rows = Test(query, predicate);
            }
            #endregion

            #region #21 LessThanOrEqual, A "less than or equal to" comparison, such as (a <= b).
            {
                predicate = u => u.id <= 5;
                var rows = Test(query, predicate);
            }
            #endregion

            #region #23 MemberAccess, An operation that reads from a field or property, such as obj.SampleProperty.
            {
                predicate = u => u.id == 5;
                var rows = Test(query, predicate);
            }
            #endregion

            #region #25 Modulo, An arithmetic remainder operation, such as (a % b)
            {
                predicate = u => (u.id % 10) == 0;
                var rows = Test(query, predicate);
            }
            #endregion

            #region #26 Multiply, A multiplication operation, such as (a * b), without overflow checking, for numeric operands
            {
                predicate = u => u.id * 10 == 20;
                var rows = Test(query, predicate);
            }
            #endregion

            #region #28 Negate, An arithmetic negation operation, such as (-a). The object a should not be modified in place.
            {
                predicate = u => -u.id == -2;
                var rows = Test(query, predicate);
            }
            #endregion

            #region #34 Not, A bitwise complement or logical negation operation. It is equivalent to  (~a) for integral types and to (!a) for Boolean values.
            {
                predicate = u => !(u.id == 2);
                var rows = Test(query, predicate);
            }
            #endregion

            #region #35 NotEqual,  An inequality comparison, such as (a != b)
            {
                predicate = u => u.id != 2;
                var rows = Test(query, predicate);
            }
            {
                predicate = u => u.fatherId != null;
                var rows = Test(query, predicate);
            }
            #endregion

            #region #37 OrElse,  A short-circuiting conditional OR operation, such as (a || b)
            {
                predicate = u => u.id == 2 || u.id == 3;
                var rows = Test(query, predicate);
            }
            #endregion

            #region #42 Subtract,  A subtraction operation, such as (a - b), without overflow checking
            {
                predicate = u => u.id - 2 == 9;
                var rows = Test(query, predicate);
            }
            #endregion


            #region Test the priority of mathematical calculations
            {
                predicate = u => 10 + u.id * 10 == 110;
                var rows = Test(query, predicate);
            }
            #endregion
        }


    }
}
