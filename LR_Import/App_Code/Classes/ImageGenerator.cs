using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Configuration;
using LRWarehouse.DAL;
using LR_Import.ILSharedLearning.Services;


namespace LearningRegistryCache2.App_Code.Classes
{
    public class ImageGenerator
    {
        public string Url = "";
        public int Id;
        private ManualResetEvent _doneEvent;
        private string result = "";

        public ImageGenerator(string url, int id, ManualResetEvent doneEvent)
        {
            Url = url;
            Id = id;
            _doneEvent = doneEvent;
        }

        public void ImageGeneratorThreadPoolCallback(object threadContext) {
            try
            {
                WebDALServiceSoapClient ws = new WebDALServiceSoapClient();
                try
                {
                    if (WebConfigurationManager.AppSettings["IsTestEnv"] == "true")
                    {
                        result = ws.GetSearchThumbnail(this.Url, "jgtest-" + this.Id.ToString());
                    }
                    else
                    {
                        result = ws.GetSearchThumbnail(this.Url, this.Id.ToString());
                    }
                }
                catch (Exception ex)
                {
                    BaseDataManager.LogError("ImageGenerator.ImageGeneratorThreadPoolCallback(): " + ex.ToString());
                }
                try
                {
                    if (WebConfigurationManager.AppSettings["IsTestEnv"] == "true")
                    {
                        result = ws.GetDetailThumbnail(this.Url, "jgtest-" + this.Id.ToString());
                    }
                    else
                    {
                        result = ws.GetDetailThumbnail(this.Url, this.Id.ToString());
                    }
                }
                catch (Exception ex)
                {
                    BaseDataManager.LogError("ImageGenerator.ImageGeneratorThreadPoolCallback(): " + ex.ToString());
                }
            }
            catch (Exception ex)
            {
                BaseDataManager.LogError("ImageGenerator.ImageGeneratorThreadPoolCallback(): " + ex.ToString());
            }

            _doneEvent.Set();
        }
    }
}
