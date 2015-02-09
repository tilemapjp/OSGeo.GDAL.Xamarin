#!/bin/bash
set -u
 
default_android_version=17
default_architecture=arm
default_ndk_root=/android-ndk-r9d
default_prefix=${HOME}/Desktop/Android_GDAL
 
export ANDROID_DEPLOYMENT_TARGET="${ANDROID_DEPLOYMENT_TARGET:-$default_android_version}"
DEFAULT_ARCHITECTURE="${DEFAULT_ARCHITECTURE:-$default_architecture}"
DEFAULT_PREFIX="${DEFAULT_PREFIX:-$default_prefix}"
NDK_ROOT="${NDK_ROOT:-$default_ndk_root}" 
 
usage ()
    {
cat >&2 << EOF
    Usage: ${0} [-h] [-p prefix] [-a arch] [-n ndk_root] [configure_args]
        -h  Print help message
        -p  Installation prefix (default: ${HOME}/Documents/Android_GDAL...)
        -a  Architecture target for compilation (default: arm)
        -n  Android NDK root (default: /android-ndk-r9d)
 
    Any additional arguments are passed to configure.
 
    The following environment variables affect the build process:
 
        ANDROID_DEPLOYMENT_TARGET  (default: $default_android_version)
        DEFAULT_PREFIX  (default: $default_prefix)
        NDK_ROOT  (default: $default_ndk_root)
EOF
    }
 
prefix="${DEFAULT_PREFIX}"
 
while getopts ":hp:a:n:" opt; do
        case $opt in
        h  ) usage ; exit 0 ;;
        p  ) prefix="$OPTARG" ;;
        a  ) DEFAULT_ARCHITECTURE="$OPTARG" ;;
        n  ) NDK_ROOT="$OPTARG" ;;
        \? ) usage ; exit 2 ;;
        esac
done
shift $(( $OPTIND - 1 ))
 
archname="${DEFAULT_ARCHITECTURE}"
arch="${archname}"

case $arch in
 
        mips )
        extra_cflags=" "
        extra_ldflags=" "
        host="mipsel-linux-android"        
        ;;
 
        x86 )
        extra_cflags=" "
        extra_ldflags=" "
        host="i686-linux-android"        
        ;;

        arm )
        extra_cflags="-mthumb"
        extra_ldflags=" "
        host="arm-linux-androideabi"
        ;;

        armv7a )
        arch=arm
        extra_cflags="-march=armv7-a -mfloat-abi=softfp"
        extra_ldflags="-Wl,--fix-cortex-a8"
        host="arm-linux-androideabi"
        ;;
 
        * )
        echo No valid architecture found!!!
        usage
        exit 2
 
esac

#create toolchain if necesary
toolchain=`pwd`/android-${ANDROID_DEPLOYMENT_TARGET}-toolchain-${arch}
if [ ! -e ${toolchain} ]
then
    echo toolchain missing, creating
    ${NDK_ROOT}/build/tools/make-standalone-toolchain.sh \
        --platform=android-${ANDROID_DEPLOYMENT_TARGET} \
        --install-dir=${toolchain} \
        --arch=${arch} --system=darwin-x86_64
fi
export PATH=${toolchain}/bin:$PATH
 
echo "building for host ${host}"
 
#platform_dir=`xcrun -find -sdk ${platform} --show-sdk-platform-path`
#platform_sdk_dir=`xcrun -find -sdk ${platform} --show-sdk-path`
prefix="${prefix}/${archname}/android-${ANDROID_DEPLOYMENT_TARGET}.sdk"
 
echo
echo library will be exported to $prefix
 
#setup compiler flags
export CC="${toolchain}/bin/${host}-gcc"
export LIBS="-lsupc++ -lstdc++"
export CFLAGS="${extra_cflags} -DHAVE_LONG_LONG"
export LDFLAGS="${extra_ldflags}"
export CXX="${toolchain}/bin/${host}-g++"
export CXXFLAGS="${CFLAGS}"
export CPP="${toolchain}/bin/${host}-cpp"
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

#rm config.sub config.guess
#wget http://git.savannah.gnu.org/cgit/config.git/plain/config.sub
#wget http://git.savannah.gnu.org/cgit/config.git/plain/config.guess
cp -f ../config.sub config.sub
cp -f ../config.guess config.guess

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
    --without-jni \
    "$@" || exit
 
echo
echo "make install proj"
time make
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

cp -f ../config.sub config.sub
cp -f ../config.guess config.guess
 
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
    --with-static-proj4=${prefix} 
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