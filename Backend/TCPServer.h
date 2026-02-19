#pragma once 
#include <opencv2/opencv.hpp>
#include <vector>


class TCPServer {
public:
    TCPServer(int port);
    ~TCPServer();

    bool start();
    void acceptNewClients();
    void broadcast(const cv::Mat& dataMatrix);

private:
    void sendIntMat(int socket_fd, const cv::Mat& mat);

    int port;
    int serverFd = -1;
    std::vector<int> clients;
};