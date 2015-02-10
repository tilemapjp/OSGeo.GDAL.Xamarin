using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

using OSGeo.GDAL;
using System.IO;

namespace GDAL.Sample
{
	[Activity (Label = "GDAL.Sample", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : Activity
	{
		int count = 1;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			// Get our button from the layout resource,
			// and attach an event to it
			Button button = FindViewById<Button> (Resource.Id.myButton);
			
			button.Click += delegate {
				button.Text = string.Format ("{0} clicks!", count++);
			};

			Console.WriteLine ("{0}",GdalConst.CE_Failure);
			Gdal.AllRegister ();

			var type = this.GetType(); 
			var manifestResourceStream = type.Assembly.GetManifestResourceStream ("GDAL.Sample.bogota.tif");

			var documents =
				System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
			var file = System.IO.Path.Combine(documents, "bogota.tif");
			var fileStream = System.IO.File.OpenWrite (file);

			manifestResourceStream.CopyTo (fileStream);
			fileStream.Close ();

			var dataSet = Gdal.Open (file, Access.GA_ReadOnly);
			if (dataSet != null) {
				double[] adfGeoTransform = new double[6];

				Console.WriteLine ("{0} {1}",
					dataSet.GetDriver ().GetDescription (),
					dataSet.GetDriver ().GetMetadataItem (GdalConst.GDAL_DMD_LONGNAME, ""));
					
				Console.WriteLine ("Size is {0}x{1}x{2}", 
					dataSet.RasterXSize, dataSet.RasterYSize, dataSet.RasterCount );

				if( dataSet.GetProjectionRef() != null )
					Console.WriteLine ( "Projection is `{0}'", dataSet.GetProjectionRef() );

				dataSet.GetGeoTransform (adfGeoTransform);
				Console.WriteLine ("Origin = ({0},{1})",
					adfGeoTransform [0], adfGeoTransform [3]);

				Console.WriteLine ("Pixel Size = ({0},{1})",
					adfGeoTransform [1], adfGeoTransform [5]);
			}

		}
	}
}


