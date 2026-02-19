#include "TCPServer.h"
#include <iostream>
#include <sys/socket.h>
#include <netinet/in.h>
#include <unistd.h>
#include <arpa/inet.h>
#include <fcntl.h>
#include <signal.h>


TCPServer::TCPServer(int port) : port(port), serverFd(-1) {}

TCPServer::~TCPServer() {
    for (int sock : clients) {
        close(sock);
    }
    if (serverFd >= 0) {
        close(serverFd);
    }
}


bool TCPServer::start()
{
    #if defined(__APPLE__) || defined(__MACH__) || defined(__linux__)
    signal(SIGPIPE, SIG_IGN);
    #endif
    
    serverFd = socket(AF_INET, SOCK_STREAM, 0);
    if (serverFd < 0)
    {
        perror("socket");
        return false;
    }

    int opt = 1;
    setsockopt(serverFd, SOL_SOCKET, SO_REUSEADDR, &opt, sizeof(opt));

    sockaddr_in address;
    address.sin_family = AF_INET;
    address.sin_addr.s_addr = INADDR_ANY;
    address.sin_port = htons(6969);

    if (bind(serverFd, (struct sockaddr*)&address, sizeof(address)) < 0)
    {
        perror("bind");
        return false;
    }

    if (listen(serverFd, 10) < 0)
    {
        perror("listen");
        return false;
    }

    // ! this makes the ACCEPT non-blocking
    fcntl(serverFd, F_SETFL, O_NONBLOCK);

    std::cout << "Server started on port 6969\n";
    return true;
}


void TCPServer::acceptNewClients()
{
    sockaddr_in client_addr;
    socklen_t len = sizeof(client_addr);

    int clientSocket = accept(serverFd, (struct sockaddr*)&client_addr, &len);

    if (clientSocket >= 0)
    {
        std::cout << "New client connected\n";
        clients.push_back(clientSocket);
    }
}


void TCPServer::sendIntMat(int socket_fd, const cv::Mat& mat) { 
    // * forcing contiguous memory to strip the paddin from cv2
    cv::Mat contiguousMat = mat.isContinuous() ? mat : mat.clone();

    // * preparing and sending the header
    int header[4] = {contiguousMat.type(), contiguousMat.rows, contiguousMat.cols, contiguousMat.channels()}; 
    send(socket_fd, header, sizeof(header), 0); 
    
    // * send the data
    int totalBytes = contiguousMat.total() * contiguousMat.elemSize();
    send(socket_fd, contiguousMat.data, totalBytes, 0); 
    
    std::cout << "Sent " << totalBytes << " bytes to Unity." << std::endl; 
}


void TCPServer::broadcast(const cv::Mat& dataMatrix)
{
    if (clients.empty() || dataMatrix.empty()) return;

    cv::Mat contiguous = dataMatrix.isContinuous() ? dataMatrix : dataMatrix.clone();
    int header[4] = {contiguous.type(), contiguous.rows, contiguous.cols, contiguous.channels()};
    int totalBytes = contiguous.total() * contiguous.elemSize();

    for (auto it = clients.begin(); it != clients.end();) {
        int sock = *it;

        int result1 = send(sock, header, sizeof(header), 0);
        int result2 = send(sock, contiguous.data, totalBytes, 0);

        if (result1 <= 0 || result2 <= 0) {
            std::cout << "Client disconnected\n";
            close(sock);
            it = clients.erase(it);
        } else {
            ++it;
        }
    }
}
