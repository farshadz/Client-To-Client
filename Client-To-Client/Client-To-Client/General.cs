using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client_To_Client
{
    class General
    {
        public  void creatErrorLog(Exception ex)
        {
            DateTime dt = DateTime.Now;
            string fileName = dt.ToShortDateString();
            string message = string.Format("Time: {0}", DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt"));
            message += Environment.NewLine;
            message += "-----------------------------------------------------------";
            message += Environment.NewLine;
            message += string.Format("Message: {0}", ex.Message);
            message += Environment.NewLine;
            message += string.Format("StackTrace: {0}", ex.StackTrace);
            message += Environment.NewLine;
            message += string.Format("Source: {0}", ex.Source);
            message += Environment.NewLine;
            message += string.Format("TargetSite: {0}", ex.TargetSite.ToString());
            message += Environment.NewLine;
            message += "-----------------------------------------------------------";
            message += Environment.NewLine;
            string path = AppDomain.CurrentDomain.BaseDirectory.ToString() + @"\Error Log";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            using (StreamWriter writer = new StreamWriter(path + @"\"+fileName+".txt", true))
            {
                writer.WriteLine(message);
                writer.Close();
            }
        }
    }
}
