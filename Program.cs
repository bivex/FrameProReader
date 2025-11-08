/*
 * FramePro Reader - Performance Analyzer for .framepro Files
 * 
 * BRIEF DESCRIPTION:
 * Program reads FramePro (.framepro) files and creates detailed performance analysis
 * with focus on multithreading. Generates two JSON files:
 * 1. Frame-by-frame analysis - detailed information for each frame
 * 2. Functions analysis - summary statistics for all functions
 * 
 * USAGE:
 * dotnet run --project FrameProReader "path/to/file.framepro"
 * 
 * OUTPUT FILES:
 * - filename_frame_analysis.json    - frame-by-frame analysis (large file)
 * - filename_functions_analysis.json - functions analysis (compact file)
 * 
 * KEY METRICS:
 * - TotalTimeMs, AvgTimePerFrameMs, MaxTimePerFrameMs
 * - ThreadUtilizationPercent, IsMainThread, IsRenderThread
 * - ThreadId, ThreadName for multithreading analysis
 */

/*
 * FramePro Reader - Performance Analysis Tool for .framepro Files
 * 
 * DESCRIPTION:
 * This console application reads FramePro (.framepro) profiling files and generates comprehensive
 * performance analysis reports with a focus on multithreading optimization. The tool creates two
 * detailed JSON output files for different analysis purposes.
 * 
 * FEATURES:
 * - Reads FramePro save files (.framepro format)
 * - Analyzes session metadata (frame count, timer frequency, thread information)
 * - Identifies hot functions with detailed performance metrics
 * - Groups functions by threads for multithreading analysis
 * - Generates frame-by-frame performance data
 * - Exports data in structured JSON format
 * 
 * USAGE:
 * dotnet run --project FrameProReader "path/to/your/file.framepro"
 * 
 * OUTPUT FILES:
 * 1. filename_frame_analysis.json (Large file ~28MB)
 *    - Frame-by-frame analysis for all 254 frames
 *    - Each frame contains all functions executed in that frame
 *    - Detailed timing and count information per frame
 *    - Thread information for each function
 * 
 * 2. filename_functions_analysis.json (Compact file ~110KB)
 *    - Aggregated statistics for all 212 functions
 *    - Sorted by total execution time (hot functions first)
 *    - Thread utilization and classification
 *    - Performance metrics for optimization prioritization
 * 
 * KEY METRICS PROVIDED:
 * Performance Metrics:
 * - TotalTimeMs: Total execution time across all frames
 * - TotalCount: Total number of function calls
 * - AvgTimePerFrameMs: Average execution time per frame
 * - MaxTimePerFrameMs: Maximum execution time in any single frame
 * - AvgCountPerFrame: Average number of calls per frame
 * - MaxCountPerFrame: Maximum number of calls in any single frame
 * 
 * Multithreading Analysis:
 * - ThreadId: Unique identifier for the thread
 * - ThreadName: Human-readable thread name
 * - ThreadUtilizationPercent: Percentage of thread utilization
 * - IsMainThread: Whether function runs on main thread (UI blocking)
 * - IsRenderThread: Whether function runs on render thread (FPS impact)
 * - IsWorkerThread: Whether function runs on worker thread (parallelizable)
 * 
 * OPTIMIZATION GUIDANCE:
 * The JSON files provide data to identify:
 * 1. Performance Bottlenecks:
 *    - Functions with high TotalTimeMs (optimization priority)
 *    - Functions with high MaxTimePerFrameMs (cause frame drops)
 *    - Functions with high ThreadUtilizationPercent (thread saturation)
 * 
 * 2. Multithreading Issues:
 *    - Functions on main thread (IsMainThread: true) blocking UI
 *    - Functions on render thread (IsRenderThread: true) affecting FPS
 *    - Overloaded threads (ThreadUtilizationPercent > 80%)
 * 
 * 3. Optimization Opportunities:
 *    - Functions suitable for worker threads (IsWorkerThread: true)
 *    - Functions with high call frequency (AvgCountPerFrame)
 *    - Functions with inconsistent performance (high MaxTimePerFrameMs vs AvgTimePerFrameMs)
 * 
 * TECHNICAL DETAILS:
 * - Built with .NET 8.0
 * - Uses FrameProCore.dll for reading .framepro files
 * - Requires SCLCoreCLR.dll dependency
 * - Outputs structured JSON with System.Text.Json
 * - Handles large datasets efficiently (212 functions × 254 frames)
 * 
 * EXAMPLE ANALYSIS WORKFLOW:
 * 1. Run the tool on your .framepro file
 * 2. Open functions_analysis.json to identify top performance issues
 * 3. Filter by IsMainThread: true to find UI-blocking functions
 * 4. Sort by TotalTimeMs to prioritize optimization efforts
 * 5. Use frame_analysis.json to analyze specific problematic frames
 * 6. Cross-reference thread utilization to balance workload
 * 
 */

using System;
using System.IO;
using System.Text.Json;
using FramePro;
using SCLCoreCLR; // Add this using directive
using System.Collections.Generic; // Added for Dictionary and List
using System.Linq; // Added for OrderByDescending and Take

namespace FrameProReader
{
    // Classes for JSON serialization
    public class FrameAnalysisData
    {
        public int FrameNumber { get; set; }
        public double TimeMs { get; set; }
        public int Count { get; set; }
        public double MaxTimeMs { get; set; }
    }

    public class FunctionFrameAnalysis
    {
        public string FunctionName { get; set; } = "";
        public int ThreadId { get; set; }
        public string ThreadName { get; set; } = "";
        public double TimeMs { get; set; }
        public int Count { get; set; }
        public double MaxTimeMs { get; set; }
        
        // Key parameters for multithreading analysis
        public double TotalTimeMs { get; set; }           // Total function execution time
        public long TotalCount { get; set; }              // Total number of calls
        public double MaxTimePerFrameMs { get; set; }     // Maximum time per frame
        public long MaxCountPerFrame { get; set; }        // Maximum number of calls per frame
        public double AvgTimePerFrameMs { get; set; }     // Average time per frame
        public double AvgCountPerFrame { get; set; }      // Average number of calls per frame
        public double ThreadUtilizationPercent { get; set; } // Thread utilization percentage
        public bool IsMainThread { get; set; }            // Whether runs on main thread
        public bool IsRenderThread { get; set; }           // Whether runs on render thread
        public bool IsWorkerThread { get; set; }           // Whether runs on worker thread
        public int ThreadPriority { get; set; }            // Thread priority (if available)
    }

    public class FrameData
    {
        public int FrameNumber { get; set; }
        public List<FunctionFrameAnalysis> Functions { get; set; } = new List<FunctionFrameAnalysis>();
    }

    public class FrameByFrameAnalysis
    {
        public string SessionName { get; set; } = "";
        public int TotalFrames { get; set; }
        public List<FrameData> Frames { get; set; } = new List<FrameData>();
    }

    public class RegularFunctionAnalysis
    {
        public string FunctionName { get; set; } = "";
        public int ThreadId { get; set; }
        public string ThreadName { get; set; } = "";
        
        // Performance metrics
        public double TotalTimeMs { get; set; }
        public long TotalCount { get; set; }
        public double MaxTimePerFrameMs { get; set; }
        public long MaxCountPerFrame { get; set; }
        public double AvgTimePerFrameMs { get; set; }
        public double AvgCountPerFrame { get; set; }
        
        // Thread analysis
        public double ThreadUtilizationPercent { get; set; }
        public bool IsMainThread { get; set; }
        public bool IsRenderThread { get; set; }
        public bool IsWorkerThread { get; set; }
        public int ThreadPriority { get; set; }
    }

    public class RegularFunctionsAnalysis
    {
        public string SessionName { get; set; } = "";
        public int TotalFrames { get; set; }
        public int TotalFunctions { get; set; }
        public List<RegularFunctionAnalysis> Functions { get; set; } = new List<RegularFunctionAnalysis>();
    }

    class Program
    {
        static void Main(string[] args)
        {
            Session? session = null; // Declare session as nullable and initialize to null
            
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: dotnet run <filename.framepro>");
                return;
            }

            string filename = args[0];

            if (!File.Exists(filename))
            {
                Console.WriteLine($"Error: File '{filename}' not found.");
                return;
            }

            Console.WriteLine($"Attempting to read {filename}...");

            try
            {
                // FramePro Session requires an ILog implementation
                // For simplicity, we'll use a basic console logger here.
                ILog logger = new ConsoleLogger();
                CoreSettings settings = new CoreSettings();
                // Declare session without initializing to null

                try
                {
                    session = new Session(settings, logger);
                    string error = string.Empty;
                    if (session.Read(filename, ref error))
                    {
                        Console.WriteLine("File read successfully!");
                        Console.WriteLine($"Session Name: {session.SessionDetails.m_Name}");
                        Console.WriteLine($"Build ID: {session.SessionDetails.m_BuildId}");
                        Console.WriteLine($"Date: {session.SessionDetails.m_Date}");
                        Console.WriteLine($"Timer Frequency: {session.TimerFrequency}");
                        Console.WriteLine($"Frame Count: {session.FrameCount}");
                        Console.WriteLine($"Total Time Span Count: {session.TimeSpanCount}");

                        // Example of accessing some data
                        if (session.FrameCount > 0)
                        {
                            FramePro.Frame firstFrame = session.GetFrame(0);
                            Console.WriteLine($"First Frame Start Time: {firstFrame.StartTime}");
                            Console.WriteLine($"First Frame End Time: {firstFrame.EndTime}");
                            Console.WriteLine($"FramePro Library Version: {FramePro.Session.FrameProLibVersion}");
                        }

                        // Display thread information with functions
                        Console.WriteLine("\nThread Information:");
                        List<int> threadIds = session.GetThreadIds();
                        
                        // Get all TimeSpan stats first
                        List<ScopeSessionStats> timeSpanStats = session.GetTimeSpanStats();
                        
                        // Group functions by thread based on function names
                        Dictionary<int, List<(string functionName, double durationMs)>> threadFunctions = new Dictionary<int, List<(string, double)>>();
                        
                        // Initialize thread function lists
                        foreach (int threadId in threadIds)
                        {
                            threadFunctions[threadId] = new List<(string, double)>();
                        }
                        
                        // Distribute functions to threads using FramePro API
                        foreach (var stat in timeSpanStats)
                        {
                            double durationMs = (double)stat.m_TotalTime / session.TimerFrequency * 1000.0;
                            
                            // Get function ID by name
                            long functionId = session.GetTimeSpanNameId(stat.m_Name);
                            if (functionId == 0) continue; // Skip if function not found
                            
                            // Find which thread has the most activity for this function
                            int bestThreadId = -1;
                            long maxTimeForThread = 0;
                            
                            foreach (int threadId in threadIds)
                            {
                                try
                                {
                                    // Get average time per frame for this function in this thread
                                    long avgTimeForThread = session.GetTimeSpanFrameAverageTimeForThread(functionId, threadId);
                                    if (avgTimeForThread > maxTimeForThread)
                                    {
                                        maxTimeForThread = avgTimeForThread;
                                        bestThreadId = threadId;
                                    }
                                }
                                catch
                                {
                                    // Ignore errors and continue
                                }
                            }
                            
                            if (bestThreadId != -1)
                            {
                                threadFunctions[bestThreadId].Add((stat.m_Name, durationMs));
                            }
                            else
                            {
                                // If no thread-specific data found, add to main thread or first available thread
                                int mainThreadId = threadIds.FirstOrDefault(id => session.GetThreadName(id).Contains("Main Thread"));
                                if (mainThreadId != -1)
                                {
                                    threadFunctions[mainThreadId].Add((stat.m_Name, durationMs));
                                }
                                else if (threadIds.Count > 0)
                                {
                                    threadFunctions[threadIds[0]].Add((stat.m_Name, durationMs));
                                }
                            }
                        }
                        
                        // Display functions for each thread with detailed stats
                        foreach (int threadId in threadIds)
                        {
                            string threadName = session.GetThreadName(threadId);
                            Console.WriteLine($"\n═══════════════════════════════════════════════════════════════");
                            Console.WriteLine($"Thread ID: {threadId} | Name: {threadName}");
                            Console.WriteLine("═══════════════════════════════════════════════════════════════");
                            
                            var functions = threadFunctions[threadId].OrderByDescending(f => f.durationMs);
                            
                            int functionCount = 0;
                            foreach (var func in functions)
                            {
                                // Show top 10 functions per thread with detailed stats
                                if (functionCount < 10)
                                {
                                    // Get detailed stats for this function
                                    long functionId = session.GetTimeSpanNameId(func.functionName);
                                    if (functionId != 0)
                                    {
                                        try
                                        {
                                            // Get frame stats for this function
                                            long maxTimePerFrame = session.GetTimeSpanFrameMaxTime(functionId);
                                            long maxCountPerFrame = session.GetTimeSpanFrameMaxCount(functionId);
                                            long avgTimePerFrame = session.GetTimeSpanFrameAverageTime(functionId);
                                            double avgCountPerFrame = session.GetTimeSpanFrameAverageCount(functionId);
                                            
                                            // Convert to milliseconds
                                            double maxTimeMs = (double)maxTimePerFrame / session.TimerFrequency * 1000.0;
                                            double avgTimeMs = (double)avgTimePerFrame / session.TimerFrequency * 1000.0;
                                            
                                            Console.WriteLine($"  {functionCount + 1,2}. {func.functionName,-40}");
                                            Console.WriteLine($"      Total Time: {func.durationMs,8:F2} ms");
                                            Console.WriteLine($"      Max Time/Frame: {maxTimeMs,8:F2} ms | Max Count/Frame: {maxCountPerFrame,3}");
                                            Console.WriteLine($"      Avg Time/Frame: {avgTimeMs,8:F2} ms | Avg Count/Frame: {avgCountPerFrame,6:F2}");
                                            Console.WriteLine();
                                        }
                                        catch
                                        {
                                            // Fallback to simple display if detailed stats fail
                                            Console.WriteLine($"  {functionCount + 1,2}. {func.functionName,-40} {func.durationMs,8:F2} ms");
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine($"  {functionCount + 1,2}. {func.functionName,-40} {func.durationMs,8:F2} ms");
                                    }
                                }
                                functionCount++;
                            }
                            
                            if (functionCount > 10)
                            {
                                Console.WriteLine($"  ... and {functionCount - 10} more functions");
                            }
                            
                            Console.WriteLine($"Total functions in thread: {functionCount}");
                        }

                        // Frame-by-frame analysis for each thread
                        Console.WriteLine("\n═══════════════════════════════════════════════════════════════");
                        Console.WriteLine("FRAME-BY-FRAME ANALYSIS (Saving to JSON)");
                        Console.WriteLine("═══════════════════════════════════════════════════════════════");
                        
                        // Create frame-by-frame analysis data structure
                        var frameAnalysis = new FrameByFrameAnalysis
                        {
                            SessionName = Path.GetFileNameWithoutExtension(args[0]),
                            TotalFrames = session.FrameCount
                        };
                        
                        // Initialize frame data structures
                        for (int frameIndex = 0; frameIndex < session.FrameCount; frameIndex++)
                        {
                            frameAnalysis.Frames.Add(new FrameData
                            {
                                FrameNumber = frameIndex + 1,
                                Functions = new List<FunctionFrameAnalysis>()
                            });
                        }
                        
                        // Process ALL functions
                        var allFunctions = timeSpanStats.OrderByDescending(s => s.m_TotalTime);
                        int processedFunctions = 0;
                        
                        foreach (var stat in allFunctions)
                        {
                            long functionId = session.GetTimeSpanNameId(stat.m_Name);
                            if (functionId != 0)
                            {
                                try
                                {
                                    processedFunctions++;
                                    if (processedFunctions % 50 == 0)
                                    {
                                        Console.WriteLine($"Processing function {processedFunctions}/{timeSpanStats.Count}: {stat.m_Name}");
                                    }
                                    
                                    // Find which thread has the most activity for this function
                                    int bestThreadId = -1;
                                    long maxTimeForThread = 0;
                                    string bestThreadName = "";

                                    foreach (int threadId in threadIds)
                                    {
                                        try
                                        {
                                            // Get average time per frame for this function in this thread
                                            long avgTimeForThread = session.GetTimeSpanFrameAverageTimeForThread(functionId, threadId);
                                            if (avgTimeForThread > maxTimeForThread)
                                            {
                                                maxTimeForThread = avgTimeForThread;
                                                bestThreadId = threadId;
                                                bestThreadName = session.GetThreadName(threadId);
                                            }
                                        }
                                        catch
                                        {
                                            // Ignore errors and continue
                                        }
                                    }

                                    // If no thread-specific data found, use main thread or first available thread
                                    if (bestThreadId == -1)
                                    {
                                        int mainThreadId = threadIds.FirstOrDefault(id => session.GetThreadName(id).Contains("Main Thread"));
                                        if (mainThreadId != -1)
                                        {
                                            bestThreadId = mainThreadId;
                                            bestThreadName = session.GetThreadName(mainThreadId);
                                        }
                                        else if (threadIds.Count > 0)
                                        {
                                            bestThreadId = threadIds[0];
                                            bestThreadName = session.GetThreadName(threadIds[0]);
                                        }
                                    }
                                    
                                    // Get frame data for this function for each frame
                                    for (int frameIndex = 0; frameIndex < session.FrameCount; frameIndex++)
                                    {
                                        try
                                        {
                                            long frameTime;
                                            int frameCount;
                                            long maxTime;
                                            long maxTimePerFrame;
                                            long maxCountPerFrame;
                                            
                                            session.GetTimeSpanFrameTime(functionId, frameIndex, out frameTime, out frameCount, out maxTime, out maxTimePerFrame, out maxCountPerFrame);
                                            
                                            if (frameTime > 0)
                                            {
                                                double timeMs = (double)frameTime / session.TimerFrequency * 1000.0;
                                                double maxTimeMs = (double)maxTime / session.TimerFrequency * 1000.0;
                                                
                                                // Get detailed statistics for the function
                                                double totalTimeMs = (double)stat.m_TotalTime / session.TimerFrequency * 1000.0;
                                                double maxTimePerFrameMs = (double)stat.m_MaxTimePerFrame / session.TimerFrequency * 1000.0;
                                                double avgTimePerFrameMs = (double)session.GetTimeSpanFrameAverageTime(functionId) / session.TimerFrequency * 1000.0;
                                                double avgCountPerFrame = session.GetTimeSpanFrameAverageCount(functionId);
                                                
                                                // Determine thread type
                                                bool isMainThread = bestThreadName.Contains("Main Thread");
                                                bool isRenderThread = bestThreadName.Contains("Render") || bestThreadName.Contains("RenderThread");
                                                bool isWorkerThread = bestThreadName.Contains("Worker") || bestThreadName.Contains("Task") || bestThreadName.Contains("Thread");
                                                
                                                // Calculate thread utilization percentage (approximate)
                                                double threadUtilizationPercent = 0.0;
                                                if (session.FrameCount > 0)
                                                {
                                                    double avgFrameTimeMs = (double)session.AverageFrameTime / session.TimerFrequency * 1000.0;
                                                    threadUtilizationPercent = Math.Min(100.0, (timeMs / avgFrameTimeMs) * 100.0);
                                                }
                                                
                                                // Add function data to the specific frame
                                                frameAnalysis.Frames[frameIndex].Functions.Add(new FunctionFrameAnalysis
                                                {
                                                    FunctionName = stat.m_Name,
                                                    ThreadId = bestThreadId,
                                                    ThreadName = bestThreadName,
                                                    TimeMs = timeMs,
                                                    Count = frameCount,
                                                    MaxTimeMs = maxTimeMs,
                                                    
                                                    // Key parameters for multithreading analysis
                                                    TotalTimeMs = totalTimeMs,
                                                    TotalCount = stat.m_TotalCount,
                                                    MaxTimePerFrameMs = maxTimePerFrameMs,
                                                    MaxCountPerFrame = stat.m_MaxCountPerFrame,
                                                    AvgTimePerFrameMs = avgTimePerFrameMs,
                                                    AvgCountPerFrame = avgCountPerFrame,
                                                    ThreadUtilizationPercent = threadUtilizationPercent,
                                                    IsMainThread = isMainThread,
                                                    IsRenderThread = isRenderThread,
                                                    IsWorkerThread = isWorkerThread,
                                                    ThreadPriority = 0 // Thread priority not available through API
                                                });
                                            }
                                        }
                                        catch
                                        {
                                            // Skip frames with errors
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Error getting frame data for {stat.m_Name}: {ex.Message}");
                                }
                            }
                        }
                        
                        Console.WriteLine($"Processed {processedFunctions} functions across {session.FrameCount} frames");
                        
                        // Save frame-by-frame analysis to JSON file
                        try
                        {
                            string frameAnalysisFileName = Path.GetFileNameWithoutExtension(args[0]) + "_frame_analysis.json";
                            string frameAnalysisContent = JsonSerializer.Serialize(frameAnalysis, new JsonSerializerOptions 
                            { 
                                WriteIndented = true 
                            });
                            File.WriteAllText(frameAnalysisFileName, frameAnalysisContent);
                            Console.WriteLine($"\nFrame-by-frame analysis saved to: {frameAnalysisFileName}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error saving frame analysis JSON file: {ex.Message}");
                        }
                        
                        // Create and save regular functions analysis
                        try
                        {
                            Console.WriteLine("\n═══════════════════════════════════════════════════════════════");
                            Console.WriteLine("CREATING REGULAR FUNCTIONS ANALYSIS");
                            Console.WriteLine("═══════════════════════════════════════════════════════════════");
                            
                            var regularFunctionsAnalysis = new RegularFunctionsAnalysis
                            {
                                SessionName = Path.GetFileNameWithoutExtension(args[0]),
                                TotalFrames = session.FrameCount,
                                TotalFunctions = timeSpanStats.Count,
                                Functions = new List<RegularFunctionAnalysis>()
                            };
                            
                            foreach (var stat in timeSpanStats.OrderByDescending(s => s.m_TotalTime))
                            {
                                long functionId = session.GetTimeSpanNameId(stat.m_Name);
                                if (functionId != 0)
                                {
                                    try
                                    {
                                        // Find which thread has the most activity for this function
                                        int bestThreadId = -1;
                                        long maxTimeForThread = 0;
                                        string bestThreadName = "";

                                        foreach (int threadId in threadIds)
                                        {
                                            try
                                            {
                                                long avgTimeForThread = session.GetTimeSpanFrameAverageTimeForThread(functionId, threadId);
                                                if (avgTimeForThread > maxTimeForThread)
                                                {
                                                    maxTimeForThread = avgTimeForThread;
                                                    bestThreadId = threadId;
                                                    bestThreadName = session.GetThreadName(threadId);
                                                }
                                            }
                                            catch
                                            {
                                                // Ignore errors and continue
                                            }
                                        }

                                        // If no thread-specific data found, use main thread or first available thread
                                        if (bestThreadId == -1)
                                        {
                                            int mainThreadId = threadIds.FirstOrDefault(id => session.GetThreadName(id).Contains("Main Thread"));
                                            if (mainThreadId != -1)
                                            {
                                                bestThreadId = mainThreadId;
                                                bestThreadName = session.GetThreadName(mainThreadId);
                                            }
                                            else if (threadIds.Count > 0)
                                            {
                                                bestThreadId = threadIds[0];
                                                bestThreadName = session.GetThreadName(threadIds[0]);
                                            }
                                        }
                                        
                                        // Get detailed statistics
                                        double totalTimeMs = (double)stat.m_TotalTime / session.TimerFrequency * 1000.0;
                                        double maxTimePerFrameMs = (double)stat.m_MaxTimePerFrame / session.TimerFrequency * 1000.0;
                                        double avgTimePerFrameMs = (double)session.GetTimeSpanFrameAverageTime(functionId) / session.TimerFrequency * 1000.0;
                                        double avgCountPerFrame = session.GetTimeSpanFrameAverageCount(functionId);
                                        
                                        // Determine thread type
                                        bool isMainThread = bestThreadName.Contains("Main Thread");
                                        bool isRenderThread = bestThreadName.Contains("Render") || bestThreadName.Contains("RenderThread");
                                        bool isWorkerThread = bestThreadName.Contains("Worker") || bestThreadName.Contains("Task") || bestThreadName.Contains("Thread");
                                        
                                        // Calculate thread utilization percentage
                                        double threadUtilizationPercent = 0.0;
                                        if (session.FrameCount > 0)
                                        {
                                            double avgFrameTimeMs = (double)session.AverageFrameTime / session.TimerFrequency * 1000.0;
                                            threadUtilizationPercent = Math.Min(100.0, (avgTimePerFrameMs / avgFrameTimeMs) * 100.0);
                                        }
                                        
                                        regularFunctionsAnalysis.Functions.Add(new RegularFunctionAnalysis
                                        {
                                            FunctionName = stat.m_Name,
                                            ThreadId = bestThreadId,
                                            ThreadName = bestThreadName,
                                            
                                            // Performance metrics
                                            TotalTimeMs = totalTimeMs,
                                            TotalCount = stat.m_TotalCount,
                                            MaxTimePerFrameMs = maxTimePerFrameMs,
                                            MaxCountPerFrame = stat.m_MaxCountPerFrame,
                                            AvgTimePerFrameMs = avgTimePerFrameMs,
                                            AvgCountPerFrame = avgCountPerFrame,
                                            
                                            // Thread analysis
                                            ThreadUtilizationPercent = threadUtilizationPercent,
                                            IsMainThread = isMainThread,
                                            IsRenderThread = isRenderThread,
                                            IsWorkerThread = isWorkerThread,
                                            ThreadPriority = 0
                                        });
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"Error processing function {stat.m_Name}: {ex.Message}");
                                    }
                                }
                            }
                            
                            // Save regular functions analysis to JSON file
                            string regularFunctionsFileName = Path.GetFileNameWithoutExtension(args[0]) + "_functions_analysis.json";
                            string regularFunctionsContent = JsonSerializer.Serialize(regularFunctionsAnalysis, new JsonSerializerOptions 
                            { 
                                WriteIndented = true 
                            });
                            File.WriteAllText(regularFunctionsFileName, regularFunctionsContent);
                            Console.WriteLine($"Regular functions analysis saved to: {regularFunctionsFileName}");
                            Console.WriteLine($"Total functions analyzed: {regularFunctionsAnalysis.Functions.Count}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error saving regular functions JSON file: {ex.Message}");
                        }

                        Console.WriteLine("\n═══════════════════════════════════════════════════════════════");
                        Console.WriteLine("GLOBAL HOT FUNCTIONS (All Threads Combined)");
                        Console.WriteLine("═══════════════════════════════════════════════════════════════");

                        // Sort hot functions by total duration
                        var sortedHotFunctions = timeSpanStats.OrderByDescending(s => s.m_TotalTime);

                        int globalCount = 0;
                        foreach (var entry in sortedHotFunctions)
                        {
                            // Convert duration from timer frequency ticks to milliseconds for readability
                            double durationMs = (double)entry.m_TotalTime / session.TimerFrequency * 1000.0;
                            
                            // Get detailed stats for this function
                            long functionId = session.GetTimeSpanNameId(entry.m_Name);
                            if (functionId != 0)
                            {
                                try
                                {
                                    // Get frame stats for this function
                                    long maxTimePerFrame = session.GetTimeSpanFrameMaxTime(functionId);
                                    long maxCountPerFrame = session.GetTimeSpanFrameMaxCount(functionId);
                                    long avgTimePerFrame = session.GetTimeSpanFrameAverageTime(functionId);
                                    double avgCountPerFrame = session.GetTimeSpanFrameAverageCount(functionId);
                                    
                                    // Convert to milliseconds
                                    double maxTimeMs = (double)maxTimePerFrame / session.TimerFrequency * 1000.0;
                                    double avgTimeMs = (double)avgTimePerFrame / session.TimerFrequency * 1000.0;
                                    
                                    Console.WriteLine($"  {globalCount + 1,2}. {entry.m_Name,-40}");
                                    Console.WriteLine($"      Total Time: {durationMs,8:F2} ms");
                                    Console.WriteLine($"      Max Time/Frame: {maxTimeMs,8:F2} ms | Max Count/Frame: {maxCountPerFrame,3}");
                                    Console.WriteLine($"      Avg Time/Frame: {avgTimeMs,8:F2} ms | Avg Count/Frame: {avgCountPerFrame,6:F2}");
                                    Console.WriteLine();
                                }
                                catch
                                {
                                    // Fallback to simple display if detailed stats fail
                                    Console.WriteLine($"  {globalCount + 1,2}. {entry.m_Name,-40} {durationMs,8:F2} ms");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"  {globalCount + 1,2}. {entry.m_Name,-40} {durationMs,8:F2} ms");
                            }
                            globalCount++;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Error reading file: {error}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                    Console.WriteLine(ex.StackTrace);
                }
                finally
                {
                    if (session != null)
                    {
                        try
                        {
                            session.Dispose();
                        }
                        catch (NullReferenceException)
                        {
                            // Ignore null reference exceptions during disposal
                            // This can happen if internal objects are already disposed
                            // Data has already been saved, so this is not critical
                        }
                        catch (Exception disposeEx)
                        {
                            // Only log non-null-reference exceptions for debugging
                            // NullReferenceException during disposal is not critical
                            if (disposeEx is not NullReferenceException)
                            {
                                Console.WriteLine($"Error during session disposal: {disposeEx.Message}");
                                Console.WriteLine(disposeEx.StackTrace);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
    }

    // Simple ILog implementation for console output
    class ConsoleLogger : ILog
    {
        public void Write(string message)
        {
            Console.Write(message);
        }

        public void WriteLine(string message, LogVerbosity verbosity = LogVerbosity.Normal)
        {
            Console.WriteLine(message);
        }

        public void DebugWrite(string message)
        {
            Console.WriteLine($"DEBUG: {message}");
        }
    }
}
