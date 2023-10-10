﻿using System.Threading.Channels;
using Bld.WlanUtils;
using Rtl8812auNet;
using Rtl8812auNet.Rtl8812au;
using Rtl8812auNet.Rtl8812au.Models;
using ChannelWidth = Rtl8812auNet.Rtl8812au.Enumerations.ChannelWidth;

namespace WiFiBroadcastNet.Devices;

public class RtlDevice : IDevice
{
    private readonly Rtl8812aDevice _rtlDevice;

    private ChannelWriter<RxFrame> _receivedFramesChannelWriter;
    private WlanChannel _channel = Channels.Ch036;
    private bool _isStarted = false;

    public RtlDevice(Rtl8812aDevice rtlDevice)
    {
        _rtlDevice = rtlDevice;
    }

    public void SetChannel(WlanChannel channel)
    {
        _channel = channel;
        if (_isStarted)
        {
            ApplyChannel();
        }
    }

    public void StartReceiving(ChannelWriter<RxFrame> receivedFramesChannel)
    {
        if (_isStarted == true)
        {
            throw new Exception("Already started");
        }

        _receivedFramesChannelWriter = receivedFramesChannel;

        StartDevice();
        ApplyChannel();
        _isStarted = true;
    }

    private void StartDevice()
    {
        _rtlDevice.Init(PacketProcessor, CreateCurrentChannel());
        ApplyChannel();
    }

    private async Task PacketProcessor(ParsedRadioPacket arg)
    {
        await _receivedFramesChannelWriter.WriteAsync(new RxFrame()
        {
            Data = arg.Data,
        });
    }

    private void ApplyChannel()
    {
        _rtlDevice.SetMonitorChannel(CreateCurrentChannel());
    }

    private SelectedChannel CreateCurrentChannel()
    {
        return new SelectedChannel
        {
            Channel = (byte)_channel.ChannelFrequencyMHz,
            ChannelOffset = 0,
            ChannelWidth = ChannelWidth.CHANNEL_WIDTH_20
        };
    }
}