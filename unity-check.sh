#!/bin/bash 
#
# Compiles the C# source tree using the Unity compiler to check if the
# the Unity version of Mono has all the dependencies you are using.
#

TARGET=$1
UNITYENGINEDLLPATH="/Applications/Unity/Unity.app/Contents/Frameworks/Managed/UnityEngine.dll"
UNITYCOMPILERPATH="/Applications/Unity/Unity.app/Contents/Frameworks/Mono/bin/smcs"
OUTPUTPATH="/tmp/tenp_mono.exe"

echo "Checking target for Unity Compatibility..."
echo "TARGET: $TARGET"
find $TARGET -type d \( -path $TARGET/obj -o -path $TARGET/bin \) -prune -o -name '*.cs' -print0 | \
        xargs -0 $UNITYCOMPILERPATH -r:$UNITYENGINEDLLPATH  -out:$OUTPUTPATH -target:library;
