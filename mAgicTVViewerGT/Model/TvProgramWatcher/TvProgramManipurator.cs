using System;
using System.IO;
using System.Text;

namespace mAgicTVViewerGT.Model.TvProgramWatcher
{
    public class TvProgramManipurator
    {
        private static int WAIT_MILLISECOND = 64;
        private static int WAIT_INTERVAL = 128;
        public static void MakeViewed(TvProgram tvPrg)
        {
            FileStream fs = null;
            StreamReader sr = null;
            StreamWriter sw = null;
            string contents = String.Empty;

            try
            {
                fs = new FileStream(tvPrg.FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                sr = new StreamReader(fs, Encoding.GetEncoding("shift_jis"));
                contents = sr.ReadToEnd();
            }
            finally
            {
                if (sr != null) sr.Close();
                sr = null;
                fs = null;
            }

            DateTime now = DateTime.Now;
            contents = contents.Replace("<history_cnt>0</history_cnt>", "<history_cnt>1</history_cnt>");
            contents = contents.Replace("<last_date>0000000000</last_date>",
                    "<last_date>" + now.Year.ToString("0000") + now.Month.ToString("00") + now.Day.ToString("00") + "00</last_date>");

            try
            {
                fs = new FileStream(tvPrg.FilePath, FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
                sw = new StreamWriter(fs, Encoding.GetEncoding("shift_jis"));
                sw.Write(contents);
            }
            finally
            {
                if (sw != null) sw.Close();
                sw = null;
                fs = null;
            }
        }

        public static void MakeUnviewed(TvProgram tvPrg)
        {
            FileStream fs = null;
            StreamReader sr = null;
            StreamWriter sw = null;
            string contents = String.Empty;

            try
            {
                fs = new FileStream(tvPrg.FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                sr = new StreamReader(fs, Encoding.GetEncoding("shift_jis"));
                contents = sr.ReadToEnd();
            }
            finally
            {
                if (sr != null) sr.Close();
                sr = null;
                fs = null;
            }

            contents = System.Text.RegularExpressions.Regex.Replace(
                    contents, "<history_cnt>[0-9]</history_cnt>", "<history_cnt>0</history_cnt>");
            contents = System.Text.RegularExpressions.Regex.Replace(
                    contents, "<last_date>[0-9]{n}</last_date>", "<last_date>0000000000</last_date>");

            try
            {
                fs = new FileStream(tvPrg.FilePath, FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
                sw = new StreamWriter(fs, Encoding.GetEncoding("shift_jis"));
                sw.Write(contents);
            }
            finally
            {
                if (sw != null) sw.Close();
                sw = null;
                fs = null;
            }
        }

        public static  void Delete(TvProgram[] tvPrgs)
        {
            string[] files = null;
            int count = 0;
            foreach (TvProgram tvPrg in tvPrgs)
            {
                tvPrg.IsSelected = false;
                try
                {
                    files = Directory.GetFiles(Directory.GetParent(tvPrg.FilePath).FullName);
                }
                catch (DirectoryNotFoundException e)
                {
                    System.Console.WriteLine(e);
                }

                int filecount = 0;
                for (int i = 0; i < files.Length; i++)
                {
                    if (files[i].ToLower().EndsWith(".dgno")) filecount++;
                }

                if (filecount == 1)
                {
                    Directory.Delete(Directory.GetParent(tvPrg.FilePath).FullName, true);
                }
                else if (filecount < 1)
                {
                    throw new FileNotFoundException(".dgno ファイルが見つかりません。");
                }
                else
                {
                    File.Delete(tvPrg.FilePath);
                }

                if (count < TvProgramManipurator.WAIT_INTERVAL)
                {
                    count++;
                }
                else
                {
                    System.Threading.Thread.Sleep(TvProgramManipurator.WAIT_MILLISECOND);
                    count = 0;
                }
            }
        }
    }
}
