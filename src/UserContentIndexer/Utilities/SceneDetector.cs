namespace UserContentIndexer.Utilities
{
    using Microsoft.Extensions.Logging;
    using OpenCvSharp;
    using UserContentIndexer.Models;
    using static System.Net.Mime.MediaTypeNames;

    public class SceneDetector
    {
        private readonly ILogger<SceneDetector> logger;

        public SceneDetector(ILogger<SceneDetector> logger)
        {
            this.logger = logger;
        }

        private const int MinFramesBetweenScenes = 15;
        private const string OutputFolder = "MAGIX";

        public IList<SceneInfo> ProcessVideo(string videoPath)
        {
            this.logger.LogInformation("SceneDetector - Start");
            var picturesDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            var scenes = new List<SceneInfo>();
            if (!Directory.Exists(OutputFolder))
            {
                Directory.CreateDirectory(Path.Combine(picturesDirectory, OutputFolder, videoPath.Split('\\').Last().Split('.').First()));
            }
            try
            {
                using var capture = new VideoCapture(videoPath);
                if (!capture.IsOpened())
                {
                    this.logger.LogError("Error opening video file");
                }
                var fps = capture.Get(VideoCaptureProperties.Fps);
                if (fps <= 0)
                {
                    this.logger.LogError("Invalid frame rate");
                }
                // Get total number of frames
                var totalFrames = (int)capture.Get(VideoCaptureProperties.FrameCount);

                // Detect scenes and collect start and end frames
                IList<SceneBounds> sceneBounds = this.DetectScenes(capture, totalFrames);

                // Save middle frames and calculate timestamps
                using (var capture2 = new VideoCapture(videoPath))
                {
                    foreach (var bounds in sceneBounds)
                    {
                        var middleFrame = bounds.StartFrame + (bounds.EndFrame - bounds.StartFrame) / 2;
                        var startTime = bounds.StartFrame / fps;
                        var endTime = bounds.EndFrame / fps;

                        capture2.Set(VideoCaptureProperties.PosFrames, middleFrame);
                        using var middleFrameMat = new Mat();
                        if (capture2.Read(middleFrameMat) && !middleFrameMat.Empty())
                        {
                            var newSize = new Size(256, 256);
                            var resizedImage = new Mat();
                            Cv2.Resize(middleFrameMat, resizedImage, newSize, 0, 0, InterpolationFlags.Linear);
                            var framePath = Path.Combine(picturesDirectory, OutputFolder, videoPath.Split('\\').Last().Split('.').First(), $"scene_{bounds.SceneNumber}.png");
                            resizedImage.SaveImage(framePath);
                            this.logger.LogInformation($"Saved frame: {framePath}");
                            scenes.Add(new SceneInfo
                            {
                                ImagePath = framePath,
                                StartTime = TimeSpan.FromSeconds(startTime),
                                EndTime = TimeSpan.FromSeconds(endTime)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError($"Error processing video: {ex.Message}");
                throw;
            }

            return scenes;
        }

        private List<SceneBounds> DetectScenes(VideoCapture capture, int totalFrames)
        {
            var sceneBounds = new List<SceneBounds>();
            var sceneNumber = 0;
            var startFrame = 0;
            var frameIndex = 0;

            using var previousFrame = new Mat();
            capture.Read(previousFrame);
            if (previousFrame.Empty())
            {
                this.logger.LogInformation("Error reading video file");
                throw new Exception("Error reading video file");
            }

            var framesSinceLastScene = MinFramesBetweenScenes;

            using var currentFrame = new Mat();
            while (capture.Read(currentFrame))
            {
                framesSinceLastScene++;

                if (framesSinceLastScene >= MinFramesBetweenScenes)
                {
                    (var isSceneChange, _) = this.DetectSceneChange(previousFrame, currentFrame);
                    if (isSceneChange)
                    {
                        this.logger.LogInformation($"Scene: {sceneNumber} Frames in Scene: {framesSinceLastScene}");
                        sceneBounds.Add(new SceneBounds
                        {
                            SceneNumber = sceneNumber,
                            StartFrame = startFrame,
                            EndFrame = frameIndex - 1
                        });
                        sceneNumber++;
                        startFrame = frameIndex;
                        framesSinceLastScene = 0;
                    }
                }

                currentFrame.CopyTo(previousFrame);
                frameIndex++;
            }

            // Add the last scene
            sceneBounds.Add(new SceneBounds
            {
                SceneNumber = sceneNumber,
                StartFrame = startFrame,
                EndFrame = totalFrames - 1
            });

            return sceneBounds;
        }


        private (bool isSceneChange, Dictionary<string, double> metrics) DetectSceneChange(Mat previousFrame, Mat currentFrame, double histogramSimilarityThreshold = 0.7, double edgeChangeThreshold = 0.1)
        {
            // Validate input frames
            if (previousFrame == null || currentFrame == null)
            {
                this.logger.LogInformation("Frames cannot be null");
                throw new ArgumentNullException("Frames cannot be null");
            }

            if (previousFrame.Size() != currentFrame.Size())
            {
                this.logger.LogInformation("Frames must have the same dimensions");
                throw new ArgumentException("Frames must have the same dimensions");
            }

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
                    this.logger.LogInformation("Error converting color space", ex);
                    throw new InvalidOperationException("Error converting color space", ex);
                }

                // More robust histogram calculation
                int[] channels = [0, 1, 2];
                int[] histSize = [32, 32, 32];
                Rangef[] ranges = [new(0, 180), new(0, 256), new(0, 256)];

                using var prevHist = new Mat();
                using var currHist = new Mat();

                try
                {
                    Cv2.CalcHist([prevHsv], channels, null, prevHist, 3, histSize, ranges);
                    Cv2.CalcHist([currHsv], channels, null, currHist, 3, histSize, ranges);

                    Cv2.Normalize(prevHist, prevHist, 0, 1, NormTypes.MinMax);
                    Cv2.Normalize(currHist, currHist, 0, 1, NormTypes.MinMax);
                }
                catch (OpenCvSharpException ex)
                {
                    this.logger.LogInformation("Error calculating histograms", ex);
                    throw new InvalidOperationException("Error calculating histograms", ex);
                }

                // Compare histograms using multiple methods for more robust detection
                var histDiffCorrel = Cv2.CompareHist(prevHist, currHist, HistCompMethods.Correl);
                var histDiffIntersect = Cv2.CompareHist(prevHist, currHist, HistCompMethods.Intersect);
                var histDiffBhattacharyya = Cv2.CompareHist(prevHist, currHist, HistCompMethods.Bhattacharyya);

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
                    var totalPixels = edgeDiff.Rows * edgeDiff.Cols;
                    var changedEdgePixels = Cv2.CountNonZero(edgeDiff);
                    var edgeChangeRatio = (double)changedEdgePixels / totalPixels;

                    // Advanced scene change detection logic
                    var isSceneChange =
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
                    this.logger.LogInformation("Error processing edges", ex);
                    throw new InvalidOperationException("Error processing edges", ex);
                }
            }
            catch (Exception ex)
            {
                // Log or handle unexpected errors
                this.logger.LogInformation($"Scene detection error: {ex.Message}");
                throw;
            }
        }
        public static void DeleteCache()
        {
            if (Directory.Exists(OutputFolder))
            {
                Directory.Delete(OutputFolder, true);
            }
        }
    }
}
