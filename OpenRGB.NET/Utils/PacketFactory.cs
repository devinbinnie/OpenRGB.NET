﻿using System;

namespace OpenRGB.NET.Utils;

/// <summary>
/// Static class to generate packets for various commands
/// </summary>
internal static class PacketFactory
{
    internal static uint ResizeZoneLength()
    {
        //4 uint zone_index
        //4 uint new_zone_size
        
        return 4 + 4;
    }
    internal static void ResizeZone(ref SpanWriter writer, uint zoneIndex, uint newZoneSize)
    {
        //4 uint zone_index
        //4 uint new_zone_size
        
        writer.WriteUInt32(zoneIndex);
        writer.WriteUInt32(newZoneSize);
    }
    
    internal static uint UpdateLedsLength(int ledCount)
    {
        //4 uint data_size
        //2 ushort led_count
        //4 * led_count uint led_data
        
        return (uint)(4 + 2 + 4 * ledCount);
    }
    internal static void UpdateLeds(ref SpanWriter writer, in ReadOnlySpan<Color> colors)
    {
        uint length = UpdateLedsLength(colors.Length);
        writer.WriteUInt32(length);
        writer.WriteUInt16((ushort)colors.Length);
        
        for (int i = 0; i < colors.Length; i++)
            colors[i].WriteTo(ref writer);
    }
    
    internal static uint UpdateZoneLedsLength(int ledCount)
    {
        //4 uint data_size
        //4 uint zone_index
        //2 ushort led_count
        //4 * led_count uint led_data
        
        return (uint)(4 + 4 + 2 + 4 * ledCount);
    }
    internal static void UpdateZoneLeds(ref SpanWriter writer, uint zoneIndex, in ReadOnlySpan<Color> colors)
    {
        uint length = UpdateZoneLedsLength(colors.Length);
        writer.WriteUInt32(length);
        writer.WriteUInt32(zoneIndex);
        writer.WriteUInt16((ushort)colors.Length);
            
        for (int i = 0; i < colors.Length; i++)
            colors[i].WriteTo(ref writer);
    }
    
    internal static uint UpdateSingleLedLength()
    {
        //4 uint led_index
        //4 color color
        
        return 4 + 4;
    }
    internal static void UpdateSingleLed(ref SpanWriter writer, uint ledIndex, in Color color)
    {
        writer.WriteUInt32(ledIndex);
        color.WriteTo(ref writer);
    }
    
    internal static uint UpdateModeLength(uint modeLength)
    {
        //4 uint length
        //4 uint mode_index
        //x mode data
        
        return 4 + 4 + modeLength;
    }
    internal static void UpdateMode(ref SpanWriter writer, Mode mode, uint modeIndex, uint protocolVersion)
    {
        uint length = UpdateModeLength(mode.GetLength());
        
        writer.WriteUInt32(length);
        writer.WriteUInt32(modeIndex);
        mode.WriteTo(ref writer, protocolVersion);
    }
}