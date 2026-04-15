using System;
using System.Text;
using PG.StarWarsGame.Engine.Utilities;
using Xunit;

namespace PG.StarWarsGame.Engine.FileSystem.Test.Utilities;

public class ValueStringBuilderTests
{
    [Fact]
        public void Ctor_Default_CanAppend()
        {
            var vsb = default(ValueStringBuilder);
            Assert.Equal(0, vsb.Length);

            vsb.Append('a');
            Assert.Equal(1, vsb.Length);
            Assert.Equal("a", vsb.ToString());
        }

        [Fact]
        public void Ctor_Span_CanAppend()
        {
            var vsb = new ValueStringBuilder(new char[1]);
            Assert.Equal(0, vsb.Length);

            vsb.Append('a');
            Assert.Equal(1, vsb.Length);
            Assert.Equal("a", vsb.ToString());
        }

        [Fact]
        public void Ctor_InitialCapacity_CanAppend()
        {
            var vsb = new ValueStringBuilder(1);
            Assert.Equal(0, vsb.Length);

            vsb.Append('a');
            Assert.Equal(1, vsb.Length);
            Assert.Equal("a", vsb.ToString());
        }

        [Fact]
        public void Append_Char_MatchesStringBuilder()
        {
            var sb = new StringBuilder();
            var vsb = new ValueStringBuilder();
            for (var i = 1; i <= 100; i++)
            {
                sb.Append((char)i);
                vsb.Append((char)i);
            }

            Assert.Equal(sb.Length, vsb.Length);
            Assert.Equal(sb.ToString(), vsb.ToString());
        }

        [Fact]
        public void Append_String_MatchesStringBuilder()
        {
            var sb = new StringBuilder();
            var vsb = new ValueStringBuilder();
            for (var i = 1; i <= 100; i++)
            {
                var s = i.ToString();
                sb.Append(s);
                vsb.Append(s);
            }

            Assert.Equal(sb.Length, vsb.Length);
            Assert.Equal(sb.ToString(), vsb.ToString());
        }

        [Theory]
        [InlineData(0, 4 * 1024 * 1024)]
        [InlineData(1025, 4 * 1024 * 1024)]
        [InlineData(3 * 1024 * 1024, 6 * 1024 * 1024)]
        public void Append_String_Large_MatchesStringBuilder(int initialLength, int stringLength)
        {
            var sb = new StringBuilder(initialLength);
            var vsb = new ValueStringBuilder(new char[initialLength]);

            var s = new string('a', stringLength);
            sb.Append(s);
            vsb.Append(s);

            Assert.Equal(sb.Length, vsb.Length);
            Assert.Equal(sb.ToString(), vsb.ToString());
        }

        [Fact]
        public void Append_CharInt_MatchesStringBuilder()
        {
            var sb = new StringBuilder();
            var vsb = new ValueStringBuilder();
            for (var i = 1; i <= 100; i++)
            {
                sb.Append((char)i, i);
                vsb.Append((char)i, i);
            }

            Assert.Equal(sb.Length, vsb.Length);
            Assert.Equal(sb.ToString(), vsb.ToString());
        }

        [Fact]
        public void AppendSpan_Capacity()
        {
            var vsb = new ValueStringBuilder();

            vsb.AppendSpan(17);
            Assert.Equal(32, vsb.Capacity);

            vsb.AppendSpan(100);
            Assert.Equal(128, vsb.Capacity);
        }

        [Fact]
        public void AppendSpan_DataAppendedCorrectly()
        {
            var sb = new StringBuilder();
            var vsb = new ValueStringBuilder();

            for (var i = 1; i <= 1000; i++)
            {
                var s = i.ToString();

                sb.Append(s);

                var span = vsb.AppendSpan(s.Length);
                Assert.Equal(sb.Length, vsb.Length);

                s.AsSpan().CopyTo(span);
            }

            Assert.Equal(sb.Length, vsb.Length);
            Assert.Equal(sb.ToString(), vsb.ToString());
        }

        [Fact]
        public void Insert_IntCharInt_MatchesStringBuilder()
        {
            var sb = new StringBuilder();
            var vsb = new ValueStringBuilder();
            var rand = new Random(42);

            for (var i = 1; i <= 100; i++)
            {
                var index = rand.Next(sb.Length);
                sb.Insert(index, new string((char)i, 1), i);
                vsb.Insert(index, (char)i, i);
            }

            Assert.Equal(sb.Length, vsb.Length);
            Assert.Equal(sb.ToString(), vsb.ToString());
        }

        [Fact]
        public void Insert_IntString_MatchesStringBuilder()
        {
            var sb = new StringBuilder();
            var vsb = new ValueStringBuilder();

            sb.Insert(0, new string('a', 6));
            vsb.Insert(0, new string('a', 6));
            Assert.Equal(6, vsb.Length);
            Assert.Equal(16, vsb.Capacity);

            sb.Insert(0, new string('b', 11));
            vsb.Insert(0, new string('b', 11));
            Assert.Equal(17, vsb.Length);
            Assert.Equal(32, vsb.Capacity);

            sb.Insert(0, new string('c', 15));
            vsb.Insert(0, new string('c', 15));
            Assert.Equal(32, vsb.Length);
            Assert.Equal(32, vsb.Capacity);

            sb.Length = 24;
            vsb.Length = 24;

            sb.Insert(0, new string('d', 40));
            vsb.Insert(0, new string('d', 40));
            Assert.Equal(64, vsb.Length);
            Assert.Equal(64, vsb.Capacity);

            Assert.Equal(sb.Length, vsb.Length);
            Assert.Equal(sb.ToString(), vsb.ToString());
        }

        [Fact]
        public void AsSpan_ReturnsCorrectValue_DoesntClearBuilder()
        {
            var sb = new StringBuilder();
            var vsb = new ValueStringBuilder();

            for (var i = 1; i <= 100; i++)
            {
                var s = i.ToString();
                sb.Append(s);
                vsb.Append(s);
            }

            var resultString = vsb.AsSpan().ToString();
            Assert.Equal(sb.ToString(), resultString);

            Assert.NotEqual(0, sb.Length);
            Assert.Equal(sb.Length, vsb.Length);
            Assert.Equal(sb.ToString(), vsb.ToString());
        }

        [Fact]
        public void ToString_ClearsBuilder_ThenReusable()
        {
            const string Text1 = "test";
            var vsb = new ValueStringBuilder();

            vsb.Append(Text1);
            Assert.Equal(Text1.Length, vsb.Length);

            var s = vsb.ToString();
            Assert.Equal(Text1, s);

            Assert.Equal(0, vsb.Length);
            Assert.Equal(string.Empty, vsb.ToString());

            const string Text2 = "another test";
            vsb.Append(Text2);
            Assert.Equal(Text2.Length, vsb.Length);
            Assert.Equal(Text2, vsb.ToString());
        }

        [Fact]
        public void Dispose_ClearsBuilder_ThenReusable()
        {
            const string Text1 = "test";
            var vsb = new ValueStringBuilder();

            vsb.Append(Text1);
            Assert.Equal(Text1.Length, vsb.Length);

            vsb.Dispose();

            Assert.Equal(0, vsb.Length);
            Assert.Equal(string.Empty, vsb.ToString());

            const string Text2 = "another test";
            vsb.Append(Text2);
            Assert.Equal(Text2.Length, vsb.Length);
            Assert.Equal(Text2, vsb.ToString());
        }

        [Fact]
        public void Indexer()
        {
            const string Text1 = "foobar";
            var vsb = new ValueStringBuilder();

            vsb.Append(Text1);

            Assert.Equal('b', vsb[3]);
            vsb[3] = 'c';
            Assert.Equal('c', vsb[3]);
            vsb.Dispose();
        }

        [Fact]
        public void Remove_ZeroLength_NoOp()
        {
            var vsb = new ValueStringBuilder();
            vsb.Append("abc");
            vsb.Remove(1, 0);
            Assert.Equal("abc", vsb.ToString());
        }

        [Fact]
        public void Remove_Start()
        {
            var vsb = new ValueStringBuilder();
            vsb.Append("abcde");
            vsb.Remove(0, 2);
            var res = vsb.ToString();
            Assert.Equal("cde", res);
        }

        [Fact]
        public void Remove_Middle()
        {
            var vsb = new ValueStringBuilder();
            vsb.Append("abcde");
            vsb.Remove(1, 3);
            var res = vsb.ToString();
            Assert.Equal("ae", res);
        }

        [Fact]
        public void Remove_End()
        {
            var vsb = new ValueStringBuilder();
            vsb.Append("abcde");
            vsb.Remove(3, 2);
            var res = vsb.ToString();
            Assert.Equal("abc", res);
        }

        [Fact]
        public void Remove_EntireContent()
        {
            var vsb = new ValueStringBuilder();
            vsb.Append("abcde");
            vsb.Remove(0, 5);
            Assert.Equal(0, vsb.Length);
            Assert.Equal(string.Empty, vsb.ToString());
        }

        [Theory]
        [InlineData(-1, 1)] // negative startIndex
        [InlineData(0, -1)] // negative length
        [InlineData(0, 6)]  // length too large
        [InlineData(3, 3)]  // range too large
        public void Remove_Invalid_ThrowsArgumentOutOfRangeException(int startIndex, int length)
        {
            var vsb = new ValueStringBuilder();
            vsb.Append("abcde");
            try
            {
                vsb.Remove(startIndex, length);
                Assert.Fail("Expected ArgumentOutOfRangeException");
            }
            catch (ArgumentOutOfRangeException)
            {
                // Expected
            }
        }

        [Fact]
        public void EnsureCapacity_IfRequestedCapacityWins()
        {
            // Note: constants used here may be dependent on minimal buffer size
            // the ArrayPool is able to return.
            var builder = new ValueStringBuilder(stackalloc char[32]);

            builder.EnsureCapacity(65);

            Assert.Equal(128, builder.Capacity);
        }

        [Fact]
        public void EnsureCapacity_IfBufferTimesTwoWins()
        {
            var builder = new ValueStringBuilder(stackalloc char[32]);

            builder.EnsureCapacity(33);

            Assert.Equal(64, builder.Capacity);
            builder.Dispose();
        }

        [Fact]
        public void EnsureCapacity_NoAllocIfNotNeeded()
        {
            // Note: constants used here may be dependent on minimal buffer size
            // the ArrayPool is able to return.
            var builder = new ValueStringBuilder(stackalloc char[64]);

            builder.EnsureCapacity(16);

            Assert.Equal(64, builder.Capacity);
            builder.Dispose();
        }
}