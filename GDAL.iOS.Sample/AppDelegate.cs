using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using UIKit;

using OSGeo.GDAL;

namespace GDAL.iOS.Sample
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the
	// User Interface of the application, as well as listening (and optionally responding) to
	// application events from iOS.
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		// class-level declarations
		UIWindow window;

		//
		// This method is invoked when the application has loaded and is ready to run. In this
		// method you should instantiate the window, load the UI into it and then make the window
		// visible.
		//
		// You have 17 seconds to return from this method, or iOS will terminate your application.
		//
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			// create a new window instance based on the screen size
			window = new UIWindow (UIScreen.MainScreen.Bounds);
			
			// If you have defined a root view controller, set it here:
			// window.RootViewController = myViewController;
			
			// make the window visible
			window.MakeKeyAndVisible ();

			Console.WriteLine ("{0}",GdalConst.CE_Failure);
			Gdal.AllRegister ();

			var srcPath = NSBundle.MainBundle.BundlePath + "/bogota.tif";
			var dstPaths = NSSearchPath.GetDirectories (NSSearchPathDirectory.DocumentDirectory, NSSearchPathDomain.User, true);
			var dstPath = dstPaths [0] + "/bogota.tif";
			var fsMan = NSFileManager.DefaultManager;
			if (fsMan.FileExists (srcPath)) {
				var err = new NSError ();
				fsMan.Copy (srcPath, dstPath, out err);
				var dataSet = Gdal.Open (srcPath, Access.GA_ReadOnly);
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
			
			return true;
		}
	}
}

