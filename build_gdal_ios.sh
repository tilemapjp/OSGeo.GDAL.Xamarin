#!/bin/bash
set -u
 
default_iphoneos_version=7.1
default_architecture=armv7
 
export IPHONEOS_DEPLOYMENT_TARGET="${IPHONEOS_DEPLOYMENT_TARGET:-$default_iphoneos_version}"
DEFAULT_ARCHITECTURE="${DEFAULT_ARCHITECTURE:-$default_architecture}"
DEFAULT_PREFIX="${HOME}/Desktop/iOS_GDAL"
 
 
usage ()
    {
cat >&2 << EOF
    Usage: ${0} [-h] [-p prefix] [-a arch] target [configure_args]
        -h  Print help message
        -p  Installation prefix (default: \$HOME/Documents/iOS_GDAL...)
        -a  Architecture target for compilation (default: armv7)
 
    The target must be "device" or "simulator".  Any additional arguments
    are passed to configure.
 
    The following environment variables affect the build process:
 
        IPHONEOS_DEPLOYMENT_TARGET  (default: $default_iphoneos_version)
        DEFAULT_PREFIX  (default: $default_prefix)
EOF
    }
 
prefix="${DEFAULT_PREFIX}"
 
while getopts ":hp:a:" opt; do
        case $opt in
        h  ) usage ; exit 0 ;;
        p  ) prefix="$OPTARG" ;;
        a  ) DEFAULT_ARCHITECTURE="$OPTARG" ;;
        \? ) usage ; exit 2 ;;
        esac
done
shift $(( $OPTIND - 1 ))
 
if (( $# < 1 )); then
    usage
    exit 2
fi
 
target=$1
shift
 
case $target in
 
        device )
        arch="${DEFAULT_ARCHITECTURE}"
        platform=iphoneos
        extra_cflags=" "
        ;;
 
        simulator )
        arch=i386
        platform=iphonesimulator
        extra_cflags="-D__IPHONE_OS_VERSION_MIN_REQUIRED=${IPHONEOS_DEPLOYMENT_TARGET%%.*}0000"
        ;;
 
        * )
        echo No target found!!!
        usage
        exit 2
 
esac
if [ $arch = "arm64" ]
    then
    host="arm-apple-darwin"
else
    host="${arch}-apple-darwin"
    extra_cflags="${extra_cflags} -DHAVE_LONG_LONG"
fi
 
echo "building for host ${host}"
 
platform_dir=`xcrun -find -sdk ${platform} --show-sdk-platform-path`
platform_sdk_dir=`xcrun -find -sdk ${platform} --show-sdk-path`
prefix="${prefix}/${arch}/${platform}${IPHONEOS_DEPLOYMENT_TARGET}.sdk"
 
echo
echo library will be exported to $prefix
 
#setup compiler flags
export CC=`xcrun -find -sdk iphoneos gcc`
#export CFLAGS="-Wno-error=unused-command-line-argument-hard-error-in-future -Wno-error=implicit-function-declaration -arch ${arch} -pipe -Os -gdwarf-2 -isysroot ${platform_sdk_dir} ${extra_cflags}"
export CFLAGS="-Wno-error=implicit-function-declaration -arch ${arch} -pipe -Os -gdwarf-2 -isysroot ${platform_sdk_dir} ${extra_cflags}"
export LDFLAGS="-arch ${arch} -isysroot ${platform_sdk_dir}"
export CXX=`xcrun -find -sdk iphoneos g++`
export CXXFLAGS="${CFLAGS}"
export CPP=`xcrun -find -sdk iphoneos cpp`
export CXXCPP="${CPP}"
 
echo CFLAGS ${CFLAGS}
 
#set proj4 install destination
proj_prefix=$prefix
echo install proj to $proj_prefix
 
#download proj4 if necesary
if [ ! -e proj-4.8.0 ]
then
    echo proj4 missing, downloading
    wget http://download.osgeo.org/proj/proj-4.8.0.tar.gz
    tar -xzf proj-4.8.0.tar.gz
fi
 
#configure and build proj4
pushd proj-4.8.0
 
echo
echo "cleaning proj"
make clean
 
echo
echo "configure proj"
./configure \
    --prefix=${proj_prefix} \
    --enable-shared=no \
    --enable-static=yes \
    --host=$host \
    "$@" || exit
 
echo
echo "make install proj"
time make install || exit
 
popd
 
#download gdal if necesary
if [ ! -e gdal-1.11.0 ]
then
    wget http://download.osgeo.org/gdal/1.11.0/gdal-1.11.0.tar.gz
    tar -xzf gdal-1.11.0.tar.gz
fi
 
#configure and build gdal
cd gdal-1.11.0
 
echo "cleaning gdal"
make clean
 
echo
echo "configure gdal"
./configure \
    --prefix="${prefix}" \
    --host=$host \
    --disable-shared \
    --enable-static \
    --with-hide-internal-symbols \
    --with-unix-stdio-64=no \
    --with-geos=no \
    --without-pg \
    --without-grass \
    --without-libgrass \
    --without-cfitsio \
    --without-pcraster \
    --without-netcdf \
    --without-ogdi \
    --without-fme \
    --without-hdf4 \
    --without-hdf5 \
    --without-jasper \
    --without-kakadu \
    --without-grib \
    --without-mysql \
    --without-ingres \
    --without-xerces \
    --without-odbc \
    --without-curl \
    --without-idb \
    --without-sde \
    --with-sse=no \
    --with-avx=no \
    --with-static-proj4=${prefix} \
#    --with-sqlite3=${platform_sdk_dir} \
    "$@" || exit
 
#echo '#include "cpl_config_extras.h"' >> port/cpl_config.h
 
echo
echo "building gdal"
time make
 
echo
echo "installing"
time make install
 
echo "Gdal build complete"