using System.ComponentModel;

namespace DMPlugin_DGJ
{
    internal class BlackInfoItem : INotifyPropertyChanged
    {
        /// <summary>
        /// 新建黑名单信息
        /// </summary>
        /// <param name="isEnable">是否生效</param>
        /// <param name="type">黑名单类型</param>
        /// <param name="Text">黑名单内容</param>
        internal BlackInfoItem(bool isEnable = false, BlackInfoType type = BlackInfoType.歌名, string Text = "")
        {
            BLK_Enable = isEnable;
            BLK_Type = type;
            BLK_Text = Text;
            RaisePropertyChanged("");
        }

        /// <summary>
        /// 是否生效
        /// </summary>
        public bool BLK_Enable
        {
            get { return _Enable; }
            set { if (_Enable != value) { _Enable = value; RaisePropertyChanged(nameof(BLK_Enable)); } }
        }
        private bool _Enable;

        /// <summary>
        /// 黑名单类型
        /// </summary>
        public BlackInfoType BLK_Type
        {
            get { return _Type; }
            set { if (_Type != value) { _Type = value; RaisePropertyChanged(nameof(BLK_Type)); } }
        }
        private BlackInfoType _Type;

        /// <summary>
        /// 黑名单内容
        /// </summary>
        public string BLK_Text
        {
            get { return _Text; }
            set { if (_Text != value) { _Text = value; RaisePropertyChanged(nameof(BLK_Text)); } }
        }
        private string _Text;

        /// <summary>
        /// .
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName)
        { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
    }

    /// <summary>
    /// 黑名单类型
    /// </summary>
    public enum BlackInfoType
    {
        /// <summary>
        /// 
        /// </summary>
        歌名,
        /// <summary>
        /// 
        /// </summary>
        歌手,
        /// <summary>
        /// 
        /// </summary>
        歌曲ID
    }

}
