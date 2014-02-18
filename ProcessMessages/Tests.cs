using System;
using NUnit.Framework;

namespace ProcessMessages
{
    [TestFixture]
    class Tests
    {
        [Test]
        public void FormatDate()
        {
            Assert.That(Utils.FormatDate("20130523"), Is.EqualTo("05/23/2013"));
        }
    }
}
