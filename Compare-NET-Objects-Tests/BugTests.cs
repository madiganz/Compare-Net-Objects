﻿using System;
using System.Collections.Generic;
using KellermanSoftware.CompareNetObjects;
using KellermanSoftware.CompareNetObjectsTests.Attributes;
using KellermanSoftware.CompareNetObjectsTests.TestClasses;
using NUnit.Framework;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using KellermanSoftware.CompareNetObjects.Reports;
using KellermanSoftware.CompareNetObjectsTests.TestClasses.HashBug;
using Newtonsoft.Json;
using Point = System.Drawing.Point;

#if !NETSTANDARD
using System.Drawing.Drawing2D;
#endif

namespace KellermanSoftware.CompareNetObjectsTests
{
    [TestFixture]
    public class BugTests
    {
        #region Class Variables
        private CompareLogic _compare;

        #endregion

        #region Setup/Teardown

        /// <summary>
        /// Code that is run before each test
        /// </summary>
        [SetUp]
        public void Initialize()
        {
            _compare = new CompareLogic();
        }

        /// <summary>
        /// Code that is run after each test
        /// </summary>
        [TearDown]
        public void Cleanup()
        {
            _compare = null;
        }
        #endregion

        #region Tests

        [Test]
        public void CompareDynamicStructWithinAStruct()
        {
            var a = (1, new List<(string first, (string, int) second)> { ("foo", ("bar", 2)) });
            var b = (1, new List<(string first, (string, int) second)> { ("foo", ("foo", 2)) });
            CompareLogic compareLogic = new CompareLogic();
            compareLogic.Config.MaxStructDepth = 4;
            var result = compareLogic.Compare(a, b);
            Console.WriteLine(result.DifferencesString);
            Assert.IsFalse(result.AreEqual);
        }

#nullable enable

        //Failed comparison of objects with complex dictionary key
        //https://github.com/GregFinzer/Compare-Net-Objects/issues/222
        [Test]
        public void ComplexDictionaryShouldCompare()
        {
            // Arrange.
            var originalGraph = ComplexDictionaryModel.Create();

            // Act.
            var serializedKeys = JsonConvert.SerializeObject(originalGraph.DictOfDicts.Keys.ToArray());
            var serializedValues = JsonConvert.SerializeObject(originalGraph.DictOfDicts.Values.ToArray());

            var keys = JsonConvert.DeserializeObject<Dictionary<short, int>[]>(serializedKeys);
            var values = JsonConvert.DeserializeObject<Dictionary<string, ushort?>[]>(serializedValues);

            var dictOfDicts = keys.Zip(values, (x, y) => (x, y)).ToDictionary(pair => pair.x, pair => pair.y, new ComplexDictionaryModel.KeyDictionaryComparer());
            var deserializedGraph = new ComplexDictionaryModel(dictOfDicts!);

            // Assert.
            Console.WriteLine("Manual Compare");
            originalGraph.ManualCompare(deserializedGraph);
            Console.WriteLine("Compare .NET Objects");
            originalGraph.CompareObjects(deserializedGraph);
        }

#nullable disable

        [Test]
        public void IPV6AddressShouldBeDifferent()
        {
            IPEndPoint a = new IPEndPoint(IPAddress.Parse("2001:4898:e0:5e:444:fcfd:cccc:1111"), 443) ;
            IPEndPoint b = new IPEndPoint(IPAddress.Parse("2001:4898:e0:5e:4f4:fcfd:854c:16d9"), 443);

            CompareLogic compareLogic = new CompareLogic();

            var result = compareLogic.Compare(a, b);
            Console.WriteLine(result.DifferencesString);
            Assert.IsFalse(result.AreEqual);
        }

        [Test]
        public void ComparingDictionariesReturnTwoDifferencesForSameKey()
        {
            var oldDoc = new TestDoc()
            {
                Fields =
                {
                    ["a"] = 1,
                    ["b"] = 2,
                },
            };

            var newDoc = new TestDoc()
            {
                Fields =
                {
                    ["a"] = 3,
                    ["c"] = 5
                },
            };

            CompareLogic compareLogic = new CompareLogic();
            compareLogic.Config.MaxDifferences = int.MaxValue;

            var result = compareLogic.Compare(oldDoc, newDoc);
            Console.WriteLine(result.DifferencesString);
            Assert.IsTrue(result.Differences.Count == 3);
        }

        [Test]
        public void IgnoreOrderOneListHasADuplicateValue()
        {
            CompareLogic compareLogic = new CompareLogic();
            compareLogic.Config.IgnoreCollectionOrder = true;

            List<string> list1 = new List<string>(){ "Item1", "Item2"};
            List<string> list2 = new List<string>() { "Item1", "Item2", "Item2" };

            var result = compareLogic.Compare(list1, list2);
            Console.WriteLine(result.DifferencesString);
            Assert.IsFalse(result.AreEqual);
        }

        [Test]
        public void IgnoredMemberGetPropertyAccessed()
        {
            //This is the comparison class
            CompareLogic compareLogic = new CompareLogic();
            compareLogic.Config.MembersToIgnore.Add("PersonWithNotImplementedProperty.Name");

            //Create a couple objects to compare
            PersonWithNotImplementedProperty person1 = new PersonWithNotImplementedProperty();
            person1.DateCreated = DateTime.Now;

            PersonWithNotImplementedProperty person2 = new PersonWithNotImplementedProperty();
            person2.DateCreated = person1.DateCreated;
            ComparisonResult result = compareLogic.Compare(person1, person2);

            Assert.IsTrue(result.AreEqual);
        }

        [Test]
        public void CheckIgnoreCollectionOrder()
        {
            Dictionary<Type, IEnumerable<string>> collectionSpec = new Dictionary<Type, IEnumerable<string>>();
            collectionSpec.Add(typeof(Foo2), new string[] { "Prop" });

            var config = new ComparisonConfig { IgnoreCollectionOrder = true, CollectionMatchingSpec = collectionSpec };
            var compareLogic = new CompareLogic(config);

            var actual = new Foo2[] { new Foo2(2), new Foo2(1) };
            var expected = new Foo2[] { new Foo2(1), new Foo2(2) };


            var result = compareLogic.Compare(expected, actual);
            Console.WriteLine(result.DifferencesString);
            Assert.IsTrue(result.AreEqual);
        }

        [Test]
        public void SimpleArrayTest()
        {
            var compareLogic = new CompareLogic();
            compareLogic.Config.IgnoreCollectionOrder = true;
            compareLogic.Config.MaxDifferences = int.MaxValue;
            ComparisonResult result = compareLogic.Compare(new[] { "one" }, new[] { "two" });
            Console.WriteLine(result.DifferencesString);
        }

        [Test]
        public void HashBugTest()
        {
            var c = new HashBugC()
            {
                Text = "I'm C"
            };

            var b = new HashBugB()
            {
                Text = "I'm B",
                CollectionOfC = new List<HashBugC>()
                {
                    c
                }
            };

            var b2 = new HashBugB()
            {
                Text = "I'm B",
                CollectionOfC = new List<HashBugC>()
                {
                    c
                }
            };

            var comparisonResult = b == b2;

            Assert.IsTrue(comparisonResult);
        }

        [Test]
        public void DotsAndTabsShouldFormatCorrectly()
        {
            List<DotsAndTabs> groundTruth = new List<DotsAndTabs>();
            List<DotsAndTabs> newResult = new List<DotsAndTabs>();

            groundTruth.Add(new DotsAndTabs { boo = "hello", gg = "hello again" });
            groundTruth.Add(new DotsAndTabs { boo = "scorpio \t. .\r\n11-nov", gg = "hello again2" });
            newResult.Add(new DotsAndTabs { boo = "hello", gg = "hello again" });
            newResult.Add(new DotsAndTabs { boo = "  .....  ", gg = "hello again2" });

            CompareLogic compareLogicObject = new CompareLogic();
            compareLogicObject.Config.MaxDifferences = int.MaxValue;
            compareLogicObject.Config.IgnoreCollectionOrder = true;

            ComparisonResult assertionResult = compareLogicObject.Compare(groundTruth, newResult);

            Console.WriteLine(assertionResult.DifferencesString + "\n\n\n");

            UserFriendlyReport friendlyReport = new UserFriendlyReport();

            Console.WriteLine(friendlyReport.OutputString(assertionResult.Differences));

        }

        [Test]
        public void ComparingListsOfNullWhileIgnoringCollectionOrderShouldNotThrowObjectReferenceError()
        {
            //Arrange
            ComparisonConfig config = new ComparisonConfig();
            config.IgnoreCollectionOrder = true;
            config.IgnoreObjectTypes = true;

            CompareLogic logic = new CompareLogic(config);
            List<object> data1 = new List<object>();
            data1.Add(null);

            List<object> data2 = new List<object>();
            data2.Add(null);

            //Act
            var result = logic.Compare(data1, data2);

            //Assert
            Assert.IsTrue(result.AreEqual, result.DifferencesString);

        }

        [Test]
        public void NegativeIntegersShouldBeNegativeOnUserFriendlyReport()
        {
            List<object> groundTruth = new List<object>();
            List<object> newResult = new List<object>();


            groundTruth.Add(new { boo = 1, gg = 7 });
            groundTruth.Add(new { boo = -5, gg = 9 });
            newResult.Add(new { boo = -6, gg = 4 });
            newResult.Add(new { boo = 5, gg = 23 });

            CompareLogic compareLogicObject = new CompareLogic();
            compareLogicObject.Config.MaxDifferences = int.MaxValue;
            compareLogicObject.Config.IgnoreCollectionOrder = true;

            ComparisonResult assertionResult = compareLogicObject.Compare(groundTruth, newResult);

            Console.WriteLine("DifferencesString");
            Console.WriteLine(assertionResult.DifferencesString);

            Console.WriteLine();
            Console.WriteLine("UserFriendlyReport");
            UserFriendlyReport friendlyReport = new UserFriendlyReport();
            string result = friendlyReport.OutputString(assertionResult.Differences);
            Console.WriteLine(result);

            Assert.IsTrue(result.Contains("[{ boo = -5, gg = 9 }]"));
        }

        [Test]
        public void ObjectWithSameHashCodeAndDifferentPropertiesShouldBeDifferent()
        {
            ClassWithOverriddenHashCode person1 = new ClassWithOverriddenHashCode();
            person1.Name = "Weird Al Yankovic";
            person1.MyCircularReference = person1;

            ClassWithOverriddenHashCode person2 = new ClassWithOverriddenHashCode();
            person2.Name = "Robin Williams";
            person2.MyCircularReference = person2;

            CompareLogic compareLogic = new CompareLogic();
            ComparisonResult result = compareLogic.Compare(person1, person2);
            Console.WriteLine(result.DifferencesString);
            Assert.IsFalse(result.AreEqual);
        }

        [Test]
        public void RefStructProperty()
        {
            var compareLogic = new CompareLogic(new ComparisonConfig
            {
                MembersToIgnore =
                {
                    "Item"
                }
            });

            var differences = compareLogic.Compare(new RefStructClass(), new RefStructClass());
            Assert.IsTrue(differences.AreEqual);
        }



        [Test]
        public void When_CompareDateTimeOffsetWithOffsets_Is_False_Do_Not_Compare_Offsets()
        {
            DateTime now = DateTime.Now;
            DateTimeOffset date1 = new DateTimeOffset(
                now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, new TimeSpan(0, 0, 0));
            DateTimeOffset date2 = new DateTimeOffset(
                now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, new TimeSpan(3, 0, 0));

            _compare.Config.CompareDateTimeOffsetWithOffsets = false;
            ComparisonResult result = _compare.Compare(date1, date2);

            if (!result.AreEqual)
                throw new Exception(result.DifferencesString);
        }

        [Test]
        public void When_CompareDateTimeOffsetWithOffsets_Is_True_Compare_Offsets()
        {
            DateTime now = DateTime.Now;
            DateTimeOffset date1 = new DateTimeOffset(
                now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, new TimeSpan(0, 0, 0));
            DateTimeOffset date2 = new DateTimeOffset(
                now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, new TimeSpan(3, 0, 0));

            _compare.Config.CompareDateTimeOffsetWithOffsets = true;
            ComparisonResult result = _compare.Compare(date1, date2);

            Assert.IsFalse(result.AreEqual);
        }

        [Test]
        public void TimespanShouldCompareWhenCompareChildrenIsFalse()
        {
            //Arrange
            var object1 = new ClassWithTimespan() { MyTimeSpan = new TimeSpan(6, 0, 0) };
            var object2 = new ClassWithTimespan { MyTimeSpan = new TimeSpan(7, 0, 0) };

            var comparerConfig = new ComparisonConfig();
            comparerConfig.CompareChildren = false;
            var comparer = new CompareLogic(comparerConfig);

            //Act
            var result = comparer.Compare(object1, object2);

            //Assert
            Assert.IsFalse(result.AreEqual);
        }

        [Test]
        public void GetPrivatePropertiesNetStandard()
        {
            //Arrange
            Type type = typeof(ClassWithPrivateProperties);

            //Act
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            //Assert
            Assert.IsTrue(props.Length > 0);
        }

        /// <summary>
        /// https://github.com/GregFinzer/Compare-Net-Objects/issues/77
        /// </summary>
        [Test]
        public void ComparisonsOfTypesWithPrivateFieldsAreAccurate()
        {
            var compareLogic = new CompareLogic(new ComparisonConfig { ComparePrivateFields = true });
            var result = compareLogic.Compare(new SomethingWithPrivateField(123), new SomethingWithPrivateField(456));
            Assert.IsFalse(result.AreEqual);
        }

        private class SomethingWithPrivateField
        {
            private readonly int _key;
            public SomethingWithPrivateField(int key) { _key = key; }
        }

        /// <summary>
        /// https://github.com/GregFinzer/Compare-Net-Objects/issues/77
        /// </summary>
        [Test]
        public void ComparisonsOfTypesWithPrivatePropertiesAreAccurate()
        {
            var compareLogic = new CompareLogic(new ComparisonConfig { ComparePrivateProperties = true });
            var result = compareLogic.Compare(new SomethingWithPrivateProperty(123), new SomethingWithPrivateProperty(456));
            Assert.IsFalse(result.AreEqual);
        }

        private class SomethingWithPrivateProperty
        {
            public SomethingWithPrivateProperty(int key) { Key = key; }
            private int Key { get; }
        }

        /// <summary>
        /// https://github.com/GregFinzer/Compare-Net-Objects/issues/110
        /// </summary>
        [Test]
        public void CsvReportWithCommaTest()
        {
            // set up data
            Person person1 = new Person();
            person1.Name = "Greg";
            person1.LastName = "Miller";
            person1.Age = 42;

            Person person2 = new Person();
            person2.Name = "Greg";
            person2.LastName = "Miller";
            person2.Age = 17;

            // compare
            var left = new List<Person> { person1 };
            var right = new List<Person> { person2 };

            CompareLogic compareLogic = new CompareLogic();
            compareLogic.Config.IgnoreCollectionOrder = true;
            compareLogic.Config.CollectionMatchingSpec.Add(
                typeof(Person),
                new string[] { "Name", "LastName" });   // specify two indexes

            ComparisonResult result = compareLogic.Compare(left, right);

            // write to csv
            var csv = new CsvReport();
            string output = csv.OutputString(result.Differences);
            Console.WriteLine(output);
            Assert.IsTrue(output.Contains("\"[Name:Greg,LastName:Miller].Age\""));
        }

        [Test]
        public void DifferentNullableDecimalFieldsShouldNotBeEqualWhenCompareChildrenIsFalse()
        {
            CompareLogic compareLogic = new CompareLogic();
            compareLogic.Config.CompareChildren = false;

            PrimitiveFieldsNullable object1 = new PrimitiveFieldsNullable();
            object1.DecimalField = 0;

            PrimitiveFieldsNullable object2 = new PrimitiveFieldsNullable();
            object2.DecimalField = 3.0M;

            Assert.IsFalse(compareLogic.Compare(object1, object2).AreEqual);
        }

        [Test]
        public void DifferentDecimalFieldsShouldNotBeEqualWhenCompareChildrenIsFalse()
        {
            CompareLogic compareLogic = new CompareLogic();
            compareLogic.Config.CompareChildren = false;

            PrimitiveFields object1 = new PrimitiveFields();
            object1.DecimalField = 0;

            PrimitiveFields object2 = new PrimitiveFields();
            object2.DecimalField = 3.0M;

            Assert.IsFalse(compareLogic.Compare(object1, object2).AreEqual);
        }

        [Test]
        public void DifferentIntegersShouldNotBeEqualInsideAnAnonymousType()
        {
            CompareLogic compareLogic = new CompareLogic();

            // try with integers
            var int1 = new { MyNumber = (int)0 };
            var int2 = new { MyNumber = (int)3 };

            // test with CompareChildren = false
            compareLogic.Config.CompareChildren = false;
            ComparisonResult test3 = compareLogic.Compare(int1, int2);
            Assert.IsFalse(test3.AreEqual, "int Test - CompareChildren = false");

            // test with CompareChildren = true
            compareLogic.Config.CompareChildren = true;
            ComparisonResult test4 = compareLogic.Compare(int1, int2);
            Assert.AreEqual(1, test4.Differences.Count);
            Assert.IsFalse(test4.AreEqual, "int Test - CompareChildren = true");
        }

        [Test]
        public void DifferentDecimalsShouldNotBeEqualInsideAnAnonymousType()
        {
            CompareLogic compareLogic = new CompareLogic();

            // try with decimals
            var dec1 = new { MyNumber = (decimal)0 };
            var dec2 = new { MyNumber = (decimal)3.0 };

            // test with CompareChildren = false
            compareLogic.Config.CompareChildren = false;
            ComparisonResult test1 = compareLogic.Compare(dec1, dec2);
            Assert.IsFalse(test1.AreEqual, "Decimal Test - CompareChildren = false");

            // test with CompareChildren = true
            compareLogic.Config.CompareChildren = true;
            ComparisonResult test2 = compareLogic.Compare(dec1, dec2);
            Assert.IsFalse(test2.AreEqual, "Decimal Test - CompareChildren = true");
        }

        [Test]
        public void NullableDecimalWithCompareChildrenFalseShouldSendADifferenceCallback()
        {
            List<Difference> differences = new List<Difference>();

            SpecialFields specialFields1 = new SpecialFields();
            specialFields1.NullableDecimalProperty = 1000;

            SpecialFields specialFields2 = new SpecialFields();
            specialFields2.NullableDecimalProperty = 2000;

            _compare.Config = new ComparisonConfig()
            {
                CompareChildren = false,
                DifferenceCallback = difference => { differences.Add(difference); }
            };

            _compare.Compare(specialFields1, specialFields2);
            Assert.That(differences.FirstOrDefault(), Is.Not.Null);
        }

        [Test]
        public void PropertyNameShouldNotHaveAPeriodInFrontOfIt()
        {
            Person person1 = new Person() { Name = "Luke Skywalker", DateCreated = DateTime.Today, ID = 1 };
            Person person2 = new Person() { Name = "Leia Skywalker", DateCreated = DateTime.Today, ID = 1 };

            var result = _compare.Compare(person1, person2);

            Assert.IsFalse(result.AreEqual, "Expected to be different");
            Assert.IsFalse(result.Differences[0].PropertyName.StartsWith("."), "Expected not to start with period");

        }

        [Test]
        public void ShowBreadCrumbTest()
        {
            var people1 = new List<Person>() { new Person() { Name = "Joe" } };
            var people2 = new List<Person>() { new Person() { Name = "Joe" } };
            var group1 = new KeyValuePair<string, List<Person>>("People", people1);
            var group2 = new KeyValuePair<string, List<Person>>("People", people2);
            _compare.Config.ShowBreadcrumb = true;
            var result = _compare.Compare(group1, group2);
            Assert.IsTrue(result.AreEqual);
        }
        [Test]
        public void ListOfDictionariesWithIgnoreOrder()
        {
            var bar1 = new List<Dictionary<string, string>>
            {
                new Dictionary<string, string>
                {
                    {"a", "b"},
                    {"c", "d"},
                    {"e", "f"},
                    {"g", "h"},
                }
            };

            var bar2 = new List<Dictionary<string, string>>
            {
                new Dictionary<string, string>
                {
                    {"e", "f"},
                    {"g", "h"},
                    {"c", "d"},
                    {"a", "b"},
                }
            };

            var comparer = new CompareLogic { Config = { IgnoreCollectionOrder = true } };
            var res = comparer.Compare(bar1, bar2);
            Assert.IsTrue(res.AreEqual, res.DifferencesString);
        }

        [Test]
        public void DbNullTest()
        {
            CompareLogic compareLogic = new CompareLogic();

            ComparisonResult result = compareLogic.Compare(DBNull.Value, DBNull.Value);
            if (!result.AreEqual)
                Console.WriteLine(result.DifferencesString);
        }

        [Test]
        public void PropertyComparerFailsWithObjectNullException()
        {
            //This is the comparison class
            CompareLogic compareLogic = new CompareLogic();
            compareLogic.Config.SkipInvalidIndexers = true;

            //Create a couple objects to compare
            Person2 person1 = new Person2();
            person1.DateCreated = DateTime.Now;
            person1.Name = "Greg";

            Person2 person2 = new Person2();
            person2.Name = "John";
            person2.DateCreated = person1.DateCreated;

            //These will be different, write out the differences
            ComparisonResult result = compareLogic.Compare(person1, person2);
            if (!result.AreEqual)
                Console.WriteLine(result.DifferencesString);
        }

        [Test]
        public void GermanUmlautsAndAccents()
        {
            string string1 = "straße";
            string string2 = "strasse";

            ComparisonResult result = _compare.Compare(string1, string2);
            Console.WriteLine(result.DifferencesString);
            Assert.IsFalse(result.AreEqual);
        }

        [Test]
        public void CircularReferencesEnumerable()
        {

            var o1 = new ClassWithOverriddenHashCode();
            o1.Name = "1";

            var o2 = new ClassWithOverriddenHashCode();
            o2.Name = "1";

            IEnumerable<ClassWithOverriddenHashCode> e1 =(new [] {o1}).Where(_ => true);
            o1.MyCircularReference = e1;
            IEnumerable<ClassWithOverriddenHashCode> e2 =(new [] {o2}).Where(_ => true);
            o2.MyCircularReference = e2;

            CompareLogic logic = new CompareLogic();

            ComparisonResult result = logic.Compare(e1, e2);
            Assert.IsTrue(result.AreEqual, result.DifferencesString);
        }

        public class Foo
        {
        }

        public class Bar
        {
        }

        [Test]
        public void GenericEnumerable()
        {
            List<Foo> fooList = new List<Foo>();
            IEnumerable<Bar> barEnumerable = fooList.Select(f => new Bar());
            List<Bar> barList = new List<Bar>();

            CompareLogic logic = new CompareLogic();
            logic.Config.IgnoreObjectTypes = true;

            ComparisonResult result = logic.Compare(barEnumerable, barList);
            Assert.IsTrue(result.AreEqual, result.DifferencesString);
        }

        [Test]
        public void ObjectTypeObjectTest()
        {
            ObjectTypeClass objectClass1 = new ObjectTypeClass();
            objectClass1.FieldObject = new object();
            objectClass1.PropertyObject = new object();
            ObjectTypeClass.StaticObject = new object();

            ObjectTypeClass objectClass2 = new ObjectTypeClass();
            objectClass2.FieldObject = new object();
            objectClass2.PropertyObject = new object();

            ComparisonResult result = _compare.Compare(objectClass1, objectClass2);

            if (!result.AreEqual)
                Assert.Fail(result.DifferencesString);
        }

        [Test]
        public void IgnoreTypesTest()
        {
            ExampleDto1 dto1 = new ExampleDto1();
            dto1.Name = "Greg";

            ExampleDto2 dto2 = new ExampleDto2();
            dto2.Name = "Greg";

            ComparisonResult result = _compare.Compare(dto1, dto2);

            //These will be different because the types are different
            Assert.IsFalse(result.AreEqual);
            Console.WriteLine(result.DifferencesString);

            _compare.Config.IgnoreObjectTypes = true;

            result = _compare.Compare(dto1, dto2);

            //Ignore types so they will be equal
            Assert.IsTrue(result.AreEqual);

            _compare.Config.Reset();

        }






        [Test]
        public void TankElementsToIncludeTest()
        {
            _compare.Config.MaxDifferences = 10;

            _compare.Config.MembersToInclude.Add("TankPerson");
            _compare.Config.MembersToInclude.Add("TankName");
            _compare.Config.MembersToInclude.Add("Name");
            _compare.Config.MembersToInclude.Add("FamilyName");
            _compare.Config.MembersToInclude.Add("GivenName");

            //Create a couple objects to compare
            TankPerson person1 = new TankPerson()
            {
                Id = 1,
                DateCreated = DateTime.Now,
                Name = new TankName { FamilyName = "Huston", GivenName = "Greg" },
                Address = "Address1"
            };
            TankPerson person2 = new TankPerson()
            {
                Id = 2,
                Name = new TankName { FamilyName = "McClane", GivenName = "John" },
                DateCreated = DateTime.UtcNow,
                Address = "Address2"
            };

            ComparisonResult result = _compare.Compare(person1, person2);
            Assert.IsFalse(result.AreEqual);

            //These will be different, write out the differences
            if (!result.AreEqual)
            {
                Console.WriteLine("------");

                Console.WriteLine("###################");
                result.Differences.ForEach(d => Console.WriteLine(d.ToString()));
            }

            _compare.Config.MembersToInclude.Clear();
            _compare.Config.MaxDifferences = 1;
        }

        private Shipment CreateShipment()
        {
            return new Shipment { Customer = "ADEG", IdentCode = 12934871928374, InsertDate = new DateTime(2012, 06, 12) };
        }

        [Test]
        public void IgnoreByAttribute_test_should_fail_difference_should_be_customer()
        {
            // Arrange
            Shipment shipment1 = CreateShipment();
            Shipment shipment2 = CreateShipment();
            shipment2.InsertDate = DateTime.Now; // InsertDate has the CompareIgnoreAttribute on it
            shipment2.Customer = "Andritz";

            _compare.Config.AttributesToIgnore.Add(typeof(CompareIgnoreAttribute));
            _compare.Config.MaxDifferences = int.MaxValue;

            // Act
            var result = _compare.Compare(shipment1, shipment2);

            // Assert
            Assert.IsFalse(result.AreEqual);
            Assert.AreEqual(1, result.Differences.Count);
            Console.WriteLine(result.DifferencesString);
            Assert.AreEqual("ADEG", result.Differences[0].Object1Value);
            Assert.AreEqual("Andritz", result.Differences[0].Object2Value);

            _compare.Config.AttributesToIgnore.Clear();
        }

        [Test]
        public void IgnoreByLackOfAttribute_test_should_fail_difference_should_be_customer()
        {
            // Arrange
            Shipment shipment1 = CreateShipment();
            Shipment shipment2 = CreateShipment();
            shipment2.InsertDate = DateTime.Now;
            shipment2.Customer = "Andritz"; // Only Customer has the CompareAttribute on it

            _compare.Config.RequiredAttributesToCompare.Add(typeof(CompareAttribute));
            _compare.Config.MaxDifferences = int.MaxValue;

            // Act
            var result = _compare.Compare(shipment1, shipment2);

            // Assert
            Assert.IsFalse(result.AreEqual);
            Assert.AreEqual(1, result.Differences.Count);
            Console.WriteLine(result.DifferencesString);
            Assert.AreEqual("ADEG", result.Differences[0].Object1Value);
            Assert.AreEqual("Andritz", result.Differences[0].Object2Value);

            _compare.Config.RequiredAttributesToCompare.Clear();
        }

        [Test]
        public void WilliamCWarnerTest()
        {
            ILabTest labTest = new LabTest();
            labTest.AlternateContainerDescription = "Test 1";
            labTest.TestName = "Test The Audit";

            ILabTest origLabLest = new LabTest();//this would be in session
            origLabLest.TestName = "Original Test Name";
            origLabLest.AlternateContainerDescription = "Test 2";

            _compare.Config.MaxDifferences = 500;
            var result = _compare.Compare(labTest, origLabLest);

            Assert.IsFalse(result.AreEqual);
            Assert.IsTrue(result.Differences.Count > 0);
            Console.WriteLine(result.DifferencesString);
        }

        [Test]
        public void IgnoreByAttribute_IgnoreCollectionOrder_ShouldPass()
        {
            // Arrange
            Shipment shipment1 = new Shipment() { Customer = "Name1" };
            shipment1.InsertDate = DateTime.Now; // InsertDate has the CompareIgnoreAttribute on it
            Shipment shipment2 = new Shipment() { Customer = "Name2" };

            _compare.Config.AttributesToIgnore.Add(typeof(CompareIgnoreAttribute));
            _compare.Config.IgnoreCollectionOrder = true;
            _compare.Config.MaxDifferences = int.MaxValue;

            List<Shipment> shipments1 = new List<Shipment>();
            shipments1.Add(shipment1);
            shipments1.Add(shipment2);

            List<Shipment> shipments2 = new List<Shipment>();
            Shipment shipment3 = new Shipment() { Customer = "Name2" };
            Shipment shipment4 = new Shipment() { Customer = "Name1" };
            shipment4.InsertDate = DateTime.Now; // InsertDate has the CompareIgnoreAttribute on it

            //add in different order
            shipments2.Add(shipment3);
            shipments2.Add(shipment4);

            //shipment1 & shipment3 are same, shipment2 and shipment4 are also same but their InsertDate is different. 
            //Also the order of items are diiferent in both list.

            // Act
            var result = _compare.Compare(shipments1, shipments2);

            // Assert
            Assert.IsTrue(result.AreEqual);
            Assert.AreEqual(0, result.Differences.Count);
            Console.WriteLine(result.DifferencesString);

            _compare.Config.AttributesToIgnore.Clear();
        }

        [Test]
        public void IgnoreByAttribute_IgnoreCollectionOrder_ShouldFail()
        {
            // Arrange
            Shipment shipment1 = new Shipment() { Customer = "Name1" };
            shipment1.InsertDate = DateTime.Now; // InsertDate has the CompareIgnoreAttribute on it
            Shipment shipment2 = new Shipment() { Customer = "Name2" };

            //No ignored attributes added so test should fail for property InsertDate
            _compare.Config.IgnoreCollectionOrder = true;
            _compare.Config.MaxDifferences = int.MaxValue;

            List<Shipment> shipments1 = new List<Shipment>();
            shipments1.Add(shipment1);
            shipments1.Add(shipment2);

            List<Shipment> shipments2 = new List<Shipment>();
            Shipment shipment3 = new Shipment() { Customer = "Name2" };
            Shipment shipment4 = new Shipment() { Customer = "Name1" };
            shipment4.InsertDate = DateTime.Now.AddDays(1); // Set different value for InsertDate

            //add in different order
            shipments2.Add(shipment3);
            shipments2.Add(shipment4);

            //shipment1 & shipment3 are same, shipment2 and shipment4 are also same but their InsertDate is different. 
            //Also the order of items are diiferent in both list.

            // Act
            var result = _compare.Compare(shipments1, shipments2);

            // Assert
            Assert.IsFalse(result.AreEqual);
            Assert.AreEqual(2, result.Differences.Count);
            Console.WriteLine(result.DifferencesString);

            _compare.Config.AttributesToIgnore.Clear();
        }

#if !NETSTANDARD

        [Test]
        public void LinearGradient()
        {
            LinearGradientBrush brush1 = new LinearGradientBrush(new Point(), new Point(0, 10), Color.Red, Color.Red);
            LinearGradientBrush brush2 = new LinearGradientBrush(new Point(), new Point(0, 10), Color.Red, Color.Blue);

            Assert.IsFalse(_compare.Compare(brush1, brush2).AreEqual);
        }

#endif

        [Test]
        public void DecimalCollectionWhenOrderIgnored()
        {
            var compare = new CompareLogic(new ComparisonConfig
            {
                IgnoreCollectionOrder = true
            });
            Assert.IsTrue(compare.Compare(new decimal[] { 10, 1 }, new[] { 10.0m, 1.0m }).AreEqual);
        }

        #endregion
    }
}