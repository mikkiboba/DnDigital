#pragma once
#include <opencv2/opencv.hpp>
#include <vector>
#include <mutex>


enum class Color : int {
    NONE    = 0,
    RED     = 1,
    GREEN   = 2,
    BLUE    = 3,
    YELLOW  = 4,
    ORANGE  = 5,
    OTHER   = 10
};


class GridProcessor {
public:
    GridProcessor();
    ~GridProcessor();

    bool elaborateFrame(cv::Mat& frame);
    cv::Mat getCurrentGrid() const { return currentMatrix; }

private:
    cv::Mat currentMatrix;
    cv::Mat candidateMatrix;
    cv::Mat lastValidWarped;

    int defaultRows = -1;
    int defaultCols = -1;
    int defaultPixelHor = 0;
    int defaultPixelVer = 0;
    int defaultCelHor = 0;
    int defaultCelVer = 0;

    int stableFramesCount = 0;

    bool isObstructed(const cv::Mat& currentWarped);
    Color detectColor(const cv::Vec3b& hsv);
    bool initializeDefaults(const cv::Mat& img);
    bool findEntities(cv::Mat& rgbFrame, cv::Mat& mask);

    bool findLargestSquareContour(const cv::Mat& thresh, std::vector<cv::Point>& bestApprox);
    std::vector<cv::Point2f> orderPoints(std::vector<cv::Point>& points);
    cv::Mat warpToSquare(const cv::Mat& image, const std::vector<cv::Point2f>& srcPoints, float side);

    void extractGridLines(const cv::Mat& binary, cv::Mat& horizontal, cv::Mat& vertical, int side);
    int detectHorizontalLines(const cv::Mat& binary, int minLineLength);
    int detectVerticalLines(const cv::Mat& binary, int minLineLength);
    bool processGrid(const cv::Mat& binary, const cv::Mat& horizontal, const cv::Mat& vertical, float side, const cv::Mat& warped);
};