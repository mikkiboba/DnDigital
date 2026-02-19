#include "GridProcessor.h"
#include "Config.h"
#include <iostream>


GridProcessor::GridProcessor() {}
GridProcessor::~GridProcessor() {}


bool GridProcessor::isObstructed(const cv::Mat& currentWarped) {
    if (lastValidWarped.empty()) {
        currentWarped.copyTo(lastValidWarped);
        return false;
    }

    cv::Mat diff, grayCurrent, grayLast;
    
    // * conversion to grayscale
    cv::cvtColor(currentWarped, grayCurrent, cv::COLOR_BGR2GRAY);
    cv::cvtColor(lastValidWarped, grayLast, cv::COLOR_BGR2GRAY);

    // * absolute difference
    cv::absdiff(grayCurrent, grayLast, diff);

    // * threshold to ignore light noise
    cv::threshold(diff, diff, 35, 255, cv::THRESH_BINARY);

    // * counting non-zeros
    double movement = cv::countNonZero(diff);

    // * high movement -> obstruction
    double obstructionPercent = (movement / (currentWarped.rows * currentWarped.cols)) * 100.0;

    // * over 15% there is a bad obstruction
    if (obstructionPercent > 15.0) {
        std::cout << "Obstruction detected: " << obstructionPercent << "% change." << std::endl;
        return true;
    }

    // * good frame -> update
    // * weighted average to handle slow lighting changes
    cv::addWeighted(lastValidWarped, 0.9, currentWarped, 0.1, 0, lastValidWarped);
    return false;
}


Color GridProcessor::detectColor(const cv::Vec3b& hsv) {
    int h = hsv[0], s = hsv[1], v = hsv[2];

    if (v < 40)                             return Color::NONE;
    if ((h <= 10 || h >= 170) && s > 70)    return Color::RED;
    if (h > 10 && h <= 25 && s > 60)        return Color::ORANGE;
    if (h > 25 && h <= 38 && s > 60)        return Color::YELLOW;
    if (h >= 38 && h <= 95 && s > 45)       return Color::GREEN;
    if (h >= 95 && h <= 125 && s > 80)      return Color::BLUE;

    return Color::NONE;
}


bool GridProcessor::initializeDefaults(const cv::Mat& img) {
    defaultPixelVer = img.rows;
    defaultPixelHor = img.cols;
    defaultCelHor = defaultPixelHor / defaultCols;
    defaultCelVer = defaultPixelVer / defaultRows;

    // * here if there is an error with the frame (we'll discard it)
    if (defaultPixelHor < 0 || defaultPixelVer < 0 || defaultCelHor < 0 || defaultCelVer < 0) {
        return false; 
    }

    currentMatrix = cv::Mat(defaultRows, defaultCols, CV_32S);
    currentMatrix.setTo(static_cast<int>(Color::NONE));
    return true;
}


bool GridProcessor::findEntities(cv::Mat& rgbFrame, cv::Mat& mask)
{
    if (defaultRows <= 0 || defaultCols <= 0) return false;

    cv::Mat hsvFrame;
    cv::cvtColor(rgbFrame, hsvFrame, cv::COLOR_BGR2HSV);

    if (mask.size() != hsvFrame.size()) return false;

    if (defaultPixelVer == 0 && defaultPixelHor == 0) {
        if (!initializeDefaults(rgbFrame)) return false;
    }

    // * to store the detected stuff
    struct Detection {
        int row, col, color, pixelCount;
    };
    std::vector<Detection> candidates;
    std::mutex mtx; 

    cv::parallel_for_(cv::Range(0, defaultRows * defaultCols), [&](const cv::Range& range) {
        for (int index = range.start; index < range.end; ++index) {
            int row = index / defaultCols;
            int col = index % defaultCols;

            int startX = col * defaultCelHor;
            int startY = row * defaultCelVer;

            cv::Rect cellROI(startX, startY, defaultCelHor, defaultCelVer);
            cellROI &= cv::Rect(0, 0, hsvFrame.cols, hsvFrame.rows);
            
            if (cellROI.width <= 0 || cellROI.height <= 0) continue;

            cv::Mat hsvCell = hsvFrame(cellROI);
            cv::Mat maskCell = mask(cellROI);

            // * count the occurences of each color in a cell
            std::vector<int> colorCounts(static_cast<int>(Color::OTHER) + 1, 0);
            int totalValidInCell = 0;

            for (int y = 0; y < hsvCell.rows; ++y) {
                for (int x = 0; x < hsvCell.cols; ++x) {
                    if (maskCell.at<uchar>(y, x) == 0) continue;

                    const cv::Vec3b& hsv = hsvCell.at<cv::Vec3b>(y, x);
                    
                    // * saturation and value filter
                    if (hsv[1] < 40 || hsv[2] < 40) continue;

                    Color detected = detectColor(hsv);
                    if (detected != Color::NONE) {
                        colorCounts[static_cast<int>(detected)]++;
                        totalValidInCell++;
                    }
                }
            }

            // * if enough colored pixels, it find the dominant color (> 55% of detected pixels)
            if (totalValidInCell >= 30) {
                int bestColor = -1;
                int maxPixels = 0;

                // ! c = 1 so it skips Color::NONE
                for (int c = 1; c < colorCounts.size(); ++c) { 
                    if (colorCounts[c] > maxPixels) {
                        maxPixels = colorCounts[c];
                        bestColor = c;
                    }
                }

                if (bestColor != -1 && maxPixels > (totalValidInCell * 0.55)) {
                    std::lock_guard<std::mutex> lock(mtx);
                    candidates.push_back({row, col, bestColor, maxPixels});
                }
            }
        }
    });

    // * sort all candidates globally by pixel count
    std::sort(candidates.begin(), candidates.end(), [](const Detection& a, const Detection& b) {
        return a.pixelCount > b.pixelCount;
    });

    cv::Mat frameMatrix(defaultRows, defaultCols, CV_32S);
    frameMatrix.setTo(static_cast<int>(Color::NONE));

    std::vector<bool> colorAlreadyAssigned(static_cast<int>(Color::OTHER) + 1, false);

    for (const auto& det : candidates) {
        if (!colorAlreadyAssigned[det.color]) {
            frameMatrix.at<int>(det.row, det.col) = det.color;
            colorAlreadyAssigned[det.color] = true;
        }
    }

    // * DEBOUNCE    
    if (candidateMatrix.empty() || candidateMatrix.size() != frameMatrix.size()) {
        candidateMatrix = frameMatrix.clone();
        stableFramesCount = 1;
    } else {
        cv::Mat diff;
        cv::compare(candidateMatrix, frameMatrix, diff, cv::CMP_NE);

        if (cv::countNonZero(diff) == 0) {
            stableFramesCount++;
        } else {
            // ! flickering or moving; reset stability timer
            frameMatrix.copyTo(candidateMatrix);
            stableFramesCount = 1;
        }
    }

    bool gridUpdated = false;
    if (stableFramesCount >= Config::DEBOUNCE_THRESHOLD) {
        cv::Mat finalDiff;
        cv::compare(currentMatrix, candidateMatrix, finalDiff, cv::CMP_NE);

        if (cv::countNonZero(finalDiff) > 0) {
            candidateMatrix.copyTo(currentMatrix);
            std::cout << "--- New Stable Grid State ---" << std::endl;
            std::cout << currentMatrix << std::endl;
            gridUpdated = true;
        }
        // Cap the counter to prevent overflow
        stableFramesCount = Config::DEBOUNCE_THRESHOLD;
    }

    return gridUpdated;
}


bool GridProcessor::findLargestSquareContour(const cv::Mat& thresh, std::vector<cv::Point>& bestApprox) {

    std::vector<std::vector<cv::Point>> contours;
    std::vector<cv::Vec4i> hierarchy;
    cv::findContours(thresh.clone(), contours, hierarchy, cv::RETR_TREE, cv::CHAIN_APPROX_SIMPLE);

    double maxArea = 0;
    bool found = false;

    for (const auto& contour : contours) {
        double area = cv::contourArea(contour);
        if (area < 100) continue;

        std::vector<cv::Point> approx;
        cv::approxPolyDP(contour, approx, 0.02 * cv::arcLength(contour, true), true);

        if (approx.size() == 4 && cv::isContourConvex(approx) && area > maxArea) {
            maxArea = area;
            bestApprox = approx;
            found = true;
        }
    }
    return found;

}



std::vector<cv::Point2f> GridProcessor::orderPoints(std::vector<cv::Point>& pts) {
    std::vector<cv::Point2f> ordered(4);
    std::sort(pts.begin(), pts.end(), [](cv::Point a, cv::Point b) { return a.y < b.y; });

    if (pts[0].x < pts[1].x) {
        ordered[0] = pts[0]; // top-left
        ordered[1] = pts[1]; // top-right
    }
    else {
        ordered[0] = pts[1];
        ordered[1] = pts[0];
    }

    if (pts[2].x < pts[3].x) {
        ordered[3] = pts[2]; // bottom-left
        ordered[2] = pts[3]; // bottom-right
    }
    else {
        ordered[3] = pts[3];
        ordered[2] = pts[2];
    }
    return ordered;
}


cv::Mat GridProcessor::warpToSquare(const cv::Mat& image, const std::vector<cv::Point2f>& srcPts, float side) {
    std::vector<cv::Point2f> dstPts = {
        cv::Point2f(0, 0),
        cv::Point2f(side - 1, 0),
        cv::Point2f(side - 1, side - 1),
        cv::Point2f(0, side - 1)
    };

    cv::Mat M = cv::getPerspectiveTransform(srcPts, dstPts);
    cv::Mat warped;
    cv::warpPerspective(image, warped, M, cv::Size(side, side));
    return warped;
}


void GridProcessor::extractGridLines(const cv::Mat& binary, cv::Mat& horizontal, cv::Mat& vertical, int side) {
    int morphSize = side / 20;  // * adjust based on grid size

    cv::Mat hor_kernel = cv::getStructuringElement(cv::MORPH_RECT, cv::Size(morphSize, 1));
    cv::morphologyEx(binary, horizontal, cv::MORPH_OPEN, hor_kernel);

    cv::Mat ver_kernel = cv::getStructuringElement(cv::MORPH_RECT, cv::Size(1, morphSize));
    cv::morphologyEx(binary, vertical, cv::MORPH_OPEN, ver_kernel);
}


int GridProcessor::detectHorizontalLines(const cv::Mat& binary, int minLineLength) {
    cv::Mat projection;
    cv::reduce(binary, projection, 1, cv::REDUCE_SUM, CV_32S);

    std::vector<int> lineCenters;

    for (int i = 0; i < projection.rows; ++i) {
        if (projection.at<int>(i, 0) > minLineLength) {
            lineCenters.push_back(i);
        }
    }

    if (lineCenters.empty())
        return 0;

    // Cluster nearby rows into one line
    int count = 1;
    for (size_t i = 1; i < lineCenters.size(); ++i) {
        if (lineCenters[i] - lineCenters[i - 1] > 5) {
            count++;
        }
    }

    return count;
}



int GridProcessor::detectVerticalLines(const cv::Mat& binary, int minLineLength) {
    cv::Mat projection;
    cv::reduce(binary, projection, 0, cv::REDUCE_SUM, CV_32S);

    std::vector<int> lineCenters;

    for (int i = 0; i < projection.cols; ++i) {
        if (projection.at<int>(0, i) > minLineLength) {
            lineCenters.push_back(i);
        }
    }

    if (lineCenters.empty())
        return 0;

    int count = 1;
    for (size_t i = 1; i < lineCenters.size(); ++i) {
        if (lineCenters[i] - lineCenters[i - 1] > 5) {
            count++;
        }
    }

    return count;
}


bool GridProcessor::processGrid(const cv::Mat& binary, const cv::Mat& horizontal, const cv::Mat& vertical, float side, const cv::Mat& warped) {
    cv::Mat mask;
    cv::absdiff(binary, horizontal, mask);
    cv::absdiff(mask, vertical, mask);

    cv::Mat morphValue = cv::getStructuringElement(cv::MORPH_RECT, cv::Size(4, 4));
    cv::morphologyEx(mask, mask, cv::MORPH_OPEN, morphValue);
    cv::medianBlur(mask, mask, 3);

    return findEntities(const_cast<cv::Mat&>(warped), mask);
}


bool GridProcessor::elaborateFrame(cv::Mat& image)
{
    cv::Mat gray, blur, thresh;
    cv::cvtColor(image, gray, cv::COLOR_BGR2GRAY);
    cv::GaussianBlur(gray, blur, cv::Size(5, 5), 0);

    // * adaptive threshold to detect outer square
    cv::adaptiveThreshold(
        blur, 
        thresh, 
        255,
        cv::ADAPTIVE_THRESH_MEAN_C,
        cv::THRESH_BINARY_INV,
        15, 
        4
    );

    // * find largest square control
    std::vector<cv::Point> bestApprox;
    if (!findLargestSquareContour(thresh, bestApprox)) return false;

    // * order square points
    std::vector<cv::Point2f> orderedPts = orderPoints(bestApprox);

    // * warping
    cv::Mat warped = warpToSquare(image, orderedPts, Config::WARP_SIDE);

    if (isObstructed(warped)) return false;

    // * threshold warped image
    cv::Mat grayWarped, binary;
    cv::cvtColor(warped, grayWarped, cv::COLOR_BGR2GRAY);

    cv::adaptiveThreshold(grayWarped, binary, 255,
                          cv::ADAPTIVE_THRESH_MEAN_C,
                          cv::THRESH_BINARY_INV,
                          15, 4);


    cv::Mat horizontal, vertical;
    extractGridLines(binary, horizontal, vertical, static_cast<int>(Config::WARP_SIDE));

    // * ensure clean binary
    cv::threshold(horizontal, horizontal, 128, 255, cv::THRESH_BINARY);
    cv::threshold(vertical, vertical, 128, 255, cv::THRESH_BINARY);

    // * combine lines
    cv::Mat gridLines;
    cv::bitwise_or(horizontal, vertical, gridLines);

    int minSumThreshold = static_cast<int>(Config::WARP_SIDE * 0.6 * 255);

    int detectedHorizontal = detectHorizontalLines(gridLines, minSumThreshold);
    int detectedVertical   = detectVerticalLines(gridLines, minSumThreshold);

    // * lines to cells
    int rowCount = detectedHorizontal - 1;
    int colCount = detectedVertical - 1;

    if (defaultRows == -1 && defaultCols == -1) {
        if (rowCount == 6 && colCount == 10) {
            defaultRows = 6;
            defaultCols = 10;
            std::cout << "Valid 6x10 grid locked." << std::endl;
        }
        else {
            return false;
        }
    }

    // * every new matrix must have the same size of the first found
    if (rowCount != defaultRows || colCount != defaultCols) return false;

    return processGrid(binary, horizontal, vertical, Config::WARP_SIDE, warped);
}


