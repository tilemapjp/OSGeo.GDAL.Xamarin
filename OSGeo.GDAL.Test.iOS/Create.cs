
using System;
using NUnit.Framework;
using System.IO;
using OSGeo.GDAL;
using OSGeo.OGR;

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
		public void CreateFile ()
		{
			var documents =
				System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
			var dstFileName = System.IO.Path.Combine(documents, "create.tif");
			var dstDS = driver.Create (dstFileName, 512, 512, 1, DataType.GDT_Byte, null);

			dstDS.SetGeoTransform (new double[6]{ 444720, 30, 0, 3751320, 0, -30 });

			var srs = new OSR.SpatialReference (null);
			srs.SetUTM (11, 1);
			srs.SetWellKnownGeogCS ("NAD27");
			string wkt;
			srs.ExportToWkt (out wkt);
			dstDS.SetProjection (wkt);

			var raster = new byte[512 * 512];

			dstDS.GetRasterBand (1).WriteRaster (
				0, 0, 512, 512, raster, 512, 512, 0, 0);

			if (dstDS != null) {
				dstDS.Dispose ();
				dstDS = null;
			}

			Assert.True(File.Exists(dstFileName));
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
			var srcTiled = srcDS.GetMetadataItem ("TILED", "IMAGE_STRUCTURE");
			var srcComp = srcDS.GetMetadataItem ("COMPRESSION", "IMAGE_STRUCTURE");
			//Assert.AreEqual (null, srcTiled);
			Assert.AreEqual (null, srcComp);

			var dstDS = driver.CreateCopy (dstFileName, srcDS, 0, 
				new string[2]{"TILED=YES","COMPRESS=PACKBITS"}, 
				null, null);
			var dstTiled = dstDS.GetMetadataItem ("TILED", "IMAGE_STRUCTURE");
			var dstComp = dstDS.GetMetadataItem ("COMPRESSION", "IMAGE_STRUCTURE");
			//Assert.AreEqual ("YES", dstTiled);
			Assert.AreEqual ("PACKBITS", dstComp);

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
