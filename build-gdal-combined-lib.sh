#!/bin/bash
PREFIX=`pwd`/install/
DIST=`pwd`/dist/
rm -rf $PREFIX
mkdir $PREFIX
LOG=./log
rm -rf $LOG
mkdir $LOG
IOS_DIST=`pwd`/OSGeo.GDAL.iOS/
AND_DIST=`pwd`/OSGeo.GDAL.Android/
 
if [ -e ${PREFIX} ]
then
	echo removing ${PREFIX}
	rm -rf ${PREFIX}
fi
if [ -e ${DIST} ]
then
    echo removing ${DIST}
    rm -rf ${DIST}
fi
 
mkdir ${PREFIX}
mkdir ${DIST}

#for iOS

export IPHONEOS_DEPLOYMENT_TARGET=7.1
 
for f in "armv7" "armv7s" "arm64"; do
echo Building iOS $f
./build_gdal_ios.sh -p ${PREFIX} -a $f device 2>&1 | tee "${LOG}/iOS_${f}.txt"
done
 
echo Building iOS simulator
./build_gdal_ios.sh -p ${PREFIX} simulator 2>&1 | tee "${LOG}/iOS_simulator.txt"
 
mkdir -p ${DIST}/iOS

rm -f ${IOS_DIST}/libgdal.a
lipo \
${PREFIX}/i386/iphonesimulator${IPHONEOS_DEPLOYMENT_TARGET}.sdk/lib/libgdal.a \
${PREFIX}/armv7/iphoneos${IPHONEOS_DEPLOYMENT_TARGET}.sdk/lib/libgdal.a \
${PREFIX}/armv7s/iphoneos${IPHONEOS_DEPLOYMENT_TARGET}.sdk/lib/libgdal.a \
${PREFIX}/arm64/iphoneos${IPHONEOS_DEPLOYMENT_TARGET}.sdk/lib/libgdal.a \
-output ${IOS_DIST}/libgdal.a \
-create | tee $LOG/lipo.txt
 
rm -f ${IOS_DIST}/libproj.a
lipo \
${PREFIX}/i386/iphonesimulator${IPHONEOS_DEPLOYMENT_TARGET}.sdk/lib/libproj.a \
${PREFIX}/armv7/iphoneos${IPHONEOS_DEPLOYMENT_TARGET}.sdk/lib/libproj.a \
${PREFIX}/armv7s/iphoneos${IPHONEOS_DEPLOYMENT_TARGET}.sdk/lib/libproj.a \
${PREFIX}/arm64/iphoneos${IPHONEOS_DEPLOYMENT_TARGET}.sdk/lib/libproj.a \
-output ${IOS_DIST}/libproj.a \
-create | tee $LOG/lipo-proj.txt

#for Android

export ANDROID_DEPLOYMENT_TARGET=17

for f in "armeabi" "armeabi-v7a" "mips" "x86"; do
echo Building Android $f
./build_gdal_android.sh -p ${PREFIX} -a $f 2>&1 | tee "${LOG}/Android_${f}.txt"
mkdir -p ${AND_DIST}/lib/${f} | tee $LOG/android-copy.txt
rm -f ${AND_DIST}/lib/${f}/libproj.so
cp -f ${PREFIX}/android-${ANDROID_DEPLOYMENT_TARGET}.sdk/${f}/lib/libproj.so.*.*.* ${AND_DIST}/lib/${f}/libproj.so | tee $LOG/android-copy.txt
rm -f ${AND_DIST}/lib/${f}/libgdal.so
cp -f ${PREFIX}/android-${ANDROID_DEPLOYMENT_TARGET}.sdk/${f}/lib/libgdal.so.*.*.* ${AND_DIST}/lib/${f}/libgdal.so | tee $LOG/android-copy.txt
done
