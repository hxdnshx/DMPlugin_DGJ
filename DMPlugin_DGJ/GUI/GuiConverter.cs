using System;
using System.Globalization;
using System.Windows.Data;

namespace DMPlugin_DGJ
{
    internal class GuiConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.GetType() == typeof(SongItem.SongStatus))
            {
                SongItem.SongStatus status = (SongItem.SongStatus)value;
                switch (status)
                {
                    case SongItem.SongStatus.WaitingDownload:
                        return "等待下载";
                    case SongItem.SongStatus.Downloading:
                        return "正在下载";
                    case SongItem.SongStatus.WaitingPlay:
                        return "等待播放";
                    case SongItem.SongStatus.Playing:
                        return "正在播放";
                    default:
                        return "？？？？";
                }
            }
            else if (value.GetType() == typeof(bool))
            {
                if ((bool)value)
                { return "支持"; }
                else
                { return "不支持"; }
            }
            else if (value.GetType() == typeof(NAudio.CoreAudioApi.DeviceState))
            {
                switch ((NAudio.CoreAudioApi.DeviceState)value)
                {
                    case NAudio.CoreAudioApi.DeviceState.Active:
                        return "可用";
                    case NAudio.CoreAudioApi.DeviceState.Disabled:
                        return "禁用";
                    case NAudio.CoreAudioApi.DeviceState.NotPresent:
                        return "NotPresent";
                    case NAudio.CoreAudioApi.DeviceState.Unplugged:
                        return "Unplugged";
                    case NAudio.CoreAudioApi.DeviceState.All:
                        return "All";
                    default:
                        return "DefaultValue";
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
