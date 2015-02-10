
using System;
using NUnit.Framework;

namespace OSGeo.GDAL.Test.iOS
{
	[TestFixture]
	public class T
	{
		[Test]
		public void Pass ()
		{
			Assert.True (true);
		}

		[Test]
		public void Fail ()
		{
			Assert.False (true);
		}

		[Test]
		[Ignore ("another time")]
		public void Ignore ()
		{
			Assert.True (false);
		}
	}
}
