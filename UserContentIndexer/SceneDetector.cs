using OpenCvSharp;

namespace UserContentIndexer
{
    public class SceneDetector
    {
        private const int MinFramesBetweenScenes = 15;
        private const double DefaultThreshold = 40;
        private const double DefaultMinPixelDiff = 0.4;
        private const string OutputFolder = "Chache";
        private const int ShortVideoDurationThreshold = 30; // seconds

        public void DeleteChache()
        {
            Directory.Delete(OutputFolder, true);
        }

        public List<string> ProcessVideo(string videoPath, double threshold = DefaultThreshold, double minPixelDiff = 0.4f)
        {
            try
            {
                using var capture = new VideoCapture(videoPath);
                if (!capture.IsOpened())
                    throw new Exception("Error opening video file");

                // Get video duration
                double duration = capture.Get(VideoCaptureProperties.PosFrames) > 0
                    ? capture.Get(VideoCaptureProperties.FrameCount) / capture.Get(VideoCaptureProperties.PosFrames)
                    : 0;

                // If video is short, use keyframe extraction
                if (duration <= ShortVideoDurationThreshold)
                {
                    return ExtractKeyframes(capture);
                }

                // Existing scene detection logic for longer videos
                var keyframePaths = new List<string>();
                if (!Directory.Exists(OutputFolder))
                {
                    Directory.CreateDirectory(OutputFolder);
                }

                using var previousFrame = new Mat();
                capture.Read(previousFrame);
                if (previousFrame.Empty())
                    throw new Exception("Error reading video file");

                // Save first frame
                string firstFramePath = Path.Combine(OutputFolder, $"frame_0.png");
                previousFrame.SaveImage(firstFramePath);
                keyframePaths.Add(firstFramePath);

                int frameIndex = 1;
                int framesSinceLastScene = MinFramesBetweenScenes;

                using var currentFrame = new Mat();
                while (capture.Read(currentFrame))
                {
                    framesSinceLastScene++;

                    if (framesSinceLastScene >= MinFramesBetweenScenes)
                    {
                        var (isSceneChange, metrics) = DetectSceneChange(previousFrame, currentFrame, threshold, minPixelDiff);

                        if (isSceneChange)
                        {
                            string framePath = Path.Combine(OutputFolder, $"frame_{frameIndex}.png");
                            currentFrame.SaveImage(framePath);
                            keyframePaths.Add(framePath);
                            framesSinceLastScene = 0;
                        }
                    }

                    currentFrame.CopyTo(previousFrame);
                    frameIndex++;
                }
                return keyframePaths;
            }
            catch (ThreadAbortException)
            {
                throw;
            }
        }

        private List<string> ExtractKeyframes(VideoCapture capture)
        {
            var keyframePaths = new List<string>();
            if (!Directory.Exists(OutputFolder))
            {
                Directory.CreateDirectory(OutputFolder);
            }

            // Total number of frames in the video
            int totalFrames = (int)capture.Get(VideoCaptureProperties.FrameCount);

            // Calculate keyframe intervals
            int[] keyframeIndices = CalculateKeyframeIndices(totalFrames);

            // Reset capture to beginning
            capture.Set(VideoCaptureProperties.PosFrames, 0);

            // Extract keyframes at specified indices
            foreach (int frameIndex in keyframeIndices)
            {
                // Set capture to specific frame
                capture.Set(VideoCaptureProperties.PosFrames, frameIndex);

                using var frame = new Mat();
                if (capture.Read(frame) && !frame.Empty())
                {
                    string framePath = Path.Combine(OutputFolder, $"frame_{frameIndex}.png");
                    frame.SaveImage(framePath);
                    keyframePaths.Add(framePath);
                }
            }

            return keyframePaths;
        }

        private int[] CalculateKeyframeIndices(int totalFrames)
        {
            // For very short videos, extract 3-5 evenly distributed frames
            if (totalFrames <= 15)
            {
                return new[] { 0, totalFrames / 2, totalFrames - 1 };
            }

            // For slightly longer short videos
            if (totalFrames <= 30)
            {
                return new[] {
                    0,
                    totalFrames / 4,
                    totalFrames / 2,
                    3 * totalFrames / 4,
                    totalFrames - 1
                };
            }

            // For videos approaching the 30-second threshold
            return new[] {
                0,
                totalFrames / 6,
                totalFrames / 3,
                totalFrames / 2,
                2 * totalFrames / 3,
                5 * totalFrames / 6,
                totalFrames - 1
            };
        }

        private (bool isSceneChange, Dictionary<string, double> metrics) DetectSceneChange(Mat previousFrame, Mat currentFrame, double histogramSimilarityThreshold = 0.7, double edgeChangeThreshold = 0.1)
        {
            // Validate input frames
            if (previousFrame == null || currentFrame == null)
                throw new ArgumentNullException("Frames cannot be null");

            if (previousFrame.Size() != currentFrame.Size())
                throw new ArgumentException("Frames must have the same dimensions");

            try
            {
                using var prevHsv = new Mat();
                using var currHsv = new Mat();

                // Color space conversion with error handling
                try
                {
                    Cv2.CvtColor(previousFrame, prevHsv, ColorConversionCodes.BGR2HSV);
                    Cv2.CvtColor(currentFrame, currHsv, ColorConversionCodes.BGR2HSV);
                }
                catch (OpenCvSharpException ex)
                {
                    throw new InvalidOperationException("Error converting color space", ex);
                }

                // More robust histogram calculation
                int[] channels = { 0, 1, 2 };
                int[] histSize = { 32, 32, 32 };
                Rangef[] ranges = { new Rangef(0, 180), new Rangef(0, 256), new Rangef(0, 256) };

                using var prevHist = new Mat();
                using var currHist = new Mat();

                try
                {
                    Cv2.CalcHist(new[] { prevHsv }, channels, null, prevHist, 3, histSize, ranges);
                    Cv2.CalcHist(new[] { currHsv }, channels, null, currHist, 3, histSize, ranges);

                    Cv2.Normalize(prevHist, prevHist, 0, 1, NormTypes.MinMax);
                    Cv2.Normalize(currHist, currHist, 0, 1, NormTypes.MinMax);
                }
                catch (OpenCvSharpException ex)
                {
                    throw new InvalidOperationException("Error calculating histograms", ex);
                }

                // Compare histograms using multiple methods for more robust detection
                double histDiffCorrel = Cv2.CompareHist(prevHist, currHist, HistCompMethods.Correl);
                double histDiffIntersect = Cv2.CompareHist(prevHist, currHist, HistCompMethods.Intersect);
                double histDiffBhattacharyya = Cv2.CompareHist(prevHist, currHist, HistCompMethods.Bhattacharyya);

                // Edge detection with improved parameters and more analysis
                using var prevGray = new Mat();
                using var currGray = new Mat();
                using var prevEdges = new Mat();
                using var currEdges = new Mat();
                using var edgeDiff = new Mat();

                try
                {
                    // Convert to grayscale
                    Cv2.CvtColor(previousFrame, prevGray, ColorConversionCodes.BGR2GRAY);
                    Cv2.CvtColor(currentFrame, currGray, ColorConversionCodes.BGR2GRAY);

                    // Adaptive edge detection with Gaussian blur for noise reduction
                    using var prevBlurred = new Mat();
                    using var currBlurred = new Mat();
                    Cv2.GaussianBlur(prevGray, prevBlurred, new Size(3, 3), 0);
                    Cv2.GaussianBlur(currGray, currBlurred, new Size(3, 3), 0);

                    // Use adaptive thresholding for edge detection
                    Cv2.Canny(prevBlurred, prevEdges, 50, 150);
                    Cv2.Canny(currBlurred, currEdges, 50, 150);

                    // Calculate edge change
                    Cv2.Absdiff(prevEdges, currEdges, edgeDiff);

                    // More nuanced edge change calculation
                    int totalPixels = edgeDiff.Rows * edgeDiff.Cols;
                    int changedEdgePixels = Cv2.CountNonZero(edgeDiff);
                    double edgeChangeRatio = (double)changedEdgePixels / totalPixels;

                    // Advanced scene change detection logic
                    bool isSceneChange =
                        (histDiffCorrel < histogramSimilarityThreshold) &&
                        (edgeChangeRatio > edgeChangeThreshold);

                    // Comprehensive metrics
                    var metrics = new Dictionary<string, double>
            {
                { "histogram_correlation", histDiffCorrel },
                { "histogram_intersection", histDiffIntersect },
                { "histogram_bhattacharyya", histDiffBhattacharyya },
                { "edge_change_ratio", edgeChangeRatio },
                { "combined_score",
                    (1 - histDiffCorrel) * edgeChangeRatio *
                    (1 - histDiffBhattacharyya)
                }
            };

                    return (isSceneChange, metrics);
                }
                catch (OpenCvSharpException ex)
                {
                    throw new InvalidOperationException("Error processing edges", ex);
                }
            }
            catch (Exception ex)
            {
                // Log or handle unexpected errors
                Console.WriteLine($"Scene detection error: {ex.Message}");
                throw;
            }
        }
    }


}
