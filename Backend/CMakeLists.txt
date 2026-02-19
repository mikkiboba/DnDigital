cmake_minimum_required(VERSION 3.10.0)
project(testOpenCV VERSION 0.1.0 LANGUAGES C CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)

include(CTest)
enable_testing()

list(APPEND CMAKE_PREFIX_PATH "${CMAKE_SOURCE_DIR}/opencv/build")
find_package(OpenCV REQUIRED)
include_directories(${OpenCV_INCLUDE_DIRS})

add_executable(main main.cpp GridProcessor.cpp TCPServer.cpp Config.h GridProcessor.h TCPServer.h)

target_link_libraries(main ${OpenCV_LIBS})

set_target_properties(main PROPERTIES
    BUILD_WITH_INSTALL_RPATH TRUE
    INSTALL_RPATH "${CMAKE_SOURCE_DIR}/opencv/build/lib"
    INSTALL_RPATH_USE_LINK_PATH TRUE
)
