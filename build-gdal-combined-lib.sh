#!/bin/bash
PREFIX=`pwd`/install/
DIST=`pwd`/dist/
rm -rf $PREFIX
mkdir $PREFIX
LOG=./log
rm -rf $LOG
mkdir $LOG
 
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

lipo \
${PREFIX}/i386/iphonesimulator${IPHONEOS_DEPLOYMENT_TARGET}.sdk/lib/libgdal.a \
${PREFIX}/armv7/iphoneos${IPHONEOS_DEPLOYMENT_TARGET}.sdk/lib/libgdal.a \
${PREFIX}/armv7s/iphoneos${IPHONEOS_DEPLOYMENT_TARGET}.sdk/lib/libgdal.a \
${PREFIX}/arm64/iphoneos${IPHONEOS_DEPLOYMENT_TARGET}.sdk/lib/libgdal.a \
-output ${DIST}/iOS/libgdal.a \
-create | tee $LOG/lipo.txt
 
lipo \
${PREFIX}/i386/iphonesimulator${IPHONEOS_DEPLOYMENT_TARGET}.sdk/lib/libproj.a \
${PREFIX}/armv7/iphoneos${IPHONEOS_DEPLOYMENT_TARGET}.sdk/lib/libproj.a \
${PREFIX}/armv7s/iphoneos${IPHONEOS_DEPLOYMENT_TARGET}.sdk/lib/libproj.a \
${PREFIX}/arm64/iphoneos${IPHONEOS_DEPLOYMENT_TARGET}.sdk/lib/libproj.a \
-output ${DIST}/iOS/libproj.a \
-create | tee $LOG/lipo-proj.txt

#for Android

export ANDROID_DEPLOYMENT_TARGET=17

for f in "arm" "armv7a" "mips" "x86"; do
echo Building Android $f
./build_gdal_android.sh -p ${PREFIX} -a $f 2>&1 | tee "${LOG}/Android_${f}.txt"
mkdir -p ${DIST}/Android/${f} | tee $LOG/android-copy.txt
cp -f ${PREFIX}/${f}/android-${ANDROID_DEPLOYMENT_TARGET}.sdk/lib/libproj.a ${DIST}/Android/${f}/libproj.a | tee $LOG/android-copy.txt
cp -f ${PREFIX}/${f}/android-${ANDROID_DEPLOYMENT_TARGET}.sdk/lib/libgdal.a ${DIST}/Android/${f}/libgdal.a | tee $LOG/android-copy.txt
done
