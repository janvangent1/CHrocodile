cmake_minimum_required(VERSION 3.9)

project(Demo_Plugin_FlyingSpot)

if (WIN32)
	if(CMAKE_CXX_FLAGS MATCHES ".*\-m32.*" OR CMAKE_SIZEOF_VOID_P EQUAL 4)
		message([INFO] " Cmake will link against 32bit chrocolib" ...)
		LINK_DIRECTORIES(../../../libcore/lib/win/Win32)
	else()
		LINK_DIRECTORIES(../../../libcore/lib/win/x64)
	endif()
else()
	if(CMAKE_CXX_FLAGS MATCHES ".*\-m32.*" OR CMAKE_SIZEOF_VOID_P EQUAL 4)
		message([INFO] " Cmake will link against 32bit chrocolib" ...)
		LINK_DIRECTORIES(../../../libcore/lib/linux/x32)
	else()
		LINK_DIRECTORIES(../../../libcore/lib/linux/x64)
	endif()
endif()

set(CMAKE_INCLUDE_CURRENT_DIR ON)
set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_EXTENSIONS ON)

set(FSS_PLUGIN_SRC ${LIBCORE_ROOT}/plugins/FlyingSpotPlugin)

include_directories("../../../libcore/plugins/FlyingSpotPlugin")
include_directories("../../../libcore/include")

file(GLOB sources "*.cpp" "*.h")

set(FSS_IF_FILES ${FSS_PLUGIN_SRC}/FlyingSpotPluginInterface.h)

add_definitions(-DPLUGIN_PATH="${FSS_PLUGIN_SRC}") # this is needed to access FSS config file
add_executable(${PROJECT_NAME} ${sources})

target_include_directories(${PROJECT_NAME} PUBLIC ${LIBCORE_INC} ${FSS_PLUGIN_SRC})
# target_link_directories(Demo_FlyingSpotSimple PUBLIC "${CMAKE_SOURCE_DIR}/lib")
if(UNIX)
	target_link_libraries(${PROJECT_NAME} PUBLIC CHRocodile pthread)
else()
	target_link_libraries(${PROJECT_NAME} PUBLIC CHRocodile)
endif()

install(DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR} DESTINATION distribute
        PATTERN "CMakeLists.*" EXCLUDE
)

get_filename_component(DIR_NAME ${CMAKE_CURRENT_SOURCE_DIR} NAME)

# install only "exported" CMakeLists file
install(FILES ${CMAKE_CURRENT_SOURCE_DIR}/CMakeLists.export
    DESTINATION distribute/${DIR_NAME} RENAME CMakeLists.txt)

install(DIRECTORY ${LIBCORE_INC} DESTINATION distribute
    FILES_MATCHING
        PATTERN "*.h"
        PATTERN "*.hxx" EXCLUDE
        PATTERN "oldInterface" EXCLUDE
        PATTERN "c++11" EXCLUDE
        PATTERN "CHRocodileLib.hxx" EXCLUDE
        PATTERN "CHRocodileLibHelpers.hxx" EXCLUDE
        PATTERN ".vscode" EXCLUDE
)

install(FILES ${FSS_IF_FILES}
    DESTINATION distribute/include)

#install(FILES ../serial_demo_doxygen.ini
 #       DESTINATION distribute)
