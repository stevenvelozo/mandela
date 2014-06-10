#!/bin/bash
MONO_FRAMEWORK=/Library/Frameworks/Mono.framework/Versions/Current
export DYLD_FALLBACK_LIBRARY_PATH="$MONO_FRAMEWORK/lib:/usr/local/lib:/usr/lib"
EXE_DIR=`dirname $0`
$MONO_FRAMEWORK/bin/mono $MONO_OPTIONS $EXE_DIR/Mandela.exe "$@"
