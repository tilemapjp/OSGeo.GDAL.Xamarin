using System;
using OSGeo.GDAL;

namespace OSGeo.GDAL.Test
{
	public class StartPhase
	{
		public static string BogotaTiffPath;

		public static void Init() 
		{
			Gdal.AllRegister ();

			var type = typeof(StartPhase); 
			var manifestResourceStream = type.Assembly.GetManifestResourceStream ("OSGeo.GDAL.Test.bogota.tif");

			var documents =
				System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
			BogotaTiffPath = System.IO.Path.Combine(documents, "bogota.tif");
			var fileStream = System.IO.File.OpenWrite (BogotaTiffPath);

			manifestResourceStream.CopyTo (fileStream);
			fileStream.Close ();
		}
	}
}

