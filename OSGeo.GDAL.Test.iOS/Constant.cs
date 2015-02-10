using System;
using NUnit.Framework;
using OSGeo.GDAL;

namespace OSGeo.GDAL.Test
{
	[TestFixture]
	public class Constant
	{
		[SetUp]
		public void Setup ()
		{
		}

		[TearDown]
		public void Tear ()
		{
		}

		[Test]
		public void ValueCheck ()
		{
			var fail = (int)GdalConst.CE_Failure;
			Assert.AreEqual (3, fail);
		}
	}
}
