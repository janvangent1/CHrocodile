cmake_minimum_required (VERSION 3.9)

project(Demo_Single_SpecTable)

if (WIN32)
	if(CMAKE_CXX_FLAGS MATCHES ".*\-m32.*" OR CMAKE_SIZEOF_VOID_P EQUAL 4)
        set(OUT_DIR "${CMAKE_CURRENT_SOURCE_DIR}/../../../libcore/lib/win/Win32/")
	else()
        set(OUT_DIR "${CMAKE_CURRENT_SOURCE_DIR}/../../../libcore/lib/win/x64/")
	endif()
else()
	if(CMAKE_CXX_FLAGS MATCHES ".*\-m32.*" OR CMAKE_SIZEOF_VOID_P EQUAL 4)
        set(OUT_DIR "${CMAKE_CURRENT_SOURCE_DIR}/../../../libcore/lib/linux/x32/")
	else()
        set(OUT_DIR "${CMAKE_CURRENT_SOURCE_DIR}/../../../libcore/lib/linux/x64/")
    endif()
endif()

LINK_DIRECTORIES(${OUT_DIR})

set(CMAKE_RUNTIME_OUTPUT_DIRECTORY ${OUT_DIR})
set(CMAKE_LIBRARY_OUTPUT_DIRECTORY ${OUT_DIR})
set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY ${OUT_DIR})

# Second, for multi-config builds (e.g. msvc)
foreach( OUTPUTCONFIG ${CMAKE_CONFIGURATION_TYPES} )
    string( TOUPPER ${OUTPUTCONFIG} OUTPUTCONFIG )
    set( CMAKE_RUNTIME_OUTPUT_DIRECTORY_${OUTPUTCONFIG} ${OUT_DIR} )
    set( CMAKE_LIBRARY_OUTPUT_DIRECTORY_${OUTPUTCONFIG} ${OUT_DIR} )
    set( CMAKE_ARCHIVE_OUTPUT_DIRECTORY_${OUTPUTCONFIG} ${OUT_DIR} )
endforeach( )


set(CMAKE_INCLUDE_CURRENT_DIR ON)
include_directories(../../../libcore/include)

file(GLOB SOURCES "*.h"	"*.cpp")
add_executable(${PROJECT_NAME} ${SOURCES})
target_link_libraries (${PROJECT_NAME} CHRocodile)

