using GpuSSharp.Libs.Nvml;
using GpuSSharp.Types;
using Grpc.Core;
using Newtonsoft.Json;
using Serilog;
using SLimit.Contracts;
using SLimit.Daemon.Types.Converters;

namespace SLimit.Daemon;

public class GpuControlService : GpuControl.GpuControlBase
{
    public override Task<DaemonStatusReply> GpuApplyFanCurve(
        FancurveSetRequest request,
        ServerCallContext context)
    {
        Log.Information("Received Fan Curve Apply Request for GPU: {gpuId}", request.GpuId);
        
        var gpu = Program.GpuService?.GetGpuByPcieId(request.GpuId);

        if (gpu is null)
        {
            Log.Error("Could not find GPU with ID {gpuId}", request.GpuId);
            return Task.FromResult(new DaemonStatusReply
            {
                StatusCode = 1,
                Message = $"Requested GPU ID {request.GpuId} does not exist."
            });
        }

        var fanCurve = JsonConvert.DeserializeObject<FanCurve>(request.CurveJson,
            new JsonSerializerSettings() { Converters = [new GpuClockTuneConverter()] });

        if (fanCurve is null)
        {
            Log.Error("Error while reading requested fan curve on GPU ID {gpuId}", request.GpuId);
            return Task.FromResult(new DaemonStatusReply
            {
                StatusCode = 2,
                Message = $"Error while reading requested fan curve."
            });
        }
        
        var cts = new CancellationTokenSource();
        
        var task = Task.Run(async () => await FanCurves.FanSpeedProfileThread(500,gpu,fanCurve,cts.Token),  cts.Token);

        var success = true;
        
        success &= FanCurves.RunningFanProfilesTasks.TryAdd(request.GpuId, task);
        success &= FanCurves.RunningFanProfilesCancelTokens.TryAdd(request.GpuId, cts);
        
        return Task.FromResult(new DaemonStatusReply
        {
            StatusCode = success ? 0 : 2,
            Message = success ? $"Power limit successfully set" : "Error while applying power limit"
        });
    }
    
    
    public override Task<DaemonStatusReply> GpuApplyPowerLimit(
        PowerLimitSetRequest request,
        ServerCallContext context)
    {
        Log.Information("Received Power Limit Set Request for GPU: {gpuId} PL: {powerLimitMw} mW", request.GpuId, request.PowerLimitMw);
        
        var gpu = Program.GpuService?.GetGpuByPcieId(request.GpuId);

        if (gpu is null)
        {
            Log.Error("Could not find GPU with ID {gpuId}", request.GpuId);
            return Task.FromResult(new DaemonStatusReply
            {
                StatusCode = 1,
                Message = $"Requested GPU ID {request.GpuId} does not exist."
            });
        }
        
        var success = gpu.SetGpuPowerLimit(request.PowerLimitMw);
        
        if (!success)
            Log.Error("Error while applying power limit");
            
        
        return Task.FromResult(new DaemonStatusReply
        {
            
            StatusCode = success ? 0 : 2,
            Message = success ? $"Power limit successfully set" : "Error while applying power limit"
        });
    }
    
    public override Task<DaemonStatusReply> GpuApplyVoltageOffset(
        VoltageOffsetSetRequest request,
        ServerCallContext context)
    {
        Log.Information("Received Voltage Offset Set Request for GPU: {gpuId} Voff: {powerLimitMw} mV", request.GpuId, request.VoltOffsetMv);
        
        var gpu = Program.GpuService?.GetGpuByPcieId(request.GpuId);

        if (gpu is null)
        {
            Log.Error("Could not find GPU with ID {gpuId}", request.GpuId);
            return Task.FromResult(new DaemonStatusReply
            {
                StatusCode = 1,
                Message = $"Requested GPU ID {request.GpuId} does not exist."
            });
        }
        
        var success = gpu.SetCoreVoltageOffset(request.VoltOffsetMv);
        
        return Task.FromResult(new DaemonStatusReply
        {
            StatusCode = success ? 0 : 2,
            Message = success ? $"Voltage offset successfully set" : "Error while applying voltage offset"
        });
    }
    
    public override Task<DaemonStatusReply> GpuApplyCoreTune(
        ClockSetRequest request,
        ServerCallContext context)
    {
        
        GpuClockTune tune = ToDomain(request.Tune);
        Log.Information("Received Core Tune Apply Request for GPU: {gpuId} \nTune: {tuneInfo}", request.GpuId, tune);

        var gpu = Program.GpuService?.GetGpuByPcieId(request.GpuId);

        if (gpu is null)
        {
            Log.Error("Could not find GPU with ID {gpuId}", request.GpuId);
            return Task.FromResult(new DaemonStatusReply
            {
                StatusCode = 1,
                Message = $"Requested GPU ID {request.GpuId} does not exist."
            });
        }
        
        var success = gpu.SetCoreTuning(tune);

        return Task.FromResult(new DaemonStatusReply
        {
            StatusCode = success ? 0 : 2,
            Message = success ? $"Core tuning successfully set" : "Error while applying core tuning"
        });
    }
    
    public override Task<DaemonStatusReply> GpuApplyMemoryTune(
        ClockSetRequest request,
        ServerCallContext context)
    {
        
        GpuClockTune tune = ToDomain(request.Tune);
        Log.Information("Received Memory Tune Apply Request for GPU: {gpuId} \nTune: {tuneInfo}", request.GpuId, tune);

        var gpu = Program.GpuService?.GetGpuByPcieId(request.GpuId);

        if (gpu is null)
        {
            Log.Error("Could not find GPU with ID {gpuId}", request.GpuId);
            return Task.FromResult(new DaemonStatusReply
            {
                StatusCode = 1,
                Message = $"Requested GPU ID {request.GpuId} does not exist."
            });
        }
        
        var success = gpu.SetMemTuning(tune);

        return Task.FromResult(new DaemonStatusReply
        {
            StatusCode = success ? 0 : 2,
            Message = success ? $"Memory tuning successfully set" : "Memory while applying core tuning"
        });
    }
    
    
    private static GpuClockTune ToDomain(ClockTuneMessage? message)
    {
        if (message is null)
        {
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "Missing tune message."));
        }

        return message.ValueCase switch
        {
            ClockTuneMessage.ValueOneofCase.Offset =>
                new GpuClockTune.Offset(
                    message.Offset.OffsetMhz,
                    (GpuPState)message.Offset.PState),

            ClockTuneMessage.ValueOneofCase.Overdrive =>
                new GpuClockTune.Overdrive(
                    message.Overdrive.Percent),

            ClockTuneMessage.ValueOneofCase.ClockRange =>
                new GpuClockTune.ClockRange(
                    message.ClockRange.MinMhz,
                    message.ClockRange.MaxMhz),

            _ => throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "Unknown tune message type."))
        };
    }
}