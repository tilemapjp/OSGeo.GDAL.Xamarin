
using System;
using NUnit.Framework;
using OSGeo.GDAL;

namespace OSGeo.GDAL.Test
{
	[TestFixture]
	public class Warp
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
		public void WarpTest ()
		{
			var srcDS = Gdal.Open (StartPhase.BogotaTiffPath, Access.GA_ReadOnly);
			var documents =
				System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
			var dstFileName = System.IO.Path.Combine(documents, "warpout.tif");
			var dstDS = Gdal.Open (dstFileName, Access.GA_Update);

		}
	}
}
