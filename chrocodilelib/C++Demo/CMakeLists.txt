cmake_minimum_required (VERSION 3.9)

project(CHR_Lib_Demo)

set(CMAKE_INCLUDE_CURRENT_DIR ON)

add_subdirectory(Async/Demo1)
add_subdirectory(Async/Demo2)

add_subdirectory(Basic_Single_Channel/DataBuffer)
add_subdirectory(Basic_Single_Channel/PullSample)
add_subdirectory(Basic_Single_Channel/SpecTable)

add_subdirectory(Basic_Multi_Channel/DataBuffer)
add_subdirectory(Basic_Multi_Channel/PullSample)
add_subdirectory(Basic_Multi_Channel/SpecTable)

add_subdirectory(EncoderTrigger)

add_subdirectory(SharedConnection)

add_subdirectory(Plugin/CLS2CalibPlugin)
add_subdirectory(Plugin/FSSSimpleDemo)
