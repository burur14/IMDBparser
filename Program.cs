using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;

namespace IMDBparser
{
    class Program
    {
        string Url { get;} 
        string Directory { get;}

        int FileCounter = 0;

        public Program(string url, string directory) { 
            this.Url = url; 
            this.Directory = directory;
        }
        
        public void parse()
        {
            ChromeDriverService chromeDriverService = ChromeDriverService.CreateDefaultService();
            chromeDriverService.HideCommandPromptWindow = true;
            var options = new ChromeOptions();

            using (var driver = new ChromeDriver(chromeDriverService, options))
            {
                driver.Manage().Window.Maximize();
                driver.Navigate().GoToUrl(Url);

                var divElements = driver.FindElements(By.CssSelector("#ratings-container .lister-item-image img"));
                List<string> urlList = getLinks(driver, divElements);
                DownloadPosters(urlList);

                while(driver.FindElements(By.CssSelector(".flat-button.lister-page-next.next-page")).Count > 0){
                    driver.FindElements(By.CssSelector(".flat-button.lister-page-next.next-page"))[0].Click();

                    divElements = driver.FindElements(By.CssSelector("#ratings-container .lister-item-image img"));
                    urlList = getLinks(driver, divElements);
                    DownloadPosters(urlList);
                }
            }
        }

        void DownloadPosters(List<string> urlList)
        {
            foreach (var urlItem in urlList)
            {
                Image? image = DownloadImageFromUrl(urlItem);
                string fileName = Path.Combine(Directory, "poster" + $"{FileCounter}" + ".jpg");
                image.Save(fileName);
                ++FileCounter;
            }
        }

        List<string> getLinks(ChromeDriver driver, ReadOnlyCollection<IWebElement> divElements)
        {
            List<string> urlList = new List<string>();

            foreach (var item in divElements)
            {
                Actions actions = new Actions(driver);
                actions.MoveToElement(item);
                actions.Perform();
                string src = item.GetAttribute("src");
                urlList.Add(src);
            }

            return urlList;
        }

        public static Image DownloadImageFromUrl(string imageUrl)
        {
            Image image;

            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create(imageUrl);
                webRequest.AllowWriteStreamBuffering = true;
                webRequest.Timeout = 30000;

                System.Net.WebResponse webResponse = webRequest.GetResponse();

                System.IO.Stream stream = webResponse.GetResponseStream();

                image = System.Drawing.Image.FromStream(stream);

                webResponse.Close();
            }
            catch (Exception ex)
            {
                return null;
            }

            return image;
        }
    }
}