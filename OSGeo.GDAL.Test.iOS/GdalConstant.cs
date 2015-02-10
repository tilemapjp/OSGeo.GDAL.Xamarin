using System;
using NUnit.Framework;
using OSGeo.GDAL;

namespace OSGeo.GDAL.Test
{
	[TestFixture]
	public class GdalConstant
	{

		[SetUp]
		public void Setup ()
		{
			Gdal.AllRegister ();
		}


		[TearDown]
		public void Tear ()
		{
		}

		[Test]
		public void ConstCheck ()
		{
			var fail = (int)GdalConst.CE_Failure;
			Assert.AreEqual (3, fail);
		}

		[Test]
		public void LoadTiff ()
		{
			var type = this.GetType(); 
			var manifestResourceStream = type.Assembly.GetManifestResourceStream ("OSGeo.GDAL.Test.bogota.tif");

			var documents =
				System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
			var file = System.IO.Path.Combine(documents, "bogota.tif");
			var fileStream = System.IO.File.OpenWrite (file);

			manifestResourceStream.CopyTo (fileStream);
			fileStream.Close ();

			var dataSet = Gdal.Open (file, Access.GA_ReadOnly);
			if (dataSet != null) {
				double[] adfGeoTransform = new double[6];

				Assert.AreEqual ("GTiff", dataSet.GetDriver ().GetDescription ());
				Assert.AreEqual ("GeoTIFF", dataSet.GetDriver ().GetMetadataItem (GdalConst.GDAL_DMD_LONGNAME, ""));

				Assert.AreEqual (512, dataSet.RasterXSize);
				Assert.AreEqual (512, dataSet.RasterYSize);
				Assert.AreEqual (1, dataSet.RasterCount);

				Assert.AreEqual (
					"LOCAL_CS[\"unnamed\",GEOGCS[\"unknown\",DATUM[\"unknown\",SPHEROID[\"unretrievable - using WGS84\",6378137,298.257223563]],PRIMEM[\"Greenwich\",0],UNIT[,0.0174532925199433]],AUTHORITY[\"EPSG\",\"21892\"],UNIT[\"unknown\",1]]",
					dataSet.GetProjectionRef ()
				);

				dataSet.GetGeoTransform (adfGeoTransform);
				Assert.AreEqual (440720, adfGeoTransform [0]);
				Assert.AreEqual (100000, adfGeoTransform [3]);
				Assert.AreEqual (60, adfGeoTransform [1]);
				Assert.AreEqual (-60, adfGeoTransform [5]);
			}
		}

		[Test]
		[Ignore ("another time")]
		public void Ignore ()
		{
			Assert.True (false);
		}

		[Test]
		public void Inconclusive ()
		{
			Assert.Inconclusive ("Inconclusive");
		}
	}
}
