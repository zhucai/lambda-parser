using Zhucai.LambdaParser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Zhucai.LambdaParser.ObjectDynamicExtension;

namespace Test_Zhucai.LambdaParser
{
    [TestClass()]
    public class ExpressionParserTest
    {
        /// <summary>
        /// 数字运算
        /// </summary>
        [TestMethod]
        public void ParseDelegateTest_Number()
        {
            {
                var expected = ExpressionParser.Compile("(int m,int n)=>m*n").DynamicInvoke(3, 8);
                var actual = 3 * 8;

                Assert.AreEqual(expected, actual);
            }
            {
                var expected = ExpressionParser.Compile("(int m,int n,double l)=>l/m*n").DynamicInvoke(3, 8, 24.46);
                var actual = 24.46 / 3 * 8;

                Assert.AreEqual(expected, actual);
            }
            {
                string code = "()=>1+2-3*4/2+20%7;";
                int expected = 1 + 2 - 3 * 4 / 2 + 20 % 7;

                Func<int> func = ExpressionParser.Compile<Func<int>>(code);
                int actual = func();
                Assert.AreEqual(expected, actual);
            }
            {
                // 在无参数的情况下可以省略()=>
                string code = "2 + 3 * 4 / 2 - 4 % (2 + 1) + 32 - 43 * (6 - 4) + 3.3";
                double expected = 2 + 3 * 4 / 2 - 4 % (2 + 1) + 32 - 43 * (6 - 4) + 3.3;

                Func<double> func = ExpressionParser.Compile<Func<double>>(code);
                double actual = func();
                Assert.AreEqual(expected, actual);
            }
            {
                string code = "2 + 3 * 4 / 2 - 4 % (2.4 + 1) + 32 - 43 * (6 - 4) + 3.3 * (322 / (43 - (3 - 1)))";
                double expected = 2 + 3 * 4 / 2 - 4 % (2.4 + 1) + 32 - 43 * (6 - 4) + 3.3 * (322 / (43 - (3 - 1)));

                Func<double> func = ExpressionParser.Compile<Func<double>>(code);
                double actual = func();
                Assert.AreEqual(expected, actual);
            }
            {
                string code = "3.32 * 12d + 34L - 12d";
                double expected = 3.32 * 12d + 34L - 12d;

                Func<double> func = ExpressionParser.Compile<Func<double>>(code);
                double actual = func();
                Assert.AreEqual(expected, actual);
            }
            {
                // number+string
                Func<int, string> f = ExpressionParser.Compile<Func<int, string>>(
                    "m=>2+m.ToString(\"000\")");
                string expected = f(2); // "2002"
                Assert.AreEqual(expected, "2002");
            }
            {
                int m = 10;
                var expected = -m;

                Delegate dele = ExpressionParser.Compile("(int m)=>-m");
                var actual = dele.DynamicInvoke(10);

                Assert.AreEqual(expected, actual);
            }
        }
        /// <summary>
        /// 字符串拼接和函数调用
        /// </summary>
        [TestMethod]
        public void ParseDelegateTest_StringAdd()
        {
            string code = "2.ToString()+3+(4*2)";
            string expected = "238";

            Func<string> func = ExpressionParser.Compile<Func<string>>(code);

            string actual = func();

            Assert.AreEqual(expected, actual);
        }
        /// <summary>
        /// 成员访问，方法调用，构造函数(传参)
        /// </summary>
        [TestMethod]
        public void ParseDelegateTest_Member_Method_Ctor()
        {
            string code = "()=>new Test_Zhucai.LambdaParser.TestClass(9,2).Member1";
            string code2 = "()=>new Test_Zhucai.LambdaParser.TestClass(){Member1 = 5,Member2 = 4}.GetMemberAll()";

            Func<int> func = ExpressionParser.Compile<Func<int>>(code);
            Func<int> func2 = ExpressionParser.Compile<Func<int>>(code2);

            int actual = func();
            int actual2 = func2();

            Assert.AreEqual(actual, actual2);
        }
        /// <summary>
        /// 委托传多个参数
        /// </summary>
        [TestMethod]
        public void ParseDelegateTest_MultiLambdaParam()
        {
            string code = "(m,n,l)=>m+n+l"; // m:1 n:2 l:"3"
            string expected = "33";

            Func<int, int, string, string> func = ExpressionParser.Compile<Func<int, int, string, string>>(code);
            string actual = func(1, 2, "3");
            Assert.AreEqual(expected, actual);
        }
        /// <summary>
        /// 泛型类
        /// </summary>
        [TestMethod]
        public void ParseDelegateTest_Generic()
        {
            {
                string code = "()=>typeof(List<string>).FullName";
                var expected = typeof(List<string>).FullName;

                Func<string> func = ExpressionParser.Compile<Func<string>>(code, "System", "System.Collections.Generic");
                var actual = func();
                Assert.AreEqual(expected, actual);
            }
            {
                string code = "()=>typeof(List<List<string>>).FullName";
                var expected = typeof(List<List<string>>).FullName;

                Func<string> func = ExpressionParser.Compile<Func<string>>(code, "System", "System.Collections.Generic");
                var actual = func();
                Assert.AreEqual(expected, actual);
            }
        }
        /// <summary>
        /// 泛型类
        /// </summary>
        [TestMethod]
        public void ParseDelegateTest_GenericList()
        {
            {
                string code = "(m)=>new List<string>(){ Capacity = m * 3 }.Capacity";
                var expected = new List<string>() { Capacity = 3 * 3 }.Capacity;

                Func<int, int> func = ExpressionParser.Compile<Func<int, int>>(code, "System", "System.Collections.Generic");
                var actual = func(3);
                Assert.AreEqual(expected, actual);
            }
            {
                string code = "()=>new List<string>(){ \"abc\",\"def\" }[1]";
                var expected = new List<string>() { "abc", "def" }[1];

                var actual = ExpressionParser.Compile(code, "System", "System.Collections.Generic").DynamicInvoke();
                Assert.AreEqual(expected, actual);
            }
        }
        /// <summary>
        /// 测试Nullable
        /// </summary>
        [TestMethod]
        public void ParseDelegateTest_Nullable()
        {
            {
                Delegate dele = ExpressionParser.Compile("(int? m)=>m==null");
                var actual = dele.DynamicInvoke((int?)null);

                Assert.AreEqual(true, actual);
            }
            {
                Delegate dele = ExpressionParser.Compile("(int?)null");
                var actual = dele.DynamicInvoke();

                Assert.AreEqual(null, null);
            }
            {
                Delegate dele = ExpressionParser.Compile("new System.Nullable<int>(10).Value - 3");
                Nullable<int> actual = (Nullable<int>)dele.DynamicInvoke();

                Assert.AreEqual(7, actual);
            }
            {
                Delegate dele = ExpressionParser.Compile("new long?(10L).Value - 3");
                long? actual = (long?)dele.DynamicInvoke();

                Assert.AreEqual(7, actual);
            }
        }
        /// <summary>
        /// new数组，数组访问
        /// </summary>
        [TestMethod]
        public void ParseDelegateTest_Array()
        {
            string code = "()=>new string[]{\"aa\",\"bb\"}[1]";
            string expected = "bb";

            Func<string> func = ExpressionParser.Compile<Func<string>>(code);
            string actual = func();
            Assert.AreEqual(expected, actual);
        }
        /// <summary>
        /// new数组，数组访问2
        /// </summary>
        [TestMethod]
        public void ParseDelegateTest_Array2()
        {
            string code = "()=>new string[5].Length";
            int expected = 5;

            Func<int> func = ExpressionParser.Compile<Func<int>>(code);
            int actual = func();
            Assert.AreEqual(expected, actual);
        }
        //[TestMethod]
        //public void ParseDelegateTest_ListInit()
        //{
        //    string code = "()=>new List<string>().Count";
        //    var expected = new List<string>().Count;

        //    Func<string> func = ExpressionParser.Compile<Func<string>>(code,"System");
        //    string actual = func();
        //    Assert.AreEqual(expected, actual);
        //}
        /// <summary>
        /// new多维数组
        /// </summary>
        [TestMethod]
        public void ParseDelegateTest_ArrayMultiRank()
        {
            string code = "()=>new string[5,4].Rank";
            int expected = 2;

            Func<int> func = ExpressionParser.Compile<Func<int>>(code);
            int actual = func();
            Assert.AreEqual(expected, actual);
        }
        /// <summary>
        /// 索引器访问
        /// </summary>
        [TestMethod]
        public void ParseDelegateTest_Indexer()
        {
            string code = "m=>(string)m[1]";
            string expected = "bb";

            Func<ArrayList, string> func = ExpressionParser.Compile<Func<ArrayList, string>>(code);
            string actual = func(new ArrayList(new string[] { "aa", "bb" }));
            Assert.AreEqual(expected, actual);
        }
        /// <summary>
        /// 重复生成
        /// </summary>
        [TestMethod]
        public void ParseDelegateTest_Repeater()
        {
            string code = "m=>(string)m[1]";
            string expected = "bb";

            Func<ArrayList, string> func = ExpressionParser.Compile<Func<ArrayList, string>>(code);
            func = ExpressionParser.Compile<Func<ArrayList, string>>(code);
            string actual = func(new ArrayList(new string[] { "aa", "bb" }));
            Assert.AreEqual(expected, actual);
        }
        /// <summary>
        /// 成员初始化
        /// </summary>
        [TestMethod]
        public void ParseDelegateTest_MemberInit()
        {
            string code = "()=>new Test_Zhucai.LambdaParser.TestClass(){Member1 = 20}.ToString()";
            string expected = "20";

            Func<string> func = ExpressionParser.Compile<Func<string>>(code);
            string actual = func();
            Assert.AreEqual(expected, actual);
        }
        /// <summary>
        /// 测试操作符typeof,sizeof
        /// </summary>
        [TestMethod]
        public void ParseDelegateTest_OperatorTypeofSizeof()
        {
            string code = "()=>typeof(string).FullName+sizeof(int)";
            string expected = typeof(string).FullName + sizeof(int);

            Func<string> func = ExpressionParser.Compile<Func<string>>(code);
            string actual = func();
            Assert.AreEqual(expected, actual);
        }
        /// <summary>
        /// 测试操作符!
        /// </summary>
        [TestMethod]
        public void ParseDelegateTest_OperatorNot()
        {
            string code = "(m)=>!m";
            bool expected = true;

            Func<bool, bool> func = ExpressionParser.Compile<Func<bool, bool>>(code);
            bool actual = func(false);
            Assert.AreEqual(expected, actual);
        }
        /// <summary>
        /// 测试操作符~
        /// </summary>
        [TestMethod]
        public void ParseDelegateTest_OperatorBitNot()
        {
            string code = "()=>~9";
            int expected = ~9;

            Func<int> func = ExpressionParser.Compile<Func<int>>(code);
            int actual = func();
            Assert.AreEqual(expected, actual);
        }
        /// <summary>
        /// 测试操作符convert()
        /// </summary>
        [TestMethod]
        public void ParseDelegateTest_OperatorConvert()
        {
            string code = "()=>(int)(9.8+3.3)";
            int expected = (int)(9.8 + 3.3);

            Func<int> func = ExpressionParser.Compile<Func<int>>(code);
            int actual = func();
            Assert.AreEqual(expected, actual);
        }
        /// <summary>
        /// 测试操作符＞＞,＜＜
        /// </summary>
        [TestMethod]
        public void ParseDelegateTest_OperatorBitShift()
        {
            string code = "()=>(9>>3)-(5<<2)";
            int expected = (9 >> 3) - (5 << 2);

            Func<int> func = ExpressionParser.Compile<Func<int>>(code);
            int actual = func();
            Assert.AreEqual(expected, actual);
        }
        /// <summary>
        /// 测试操作符＞
        /// </summary>
        [TestMethod]
        public void ParseDelegateTest_OperatorGreaterThan()
        {
            {
                string code = "()=>9>2";
                bool expected = (9 > 2);

                Func<bool> func = ExpressionParser.Compile<Func<bool>>(code);
                bool actual = func();
                Assert.AreEqual(expected, actual);
            }
            {
                string code = "()=>2>9";
                bool expected = (2 > 9);

                Func<bool> func = ExpressionParser.Compile<Func<bool>>(code);
                bool actual = func();
                Assert.AreEqual(expected, actual);
            }
            {
                string code = "()=>2>2";
                bool expected = (2 > 2);

                Func<bool> func = ExpressionParser.Compile<Func<bool>>(code);
                bool actual = func();
                Assert.AreEqual(expected, actual);
            }
        }
        /// <summary>
        /// 测试操作符＜
        /// </summary>
        [TestMethod]
        public void ParseDelegateTest_OperatorLessThan()
        {
            {
                string code = "()=>9<2";
                bool expected = (9 < 2);

                Func<bool> func = ExpressionParser.Compile<Func<bool>>(code);
                bool actual = func();
                Assert.AreEqual(expected, actual);
            }
            {
                string code = "()=>2<9";
                bool expected = (2 < 9);

                Func<bool> func = ExpressionParser.Compile<Func<bool>>(code);
                bool actual = func();
                Assert.AreEqual(expected, actual);
            }
            {
                string code = "()=>2<2";
                bool expected = (2 < 2);

                Func<bool> func = ExpressionParser.Compile<Func<bool>>(code);
                bool actual = func();
                Assert.AreEqual(expected, actual);
            }
        }
        /// <summary>
        /// 测试操作符＜=
        /// </summary>
        [TestMethod]
        public void ParseDelegateTest_OperatorLessThanOrEqual()
        {
            {
                string code = "()=>9<=2";
                bool expected = (9 <= 2);

                Func<bool> func = ExpressionParser.Compile<Func<bool>>(code);
                bool actual = func();
                Assert.AreEqual(expected, actual);
            }
            {
                string code = "()=>2<=2";
                bool expected = (2 <= 2);

                Func<bool> func = ExpressionParser.Compile<Func<bool>>(code);
                bool actual = func();
                Assert.AreEqual(expected, actual);
            }
            {
                string code = "()=>2<=9";
                bool expected = (2 <= 9);

                Func<bool> func = ExpressionParser.Compile<Func<bool>>(code);
                bool actual = func();
                Assert.AreEqual(expected, actual);
            }
        }
        /// <summary>
        /// 测试操作符＞=
        /// </summary>
        [TestMethod]
        public void ParseDelegateTest_OperatorGreaterThanOrEqual()
        {
            {
                string code = "()=>9>=2";
                bool expected = (9 >= 2);

                Func<bool> func = ExpressionParser.Compile<Func<bool>>(code);
                bool actual = func();
                Assert.AreEqual(expected, actual);
            }
            {
                string code = "()=>2>=2";
                bool expected = (2 >= 2);

                Func<bool> func = ExpressionParser.Compile<Func<bool>>(code);
                bool actual = func();
                Assert.AreEqual(expected, actual);
            }
            {
                string code = "()=>2>=9";
                bool expected = (2 >= 9);

                Func<bool> func = ExpressionParser.Compile<Func<bool>>(code);
                bool actual = func();
                Assert.AreEqual(expected, actual);
            }
        }
        /// <summary>
        /// 测试操作符==
        /// </summary>
        [TestMethod]
        public void ParseDelegateTest_OperatorEqual()
        {
            {
                string code = "()=>9==2";
                bool expected = (9 == 2);

                Func<bool> func = ExpressionParser.Compile<Func<bool>>(code);
                bool actual = func();
                Assert.AreEqual(expected, actual);
            }
            {
                string code = "()=>2==2";
                bool expected = (2 == 2);

                Func<bool> func = ExpressionParser.Compile<Func<bool>>(code);
                bool actual = func();
                Assert.AreEqual(expected, actual);
            }
            {
                string code = "()=>2==9";
                bool expected = (2 == 9);

                Func<bool> func = ExpressionParser.Compile<Func<bool>>(code);
                bool actual = func();
                Assert.AreEqual(expected, actual);
            }
        }
        /// <summary>
        /// 测试操作符!=
        /// </summary>
        [TestMethod]
        public void ParseDelegateTest_OperatorNotEqual()
        {
            {
                string code = "()=>9!=2";
                bool expected = (9 != 2);

                Func<bool> func = ExpressionParser.Compile<Func<bool>>(code);
                bool actual = func();
                Assert.AreEqual(expected, actual);
            }
            {
                string code = "()=>2!=2";
                bool expected = (2 != 2);

                Func<bool> func = ExpressionParser.Compile<Func<bool>>(code);
                bool actual = func();
                Assert.AreEqual(expected, actual);
            }
            {
                string code = "()=>2!=9";
                bool expected = (2 != 9);

                Func<bool> func = ExpressionParser.Compile<Func<bool>>(code);
                bool actual = func();
                Assert.AreEqual(expected, actual);
            }
        }
        /// <summary>
        /// 测试操作符is
        /// </summary>
        [TestMethod]
        public void ParseDelegateTest_OperatorIs()
        {
            {
                string code = "(m)=>((object)m) is int";
                bool expected = ((object)2) is int;

                Func<object, bool> func = ExpressionParser.Compile<Func<object, bool>>(code);
                bool actual = func(2);
                Assert.AreEqual(expected, actual);
            }
            {
                string code = "()=>((object)\"abc\") is int";
                bool expected = ((object)"abc") is int;

                Func<bool> func = ExpressionParser.Compile<Func<bool>>(code);
                bool actual = func();
                Assert.AreEqual(expected, actual);
            }
        }
        /// <summary>
        /// 测试操作符as
        /// </summary>
        [TestMethod]
        public void ParseDelegateTest_OperatorAs()
        {
            {
                string code = "(m)=>((object)m) as string";
                string expected = ((object)2) as string;

                Func<object, string> func = ExpressionParser.Compile<Func<object, string>>(code);
                string actual = func(2);
                Assert.AreEqual(expected, actual);
            }
            {
                string code = "()=>((object)\"abc\") as string";
                string expected = ((object)"abc") as string;

                Func<string> func = ExpressionParser.Compile<Func<string>>(code);
                string actual = func();
                Assert.AreEqual(expected, actual);
            }
        }
        /// <summary>
        /// 测试操作符^
        /// </summary>
        [TestMethod]
        public void ParseDelegateTest_OperatorExclusiveOr()
        {
            string code = "()=>3^7";
            int expected = 3 ^ 7;

            Func<int> func = ExpressionParser.Compile<Func<int>>(code);
            int actual = func();
            Assert.AreEqual(expected, actual);
        }
        /// <summary>
        /// 测试操作符&
        /// </summary>
        [TestMethod]
        public void ParseDelegateTest_OperatorAnd()
        {
            string code = "()=>3&7";
            int expected = 3 & 7;

            Func<int> func = ExpressionParser.Compile<Func<int>>(code);
            int actual = func();
            Assert.AreEqual(expected, actual);
        }
        /// <summary>
        /// 测试操作符|
        /// </summary>
        [TestMethod]
        public void ParseDelegateTest_OperatorOr()
        {
            string code = "()=>3|7";
            int expected = 3 | 7;

            Func<int> func = ExpressionParser.Compile<Func<int>>(code);
            int actual = func();
            Assert.AreEqual(expected, actual);
        }
        /// <summary>
        /// 测试操作符&&
        /// </summary>
        [TestMethod]
        public void ParseDelegateTest_OperatorAndAlso()
        {
            {
                string code = "()=>true&&false";
                bool expected = true && false;

                Func<bool> func = ExpressionParser.Compile<Func<bool>>(code);
                bool actual = func();
                Assert.AreEqual(expected, actual);
            }
            {
                string code = "()=>true&&true";
                bool expected = true && true;

                Func<bool> func = ExpressionParser.Compile<Func<bool>>(code);
                bool actual = func();
                Assert.AreEqual(expected, actual);
            }
            {
                string code = "()=>false && false";
                bool expected = false && false;

                Func<bool> func = ExpressionParser.Compile<Func<bool>>(code);
                bool actual = func();
                Assert.AreEqual(expected, actual);
            }
        }
        /// <summary>
        /// 测试操作符||
        /// </summary>
        [TestMethod]
        public void ParseDelegateTest_OperatorOrElse()
        {
            {
                string code = "()=>true||false";
                bool expected = true || false;

                Func<bool> func = ExpressionParser.Compile<Func<bool>>(code);
                bool actual = func();
                Assert.AreEqual(expected, actual);
            }
            {
                string code = "()=>true||true";
                bool expected = true || true;

                Func<bool> func = ExpressionParser.Compile<Func<bool>>(code);
                bool actual = func();
                Assert.AreEqual(expected, actual);
            }
            {
                string code = "()=>false||false";
                bool expected = false || false;

                Func<bool> func = ExpressionParser.Compile<Func<bool>>(code);
                bool actual = func();
                Assert.AreEqual(expected, actual);
            }
        }
        /// <summary>
        /// 测试操作符?:
        /// </summary>
        [TestMethod]
        public void ParseDelegateTest_OperatorCondition()
        {
            {
                string code = "()=>1==1?1:2";
                int expected = 1 == 1 ? 1 : 2;

                Func<int> func = ExpressionParser.Compile<Func<int>>(code);
                int actual = func();
                Assert.AreEqual(expected, actual);
            }
            {
                string code = "()=>1==2?1:2";
                int expected = 1 == 2 ? 1 : 2;

                Func<int> func = ExpressionParser.Compile<Func<int>>(code);
                int actual = func();
                Assert.AreEqual(expected, actual);
            }
        }
        /// <summary>
        /// 测试操作符??
        /// </summary>
        [TestMethod]
        public void ParseDelegateTest_OperatorDoubleQuestionMark()
        {
            string code = "(m)=>m??\"bb\"";
            string expected = "aa";

            Func<string, string> func = ExpressionParser.Compile<Func<string, string>>(code);
            string actual = func("aa");
            Assert.AreEqual(expected, actual);

            // use code to test 2
            expected = "bb";
            actual = func(null);
            Assert.AreEqual(expected, actual);
        }
        /// <summary>
        /// 测试namespace
        /// </summary>
        [TestMethod]
        public void ParseDelegateTest_Namespace()
        {
            string code = "new TestClass().Member1";
            Func<int> func = ExpressionParser.Compile<Func<int>>(code, typeof(TestClass).Namespace);
            int actual = func();
            Assert.AreEqual(0, actual);
        }
        /// <summary>
        /// 测试默认实例
        /// </summary>
        [TestMethod]
        public void ParseDelegateTest_DefaultInstance()
        {
            {
                string code = "Member1*2";
                Func<TestClass, int> func = ExpressionParser.Compile<Func<TestClass, int>>(code, true);
                int actual = func(new TestClass(13, 13));
                Assert.AreEqual(13 * 2, actual);
            }
            {
                string code = "Member1 == 13";
                Delegate func = ExpressionParser.Compile(code, typeof(TestClass));
                var actual = func.DynamicInvoke(new TestClass(13, 13));
                Assert.AreEqual(true, actual);
            }
            {
                string code = "m=>Member1*m.Member2";
                Func<TestClass, TestClass, int> func = ExpressionParser.Compile<Func<TestClass, TestClass, int>>(code, true);
                int actual = func(new TestClass(13, 13), new TestClass(23, 23));
                Assert.AreEqual(13 * 23, actual);
            }
        }
        /// <summary>
        /// 测试Exec方法
        /// </summary>
        [TestMethod]
        public void ParseDelegateTest_DefaultExec()
        {
            object obj = new { Name = "zhangsan", Id = 18 };
            {
                object actual = obj.E("Id");

                Assert.AreEqual(18, actual);
            }
            {
                int actual = obj.E<int>("Id");

                Assert.AreEqual(18, actual);
            }
            {
                string actual = obj.E<string>("Name + Id");

                Assert.AreEqual("zhangsan18", actual);
            }
            {
                object obj2 = new { Name = "李四", Id = 18 };
                object obj3 = new { Name = "王五", Id = 18 };
                string actual = obj.E<string>("$0.Name + $1.Name + $2.Name", obj2, obj3);

                Assert.AreEqual("zhangsan李四王五", actual);
            }
            {
                object obj2 = new { Name = "李四", Id = 18 };
                string actual = obj.E<string>("Name + $1.Name", obj2);

                Assert.AreEqual("zhangsan李四", actual);
            }
            {
                object obj2 = new TestClass(2, 3);
                int actual = obj2.E<int>("GetMemberAll()");

                Assert.AreEqual(5, actual);
            }
        }

        /// <summary>
        /// 测试一个复杂代码
        /// </summary>
        [TestMethod]
        public void ParseDelegateTest_Other()
        {
            {
                string code = @"()=>new Test_Zhucai.LambdaParser.TestClass()
                    {
                        Member1 = 3
                    }.Member1 == 3 ? new Test_Zhucai.LambdaParser.TestClass[]
                {
                    new Test_Zhucai.LambdaParser.TestClass()
                    {
                        Member1 = 3
                    }
                }[4-4].Member1 + 3 * new Test_Zhucai.LambdaParser.TestClass()
                    {
                        Member2 = 5
                    }.Member2 : new Test_Zhucai.LambdaParser.TestClass()
                    {
                        Member2 = 5,
                        Member1 = 9,
                    }.GetMemberAll();";
                int expected = new Test_Zhucai.LambdaParser.TestClass()
                    {
                        Member1 = 3
                    }.Member1 == 3 ? new Test_Zhucai.LambdaParser.TestClass[]
                {
                    new Test_Zhucai.LambdaParser.TestClass()
                    {
                        Member1 = 3
                    }
                }[4 - 4].Member1 + 3 * new Test_Zhucai.LambdaParser.TestClass()
                    {
                        Member2 = 5
                    }.Member2 : new Test_Zhucai.LambdaParser.TestClass()
                    {
                        Member2 = 5,
                        Member1 = 9,
                    }.GetMemberAll();

                Func<int> func = ExpressionParser.Compile<Func<int>>(code);
                int actual = func();
                Assert.AreEqual(expected, actual);
            }
            {
                string code = @"()=>new Test_Zhucai.LambdaParser.TestClass()
                    {
                        Member1 = 3
                    }.Member1 != 3 ? new Test_Zhucai.LambdaParser.TestClass[]
                {
                    new Test_Zhucai.LambdaParser.TestClass()
                    {
                        Member1 = 3
                    }
                }[new Test_Zhucai.LambdaParser.TestClass().Member1].Member1 + 3 * new Test_Zhucai.LambdaParser.TestClass()
                    {
                        Member2 = 5
                    }.Member2 : new Test_Zhucai.LambdaParser.TestClass()
                    {
                        Member2 = 5,
                        Member1 = 9,
                    }.GetMemberAll();";
                int expected = new Test_Zhucai.LambdaParser.TestClass()
                {
                    Member1 = 3
                }.Member1 != 3 ? new Test_Zhucai.LambdaParser.TestClass[]
                {
                    new Test_Zhucai.LambdaParser.TestClass()
                    {
                        Member1 = 3
                    }
                }[new Test_Zhucai.LambdaParser.TestClass().Member1].Member1 + 3 * new Test_Zhucai.LambdaParser.TestClass()
                {
                    Member2 = 5
                }.Member2 : new Test_Zhucai.LambdaParser.TestClass()
                {
                    Member2 = 5,
                    Member1 = 9,
                }.GetMemberAll();

                Func<int> func = ExpressionParser.Compile<Func<int>>(code);
                int actual = func();
                Assert.AreEqual(expected, actual);
            }
        }
    }

    public class TestClass
    {
        public int Member1 { get; set; }
        public int Member2;
        public TestClass()
        {
        }
        public TestClass(int member, int member2)
        {
            this.Member1 = member;
            this.Member2 = member2;
        }
        public int GetMemberAll()
        {
            return this.Member1 + this.Member2;
        }
        public override string ToString()
        {
            return Member1.ToString();
        }
    }
}
