#include <opencv2/opencv.hpp>
#include <iostream>
#include <vector>
#include <windows.h>
#include <cmath>

// 全局变量，用于存储鼠标选择的 ROI
cv::Rect roi_original, roi_blurred;
bool selecting = false; // 是否正在选择区域
cv::Point start_point;  // 鼠标按下的起点

// 鼠标回调函数数据结构
struct MouseCallbackData {
    cv::Mat* image;
    cv::Mat* outputImage;
    cv::Rect roi;
    bool selecting;
    cv::Point startPoint;
};

// 鼠标回调函数
void mouseCallback(int event, int x, int y, int flags, void* userdata) {
    // 获取传递的结构体数据
    MouseCallbackData* data = reinterpret_cast<MouseCallbackData*>(userdata);

    switch (event) {
    case cv::EVENT_LBUTTONDOWN: // 鼠标左键按下
        data->selecting = true;
        data->startPoint = cv::Point(x, y);
        data->roi = cv::Rect(x, y, 0, 0); // 初始化 ROI
        *(data->outputImage) = data->image->clone(); // 克隆原始图像
        break;

    case cv::EVENT_MOUSEMOVE: // 鼠标移动
        if (data->selecting) {
            data->roi.width = x - data->startPoint.x;
            data->roi.height = y - data->startPoint.y;
            cv::Mat tempImage = data->outputImage->clone(); // 克隆带有 ROI 的图像
            cv::rectangle(tempImage, data->roi, cv::Scalar(0, 255, 0), 2); // 绘制矩形
            cv::imshow("Original Image", tempImage); // 更新显示
        }
        break;

    case cv::EVENT_LBUTTONUP: // 鼠标左键释放
        data->selecting = false;
        data->roi.width = x - data->startPoint.x;
        data->roi.height = y - data->startPoint.y;
        cv::rectangle(*(data->outputImage), data->roi, cv::Scalar(0, 255, 0), 2); // 最终绘制矩形
        cv::imshow("Original Image", *(data->outputImage)); // 更新显示
        break;
    }
}

// 鼠标回调函数用于高斯模糊图像
void mouseCallbackBlurred(int event, int x, int y, int flags, void* userdata) {
    cv::Mat* image = reinterpret_cast<cv::Mat*>(userdata);
    static cv::Mat blurred_image_with_roi; // 用于保存带有 ROI 的图像

    switch (event) {
    case cv::EVENT_LBUTTONDOWN: // 鼠标左键按下
        selecting = true;
        start_point = cv::Point(x, y);
        roi_blurred = cv::Rect(x, y, 0, 0); // 初始化 ROI
        blurred_image_with_roi = image->clone(); // 克隆高斯模糊图像
        break;

    case cv::EVENT_MOUSEMOVE: // 鼠标移动
        if (selecting) {
            roi_blurred.width = x - start_point.x;
            roi_blurred.height = y - start_point.y;
            cv::Mat temp_image = blurred_image_with_roi.clone(); // 克隆带有 ROI 的图像
            cv::rectangle(temp_image, roi_blurred, cv::Scalar(255, 0, 0), 2); // 绘制矩形
            cv::imshow("Blurred Image", temp_image); // 更新显示
        }
        break;

    case cv::EVENT_LBUTTONUP: // 鼠标左键释放
        selecting = false;
        roi_blurred.width = x - start_point.x;
        roi_blurred.height = y - start_point.y;
        cv::rectangle(blurred_image_with_roi, roi_blurred, cv::Scalar(255, 0, 0), 2); // 最终绘制矩形
        cv::imshow("Blurred Image", blurred_image_with_roi); // 更新显示
        break;
    }
}

// 计算图像强度的函数
double calculateImageIntensity(const cv::Mat& grad_amp, const cv::Mat& binary_image) {
    cv::Mat grad_amp_8u;
    double min_val, max_val;
    cv::minMaxLoc(grad_amp, &min_val, &max_val);
    grad_amp.convertTo(grad_amp_8u, CV_8U, 255.0 / max_val);

    cv::Mat binary_image_8u;
    binary_image.convertTo(binary_image_8u, CV_8U);

    cv::Mat result_image;
    cv::multiply(grad_amp_8u, binary_image_8u, result_image, 1, CV_8U);

    return cv::mean(result_image)[0];
}

// 计算梯度幅度和强度的辅助函数
double processImage(const cv::Mat& image, int kernel_size, int scale, int delta) {
    cv::Mat gray_image;
    cv::cvtColor(image, gray_image, cv::COLOR_BGR2GRAY);

    cv::Mat grad_x, grad_y;
    cv::Sobel(gray_image, grad_x, CV_32F, 1, 0, kernel_size, scale, delta, cv::BORDER_DEFAULT);
    cv::Sobel(gray_image, grad_y, CV_32F, 0, 1, kernel_size, scale, delta, cv::BORDER_DEFAULT);

    cv::Mat grad_amp;
    cv::magnitude(grad_x, grad_y, grad_amp);

    cv::Mat binary_image;
    cv::threshold(grad_amp, binary_image, 20, 255, cv::THRESH_BINARY);

    return calculateImageIntensity(grad_amp, binary_image);
}

// 计算拉普拉斯梯度的函数
double calculateLaplaceGradient(const cv::Mat& image) {
    // 转换为灰度图像（如果不是灰度图）
    cv::Mat gray_image;
    if (image.channels() == 3) {
        cv::cvtColor(image, gray_image, cv::COLOR_BGR2GRAY);
    } else {
        gray_image = image.clone();
    }

    // 计算拉普拉斯梯度
    cv::Mat laplace;
    cv::Laplacian(gray_image, laplace, CV_64F, 3); // 使用拉普拉斯算子计算梯度

    // 转换为绝对值
    cv::Mat abs_laplace;
    cv::convertScaleAbs(laplace, abs_laplace);

    // 计算均值作为强度值
    return cv::mean(abs_laplace)[0];
}
struct ZImage {
    double z_height;   // z轴高度
    cv::Mat image;     // 对应的图像
};

std::vector<ZImage> z_images;
std::vector<cv::Point2d> points; // 存储z高度和强度的点
std::vector<cv::Point2d> points1; // 存储z高度和强度的点
int main() {
    SetConsoleOutputCP(CP_UTF8);

    // // 提示用户输入图像文件名
    // std::string image_filename;
    // std::cout << "请输入图像文件名（例如：apple.jpg）：";
    // std::cin >> image_filename;

    // // 读取图像
    // cv::Mat image = cv::imread(image_filename);
    // if (image.empty()) {
    //     std::cerr << "无法读取图像，请检查文件名和路径是否正确！" << std::endl;
    //     return -1;
    // }

    int n;
    std::cout << "请输入要添加的z高度-图像对数量: ";
    std::cin >> n;

    for (int i = 0; i < n; ++i) {
        double z;
        std::string img_path;
        std::cout << "请输入第" << (i+1) << "组的z高度: ";
        std::cin >> z;
        std::cout << "请输入第" << (i+1) << "组的图像文件名: ";
        std::cin >> img_path;

        cv::Mat img = cv::imread(img_path);
        if (img.empty()) {
            std::cerr << "无法读取图像: " << img_path << std::endl;
            continue;
        }
        z_images.push_back({z, img});
    }

    // 示例：遍历输出
    for (const auto& zi : z_images) {
        std::cout << "z高度: " << zi.z_height << "，图像尺寸: " << zi.image.cols << "x" << zi.image.rows << std::endl;
        // cv::imshow("Z Image", zi.image);
        // cv::waitKey(0);
    }

    // 提示用户输入 Sobel 算子的参数
    int kernel_size, scale, delta;
    std::cout << "### 注意事项\n"
                 "1. **核大小必须为奇数**：\n"
                 "   - Sobel 算子要求核大小为奇数（如 3、5、7），以确保卷积操作的中心像素能够对齐。\n"
                 "\n"
                 "2. **与图像分辨率的关系**：\n"
                 "   - 对于高分辨率图像，较大的核可能更适合，因为图像中的边缘通常更宽。\n"
                 "   - 对于低分辨率图像，较小的核可能更适合，以避免过度平滑。\n"
                 "\n";
    std::cout << "请输入 Sobel 核大小（奇数，推荐 3 或 5）: ";
    std::cin >> kernel_size;
    if (kernel_size % 2 == 0) {
        std::cerr << "核大小必须是奇数！" << std::endl;
        return -1;
    }

    std::cout << "请输入 Sobel 缩放因子（推荐 1）: ";
    std::cin >> scale;

    std::cout << "请输入 Sobel 偏移量（推荐 0）: ";
    std::cin >> delta;

    // // 对图像进行高斯模糊
    // cv::Mat blurred_image;
    // cv::GaussianBlur(image, blurred_image, cv::Size(5, 5), 1.5);

    // // 显示原图并设置鼠标回调
    // cv::Mat original_image_with_roi;
    // MouseCallbackData original_data = { &image, &original_image_with_roi, cv::Rect(), false, cv::Point() };
    // cv::imshow("Original Image", image);// 显示原图
    // cv::setMouseCallback("Original Image", mouseCallback, &original_data);

    // std::cout << "请用鼠标选择原图的感兴趣区域（ROI），然后按任意键继续..." << std::endl;
    // cv::waitKey(0);

    // // 检查原图 ROI 是否有效
    // roi_original = original_data.roi & cv::Rect(0, 0, image.cols, image.rows); // 限制 ROI 在图像范围内
    // if (roi_original.width <= 0 || roi_original.height <= 0) {
    //     std::cerr << "未选择有效的原图感兴趣区域！" << std::endl;
    //     return -1;
    // }

    // // // 显示高斯模糊图像并设置鼠标回调
    // // cv::imshow("Blurred Image", blurred_image);
    // // cv::setMouseCallback("Blurred Image", mouseCallbackBlurred, &blurred_image);

    // // std::cout << "请用鼠标选择高斯模糊图像的感兴趣区域（ROI），然后按任意键继续..." << std::endl;
    // // cv::waitKey(0);

    // // // 检查高斯模糊图像 ROI 是否有效
    // // roi_blurred = roi_blurred & cv::Rect(0, 0, blurred_image.cols, blurred_image.rows); // 限制 ROI 在模糊图像范围内
    // // if (roi_blurred.width <= 0 || roi_blurred.height <= 0) {
    // //     std::cerr << "未选择有效的高斯模糊图像感兴趣区域！" << std::endl;
    // //     return -1;
    // // }

    // // 提取感兴趣区域
    // cv::Mat roi_original_image = image(roi_original);
    // cv::Mat roi_blurred_image = blurred_image(roi_blurred);

    // // 计算强度
    // double original_intensity = processImage(image, kernel_size, scale, delta);
    // double roi_original_intensity = processImage(roi_original_image, kernel_size, scale, delta);
    // // double blurred_intensity = processImage(blurred_image, kernel_size, scale, delta);
    // // double roi_blurred_intensity = processImage(roi_blurred_image, kernel_size, scale, delta);

    // // 计算拉普拉斯梯度
    // double original_laplace_intensity = calculateLaplaceGradient(image);
    // // double blurred_laplace_intensity = calculateLaplaceGradient(blurred_image);
    // double roi_original_laplace_intensity = calculateLaplaceGradient(roi_original_image);
    // // double roi_blurred_laplace_intensity = calculateLaplaceGradient(roi_blurred_image);

    // // 输出结果
    // std::cout << "原图的图像强度: " << original_intensity << std::endl;
    // std::cout << "原图ROI区域的图像强度: " << roi_original_intensity << std::endl;
    // // std::cout << "高斯模糊后的图像强度: " << blurred_intensity << std::endl;
    // // std::cout << "高斯模糊ROI区域的图像强度: " << roi_blurred_intensity << std::endl;

    // std::cout << "原图的拉普拉斯梯度强度: " << original_laplace_intensity << std::endl;
    // std::cout << "原图ROI区域的拉普拉斯梯度强度: " << roi_original_laplace_intensity << std::endl;
    // // std::cout << "高斯模糊后的拉普拉斯梯度强度: " << blurred_laplace_intensity << std::endl;
    // // std::cout << "高斯模糊ROI区域的拉普拉斯梯度强度: " << roi_blurred_laplace_intensity << std::endl;

    // // 显示结果 
    // cv::imshow("ROI Original 文字Image", roi_original_image);
    // cv::imshow("ROI Blurred Image", roi_blurred_image);
    // ...existing code...

// 遍历结构体数组，对每个z高度-图像对进行处理
    for (size_t i = 0; i < z_images.size(); ++i) {
        cv::Mat& image = z_images[i].image;
        double z = z_images[i].z_height;

        // 显示原图并设置鼠标回调
        cv::Mat original_image_with_roi;
        MouseCallbackData original_data = { &image, &original_image_with_roi, cv::Rect(), false, cv::Point() };
        cv::imshow("Original Image", image);
        cv::setMouseCallback("Original Image", mouseCallback, &original_data);

        std::cout << "请用鼠标选择z高度为 " << z << " 的图像的感兴趣区域（ROI），然后按任意键继续..." << std::endl;
        cv::waitKey(0);

// 检查原图 ROI 是否有效
        roi_original = original_data.roi & cv::Rect(0, 0, image.cols, image.rows);
        if (roi_original.width <= 0 || roi_original.height <= 0) {
            std::cerr << "未选择有效的原图感兴趣区域！" << std::endl;
            continue;
        }

        cv::Mat roi_original_image = image(roi_original);
        double roi_original_intensity = processImage(roi_original_image, kernel_size, scale, delta);// 计算 ROI 区域的图像强度
        double roi_original_laplace_intensity = calculateLaplaceGradient(roi_original_image);

        // 存入点数组
        points.emplace_back(z, roi_original_intensity);
        points1.emplace_back(z, roi_original_laplace_intensity);

        // 输出结果
        std::cout << "z高度: " << z << std::endl;
        std::cout << "原图的图像强度: " << processImage(image, kernel_size, scale, delta) << std::endl;
        std::cout << "原图ROI区域的图像强度: " << roi_original_intensity << std::endl;
        std::cout << "原图的拉普拉斯梯度强度: " << calculateLaplaceGradient(image) << std::endl;
        std::cout << "原图ROI区域的拉普拉斯梯度强度: " << calculateLaplaceGradient(roi_original_image) << std::endl;

        cv::imshow("ROI Original Image", roi_original_image);
        cv::waitKey(0);

        cv::destroyWindow("Original Image");
        cv::destroyWindow("ROI Original Image");
    }
    cv::Mat plot_img(400, 600, CV_8UC3, cv::Scalar(255,255,255));

    // 拟合（多项式拟合，这里用2次多项式为例）
    if (points.size() >= 3) {
        cv::Mat X(points.size(), 3, CV_64F);
        cv::Mat Y(points.size(), 1, CV_64F);
        for (size_t i = 0; i < points.size(); ++i) {
            X.at<double>(i, 0) = points[i].x * points[i].x;
            X.at<double>(i, 1) = points[i].x;
            X.at<double>(i, 2) = 1.0;
            Y.at<double>(i, 0) = points[i].y;
        }
        cv::Mat coeffs;
        cv::solve(X, Y, coeffs, cv::DECOMP_QR); // coeffs: [a, b, c] for ax^2+bx+c

        // 画图
        int width = 600, height = 400, margin = 50;


        // 计算z和y的范围
        double min_z = points[0].x, max_z = points[0].x, min_y = points[0].y, max_y = points[0].y;
        for (const auto& pt : points) {
            min_z = std::min(min_z, pt.x);
            max_z = std::max(max_z, pt.x);
            min_y = std::min(min_y, pt.y);
            max_y = std::max(max_y, pt.y);
        }
        if (fabs(max_z - min_z) < 1e-6) max_z += 1; // 防止除0
        if (fabs(max_y - min_y) < 1e-6) max_y += 1;

        // 画点
        for (const auto& pt : points) {
            int px = margin + int((pt.x - min_z) / (max_z - min_z) * (width - 2 * margin));
            int py = height - margin - int((pt.y - min_y) / (max_y - min_y) * (height - 2 * margin));
            cv::circle(plot_img, cv::Point(px, py), 4, cv::Scalar(0,0,255), -1);
        }

        // 画拟合曲线
        std::vector<cv::Point> curve;
        for (int i = 0; i < width - 2 * margin; ++i) {
            double z = min_z + (max_z - min_z) * i / (width - 2 * margin);
            double y = coeffs.at<double>(0,0)*z*z + coeffs.at<double>(1,0)*z + coeffs.at<double>(2,0);
            int px = margin + i;
            int py = height - margin - int((y - min_y) / (max_y - min_y) * (height - 2 * margin));
            curve.emplace_back(px, py);
        }
        cv::polylines(plot_img, curve, false, cv::Scalar(255,0,0), 2);

        // 坐标轴
        cv::line(plot_img, cv::Point(margin, height - margin), cv::Point(width - margin, height - margin), cv::Scalar(0,0,0), 2);
        cv::line(plot_img, cv::Point(margin, height - margin), cv::Point(margin, margin), cv::Scalar(0,0,0), 2);

        // 标注
        cv::putText(plot_img, "z", cv::Point(width - margin + 10, height - margin + 5), cv::FONT_HERSHEY_SIMPLEX, 0.7, cv::Scalar(0,0,0), 2);
        cv::putText(plot_img, "intensity", cv::Point(margin - 40, margin - 10), cv::FONT_HERSHEY_SIMPLEX, 0.7, cv::Scalar(0,0,0), 2);

        // // 显示两个窗口
        // cv::imshow("z-ROI强度拟合曲线", plot_img);
        // cv::imshow("z-ROIlaplace强度拟合曲线", plot_img1);
        // cv::waitKey(0); // 等待用户按键
        // cv::destroyAllWindows(); // 关闭所有窗口
    } else {
        std::cout << "点数不足，无法拟合！" << std::endl;
    }
    if (points1.size() >= 3) {
        cv::Mat X(points1.size(), 3, CV_64F);
        cv::Mat Y(points1.size(), 1, CV_64F);
        for (size_t i = 0; i < points1.size(); ++i) {
            X.at<double>(i, 0) = points1[i].x * points1[i].x;
            X.at<double>(i, 1) = points1[i].x;
            X.at<double>(i, 2) = 1.0;
            Y.at<double>(i, 0) = points1[i].y;
        }
        cv::Mat coeffs1;
        cv::solve(X, Y, coeffs1, cv::DECOMP_QR); // coeffs: [a, b, c] for ax^2+bx+c

        // 画图
        int width = 600, height = 400, margin = 50;
        cv::Mat plot_img1(height, width, CV_8UC3, cv::Scalar(255,255,255));

        // 计算z和y的范围
        double min_z = points1[0].x, max_z = points1[0].x, min_y = points1[0].y, max_y = points1[0].y;
        for (const auto& pt : points1) {
            min_z = std::min(min_z, pt.x);
            max_z = std::max(max_z, pt.x);
            min_y = std::min(min_y, pt.y);
            max_y = std::max(max_y, pt.y);
        }
        if (fabs(max_z - min_z) < 1e-6) max_z += 1; // 防止除0
        if (fabs(max_y - min_y) < 1e-6) max_y += 1;

        // 画点
        for (const auto& pt : points1) {
            int px = margin + int((pt.x - min_z) / (max_z - min_z) * (width - 2 * margin));
            int py = height - margin - int((pt.y - min_y) / (max_y - min_y) * (height - 2 * margin));
            cv::circle(plot_img1, cv::Point(px, py), 4, cv::Scalar(0,0,255), -1);
        }

        // 画拟合曲线
        std::vector<cv::Point> curve;
        for (int i = 0; i < width - 2 * margin; ++i) {
            double z = min_z + (max_z - min_z) * i / (width - 2 * margin);
            double y = coeffs1.at<double>(0,0)*z*z + coeffs1.at<double>(1,0)*z + coeffs1.at<double>(2,0);
            int px = margin + i;
            int py = height - margin - int((y - min_y) / (max_y - min_y) * (height - 2 * margin));
            curve.emplace_back(px, py);
        }
        cv::polylines(plot_img1, curve, false, cv::Scalar(255,0,0), 2);

        // 坐标轴
        cv::line(plot_img1, cv::Point(margin, height - margin), cv::Point(width - margin, height - margin), cv::Scalar(0,0,0), 2);
        cv::line(plot_img1, cv::Point(margin, height - margin), cv::Point(margin, margin), cv::Scalar(0,0,0), 2);

        // 标注
        cv::putText(plot_img1, "z", cv::Point(width - margin + 10, height - margin + 5), cv::FONT_HERSHEY_SIMPLEX, 0.7, cv::Scalar(0,0,0), 2);
        cv::putText(plot_img1, "intensity", cv::Point(margin - 40, margin - 10), cv::FONT_HERSHEY_SIMPLEX, 0.7, cv::Scalar(0,0,0), 2);

        // 显示两个窗口
        cv::imshow("z-ROI强度拟合曲线", plot_img);
        cv::imshow("z-ROIlaplace强度拟合曲线", plot_img1);
        cv::waitKey(0); // 等待用户按键
        cv::destroyAllWindows(); // 关闭所有窗口
    } else {
        std::cout << "点数不足，无法拟合！" << std::endl;
    }

    return 0;
}
