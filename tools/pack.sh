#!/bin/bash

# This script builds and packs the artifacts. Use when you have MSBuild installed.
version=$(cat version)
releaseconf=$1
if [ -z $releaseconf ]; then
	releaseconf=Release
fi

# Check for dependencies
zippath=`which zip`
if [ ! $? == 0 ]; then
	echo zip is not found.
	exit 1
fi

# Pack binary
echo Packing binary...
cd "../VisualCard/bin/$releaseconf/netstandard2.0/" && "$zippath" -r /tmp/$version-bin.zip . && cd -
cd "../VisualCard.ShowContacts/bin/$releaseconf/net6.0/" && "$zippath" -r /tmp/$version-bin.zip . && cd -
if [ ! $? == 0 ]; then
	echo Packing using zip failed.
	exit 1
fi

# Inform success
mv ~/tmp/$version-bin.zip .
mv ~/tmp/$version-demo.zip .
echo Build and pack successful.
exit 0
