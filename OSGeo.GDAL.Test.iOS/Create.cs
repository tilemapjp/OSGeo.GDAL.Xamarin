
using System;
using NUnit.Framework;
using System.IO;

namespace OSGeo.GDAL.Test
{
	[TestFixture]
	public class Create
	{
		string format;
		Driver driver;

		[SetUp]
		public void Setup ()
		{
			format = "GTiff";
			driver = Gdal.GetDriverByName (format);
		}

		[TearDown]
		public void Tear ()
		{
		}

		[Test]
		public void EnableCheck ()
		{
			var metaDataCreate = driver.GetMetadataItem (GdalConst.GDAL_DCAP_CREATE,"");
			var metaDataCopy = driver.GetMetadataItem (GdalConst.GDAL_DCAP_CREATECOPY,"");

			Console.WriteLine ("Driver {0} supports Create() method: {1}",format,metaDataCreate);
			Assert.AreEqual (metaDataCreate, "YES");

			Console.WriteLine ("Driver {0} supports CreateCopy() method: {1}",format,metaDataCopy);
			Assert.AreEqual (metaDataCopy, "YES");
		}

		[Test]
		public void CreateCopy ()
		{
			var documents =
				System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
			var dstFileName = System.IO.Path.Combine(documents, "bogota2.tif");
			if (File.Exists (dstFileName))
				File.Delete (dstFileName);
				
			var srcDS = Gdal.Open (StartPhase.BogotaTiffPath, Access.GA_ReadOnly);

			var dstDS = driver.CreateCopy (dstFileName, srcDS, 0, null, null, null);

			if (dstDS != null) {
				dstDS.Dispose ();
				dstDS = null;
			}
			srcDS.Dispose ();
			srcDS = null;

			Assert.True(File.Exists(dstFileName));
		}
	}
}
