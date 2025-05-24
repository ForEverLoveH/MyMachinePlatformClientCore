using System.Drawing;
using OpenCvSharp;
using OpenCvSharp.XFeatures2D;
using Point = OpenCvSharp.Point;
using Size = OpenCvSharp.Size;

namespace MyMachinePlatformClientCore.Service.OpenCVService;

public class OpencvService
{
    /// <summary>
    /// 根据传入的mat 找到其中的圆(获取找圆)
    /// </summary>
    /// <param name="image">待检测圆的图像</param>
    /// <returns>检测到的圆数组，每个圆用一个 Vec3f 表示，包含圆心坐标和半径</returns>
    public CircleSegment[] FindCircle(Mat image)
    {
        // 转换为灰度图像，霍夫圆检测需要灰度图像
        using (Mat grayImage = new Mat())
        {
            Cv2.CvtColor(image, grayImage, ColorConversionCodes.BGR2GRAY);

            // 高斯模糊，减少噪声影响
            Cv2.GaussianBlur(grayImage, grayImage, new Size(9, 9), 2, 2);

            // 应用霍夫圆变换
            CircleSegment[] circles = Cv2.HoughCircles(
                grayImage,
                HoughModes.Gradient,
                1,
                grayImage.Rows / 8, // 检测到的圆的圆心之间的最小距离
                param1: 100, // Canny 边缘检测器的高阈值
                param2: 30, // 累加器阈值，值越小检测到的圆越多
                minRadius: 1, // 最小圆半径
                maxRadius: 0 // 最大圆半径，0 表示不限制
            );
            return circles;
        }
    }

    /// <summary>
    /// 二值化图像
    /// </summary>
    /// <param name="image">待二值化的图像</param>
    /// <param name="thresholdValue">二值化阈值</param>
    /// <returns>二值化后的图像</returns>
    public Mat BinaryThreshold(Mat image, int thresholdValue)
    {
        Mat binaryImage = new Mat();
        Cv2.Threshold(image, binaryImage, thresholdValue, 255, ThresholdTypes.Binary);
        return binaryImage;
    }

    //<SUmmary>
    //灰度处理
    /// </summary>
    /// <param name="image">待处理的图像</param>
    /// <returns>灰度处理后的图像</returns>
    public Mat GrayMat(Mat image)
    {
        Mat grayImage = new Mat();
        Cv2.CvtColor(image, grayImage, ColorConversionCodes.BGR2GRAY);
        return grayImage;
    }

    /// <summary>
    /// 膨胀图像
    /// </summary>
    /// <param name="image">待膨胀的图像</param>
    /// <param name="kernelSize">膨胀核的大小</param>
    /// <returns>膨胀后的图像</returns>
    public Mat Dilate(Mat image, int kernelSize)
    {
        Mat dilatedImage = new Mat();
        Cv2.Dilate(image, dilatedImage,
            Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(kernelSize, kernelSize)));
        return dilatedImage;
    }

    /// <summary>
    /// 图像上画出圆
    /// </summary>
    /// <param name="image">待绘制的图像</param>
    /// <param name="circle">圆的参数，包括圆心坐标和半径</param>
    /// <param name="color">圆的颜色</param>
    /// <param name="thickness">圆的线宽</param>
    /// <returns>绘制后的图像</returns>
    public Mat DrawCircle(Mat image, CircleSegment[] circles, Scalar color, int thickness)
    {
        if (circles == null || circles.Length == 0)
        {
            foreach (var circle in circles)
            {
                Cv2.Circle(image, (Point)circle.Center, (int)circle.Radius, color, thickness);

            }
        }

        return image;
    }

    /// <summary>
    /// 图像上画出矩形
    /// </summary>
    /// <param name="image">待绘制的图像</param>
    /// <param name="rectangle">矩形的参数，包括左上角坐标和右下角坐标</param>
    /// <param name="color">矩形的颜色</param>
    /// <param name="thickness">矩形的线宽</param>
    /// <returns>绘制后的图像</returns>
    public Mat DrawRectangle(Mat image, Rect rectangle, Scalar color, int thickness)
    {
        Cv2.Rectangle(image, rectangle, color, thickness);
        return image;
    }

    /// <summary>
    /// 将bitmap 转 mat
    /// </summary>
    /// <param name="bitmap"></param>
    /// <returns></returns>
    public Mat SetBitMapToMat(Bitmap bmp)
    {
        Mat mat = OpenCvSharp.Extensions.BitmapConverter.ToMat(bmp);
        return mat;
    }

    /// <summary>
    /// 将mat 转 bitmap
    /// </summary>
    /// <param name="mat"></param>
    /// <returns></returns>
    public Bitmap SetMatToBitMap(Mat mat)
    {
        Bitmap bmp = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mat);
        return bmp;
    }

    /// <summary>
    ///  获取图像的像素点
    /// </summary>
    /// <param name="image">输入图像</param>
    /// <param name="x">像素点的横坐标</param>
    /// <param name="y">像素点的纵坐标</param>
    /// <returns>返回一个包含三个元素的数组，分别表示B、G、R通道的值</returns>
    public int[] GetPixel(Mat image, int x, int y)
    {
        int[] pixel = new int[3];
        pixel[0] = image.At<Vec3b>(y, x)[0];
        pixel[1] = image.At<Vec3b>(y, x)[1];
        pixel[2] = image.At<Vec3b>(y, x)[2];
        return pixel;
    }

    /// <summary>
    /// 设置图像的像素点
    /// </summary>
    /// <param name="image">输入图像</param>
    /// <param name="x">像素点的横坐标</param>
    /// <param name="y">像素点的纵坐标</param>
    /// <param name="pixel">包含三个元素的数组，分别表示B、G、R通道的值</param>
    /// <returns>返回设置后的图像</returns>
    public Mat SetPixel(Mat image, int x, int y, int[] pixel)
    {
        image.Set<Vec3b>(y, x, new Vec3b((byte)pixel[0], (byte)pixel[1], (byte)pixel[2]));
        return image;
    }

    /// <summary>
    /// 深度复制bitmap
    /// </summary>
    /// <param name="bitmap"></param>
    /// <returns></returns>
    public Bitmap DeepCopyBitmap(Bitmap bitmap)
    {
        if (bitmap == null) return null;
        Bitmap dstBitmap = bitmap.Clone(new Rectangle(0, 0, bitmap.Width, bitmap.Height), bitmap.PixelFormat);
        return dstBitmap;
    }

    /// <summary>
    /// 深度复制mat
    /// </summary>
    /// <param name="mat"></param>
    /// <returns></returns>
    public Mat DeepCopyMat(Mat mat)
    {
        if (mat == null) return null;
        Mat dstMat = mat.Clone();
        return dstMat;
    }

    /// <summary>
    /// 从图像中查找指定的模板
    /// </summary>
    /// <param name="image">待查找的图像</param>
    /// <param name="template">待查找的模板图像</param>
    /// <param name="threshold">匹配阈值，默认为0.9</param>
    /// <returns>返回匹配结果，如果匹配成功，则返回匹配结果图像，否则返回null</returns>
    public Mat FindPictureFromImage(Mat image, Mat template, double threshold = 0.9)
    {
        Mat result = new Mat();
        Cv2.MatchTemplate(image, template, result, TemplateMatchModes.CCoeffNormed);
        Point minLoc, maxLoc;
        Cv2.MinMaxLoc(result, out minLoc, out maxLoc);
        if (result.At<float>(maxLoc.Y, maxLoc.X) > threshold)
        {
            return result;
        }
        else

            return null;
    }

    /// <summary>
    /// 从图像中查找指定的模板
    /// </summary>
    /// <param name="image"></param>
    /// <param name="template"></param>
    /// <param name="threshold"></param>
    /// <returns></returns>
    public Bitmap FindPictureFromImage(Bitmap image, Bitmap template, double threshold = 0.9)
    {
        Mat imageMat = SetBitMapToMat(image);
        Mat templateMat = SetBitMapToMat(template);
        Mat result = FindPictureFromImage(imageMat, templateMat, threshold);
        return SetMatToBitMap(result);
    }

    /// <summary>
    /// SURF特征匹配
    /// </summary>
    /// <param name="image">待匹配的图像</param>
    /// <param name="template">待匹配的模板图像</param>
    /// <param name="threshold">匹配阈值，默认为0.9</param>
    /// <returns>返回匹配结果，如果匹配成功，则返回匹配结果图像，否则返回null</returns>
    public Mat FindPictureFromImageBySURF(Mat image, Mat templateImage, double threshold = 0.9)
    {
        var gray1 = new Mat();
        var gray2 = new Mat();

        Cv2.CvtColor(templateImage, gray1, ColorConversionCodes.BGR2GRAY);
        Cv2.CvtColor(image, gray2, ColorConversionCodes.BGR2GRAY);

        var surf = SURF.Create(1000, 4, 2, true);

        var descriptors1 = new Mat();
        var descriptors2 = new Mat();
        surf.DetectAndCompute(gray1, null, out KeyPoint[] keypoints1, descriptors1);
        surf.DetectAndCompute(gray2, null, out KeyPoint[] keypoints2, descriptors2);

        var matcher = new BFMatcher(NormTypes.L2, false);
        DMatch[] matches = matcher.Match(descriptors1, descriptors2);

        matches = FindGoodMatchs(descriptors1, matches);

        Mat img = new Mat();
        Cv2.DrawMatches(gray1, keypoints1, gray2, keypoints2, matches, img, null, null, null,
            DrawMatchesFlags.NotDrawSinglePoints);

        return img;
    }

    /// <summary>
    /// SURF特征匹配
    /// </summary>
    /// <param name="image"></param>
    /// <param name="template"></param>
    /// <param name="threshold"></param>
    /// <returns></returns>
    public Bitmap FindPictureFromImageBySURF(Bitmap image, Bitmap template, double threshold = 0.9)
    {
        Mat imageMat = SetBitMapToMat(image);
        Mat templateMat = SetBitMapToMat(template);
        Mat result = FindPictureFromImageBySURF(imageMat, templateMat, threshold);
        return SetMatToBitMap(result);

    }

    /// <summary>
    /// 通过距离处理图像中中的特征匹配
    /// </summary>
    /// <param name="descriptors1"></param>
    /// <param name="matches"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private DMatch[] FindGoodMatchs(Mat src, DMatch[] matches)
    {
        double max = 0, min = 1000;
        for (int i = 0; i < src.Rows; i++)
        {
            double dist = matches[i].Distance;
            if (dist > max)
            {
                max = dist;
            }

            if (dist < min)
            {
                min = dist;
            }
        }

        var good = new List<DMatch>();
        for (int i = 0; i < src.Rows; i++)
        {
            double dist = matches[i].Distance;
            if (dist < Math.Max(3 * min, 0.02))
            {
                good.Add(matches[i]);
            }
        }

        return good.ToArray();
    }

     /// <summary>
     /// 获取图像的特征点(输入图像中检测直线并返回这些直线的端点)
     /// </summary>
     /// <param name="image"></param>
     /// <returns></returns>
    public Point[] GetFeaturePixels(Mat image)
    {
        // 将图像转换为灰度图像
        Mat grayImage = new Mat();
        Cv2.CvtColor(image, grayImage, ColorConversionCodes.BGR2GRAY);
        // 使用Canny边缘检测算法获取边缘
        Mat edges = new Mat();
        Cv2.Canny(grayImage, edges, 50, 150);
        // 使用Hough变换检测直线
        LineSegmentPoint[] lines = Cv2.HoughLinesP(edges, 1, Math.PI / 180, 50, 50, 10);
        // 返回检测到的直线端点
        Point[] featurePixels = new Point[lines.Length];
        for (int i = 0; i < lines.Length; i++)
        {
            featurePixels[i] = lines[i].P1;
            
        }
        return featurePixels;
    }

    /// <summary>>
    /// 傅里叶变换
    /// </summary>
    /// <param name="image"></param>
    /// <returns></returns>
    public Mat FourierTransform(Mat image)
    {
        Mat grayImage = new Mat();
        Cv2.CvtColor(image, grayImage, ColorConversionCodes.BGR2GRAY);
        Mat complexImage = new Mat();
        Cv2.CvtColor(image, complexImage, ColorConversionCodes.BGR2GRAY);
        Cv2.Dft(grayImage, complexImage, DftFlags.ComplexOutput);
        return complexImage;
    }

    /// <summary>
    /// 傅里叶变换
    /// </summary>
    /// <param name="image"></param>
    /// <returns></returns>
    public Bitmap FourierTransform(Bitmap image)
    {
        Mat imageMat = SetBitMapToMat(image);
        Mat result = FourierTransform(imageMat);
        return SetMatToBitMap(result);
        
    }

    /// <summary>
    /// 羽化处理将一个图像中的某个区域羽化处理，使其边缘更加平滑，同时保持其他区域不变
    /// </summary>
    /// <param name="image">待羽化处理的图像</param>
    /// <returns>羽化处理后的图像</returns>
    public Mat Feather(Mat image)
    {
        // 复制原始图像，避免修改原始数据
        Mat result = image.Clone();
        // 创建一个与原始图像大小相同的掩码，初始化为全白
        Mat mask = new Mat(image.Size(), MatType.CV_8UC1, Scalar.White);
        // 定义羽化区域，这里以图像中心为基准，设置一个矩形区域
        int width = image.Width;
        int height = image.Height;
        int x = width / 4;
        int y = height / 4;
        int w = width / 2;
        int h = height / 2;
        Rect roi = new Rect(x, y, w, h);
        // 在掩码上绘制一个圆形区域，该区域内的像素值为 255，区域外的像素值将被羽化
        Cv2.Circle(mask, new Point(width / 2, height / 2), Math.Min(width, height) / 2, Scalar.Black, -1);
        // 对掩码进行高斯模糊，实现羽化效果
        int kernelSize = 101; // 高斯核大小，可根据需要调整
        Cv2.GaussianBlur(mask, mask, new Size(kernelSize, kernelSize), 0);
        // 将掩码转换为 3 通道，以便与图像进行混合
        Mat mask3Channel = new Mat();
        Cv2.CvtColor(mask, mask3Channel, ColorConversionCodes.GRAY2BGR);
        // 将掩码归一化到 0 到 1 之间
        mask3Channel.ConvertTo(mask3Channel, MatType.CV_32F, 1.0 / 255.0);
        // 创建一个反向掩码
        Mat inverseMask = new Mat();
        Cv2.Subtract(new Scalar(1.0, 1.0, 1.0), mask3Channel, inverseMask);
        // 复制原始图像并转换为 32 位浮点数类型
        Mat imageFloat = new Mat();
        image.ConvertTo(imageFloat, MatType.CV_32F);
        // 应用掩码进行混合操作
        Mat blendedImage = new Mat();
        Cv2.Multiply(imageFloat, inverseMask, blendedImage);
        // 对原始图像进行高斯模糊
        Mat blurredImage = new Mat();
        Cv2.GaussianBlur(imageFloat, blurredImage, new Size(kernelSize, kernelSize), 0);
        // 应用掩码对模糊图像进行混合
        Mat blurredBlendedImage = new Mat();
        Cv2.Multiply(blurredImage, mask3Channel, blurredBlendedImage);
        // 将混合后的图像相加
        Mat finalImage = new Mat();
        Cv2.Add(blendedImage, blurredBlendedImage, finalImage);
        // 将结果转换回 8 位无符号整数类型
        finalImage.ConvertTo(result, MatType.CV_8U);
        // 释放临时创建的 Mat 对象
        mask.Dispose();
        mask3Channel.Dispose();
        inverseMask.Dispose();
        imageFloat.Dispose();
        blendedImage.Dispose();
        blurredImage.Dispose();
        blurredBlendedImage.Dispose();
        finalImage.Dispose();

        return result;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="image"></param>
    /// <returns></returns>
    public Mat Feather(Bitmap image)
    { 
        Mat imageMat = SetBitMapToMat(image);
        Mat result = Feather(imageMat);
        return result;
    }

    /// <summary>
    /// 图像的直方图均衡化
    /// </summary>
    /// <param name="image">待处理的图像</param>
    /// <returns>直方图均衡化后的图像</returns>
    public Mat HistogramEqualization(Mat image)
    {
        Mat result;
        if (image.Channels() == 1)
        {
            // 单通道图像（灰度图）直接进行直方图均衡化
            result = new Mat();
            Cv2.EqualizeHist(image, result);
        }
        else
        {
            // 多通道图像（如彩色图）需要先转换到 YCrCb 颜色空间
            Mat ycrcb = new Mat();
            Cv2.CvtColor(image, ycrcb, ColorConversionCodes.BGR2YCrCb);
            // 分离 YCrCb 通道
            Mat[] channels = Cv2.Split(ycrcb);
            // 对 Y 通道（亮度通道）进行直方图均衡化
            Cv2.EqualizeHist(channels[0], channels[0]);
            // 合并处理后的通道
            Cv2.Merge(channels, ycrcb);
            // 将 YCrCb 图像转换回 BGR 颜色空间
            result = new Mat();
            Cv2.CvtColor(ycrcb, result, ColorConversionCodes.YCrCb2BGR);
            // 释放临时对象
            ycrcb.Dispose();
            foreach (var channel in channels)
            {
                channel.Dispose();
            }
        }
        return result;
    }
    /// <summary>
    /// 根据俩张图片存在相似的像素点，平滑处理后将俩张图片合并后返回一张图
    /// </summary>
    /// <param name="image">第一张图片</param>
    /// <param name="template">第二张图片</param>
    /// <returns>合并后的图片</returns>
    public Mat FindSimilarPoint(Mat image, Mat template)
    {
        // 确保两张图片尺寸一致
        if (image.Size() != template.Size())
        {
            Cv2.Resize(template, template, image.Size());
        }

        // 将图像转换为灰度图，方便计算相似度
        Mat grayImage = new Mat();
        Mat grayTemplate = new Mat();
        Cv2.CvtColor(image, grayImage, ColorConversionCodes.BGR2GRAY);
        Cv2.CvtColor(template, grayTemplate, ColorConversionCodes.BGR2GRAY);
        // 计算两张灰度图的差异图
        Mat diff = new Mat();
        Cv2.Absdiff(grayImage, grayTemplate, diff);
        // 设定相似度阈值，可根据实际情况调整
        double threshold = 30;
        Mat mask = new Mat();
        Cv2.Threshold(diff, mask, threshold, 255, ThresholdTypes.BinaryInv);
        // 对掩码进行平滑处理，使用高斯模糊
        Cv2.GaussianBlur(mask, mask, new Size(15, 15), 0);
        // 将掩码转换为 3 通道，以便与彩色图像进行混合
        Mat mask3Channel = new Mat();
        Cv2.CvtColor(mask, mask3Channel, ColorConversionCodes.GRAY2BGR);
        // 将掩码归一化到 0 到 1 之间
        mask3Channel.ConvertTo(mask3Channel, MatType.CV_32F, 1.0 / 255.0);
        // 创建反向掩码
        Mat inverseMask = new Mat();
        Cv2.Subtract(new Scalar(1.0, 1.0, 1.0), mask3Channel, inverseMask);
        // 复制原始图像并转换为 32 位浮点数类型
        Mat imageFloat = new Mat();
        Mat templateFloat = new Mat();
        image.ConvertTo(imageFloat, MatType.CV_32F);
        template.ConvertTo(templateFloat, MatType.CV_32F);
        // 应用掩码进行混合操作
        Mat blendedImage = new Mat();
        Mat blendedTemplate = new Mat();
        Cv2.Multiply(imageFloat, inverseMask, blendedImage);
        Cv2.Multiply(templateFloat, mask3Channel, blendedTemplate);
        // 将混合后的图像相加
        Mat result = new Mat();
        Cv2.Add(blendedImage, blendedTemplate, result);
        // 将结果转换回 8 位无符号整数类型
        result.ConvertTo(result, MatType.CV_8U);
        // 释放临时创建的 Mat 对象
        grayImage.Dispose();
        grayTemplate.Dispose();
        diff.Dispose();
        mask.Dispose();
        mask3Channel.Dispose();
        inverseMask.Dispose();
        imageFloat.Dispose();
        templateFloat.Dispose();
        blendedImage.Dispose();
        blendedTemplate.Dispose();

        return result;
    }

    /// <summary>
    /// 根据俩张图片存在相似的像素点，平滑处理后将俩张图片合并后返回一张图
    /// </summary>
    /// <param name="image">第一张图片</param>
    /// <param name="template">第二张图片</param>
    /// <returns>合并后的图片</returns>
    public Bitmap FindSimilarPoint(Bitmap image, Bitmap template)
    {
        
        Mat result = FindSimilarPoint(SetBitMapToMat(image), SetBitMapToMat(template));
        return SetMatToBitMap(result);    
    }

    /// <summary>
    ///轮廓提取
    /// </summary>
    /// <param name="image">待处理的图像</p>
    /// <returns>轮廓提取后的图像</returns>
    public Mat ContourExtraction(Mat image)
    {
        Mat grayImage = new Mat();
        Mat cannyImage = new Mat();
        Cv2.CvtColor(image, grayImage, ColorConversionCodes.BGR2GRAY);
        Cv2.Canny(grayImage, cannyImage, 100, 200);
        return cannyImage;
    }

    /// <summary>
    /// 柔化处理
    /// </summary>
    /// <param name="image">待处理的图像</param>
    /// <returns>柔化处理后的图像</returns>
    public Mat Smooth(Mat image)
    {
        Mat result = new Mat();
        Cv2.GaussianBlur(image, result, new Size(5, 5), 0);
        return result;
    }

    /// <summary>>
    /// 图像的直方图均衡化
    /// </summary>
    /// <param name="image">待处理的图像</param>
    /// <returns>直方图均衡化后的图像</returns>
    public Bitmap HistogramEqualization(Bitmap image)
    {
        Mat imageMat = SetBitMapToMat(image);
        Mat result = HistogramEqualization(imageMat);
        return SetMatToBitMap(result);
    }

    /// <summary>
    /// 图像的光影变换
    /// </summary>
    /// <param name="image">待处理的图像</param>
    /// <returns>光影变换后的图像</returns>
    public Mat Light(Mat image)
    {
        
        Mat result = new Mat();
        Cv2.Laplacian(image, result, MatType.CV_8U);
        return result; 
    }
    
    

}