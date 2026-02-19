#pragma once
#include <string>


namespace Config {
    // * network settings
    const int SERVER_PORT = 6969;
    const std::string STREAM_URL = "http://192.168.1.12:8080/video";

    // * computer vision settings
    const float WARP_SIDE = 500.0f;
    const int DEBOUNCE_THRESHOLD = 5; // * how many frames needed to send
    const double OBSTRUCTION_THRESHOLD = 15.0;

    const std::string WOOPER_FILE_PATH = "/Users/mikki/Documents/dnd-cpippi/projection/imgsToProject/wooper.wooper";
}