#include <iostream>
#include <filesystem>
#include <opencv2/opencv.hpp>

#include "Config.h"
#include "TCPServer.h"
#include "GridProcessor.h"

void openStream(cv::VideoCapture& capture, const std::string& streamURL, int delayMs = 1000) {
    capture.open(streamURL, cv::CAP_FFMPEG);
    while(!capture.isOpened()) {
        std::cerr << "Failed to connect to webcam. Retrying in " << delayMs << "ms...\n";
        cv::waitKey(delayMs);
        capture.open(streamURL, cv::CAP_FFMPEG);
    }
}

bool rangeFileExists() {
    return std::filesystem::exists(Config::WOOPER_FILE_PATH);
}

int main() {
    TCPServer server(Config::SERVER_PORT);
    if (!server.start()) {
        std::cerr << "Server failed to start. Exiting.\n";
        return -1;
    }

    GridProcessor processor;

    cv::VideoCapture cap;
    openStream(cap, Config::STREAM_URL, 1000);
    cap.set(cv::CAP_PROP_BUFFERSIZE, 10);
    cap.set(cv::CAP_PROP_FPS, 15);

    cv::Mat frame;

    std::cout << "Application started successfully.\n";

    while (true) {
        cap.read(frame);
        if (frame.empty()) continue;

        if (rangeFileExists()) continue;

        cv::flip(frame, frame, 1);

        bool gridUpdated = processor.elaborateFrame(frame);

        server.acceptNewClients();
        if (gridUpdated) {
            server.broadcast(processor.getCurrentGrid());
        }

        cv::imshow("D&D Board Camera", frame);
        if (cv::waitKey(10) == 'q') {
            break;
        }
    }

    return 0;
}