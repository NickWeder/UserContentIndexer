# User Content Indexer API

A .NET-based API service that analyzes video content using machine learning models to extract scene information, transcribe audio, and provide detailed scene descriptions.

## Features

- **Scene Detection**: Automatically splits videos into meaningful scenes using OpenCV
- **Audio Transcription**: Transcribes audio content using Whisper ML model
- **Scene Analysis**: Analyzes video scenes using LLaVA (Large Language and Vision Assistant)
- **Format Conversion**: Converts various audio formats to WAV for processing
- **JSON Output**: Provides structured output containing scene descriptions, transcriptions, and metadata

## Technologies Used

- **.NET 8.0**
- **OpenCV Sharp**: For video processing and scene detection
- **Whisper.net**: For audio transcription
- **LLaVA**: For scene analysis and description
- **NAudio**: For audio format conversion
- **ML Models**:
  - Whisper (small/medium)
  - LLaVA v1.5-7b
  - GGML/GGUF model formats

## Prerequisites

- .NET 8.0 SDK
- Sufficient storage space for ML models
- GPU support (optional but recommended)

## Installation

1. Clone the repository:
```bash
git clone https://github.com/yourusername/user-content-indexer-api.git
```

2. Navigate to the project directory:
```bash
cd user-content-indexer-api
```

3. Install dependencies:
```bash
dotnet restore
```

4. Build the project:
```bash
dotnet build
```

## Usage

1. Start the API:
```bash
dotnet run
```

2. Make a GET request with the video path in the header:
```http
GET /UserContentIndexer
Header: videoPath=/path/to/your/video.mp4
```

### Response Format

The API returns a JSON structure containing:
```json
{
  "videoname": "string",
  "imageDescriptions": [
    {
      "videodescription": "string",
      "tags": {
        "primarySubjectTags": "string",
        "atmosphereTags": "string",
        "styleTags": "string",
        "cameraAngleTags": "string"
      },
      "previewImage": "string",
      "startOfScene": "timespan",
      "endOfScene": "timespan"
    }
  ],
  "whisperResults": [
    {
      "text": "string",
      "start": "timespan",
      "end": "timespan"
    }
  ]
}
```

## Architecture

The project follows a clean architecture pattern with:
- Controllers for API endpoints
- Services for business logic
- Utilities for helper functions
- Models for data structures
- Interfaces for dependency injection

### Key Components

- **SceneDetector**: Handles video scene detection and frame extraction
- **AudioService**: Manages audio transcription using Whisper
- **ImageAnalyzeService**: Processes scenes using LLaVA
- **ModelManager**: Handles ML model loading and management
- **AudioConverter**: Converts audio formats

## Performance

The system provides timing information for:
- Scene splitting
- Audio transcription
- Image description generation

## Model Downloads

Models are automatically downloaded from Hugging Face on first use:
- Whisper models: Available in different sizes (small/medium)
- LLaVA models: GGML/GGUF format for efficient inference

## Error Handling

The system includes comprehensive error handling for:
- File operations
- Model loading
- Video processing
- Audio conversion
- Scene detection
- API requests

## Logging

Extensive logging is implemented using Microsoft.Extensions.Logging:
- Operation timing
- Process status
- Error conditions
- Model loading status

## Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

Apache-2.0 license

## Acknowledgments

- Whisper by OpenAI
- LLaVA
- OpenCV community
- NAudio project
