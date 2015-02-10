using System;
using NUnit.Framework;
using OSGeo.GDAL;

namespace OSGeo.GDAL.Test
{
	[TestFixture]
	public class Tiff
	{
		Dataset dataSet;

		[SetUp]
		public void Setup ()
		{
			Gdal.AllRegister ();

			var type = this.GetType(); 
			var manifestResourceStream = type.Assembly.GetManifestResourceStream ("OSGeo.GDAL.Test.bogota.tif");

			var documents =
				System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
			var file = System.IO.Path.Combine(documents, "bogota.tif");
			var fileStream = System.IO.File.OpenWrite (file);

			manifestResourceStream.CopyTo (fileStream);
			fileStream.Close ();

			dataSet = Gdal.Open (file, Access.GA_ReadOnly);
		}

		[TearDown]
		public void Tear ()
		{
		}

		[Test]
		public void Load ()
		{
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

		[Test]
		public void RasterBand ()
		{
			int nBlockXSize, nBlockYSize;
			var band = dataSet.GetRasterBand (1);
			band.GetBlockSize (out nBlockXSize, out nBlockYSize);
			var type = Gdal.GetDataTypeName (band.DataType);
			var colorInterp = Gdal.GetColorInterpretationName (band.GetColorInterpretation ());
			Console.WriteLine ("Block={0}x{1} Type={2}, ColorInterp={3}",
				nBlockXSize, nBlockYSize, type, colorInterp
			);

			Assert.AreEqual (512, nBlockXSize);
			Assert.AreEqual (16, nBlockYSize);
			Assert.AreEqual ("Byte", type);
			Assert.AreEqual ("Gray", colorInterp);

			int bGotMin, bGotMax;
			double[] adfMinMax = new double[2];

			band.GetMinimum (out adfMinMax [0], out bGotMin);
			band.GetMaximum (out adfMinMax [1], out bGotMax);
			if (!(bGotMin != 0 && bGotMax != 0))
				band.ComputeRasterMinMax (adfMinMax, 1);

			Console.WriteLine ( "Min={0}, Max={1}", adfMinMax[0], adfMinMax[1] );
			Assert.AreEqual (0, adfMinMax [0]);
			Assert.AreEqual (255, adfMinMax [1]);

			var overviewCount = band.GetOverviewCount ();
			if( overviewCount > 0 )
				Console.WriteLine( "Band has {0} overviews.", overviewCount );
			Assert.AreEqual (0, overviewCount);

			var colorTable = band.GetColorTable ();
			if( colorTable != null)
				Console.WriteLine("Band has a color table with {0} entries.",
					colorTable.GetCount());
			Assert.AreEqual (null, colorTable);
		}

		[Test]
		public void ReadRaster() {
			var band = dataSet.GetRasterBand (1);

			float[] pafScanline;
			var nXSize = band.XSize;
			Console.WriteLine( "XSize is {0}.", nXSize );
			Assert.AreEqual (512, nXSize);

			pafScanline = new float[nXSize];
			band.ReadRaster (0, 0, nXSize, 1, pafScanline, nXSize, 1, 0, 0);

			var testVal = new float[512]{
				107f,123f,132f,115f,132f,132f,140f,132f,132f,132f,107f,132f,107f,132f,132f,107f,123f,115f,156f,148f,107f,132f,107f,115f,99f,123f,99f,74f,115f,82f,115f,115f,107f,
				123f,123f,99f,123f,123f,115f,115f,107f,90f,99f,107f,107f,99f,123f,107f,140f,123f,123f,115f,99f,132f,123f,115f,115f,123f,132f,115f,123f,132f,214f,156f,165f,
				148f,115f,148f,156f,148f,140f,165f,156f,197f,156f,197f,140f,173f,156f,165f,148f,156f,206f,214f,181f,206f,173f,222f,206f,255f,214f,173f,214f,255f,214f,247f,255f,
				230f,206f,197f,214f,173f,214f,222f,181f,255f,247f,189f,197f,156f,189f,173f,173f,206f,173f,230f,165f,181f,189f,239f,197f,197f,222f,173f,181f,189f,165f,148f,132f,
				140f,247f,165f,189f,189f,115f,156f,206f,189f,222f,197f,156f,165f,156f,99f,132f,140f,173f,173f,132f,148f,107f,247f,197f,115f,107f,115f,123f,156f,173f,115f,123f,
				107f,115f,173f,132f,90f,115f,148f,115f,115f,132f,132f,173f,115f,99f,115f,123f,156f,156f,132f,189f,165f,123f,115f,123f,181f,173f,181f,197f,189f,99f,156f,214f,
				255f,255f,255f,214f,165f,165f,165f,255f,255f,181f,148f,107f,148f,140f,140f,181f,148f,132f,173f,189f,181f,173f,173f,165f,148f,148f,140f,189f,239f,140f,156f,255f,
				107f,181f,173f,165f,99f,132f,99f,181f,132f,140f,148f,74f,247f,197f,197f,214f,189f,181f,189f,181f,214f,197f,173f,239f,173f,230f,140f,189f,214f,222f,255f,206f,
				230f,255f,255f,255f,255f,255f,255f,206f,165f,165f,173f,214f,189f,156f,165f,214f,222f,255f,255f,255f,173f,165f,173f,189f,173f,140f,132f,156f,247f,247f,255f,140f,
				206f,255f,148f,255f,148f,115f,173f,181f,181f,173f,173f,214f,230f,140f,230f,148f,156f,206f,189f,156f,156f,247f,165f,222f,206f,181f,99f,132f,82f,90f,66f,82f,
				82f,165f,66f,74f,66f,82f,99f,181f,74f,82f,132f,140f,148f,132f,140f,123f,107f,148f,90f,82f,123f,90f,74f,66f,66f,74f,107f,107f,99f,66f,107f,99f,
				66f,107f,74f,123f,156f,123f,82f,255f,58f,74f,148f,90f,90f,115f,173f,99f,82f,82f,107f,173f,74f,66f,90f,58f,82f,82f,41f,74f,66f,58f,74f,123f,
				66f,82f,66f,66f,99f,74f,90f,90f,82f,74f,107f,156f,148f,107f,115f,90f,123f,132f,206f,156f,132f,132f,255f,230f,107f,140f,181f,140f,181f,230f,123f,156f,
				206f,99f,165f,115f,107f,156f,148f,148f,123f,99f,140f,123f,107f,132f,99f,123f,99f,90f,115f,115f,140f,107f,173f,148f,165f,132f,140f,99f,107f,99f,99f,115f,
				107f,132f,156f,222f,165f,148f,255f,165f,132f,90f,107f,132f,90f,90f,115f,165f,49f,33f,58f,41f,33f,74f,16f,49f,41f,115f,99f,206f,156f,140f,90f,123f,
				107f,99f,90f,99f,132f,148f,140f,173f,132f,181f,148f,165f,123f,115f,99f,140f,140f,140f,132f,132f,123f,107f,156f,173f,123f,115f,82f,165f,222f,115f,82f
			};
			for (var i = 0; i < nXSize; i++) {
				Assert.AreEqual (testVal [i], pafScanline [i]);
			}
		}
	}
}
