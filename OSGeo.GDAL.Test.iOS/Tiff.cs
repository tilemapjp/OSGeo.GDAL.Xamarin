using System;
using NUnit.Framework;
using OSGeo.GDAL;

namespace OSGeo.GDAL.Test
{
	[TestFixture]
	public class Tiff
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
		public void Load ()
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

				var xSize = dataSet.RasterXSize;
				var ySize = dataSet.RasterYSize;
				var count = dataSet.RasterCount;
				Console.WriteLine ("Size is {0}x{1}x{2}", xSize, ySize, count);
				Assert.AreEqual (512, xSize);
				Assert.AreEqual (512, ySize);
				Assert.AreEqual (1, count);

				var projection = dataSet.GetProjectionRef ();
				Console.WriteLine ( "Projection is `{0}'", projection );
				Assert.AreEqual (
					"LOCAL_CS[\"unnamed\",GEOGCS[\"unknown\",DATUM[\"unknown\",SPHEROID[\"unretrievable - using WGS84\",6378137,298.257223563]],PRIMEM[\"Greenwich\",0],UNIT[,0.0174532925199433]],AUTHORITY[\"EPSG\",\"21892\"],UNIT[\"unknown\",1]]",
					projection
				);

				dataSet.GetGeoTransform (adfGeoTransform);
				var originX = adfGeoTransform [0];
				var originY = adfGeoTransform [3];
				Console.WriteLine ("Origin = ({0},{1})", originX, originY);
				Assert.AreEqual (440720, originX);
				Assert.AreEqual (100000, originY);

				var pixelW = adfGeoTransform [1];
				var pixelH = adfGeoTransform [5];
				Console.WriteLine ("Pixel Size = ({0},{1})", pixelW, pixelH);
				Assert.AreEqual (60, pixelW);
				Assert.AreEqual (-60, pixelH);
			}
		}
	}
}
