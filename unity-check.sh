#!/bin/bash 
#
# Compiles the C# source tree using the Unity compiler to check if the
# the Unity version of Mono has all the dependencies you are using.
#
# TODO!: Make this work for Windows machines
# TODO!: Make less ugly hard coded paths

TARGET=$1
UNITYENGINEDLLPATH="/Applications/Unity/Unity.app/Contents/Frameworks/Managed/UnityEngine.dll"
NUNITDLLPATH="/Applications/Xamarin Studio.app/Contents/MacOS/lib/monodevelop/AddIns/NUnit/nunit.framework.dll"
UNITYCOMPILERPATH="/Applications/Unity/Unity.app/Contents/Frameworks/Mono/bin/smcs"
OUTPUTPATH="/tmp/tenp_mono.exe"

echo "Checking target for Unity Compatibility..."
echo "TARGET: $TARGET"
find $TARGET -type d \( -path $TARGET/obj -o -path $TARGET/bin \) -prune -o -name '*.cs' -print0 | \
        xargs -0 "$UNITYCOMPILERPATH" -r:"$UNITYENGINEDLLPATH" -r:"$NUNITDLLPATH"  -out:"$OUTPUTPATH" -target:library;
