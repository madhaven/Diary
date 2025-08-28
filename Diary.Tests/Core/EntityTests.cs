using Diary.Models;

namespace Diary.Tests.Core;

public class EntityTests
{
    [Test]
    public void TestInit()
    {
        var time = DateTime.Now;
        var entry = new Entry("hello\n", time, Enumerable.Repeat(1, 6));

        Assert.That(entry.Intervals, Is.EqualTo(Enumerable.Repeat(1, 6).ToList()), "intervals should match");
        Assert.That(entry.PrintDate, Is.EqualTo(false), "Print date should match");
        Assert.That(entry.Time, Is.EqualTo(time), "Entry time should match");
        Assert.That(entry.Text.Length, Is.EqualTo(entry.Intervals.Count), "Intervals should correspond to text in the entry");
        Assert.That(new Entry().Text, Is.EqualTo(""), "Blank Entry should have blank text");
        Assert.That(new Entry().Intervals, Is.EqualTo(Enumerable.Empty<int>()), "Blank Entry intervals should match");
    }

    [Test]
    public void TestToString()
    {
        var entry = new Entry("hello\b\b\b\bmy name is x");
        Assert.That(entry.ToString(), Is.EqualTo("hmy name is x"), "String conversion failed");
    }

    [Test]
    public void TestBackspaceInToString()
    {
        var entry = new Entry("x\b\b\b\b\bhello");
        Assert.That(entry.ToString(), Is.EqualTo("hello"), "String conversion failed");
    }
}
