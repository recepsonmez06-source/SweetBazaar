using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace SweetBazaar.Core.Tests
{
    public class LevelPackJsonTests
    {
        private static LevelRecord Record(int number, int seed, int minMoves, int capacity, params int[][] boxes) =>
            new LevelRecord(number, seed, minMoves,
                new LevelDefinition { BoxCapacity = capacity, Boxes = new List<int[]>(boxes) });

        private static LevelPack SamplePack()
        {
            var pack = new LevelPack();
            pack.Add(Record(1, 1, 4, 2, new[] { 0, 1 }, new[] { 1, 0 }, new int[0], new int[0]));
            pack.Add(Record(2, 2, 7, 2, new[] { 0, 0 }, new[] { 1 }, new[] { 1 }));
            return pack;
        }

        private static string Describe(LevelRecord record) =>
            $"{record.Number}/{record.Seed}/{record.MinMoves}/{record.Definition.BoxCapacity}/" +
            string.Join("|", record.Definition.Boxes.Select(box => string.Join(",", box)));

        private const string OneLevel =
            "{\"formatVersion\":1,\"levels\":[{\"number\":1,\"seed\":5,\"boxCapacity\":2,\"minMoves\":3," +
            "\"boxes\":[[0,1],[1,0],[]]}]}";

        [Test]
        public void RoundTrip_KeepsEveryLevelExactly()
        {
            var original = SamplePack();

            var parsed = LevelPackJson.Parse(LevelPackJson.Serialize(original));

            Assert.AreEqual(original.Count, parsed.Count);
            for (int i = 0; i < original.Count; i++)
                Assert.AreEqual(Describe(original.Levels[i]), Describe(parsed.Levels[i]));
        }

        [Test]
        public void EmptyPack_RoundTrips()
        {
            var parsed = LevelPackJson.Parse(LevelPackJson.Serialize(new LevelPack()));

            Assert.AreEqual(0, parsed.Count);
        }

        [Test]
        public void Serialize_WritesOneLevelPerLine()
        {
            string json = LevelPackJson.Serialize(SamplePack());

            Assert.AreEqual(2, json.Split('\n').Count(line => line.Contains("\"number\"")));
            StringAssert.Contains("\"formatVersion\": 1", json);
        }

        [Test]
        public void Parse_IgnoresWhitespaceAndUnknownProperties()
        {
            string json = "  {\n \"extra\": {\"a\": [1, 2.5, true, false, null, \"x\\\"y\\u00e9\"]},\n" +
                          " \"formatVersion\" : 1 , \"levels\" : [ { \"note\":\"hi\", \"number\":1, \"seed\":5," +
                          " \"boxCapacity\":2, \"minMoves\":3, \"boxes\":[ [0, 1], [1,0], [ ] ] } ] }\n";

            var pack = LevelPackJson.Parse(json);

            Assert.AreEqual(1, pack.Count);
            Assert.AreEqual(3, pack.Get(1).Definition.Boxes.Count);
        }

        [Test]
        public void Parse_ProducesALevelThatBuildsABoard()
        {
            var pack = LevelPackJson.Parse(OneLevel);

            var board = Board.FromLevel(pack.Get(1).Definition);

            Assert.AreEqual(3, board.Boxes.Count);
            CollectionAssert.AreEqual(new[] { 1, 0 }, board.Boxes[1].Candies);
        }

        [TestCase("")]
        [TestCase("not json")]
        [TestCase("[]")]
        [TestCase("{\"formatVersion\":1")]
        [TestCase("{\"formatVersion\":1,\"levels\":[]} trailing")]
        [TestCase("{\"formatVersion\":1,\"levels\":[],}")]
        [TestCase("{\"formatVersion\":1,\"levels\":\"x\"}")]
        [TestCase("{\"formatVersion\":\"1\",\"levels\":[]}")]
        [TestCase("{\"formatVersion\":1.5,\"levels\":[]}")]
        public void Parse_RejectsBrokenOrWrongShapedJson(string json)
        {
            Assert.Throws<FormatException>(() => LevelPackJson.Parse(json));
        }

        [Test]
        public void Parse_RejectsAnUnsupportedFormatVersion()
        {
            Assert.Throws<FormatException>(() => LevelPackJson.Parse("{\"formatVersion\":2,\"levels\":[]}"));
        }

        [Test]
        public void Parse_RejectsMissingLevelFields()
        {
            string noSeed = OneLevel.Replace("\"seed\":5,", "");

            var error = Assert.Throws<FormatException>(() => LevelPackJson.Parse(noSeed));
            StringAssert.Contains("seed", error.Message);
        }

        [Test]
        public void Parse_RejectsNonIntegerCandyIds()
        {
            string fractional = OneLevel.Replace("[0,1],[1,0]", "[0,1.5],[1,0]");
            string text = OneLevel.Replace("[0,1],[1,0]", "[0,\"a\"],[1,0]");

            Assert.Throws<FormatException>(() => LevelPackJson.Parse(fractional));
            Assert.Throws<FormatException>(() => LevelPackJson.Parse(text));
        }

        [Test]
        public void Parse_RejectsALevelThatIsNotAnArrayOfBoxes()
        {
            string json = OneLevel.Replace("[[0,1],[1,0],[]]", "[0,1]");

            Assert.Throws<FormatException>(() => LevelPackJson.Parse(json));
        }

        [Test]
        public void Parse_RejectsNumberingGaps()
        {
            string json = OneLevel.Replace("\"number\":1", "\"number\":2");

            Assert.Throws<FormatException>(() => LevelPackJson.Parse(json));
        }

        [Test]
        public void Parse_RejectsTooDeepNesting()
        {
            string json = new string('[', 200) + new string(']', 200);

            Assert.Throws<FormatException>(() => LevelPackJson.Parse(json));
        }
    }

    public class LevelPackTests
    {
        private static LevelRecord Record(int number) =>
            new LevelRecord(number, number, 3,
                new LevelDefinition { BoxCapacity = 2, Boxes = new List<int[]> { new[] { 0, 1 }, new[] { 1, 0 }, new int[0] } });

        [Test]
        public void Add_RequiresConsecutiveNumbers()
        {
            var pack = new LevelPack();
            pack.Add(Record(1));

            Assert.Throws<ArgumentException>(() => pack.Add(Record(3)));
            Assert.Throws<ArgumentException>(() => pack.Add(Record(1)));
            pack.Add(Record(2));
            Assert.AreEqual(2, pack.Count);
        }

        [Test]
        public void Get_ReturnsLevelsByNumber()
        {
            var pack = new LevelPack();
            pack.Add(Record(1));
            pack.Add(Record(2));

            Assert.AreEqual(2, pack.Get(2).Number);
            Assert.Throws<ArgumentOutOfRangeException>(() => pack.Get(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => pack.Get(3));
        }

        [Test]
        public void Validate_ReportsInvalidLevelsWithTheirNumber()
        {
            var pack = new LevelPack();
            pack.Add(Record(1));
            pack.Add(new LevelRecord(2, 2, 3,
                new LevelDefinition { BoxCapacity = 2, Boxes = new List<int[]> { new[] { 0, 0, 0 } } }));

            Assert.That(pack.Validate(), Has.Some.StartsWith("Level 2:"));
            Assert.That(pack.Validate(), Has.None.StartsWith("Level 1:"));
        }
    }
}
