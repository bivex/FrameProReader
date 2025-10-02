# FramePro Reader

**FramePro Reader** is a console application that reads FramePro (.framepro) profiling files and generates comprehensive performance analysis reports with a focus on multithreading optimization.

## Features:
- Reads FramePro save files (.framepro format)
- Analyzes session metadata (frame count, timer frequency, thread information)
- Identifies hot functions with detailed performance metrics
- Groups functions by threads for multithreading analysis
- Generates frame-by-frame performance data
- Exports data in structured JSON format

## Usage:
```bash
dotnet run --project FrameProReader "path/to/your/file.framepro"
```

## Key Metrics Provided:

### Performance Metrics:
- **TotalTimeMs**: Total execution time across all frames
- **TotalCount**: Total number of function calls
- **AvgTimePerFrameMs**: Average execution time per frame
- **MaxTimePerFrameMs**: Maximum execution time in any single frame
- **AvgCountPerFrame**: Average number of calls per frame
- **MaxCountPerFrame**: Maximum number of calls in any single frame

### Multithreading Analysis:
- **ThreadId**: Unique identifier for the thread
- **ThreadName**: Human-readable thread name
- **ThreadUtilizationPercent**: Percentage of thread utilization
- **IsMainThread**: Whether function runs on main thread (UI blocking)
- **IsRenderThread**: Whether function runs on render thread (FPS impact)
- **IsWorkerThread**: Whether function runs on worker thread (parallelizable)

## Optimization Guidance:

The JSON files provide data to identify:

### 1. Performance Bottlenecks:
- Functions with high **TotalTimeMs** (optimization priority)
- Functions with high **MaxTimePerFrameMs** (cause frame drops)
- Functions with high **ThreadUtilizationPercent** (thread saturation)

### 2. Multithreading Issues:
- Functions on main thread (**IsMainThread: true**) blocking UI
- Functions on render thread (**IsRenderThread: true**) affecting FPS
- Overloaded threads (**ThreadUtilizationPercent > 80%**)

### 3. Optimization Opportunities:
- Functions suitable for worker threads (**IsWorkerThread: true**)
- Functions with high call frequency (**AvgCountPerFrame**)
- Functions with inconsistent performance (high **MaxTimePerFrameMs** vs **AvgTimePerFrameMs**)

## Technical Details:
- **Framework**: .NET 8.0
- **Dependencies**: FrameProCore.dll, SCLCoreCLR.dll
- **Output Format**: Structured JSON using System.Text.Json
- **Performance**: Handles large datasets efficiently (212 functions × 254 frames)

## Example Analysis Workflow:
1. Run the tool on your .framepro file
2. Open `functions_analysis.json` to identify top performance issues
3. Filter by `IsMainThread: true` to find UI-blocking functions
4. Sort by `TotalTimeMs` to prioritize optimization efforts
5. Use `frame_analysis.json` to analyze specific problematic frames
6. Cross-reference thread utilization to balance workload

## JSON Structure Example:

```json
{
  "FunctionName": "Event Wait",
  "ThreadId": 5032,
  "ThreadName": "TaskGraph Render Thread 2",
  "TotalTimeMs": 37150.96,
  "TotalCount": 6426,
  "MaxTimePerFrameMs": 519.17,
  "MaxCountPerFrame": 38,
  "AvgTimePerFrameMs": 146.84,
  "AvgCountPerFrame": 25.40,
  "ThreadUtilizationPercent": 100,
  "IsMainThread": false,
  "IsRenderThread": true,
  "IsWorkerThread": true,
  "ThreadPriority": 0
}
```

## Author: AI Assistant
## Version: 1.0
## Date: October 2025

**FramePro Reader** is a console application that reads FramePro (.framepro) profiling files and generates comprehensive performance analysis reports with a focus on multithreading optimization.

### Features:
- Reads FramePro save files (.framepro format)
- Analyzes session metadata (frame count, timer frequency, thread information)
- Identifies hot functions with detailed performance metrics
- Groups functions by threads for multithreading analysis
- Generates frame-by-frame performance data
- Exports data in structured JSON format

### Usage:
```bash
dotnet run --project FrameProReader "path/to/your/file.framepro"
```

### Output Files:

#### 1. Frame Analysis (filename_frame_analysis.json)
- **Size**: ~28MB
- **Content**: Frame-by-frame analysis for all 254 frames
- **Structure**: Each frame contains all functions executed in that frame
- **Data**: Detailed timing and count information per frame
- **Thread Info**: Thread information for each function

#### 2. Functions Analysis (filename_functions_analysis.json)
- **Size**: ~110KB
- **Content**: Aggregated statistics for all 212 functions
- **Sorting**: By total execution time (hot functions first)
- **Thread Analysis**: Thread utilization and classification
- **Metrics**: Performance metrics for optimization prioritization

### Key Metrics Provided:

#### Performance Metrics:
- **TotalTimeMs**: Total execution time across all frames
- **TotalCount**: Total number of function calls
- **AvgTimePerFrameMs**: Average execution time per frame
- **MaxTimePerFrameMs**: Maximum execution time in any single frame
- **AvgCountPerFrame**: Average number of calls per frame
- **MaxCountPerFrame**: Maximum number of calls in any single frame

#### Multithreading Analysis:
- **ThreadId**: Unique identifier for the thread
- **ThreadName**: Human-readable thread name
- **ThreadUtilizationPercent**: Percentage of thread utilization
- **IsMainThread**: Whether function runs on main thread (UI blocking)
- **IsRenderThread**: Whether function runs on render thread (FPS impact)
- **IsWorkerThread**: Whether function runs on worker thread (parallelizable)

### Optimization Guidance:

The JSON files provide data to identify:

#### 1. Performance Bottlenecks:
- Functions with high **TotalTimeMs** (optimization priority)
- Functions with high **MaxTimePerFrameMs** (cause frame drops)
- Functions with high **ThreadUtilizationPercent** (thread saturation)

#### 2. Multithreading Issues:
- Functions on main thread (**IsMainThread: true**) blocking UI
- Functions on render thread (**IsRenderThread: true**) affecting FPS
- Overloaded threads (**ThreadUtilizationPercent > 80%**)

#### 3. Optimization Opportunities:
- Functions suitable for worker threads (**IsWorkerThread: true**)
- Functions with high call frequency (**AvgCountPerFrame**)
- Functions with inconsistent performance (high **MaxTimePerFrameMs** vs **AvgTimePerFrameMs**)

### Technical Details:
- **Framework**: .NET 8.0
- **Dependencies**: FrameProCore.dll, SCLCoreCLR.dll
- **Output Format**: Structured JSON using System.Text.Json
- **Performance**: Handles large datasets efficiently (212 functions × 254 frames)

### Example Analysis Workflow:
1. Run the tool on your .framepro file
2. Open `functions_analysis.json` to identify top performance issues
3. Filter by `IsMainThread: true` to find UI-blocking functions
4. Sort by `TotalTimeMs` to prioritize optimization efforts
5. Use `frame_analysis.json` to analyze specific problematic frames
6. Cross-reference thread utilization to balance workload

### JSON Structure Example:

```json
{
  "FunctionName": "Event Wait",
  "ThreadId": 5032,
  "ThreadName": "TaskGraph Render Thread 2",
  "TotalTimeMs": 37150.96,
  "TotalCount": 6426,
  "MaxTimePerFrameMs": 519.17,
  "MaxCountPerFrame": 38,
  "AvgTimePerFrameMs": 146.84,
  "AvgCountPerFrame": 25.40,
  "ThreadUtilizationPercent": 100,
  "IsMainThread": false,
  "IsRenderThread": true,
  "IsWorkerThread": true,
  "ThreadPriority": 0
}
```
