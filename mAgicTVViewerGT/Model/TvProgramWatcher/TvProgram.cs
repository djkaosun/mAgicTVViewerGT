using System;
using System.ComponentModel;
using System.IO;
using System.Xml;

namespace mAgicTVViewerGT.Model.TvProgramWatcher
{
    public class TvProgram : IEquatable<TvProgram>, INotifyPropertyChanged
    {
        private static string VIEWED = "　";
        private static string UNVIEWED = "●";
        private static string RECORDING = "◆";
        private static string VIEWED_RECORDING = "◇";

        private bool _IsSelected;
        public bool IsSelected
        {
            get
            {
                return this._IsSelected;
            }
            set
            {
                this._IsSelected = value;
                if (this.PropertyChanged != null) this.PropertyChanged(this, new PropertyChangedEventArgs("IsSelected"));
            }
        }
        public string FilePath { get; private set; }
        //public int Id { get; private set; }
        //public DateTime LimitDate { get; private set; }
        public int ContentsCount { get; private set; }
        public int ProgramCount { get; private set; }
        public int PictureCount { get; private set; }
        public int ChapterCount { get; private set; }
        //public int ExtensionCount { get; private set; }
        //public int Extension_RecSeg { get; private set; }
        //public int Extension_Quality { get; private set; }
        //public string Extension_QualityTitle { get; private set; }
        //public int Extension_VideoFormat { get; private set; }
        //public int Extension_VideoProfile { get; private set; }
        //public int Extension_VideoWidth { get; private set; }
        //public int Extension_VideoHeight { get; private set; }
        //public int Extension_VideoLevel { get; private set; }
        //public int Extension_VideoMaxBitrate { get; private set; }
        //public int Extension_VideoAverageBitrate { get; private set; }
        //public int Extension_VideoMinBitrate { get; private set; }
        //public int Extension_AudioFormat { get; private set; }
        //public int Extension_AudioBitrate { get; private set; }
        //public int Extension_SoundMux { get; private set; }
        //public int Extension_RemainCopyTimes { get; private set; }
        //public int Extension_MtvPlayerFlag { get; private set; }
        //public int Extension_MtvPlayerResumeTime { get; private set; }
        //public int Extension_MtvPlayerVolume { get; private set; }
        //public int Extension_MtvPlayerMuxMode { get; private set; }
        //public int Extension_MtvPlayerTrack { get; private set; }
        //public int Contents_Major { get; private set; }
        //public int Contents_Minor { get; private set; }
        public string Contents_FileName { get; private set; }
        //public string Contents_ProductName { get; private set; }
        //public string Contents_Format { get; private set; }
        //public DateTime Contents_ValidLimit { get; private set; }
        public bool IsUnviewed { get; private set; }
        public string Status
        {
            get {
                if (this.IsUnviewed) return TvProgram.UNVIEWED;
                else return TvProgram.VIEWED;
            }
        }
        //public DateTime Contents_LastDateTime { get; private set; }
        //public bool Contents_Protect { get; private set; }
        public string Contents_Title { get; private set; }
        public string Contents_Supplement { get; private set; }
        public string Contents_ExpText { get; private set; }
        public string Contents_ComponentText { get; private set; }
        public string Contents_AudioText { get; private set; }
        //public string Contents_DataContent { get; private set; }
        //public string Contents_Catag { get; private set; }
        //public string Contents_Caverification { get; private set; }
        //public string Contents_SeriesName { get; private set; }
        //public int Contents_ComponentType { get; private set; }
        //public int Contents_AudioType { get; private set; }
        //public int Contents_AudioEsFlag { get; private set; }
        //public int Contents_AudioMainFlag { get; private set; }
        //public int Contents_AudioQuality { get; private set; }
        //public int Contents_AudioSamplingRate { get; private set; }
        //public int Contents_Canum { get; private set; }
        //public int Contents_SeriesRepeat { get; private set; }
        //public int Contents_SeriesPattern { get; private set; }
        //public int Contents_SeriesExpireFlag { get; private set; }
        //public int Contents_AudioLanguage { get; private set; }
        //public int Contents_AudioLanguage2 { get; private set; }
        //public int Contents_ExpireDate { get; private set; }
        //public int Contents_SeriesId { get; private set; }
        //public int Contents_SeriesEpisode { get; private set; }
        //public int Contents_SeriesLastEpisode { get; private set; }
        //public int Contents_FileSize { get; private set; }
        //public int Contents_FilesizeHi { get; private set; }
        //public int Contents_EncoderId { get; private set; }
        //public int Contents_DeviceId { get; private set; }
        //public int Contents_DeviceUniqueId { get; private set; }
        //public string Contents_DeviceName { get; private set; }
        //public int Program_Formula { get; private set; }
        public int Program_Channel { get; private set; }
        public string Program_Station { get; private set; }
        //public int Program_NetworkId { get; private set; }
        //public int Program_TransportStream { get; private set; }
        //public int Program_ServiceId { get; private set; }
        public DateTime Program_StartDateTime { get; private set; }
        public uint Program_Length { get; private set; }
        public bool IsLoaded { get; private set; }
        public bool IsRecording { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private static DateTime parseDateTime(string date, string time) {
            try
            {
                return new DateTime(
                        int.Parse(date.Substring(0, 4)),
                        int.Parse(date.Substring(4, 2)),
                        int.Parse(date.Substring(6, 2)),
                        int.Parse(time.Substring(0, 2)),
                        int.Parse(time.Substring(2, 2)),
                        int.Parse(time.Substring(4, 2))
                        );
            }
            catch (ArgumentOutOfRangeException)
            {
                return DateTime.MinValue;
            }
        }

        public TvProgram(string filepath) : this(filepath, true)
        {
        }

        internal TvProgram(string filePath, bool isLoadable)
        {
            this.FilePath = filePath;
            this.IsLoaded = false;
            if (isLoadable)
            {
                XmlDocument xmlDocument = new XmlDocument();
                StreamReader reader = null;
                FileStream stream = null;

                bool loaded = false;
                for (int i = 0; i < 10; i++)
                {
                    try
                    {
                        stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        reader = new StreamReader(stream, System.Text.Encoding.GetEncoding("shift_jis"));
                        xmlDocument.Load(reader);
                        loaded = true;
                        break;
                    }
                    catch (IOException e)
                    {
                        System.Console.WriteLine(e);
                        System.Threading.Thread.Sleep(100);
                    }
                    finally
                    {
                        if (reader != null)
                        {
                            reader.Close();
                        }
                    }
                }

                if (!loaded) throw new IOException("XML ファイルを読み取れませんでした。: " + filePath);

                this.Contents_Title = xmlDocument.SelectNodes("/Meta/Contents/title").Item(0).InnerText;
                this.Program_StartDateTime = parseDateTime(
                    xmlDocument.SelectNodes("/Meta/Program/start_date").Item(0).InnerText,
                    xmlDocument.SelectNodes("/Meta/Program/start_time").Item(0).InnerText);
                this.Program_Station = xmlDocument.SelectNodes("/Meta/Program/station").Item(0).InnerText;
                this.FilePath = filePath;
                //this.Id = int.Parse(xmlDocument.SelectNodes("/Meta/id").Item(0).InnerText);
                //this.LimitDate = parseDateTime(xmlDocument.SelectNodes("/Meta/limit_date").Item(0).InnerText, "000000");
                this.ContentsCount = int.Parse(xmlDocument.SelectNodes("/Meta/contents_cnt").Item(0).InnerText);
                this.ProgramCount = int.Parse(xmlDocument.SelectNodes("/Meta/program_cnt").Item(0).InnerText);
                this.PictureCount = int.Parse(xmlDocument.SelectNodes("/Meta/picture_cnt").Item(0).InnerText);
                this.ChapterCount = int.Parse(xmlDocument.SelectNodes("/Meta/chapter_cnt").Item(0).InnerText);
                /*this.ExtensionCount = int.Parse(xmlDocument.SelectNodes("/Meta/extension_cnt").Item(0).InnerText);
                this.Extension_RecSeg = int.Parse(xmlDocument.SelectNodes("/Meta/Extension/RecSeg").Item(0).InnerText);
                this.Extension_Quality = int.Parse(xmlDocument.SelectNodes("/Meta/Extension/Quality").Item(0).InnerText);
                this.Extension_QualityTitle = xmlDocument.SelectNodes("/Meta/Extension/QualityTitle").Item(0).InnerText;
                try
                {
                    this.Extension_VideoFormat = int.Parse(xmlDocument.SelectNodes("/Meta/Extension/VideoFormat").Item(0).InnerText);
                }
                catch (NullReferenceException)
                {
                    Console.WriteLine(filePath + " に \"/Meta/Extension/VideoFormat\" がありません。");
                }

                try
                {
                    this.Extension_VideoProfile = int.Parse(xmlDocument.SelectNodes("/Meta/Extension/VideoProfile").Item(0).InnerText);
                }
                catch (NullReferenceException)
                {
                    Console.WriteLine(filePath + " に \"/Meta/Extension/VideoProfile\" がありません。");
                }

                try
                {
                    this.Extension_VideoWidth = int.Parse(xmlDocument.SelectNodes("/Meta/Extension/VideoWidth").Item(0).InnerText);
                }
                catch (NullReferenceException)
                {
                    Console.WriteLine(filePath + " に \"/Meta/Extension/VideoWidth\" がありません。");
                }

                try
                {
                    this.Extension_VideoHeight = int.Parse(xmlDocument.SelectNodes("/Meta/Extension/VideoHeight").Item(0).InnerText);
                }
                catch (NullReferenceException)
                {
                    Console.WriteLine(filePath + " に \"/Meta/Extension/VideoHeight\" がありません。");
                }

                try
                {
                    this.Extension_VideoLevel = int.Parse(xmlDocument.SelectNodes("/Meta/Extension/VideoLevel").Item(0).InnerText);
                }
                catch (NullReferenceException)
                {
                    Console.WriteLine(filePath + " に \"/Meta/Extension/VideoLevel\" がありません。");
                }

                try
                {
                    this.Extension_VideoMaxBitrate = int.Parse(xmlDocument.SelectNodes("/Meta/Extension/VideoMaxBitrate").Item(0).InnerText);
                }
                catch (NullReferenceException)
                {
                    Console.WriteLine(filePath + " に \"/Meta/Extension/VideoMaxBitrate\" がありません。");
                }

                try
                {
                    this.Extension_VideoAverageBitrate = int.Parse(xmlDocument.SelectNodes("/Meta/Extension/VideoAvgBitrate").Item(0).InnerText);
                }
                catch (NullReferenceException)
                {
                    Console.WriteLine(filePath + " に \"/Meta/Extension/VideoAvgBitrate\" がありません。");
                }

                try
                {
                    this.Extension_VideoMinBitrate = int.Parse(xmlDocument.SelectNodes("/Meta/Extension/VideoMinBitrate").Item(0).InnerText);
                }
                catch (NullReferenceException)
                {
                    Console.WriteLine(filePath + " に \"/Meta/Extension/VideoMinBitrate\" がありません。");
                }

                try
                {
                    this.Extension_AudioFormat = int.Parse(xmlDocument.SelectNodes("/Meta/Extension/AudioFormat").Item(0).InnerText);
                }
                catch (NullReferenceException)
                {
                    Console.WriteLine(filePath + " に \"/Meta/Extension/AudioFormat\" がありません。");
                }

                try
                {
                    this.Extension_AudioBitrate = int.Parse(xmlDocument.SelectNodes("/Meta/Extension/AudioBitrate").Item(0).InnerText);

                }
                catch (NullReferenceException)
                {
                    Console.WriteLine(filePath + " に \"/Meta/Extension/AudioBitrate\" がありません。");
                }

                try
                {
                    this.Extension_SoundMux = int.Parse(xmlDocument.SelectNodes("/Meta/Extension/SoundMux").Item(0).InnerText);
                }
                catch (NullReferenceException)
                {
                    Console.WriteLine(filePath + " に \"/Meta/Extension/SoundMux\" がありません。");
                }

                try
                {
                    this.Extension_RemainCopyTimes = int.Parse(xmlDocument.SelectNodes("/Meta/Extension/RemainCopyTimes").Item(0).InnerText);
                }
                catch (NullReferenceException)
                {
                    Console.WriteLine(filePath + " に \"/Meta/Extension/RemainCopyTimes\" がありません。");
                }

                try
                {
                    this.Extension_MtvPlayerFlag = int.Parse(xmlDocument.SelectNodes("/Meta/Extension/mtvPlayer_Flag").Item(0).InnerText);
                }
                catch (NullReferenceException)
                {
                    Console.WriteLine(filePath + " に \"/Meta/Extension/mtvPlayer_Flag\" がありません。");
                }

                try
                {
                    this.Extension_MtvPlayerResumeTime = int.Parse(xmlDocument.SelectNodes("/Meta/Extension/mtvPlayer_ResumeTime").Item(0).InnerText);
                }
                catch (NullReferenceException)
                {
                    Console.WriteLine(filePath + " に \"/Meta/Extension/mtvPlayer_ResumeTime\" がありません。");
                }

                try
                {
                    this.Extension_MtvPlayerVolume = int.Parse(xmlDocument.SelectNodes("/Meta/Extension/mtvPlayer_Volume").Item(0).InnerText);
                }
                catch (NullReferenceException)
                {
                    Console.WriteLine(filePath + " に \"/Meta/Extension/mtvPlayer_Volume\" がありません。");
                }

                try
                {
                    this.Extension_MtvPlayerMuxMode = int.Parse(xmlDocument.SelectNodes("/Meta/Extension/mtvPlayer_MuxMode").Item(0).InnerText);
                }
                catch (NullReferenceException)
                {
                    Console.WriteLine(filePath + " に \"/Meta/Extension/mtvPlayer_MuxMode\" がありません。");
                }
                try
                {
                    this.Extension_MtvPlayerTrack = int.Parse(xmlDocument.SelectNodes("/Meta/Extension/mtvPlayer_Track").Item(0).InnerText);
                }
                catch (NullReferenceException)
                {
                    Console.WriteLine(filePath + " に \"/Meta/Extension/mtvPlayer_MuxMode\" がありません。");
                }//*/
                //this.Contents_Major = int.Parse(xmlDocument.SelectNodes("/Meta/Contents/major").Item(0).InnerText);
                //this.Contents_Minor = int.Parse(xmlDocument.SelectNodes("/Meta/Contents/minor").Item(0).InnerText);
                this.Contents_FileName = xmlDocument.SelectNodes("/Meta/Contents/filename").Item(0).InnerText;
                //this.Contents_ProductName = xmlDocument.SelectNodes("/Meta/Contents/productname").Item(0).InnerText;
                //this.Contents_Format = xmlDocument.SelectNodes("/Meta/Contents/format").Item(0).InnerText;
                //this.Contents_ValidLimit = parseDateTime(xmlDocument.SelectNodes("/Meta/Contents/valid_limit").Item(0).InnerText, "000000");
                int Contents_HistoryCount = int.Parse(xmlDocument.SelectNodes("/Meta/Contents/history_cnt").Item(0).InnerText);
                this.IsUnviewed = !(Contents_HistoryCount > 0);
                //this.Contents_LastDateTime = parseDateTime(
                //    xmlDocument.SelectNodes("/Meta/Contents/last_date").Item(0).InnerText,
                //    xmlDocument.SelectNodes("/Meta/Contents/last_time").Item(0).InnerText);
                //this.Contents_Protect = (xmlDocument.SelectNodes("/Meta/Contents/protect").Item(0).InnerText != "0");
                this.Contents_Supplement = xmlDocument.SelectNodes("/Meta/Contents/supplement").Item(0).InnerText;
                this.Contents_ExpText = xmlDocument.SelectNodes("/Meta/Contents/exptext").Item(0).InnerText;
                this.Contents_ComponentText = xmlDocument.SelectNodes("/Meta/Contents/componenttext").Item(0).InnerText;
                this.Contents_AudioText = xmlDocument.SelectNodes("/Meta/Contents/audiotext").Item(0).InnerText;
                //this.Contents_DataContent = xmlDocument.SelectNodes("/Meta/Contents/datacontent").Item(0).InnerText;
                //this.Contents_Catag = xmlDocument.SelectNodes("/Meta/Contents/catag").Item(0).InnerText;
                //this.Contents_Caverification = xmlDocument.SelectNodes("/Meta/Contents/caverification").Item(0).InnerText;
                //this.Contents_SeriesName = xmlDocument.SelectNodes("/Meta/Contents/seriesname").Item(0).InnerText;
                //this.Contents_ComponentType = int.Parse(xmlDocument.SelectNodes("/Meta/Contents/componenttype").Item(0).InnerText);
                //this.Contents_AudioType = int.Parse(xmlDocument.SelectNodes("/Meta/Contents/audiotype").Item(0).InnerText);
                //this.Contents_AudioEsFlag = int.Parse(xmlDocument.SelectNodes("/Meta/Contents/audioesflag").Item(0).InnerText);
                //this.Contents_AudioMainFlag = int.Parse(xmlDocument.SelectNodes("/Meta/Contents/audiomainflag").Item(0).InnerText);
                //this.Contents_AudioQuality = int.Parse(xmlDocument.SelectNodes("/Meta/Contents/audioquality").Item(0).InnerText);
                //this.Contents_AudioSamplingRate = int.Parse(xmlDocument.SelectNodes("/Meta/Contents/audiosamplingrate").Item(0).InnerText);
                //this.Contents_Canum = int.Parse(xmlDocument.SelectNodes("/Meta/Contents/canum").Item(0).InnerText);
                //this.Contents_SeriesRepeat = int.Parse(xmlDocument.SelectNodes("/Meta/Contents/seriesrepeat").Item(0).InnerText);
                //this.Contents_SeriesPattern = int.Parse(xmlDocument.SelectNodes("/Meta/Contents/seriespattern").Item(0).InnerText);
                //this.Contents_SeriesExpireFlag = int.Parse(xmlDocument.SelectNodes("/Meta/Contents/seriesexpireflag").Item(0).InnerText);
                //this.Contents_AudioLanguage = int.Parse(xmlDocument.SelectNodes("/Meta/Contents/audiolanguage").Item(0).InnerText);
                //this.Contents_AudioLanguage2 = int.Parse(xmlDocument.SelectNodes("/Meta/Contents/audiolanguage2").Item(0).InnerText);
                //this.Contents_ExpireDate = int.Parse(xmlDocument.SelectNodes("/Meta/Contents/expiredate").Item(0).InnerText);
                //this.Contents_SeriesId = int.Parse(xmlDocument.SelectNodes("/Meta/Contents/seriesid").Item(0).InnerText);
                //this.Contents_SeriesEpisode = int.Parse(xmlDocument.SelectNodes("/Meta/Contents/seriesepisode").Item(0).InnerText);
                //this.Contents_SeriesLastEpisode = int.Parse(xmlDocument.SelectNodes("/Meta/Contents/serieslastepisode").Item(0).InnerText);
                //this.Contents_FileSize = int.Parse(xmlDocument.SelectNodes("/Meta/Contents/filesize").Item(0).InnerText);
                //this.Contents_FilesizeHi = int.Parse(xmlDocument.SelectNodes("/Meta/Contents/filesize_hi").Item(0).InnerText);
                //this.Contents_EncoderId = int.Parse(xmlDocument.SelectNodes("/Meta/Contents/encoderid").Item(0).InnerText);
                //this.Contents_DeviceId = int.Parse(xmlDocument.SelectNodes("/Meta/Contents/deviceid").Item(0).InnerText);
                //this.Contents_DeviceUniqueId = int.Parse(xmlDocument.SelectNodes("/Meta/Contents/deviceuniqueid").Item(0).InnerText);
                //this.Contents_DeviceName = xmlDocument.SelectNodes("/Meta/Contents/devicename").Item(0).InnerText;
                //this.Program_Formula = int.Parse(xmlDocument.SelectNodes("/Meta/Program/formula").Item(0).InnerText);
                this.Program_Channel = int.Parse(xmlDocument.SelectNodes("/Meta/Program/channel").Item(0).InnerText);
                //this.Program_NetworkId = int.Parse(xmlDocument.SelectNodes("/Meta/Program/networkid").Item(0).InnerText);
                //this.Program_TransportStream = int.Parse(xmlDocument.SelectNodes("/Meta/Program/transportstream").Item(0).InnerText);
                //this.Program_ServiceId = int.Parse(xmlDocument.SelectNodes("/Meta/Program/serviceid").Item(0).InnerText);
                DateTime Program_EndDateTime = parseDateTime(
                    xmlDocument.SelectNodes("/Meta/Program/end_date").Item(0).InnerText,
                    xmlDocument.SelectNodes("/Meta/Program/end_time").Item(0).InnerText);
                this.Program_Length = (uint)Math.Round((Program_EndDateTime - this.Program_StartDateTime).TotalMinutes, MidpointRounding.AwayFromZero);

                this.IsLoaded = true;
            }
        }
/*
        protected internal TvProgram(
                string contents_title,
                DateTime program_startdatetime,
                string program_station,
                string filepath,
                int id,
                DateTime limitdate,
                int contentscount,
                int programcount,
                int picturecount,
                int chaptercount,
                int eextensioncount,
                int extension_recseg,
                int extension_quality,
                string extension_qualitytitle,
                int extension_videoformat,
                int extension_videoprofile,
                int extension_videowidth,
                int extension_videoheight,
                int extension_videolevel,
                int extension_videomaxbitrate,
                int extension_videoavgbitrate,
                int extension_videominbitrate,
                int extension_audioformat,
                int extension_audiobitrate,
                int extension_soundmux,
                int extension_remaincopytimes,
                int extension_mtvplayer_flag,
                int extension_mtvplayer_resumetime,
                int extension_mtvplayer_volume,
                int extension_mtvplayer_muxmode,
                int extension_mtvplayer_track,
                int contents_major,
                int contents_minor,
                string contents_filename,
                string contents_productname,
                string contents_format,
                DateTime contents_validlimit,
                int contents_historycount,
                DateTime contents_lastdatetime,
                bool contents_protect,
                string contents_supplement,
                string contents_exptext,
                string contents_componenttext,
                string contents_audiotext,
                string contents_datacontent,
                string contents_catag,
                string contents_caverification,
                string contents_seriesname,
                int contents_componenttype,
                int contents_audiotype,
                int contents_audioesflag,
                int contents_audiomainflag,
                int contents_audioquality,
                int contents_audiosamplingrate,
                int contents_canum,
                int contents_seriesrepeat,
                int contents_seriespattern,
                int contents_seriesexpireflag,
                int contents_audiolanguage,
                int contents_audiolanguage2,
                int contents_expiredate,
                int contents_seriesid,
                int contents_seriesepisode,
                int contents_serieslastepisode,
                int contents_filesize,
                int contents_filesizehi,
                int contents_encoderid,
                int contents_deviceid,
                int contents_deviceuniqueid,
                string contents_devicename,
                int program_formula,
                int program_channel,
                int program_networkid,
                int program_transportstream,
                int program_serviceid,
                DateTime program_enddatetime
                )
        {
            this.Contents_Title = contents_title;
            this.Program_StartDateTime = program_startdatetime;
            this.Program_Station = program_station;
            this.FilePath = filepath;
            this.Id = id;
            this.LimitDate = limitdate;
            this.ContentsCount = contentscount;
            this.ProgramCount = programcount;
            this.PictureCount = picturecount;
            this.ChapterCount = chaptercount;
            this.EextensionCount = eextensioncount;
            this.Extension_RecSeg = extension_recseg;
            this.Extension_Quality = extension_quality;
            this.Extension_QualityTitle = extension_qualitytitle;
            this.Extension_VideoFormat = extension_videoformat;
            this.Extension_VideoProfile = extension_videoprofile;
            this.Extension_VideoWidth = extension_videowidth;
            this.Extension_VideoHeight = extension_videoheight;
            this.Extension_VideoLevel = extension_videolevel;
            this.Extension_VideoMaxBitrate = extension_videomaxbitrate;
            this.Extension_VideoAverageBitrate = extension_videoavgbitrate;
            this.Extension_VideoMinBitrate = extension_videominbitrate;
            this.Extension_AudioFormat = extension_audioformat;
            this.Extension_AudioBitrate = extension_audiobitrate;
            this.Extension_SoundMux = extension_soundmux;
            this.Extension_RemainCopyTimes = extension_remaincopytimes;
            this.Extension_MtvPlayerFlag = extension_mtvplayer_flag;
            this.Extension_MtvPlayerResumeTime = extension_mtvplayer_resumetime;
            this.Extension_MtvPlayerVolume = extension_mtvplayer_volume;
            this.Extension_MtvPlayerMuxMode = extension_mtvplayer_muxmode;
            this.Extension_MtvPlayerTrack = extension_mtvplayer_track;
            this.Contents_Major = contents_major;
            this.Contents_Minor = contents_minor;
            this.Contents_FileName = contents_filename;
            this.Contents_ProductName = contents_productname;
            this.Contents_Format = contents_format;
            this.Contents_ValidLimit = contents_validlimit;
            this.Contents_HistoryCount = contents_historycount;
            this.Contents_LastDateTime = contents_lastdatetime;
            this.Contents_Protect = contents_protect;
            this.Contents_Supplement = contents_supplement;
            this.Contents_ExpText = contents_exptext;
            this.Contents_ComponentText = contents_componenttext;
            this.Contents_AudioText = contents_audiotext;
            this.Contents_DataContent = contents_datacontent;
            this.Contents_Catag = contents_catag;
            this.Contents_Caverification = contents_caverification;
            this.Contents_SeriesName = contents_seriesname;
            this.Contents_ComponentType = contents_componenttype;
            this.Contents_AudioType = contents_audiotype;
            this.Contents_AudioEsFlag = contents_audioesflag;
            this.Contents_AudioMainFlag = contents_audiomainflag;
            this.Contents_AudioQuality = contents_audioquality;
            this.Contents_AudioSamplingRate = contents_audiosamplingrate;
            this.Contents_Canum = contents_canum;
            this.Contents_SeriesRepeat = contents_seriesrepeat;
            this.Contents_SeriesPattern = contents_seriespattern;
            this.Contents_SeriesExpireFlag = contents_seriesexpireflag;
            this.Contents_AudioLanguage = contents_audiolanguage;
            this.Contents_AudioLanguage2 = contents_audiolanguage2;
            this.Contents_ExpireDate = contents_expiredate;
            this.Contents_SeriesId = contents_seriesid;
            this.Contents_SeriesEpisode = contents_seriesepisode;
            this.Contents_SeriesLastEpisode = contents_serieslastepisode;
            this.Contents_FileSize = contents_filesize;
            this.Contents_FilesizeHi = contents_filesizehi;
            this.Contents_EncoderId = contents_encoderid;
            this.Contents_DeviceId = contents_deviceid;
            this.Contents_DeviceUniqueId = contents_deviceuniqueid;
            this.Contents_DeviceName = contents_devicename;
            this.Program_Formula = program_formula;
            this.Program_Channel = program_channel;
            this.Program_NetworkId = program_networkid;
            this.Program_TransportStream = program_transportstream;
            this.Program_ServiceId = program_serviceid;
            this.Program_EndDateTime = program_enddatetime;
        }
*/

        bool IEquatable<TvProgram>.Equals(TvProgram other)
        {
            return this.FilePath.ToLower() == other.FilePath.ToLower();
        }
    }
}
